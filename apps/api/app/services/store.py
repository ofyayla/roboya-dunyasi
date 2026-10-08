"""Linking store purchases to parent accounts and applying store notifications (F1-17, GLR-03)."""

import logging
from datetime import datetime

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import Settings
from app.models.account import Account
from app.models.store import StoreSubscription
from app.repositories import store as store_repo
from app.services import entitlements
from app.services.errors import InvalidSignatureApiError, PurchaseLinkedError
from app.services.store_verifier import (
    EventKind,
    InvalidStoreSignatureError,
    StoreEvent,
    StoreVerifier,
    VerifiedPurchase,
)

logger = logging.getLogger(__name__)

_STATUS_FOR_KIND = {
    EventKind.GRACE: "grace",
    EventKind.EXPIRED: "expired",
    EventKind.REFUNDED: "revoked",
}


def _status_for(kind: EventKind, purchase: VerifiedPurchase) -> str:
    if kind in (EventKind.SUBSCRIBED, EventKind.RENEWED):
        return "trial" if purchase.is_trial else "active"
    return _STATUS_FOR_KIND[kind]


def _status_from_purchase(purchase: VerifiedPurchase, now: datetime) -> str:
    if purchase.expires_at <= now:
        return "expired"
    return "trial" if purchase.is_trial else "active"


async def link_purchase(
    session: AsyncSession,
    account: Account,
    store: str,
    proof: str,
    verifier: StoreVerifier,
    settings: Settings,
    now: datetime,
) -> entitlements.Entitlement:
    """The parent presents a store purchase; once its signature checks out it is tied to them."""
    try:
        purchase = verifier.verify_purchase(store, proof)
    except InvalidStoreSignatureError as e:
        raise InvalidSignatureApiError from e

    existing = await store_repo.find(session, store, purchase.original_transaction_id)
    if existing is None:
        await store_repo.add(
            session,
            store,
            purchase.original_transaction_id,
            purchase.product_id,
            _status_from_purchase(purchase, now),
            purchase.expires_at,
            purchase.is_trial,
            now,
            account_id=account.id,
        )
    else:
        # One purchase belongs to one parent account; another parent cannot take it over.
        if existing.account_id is not None and existing.account_id != account.id:
            raise PurchaseLinkedError
        existing.account_id = account.id
        existing.updated_at = now
        # A notification may already hold newer facts; the proof only extends what we know.
        if purchase.expires_at > existing.expires_at and existing.status not in ("revoked",):
            existing.expires_at = purchase.expires_at
            existing.status = _status_from_purchase(purchase, now)
            existing.is_trial = purchase.is_trial
    await session.commit()
    logger.info("store.purchase_linked", extra={"store": store})
    return await entitlements.compute(session, account, settings, now)


async def handle_notification(
    session: AsyncSession,
    store: str,
    signed_payload: str,
    verifier: StoreVerifier,
    now: datetime,
) -> None:
    """Applies one store notification. Safe to retry and to receive out of order (CLAUDE.md §8)."""
    try:
        event = verifier.verify_notification(store, signed_payload)
    except InvalidStoreSignatureError as e:
        raise InvalidSignatureApiError from e

    if not await store_repo.record_event(session, event.event_id, store, now):
        await session.commit()
        return  # already processed

    await _apply(session, store, event, now)
    await session.commit()
    logger.info("store.event_applied", extra={"store": store, "kind": event.kind.value})


async def _apply(session: AsyncSession, store: str, event: StoreEvent, now: datetime) -> None:
    purchase = event.purchase
    status = _status_for(event.kind, purchase)
    existing = await store_repo.find(session, store, purchase.original_transaction_id)
    if existing is None:
        # Not claimed by a parent yet: keep the facts so the later receipt finds them.
        sub = await store_repo.add(
            session,
            store,
            purchase.original_transaction_id,
            purchase.product_id,
            status,
            purchase.expires_at,
            purchase.is_trial,
            now,
        )
        sub.last_event_at = event.occurred_at
        return
    if event.occurred_at < existing.last_event_at:
        return  # an older event arrived after a newer one
    _update(existing, status, event, now)


def _update(sub: StoreSubscription, status: str, event: StoreEvent, now: datetime) -> None:
    sub.status = status
    sub.product_id = event.purchase.product_id
    sub.expires_at = event.purchase.expires_at
    sub.is_trial = event.purchase.is_trial
    sub.last_event_at = event.occurred_at
    sub.updated_at = now
