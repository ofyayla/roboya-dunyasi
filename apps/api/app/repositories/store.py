import uuid
from datetime import datetime

from sqlalchemy import select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.store import StoreEventRecord, StoreSubscription


async def record_event(session: AsyncSession, event_id: str, store: str, now: datetime) -> bool:
    """Returns True when the event is new, False when it was already processed (retry)."""
    statement = (
        insert(StoreEventRecord)
        .values(event_id=event_id, store=store, received_at=now)
        .on_conflict_do_nothing(index_elements=[StoreEventRecord.event_id])
        .returning(StoreEventRecord.event_id)
    )
    return (await session.execute(statement)).scalar_one_or_none() is not None


async def find(
    session: AsyncSession, store: str, original_transaction_id: str
) -> StoreSubscription | None:
    result = await session.execute(
        select(StoreSubscription).where(
            StoreSubscription.store == store,
            StoreSubscription.original_transaction_id == original_transaction_id,
        )
    )
    return result.scalar_one_or_none()


async def add(
    session: AsyncSession,
    store: str,
    original_transaction_id: str,
    product_id: str,
    status: str,
    expires_at: datetime,
    is_trial: bool,
    now: datetime,
    account_id: uuid.UUID | None = None,
) -> StoreSubscription:
    subscription = StoreSubscription(
        store=store,
        original_transaction_id=original_transaction_id,
        account_id=account_id,
        product_id=product_id,
        status=status,
        expires_at=expires_at,
        is_trial=is_trial,
        last_event_at=now,
        updated_at=now,
    )
    session.add(subscription)
    await session.flush()
    return subscription


async def for_account(session: AsyncSession, account_id: uuid.UUID) -> list[StoreSubscription]:
    result = await session.execute(
        select(StoreSubscription).where(StoreSubscription.account_id == account_id)
    )
    return list(result.scalars())
