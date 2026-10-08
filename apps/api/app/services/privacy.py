"""Consent, the privacy centre and account deletion (F1-13, F1-25; UYM-01, UYM-03)."""

import logging
from datetime import datetime, timedelta
from typing import Any

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import Settings
from app.models.account import Account
from app.models.privacy import DeletionRequest
from app.repositories import accounts, privacy, profiles
from app.repositories import store as store_repo
from app.services import legal
from app.services.errors import (
    ConsentRequiredError,
    DeletionPendingError,
    NotFoundError,
    NoticeOutdatedError,
)

logger = logging.getLogger(__name__)


async def has_consent(session: AsyncSession, account: Account, settings: Settings) -> bool:
    """Consent to the current notice version, not withdrawn."""
    notice = legal.current_notice(settings)
    return await privacy.open_consent(session, account.id, notice.version) is not None


async def require_consent(session: AsyncSession, account: Account, settings: Settings) -> None:
    """Child data is processed only with the parent's consent (UYM-01)."""
    if not await has_consent(session, account, settings):
        raise ConsentRequiredError


async def give_consent(
    session: AsyncSession, account: Account, version: str, settings: Settings, now: datetime
) -> None:
    notice = legal.current_notice(settings)
    if version != notice.version:
        raise NoticeOutdatedError
    if await privacy.open_consent(session, account.id, version) is None:
        await privacy.add_consent(session, account.id, version, now)
        await session.commit()
        logger.info("privacy.consent_given")


async def withdraw_consent(session: AsyncSession, account: Account, now: datetime) -> None:
    if await privacy.withdraw_consents(session, account.id, now):
        await session.commit()
        logger.info("privacy.consent_withdrawn")


async def request_deletion(
    session: AsyncSession, account: Account, settings: Settings, now: datetime
) -> DeletionRequest:
    existing = await privacy.deletion_request_of(session, account.id)
    if existing is not None:
        return existing
    request = await privacy.add_deletion_request(
        session, account.id, now, now + timedelta(days=settings.deletion_cooling_days)
    )
    await session.commit()
    logger.info("privacy.deletion_requested")
    return request


async def cancel_deletion(session: AsyncSession, account: Account) -> None:
    request = await privacy.deletion_request_of(session, account.id)
    if request is None:
        raise NotFoundError
    await privacy.cancel_deletion_request(session, request)
    await session.commit()


async def deletion_status(session: AsyncSession, account: Account) -> DeletionRequest:
    request = await privacy.deletion_request_of(session, account.id)
    if request is None:
        raise NotFoundError
    return request


async def process_due_deletions(session: AsyncSession, now: datetime) -> int:
    """Deletes every account whose waiting period is over. Idempotent; run from a scheduled job."""
    done = 0
    for request in await privacy.due_deletions(session, now):
        await privacy.delete_account(session, request, now)
        done += 1
    await session.commit()
    if done:
        logger.info("privacy.deletions_done", extra={"count": done})
    return done


async def my_data(session: AsyncSession, account: Account) -> dict[str, Any]:
    """Everything we hold about the parent: account, consents, devices, profiles and progress."""
    consents = await privacy.consents_of(session, account.id)
    devices = await accounts.list_devices(session, account.id)
    profile_rows = await profiles.list_for_account(session, account.id)
    subscriptions = await store_repo.for_account(session, account.id)
    request = await privacy.deletion_request_of(session, account.id)
    return {
        "account": {
            "id": str(account.id),
            "email": account.email,
            "created_at": account.created_at.isoformat(),
        },
        "consents": [
            {
                "notice_version": c.notice_version,
                "accepted_at": c.accepted_at.isoformat(),
                "withdrawn_at": c.withdrawn_at.isoformat() if c.withdrawn_at else None,
            }
            for c in consents
        ],
        "devices": [
            {
                "platform": d.platform,
                "created_at": d.created_at.isoformat(),
                "last_seen_at": d.last_seen_at.isoformat(),
            }
            for d in devices
        ],
        "profiles": [
            {
                "id": str(p.id),
                "nickname": p.nickname,
                "avatar_id": p.avatar_id,
                "age_band": p.age_band,
                "created_at": p.created_at.isoformat(),
                "progress": await profiles.stars_of(session, p.id),
            }
            for p in profile_rows
        ],
        "subscriptions": [
            {
                "store": s.store,
                "product_id": s.product_id,
                "status": s.status,
                "expires_at": s.expires_at.isoformat(),
            }
            for s in subscriptions
        ],
        "deletion_request": (
            {
                "requested_at": request.requested_at.isoformat(),
                "scheduled_for": request.scheduled_for.isoformat(),
            }
            if request
            else None
        ),
    }


def reject_if_deletion_pending(request: DeletionRequest | None) -> None:
    if request is not None:
        raise DeletionPendingError
