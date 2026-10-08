import uuid
from datetime import datetime

from sqlalchemy import delete, select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.account import Account, LoginCode
from app.models.privacy import ConsentRecord, DeletionLog, DeletionRequest


async def consents_of(session: AsyncSession, account_id: uuid.UUID) -> list[ConsentRecord]:
    result = await session.execute(
        select(ConsentRecord)
        .where(ConsentRecord.account_id == account_id)
        .order_by(ConsentRecord.accepted_at, ConsentRecord.id)
    )
    return list(result.scalars())


async def open_consent(
    session: AsyncSession, account_id: uuid.UUID, version: str
) -> ConsentRecord | None:
    result = await session.execute(
        select(ConsentRecord).where(
            ConsentRecord.account_id == account_id,
            ConsentRecord.notice_version == version,
            ConsentRecord.withdrawn_at.is_(None),
        )
    )
    return result.scalars().first()


async def add_consent(
    session: AsyncSession, account_id: uuid.UUID, version: str, now: datetime
) -> ConsentRecord:
    record = ConsentRecord(account_id=account_id, notice_version=version, accepted_at=now)
    session.add(record)
    await session.flush()
    return record


async def withdraw_consents(session: AsyncSession, account_id: uuid.UUID, now: datetime) -> int:
    count = 0
    for record in await consents_of(session, account_id):
        if record.withdrawn_at is None:
            record.withdrawn_at = now
            count += 1
    await session.flush()
    return count


async def deletion_request_of(
    session: AsyncSession, account_id: uuid.UUID
) -> DeletionRequest | None:
    result = await session.execute(
        select(DeletionRequest).where(DeletionRequest.account_id == account_id)
    )
    return result.scalar_one_or_none()


async def add_deletion_request(
    session: AsyncSession, account_id: uuid.UUID, requested_at: datetime, scheduled_for: datetime
) -> DeletionRequest:
    request = DeletionRequest(
        account_id=account_id, requested_at=requested_at, scheduled_for=scheduled_for
    )
    session.add(request)
    await session.flush()
    return request


async def cancel_deletion_request(session: AsyncSession, request: DeletionRequest) -> None:
    await session.delete(request)
    await session.flush()


async def due_deletions(session: AsyncSession, now: datetime) -> list[DeletionRequest]:
    result = await session.execute(
        select(DeletionRequest).where(DeletionRequest.scheduled_for <= now)
    )
    return list(result.scalars())


async def delete_account(
    session: AsyncSession, request: DeletionRequest, completed_at: datetime
) -> None:
    """Removes the account; devices, tokens, profiles, progress and the request cascade with it."""
    session.add(DeletionLog(requested_at=request.requested_at, completed_at=completed_at))
    account = await session.get(Account, request.account_id)
    if account is not None:
        await session.delete(account)
    await session.flush()


async def purge_login_codes(session: AsyncSession, before: datetime) -> int:
    result = await session.execute(delete(LoginCode).where(LoginCode.expires_at < before))
    return int(result.rowcount or 0)  # type: ignore[attr-defined]
