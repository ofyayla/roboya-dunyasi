import uuid
from datetime import datetime

from sqlalchemy import func, select, update
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.account import LoginCode, RefreshToken


async def count_recent_codes(session: AsyncSession, email_digest: str, since: datetime) -> int:
    result = await session.execute(
        select(func.count())
        .select_from(LoginCode)
        .where(LoginCode.email_digest == email_digest, LoginCode.created_at >= since)
    )
    return int(result.scalar_one())


async def add_code(
    session: AsyncSession,
    code_id: uuid.UUID,
    email_digest: str,
    code_digest: str,
    now: datetime,
    expires_at: datetime,
) -> LoginCode:
    code = LoginCode(
        id=code_id,
        email_digest=email_digest,
        code_digest=code_digest,
        created_at=now,
        expires_at=expires_at,
    )
    session.add(code)
    await session.flush()
    return code


async def latest_open_code(
    session: AsyncSession, email_digest: str, now: datetime
) -> LoginCode | None:
    """The newest unconsumed, unexpired code for this email (older ones stop working)."""
    result = await session.execute(
        select(LoginCode)
        .where(
            LoginCode.email_digest == email_digest,
            LoginCode.consumed_at.is_(None),
            LoginCode.expires_at > now,
        )
        .order_by(LoginCode.created_at.desc())
        .limit(1)
    )
    return result.scalar_one_or_none()


async def add_refresh_token(
    session: AsyncSession,
    token_digest: str,
    account_id: uuid.UUID,
    device_registration_id: uuid.UUID,
    now: datetime,
    expires_at: datetime,
) -> RefreshToken:
    token = RefreshToken(
        token_digest=token_digest,
        account_id=account_id,
        device_registration_id=device_registration_id,
        created_at=now,
        expires_at=expires_at,
    )
    session.add(token)
    await session.flush()
    return token


async def find_refresh_token(session: AsyncSession, token_digest: str) -> RefreshToken | None:
    result = await session.execute(
        select(RefreshToken).where(RefreshToken.token_digest == token_digest)
    )
    return result.scalar_one_or_none()


async def revoke_device_tokens(
    session: AsyncSession, device_registration_id: uuid.UUID, now: datetime
) -> None:
    await session.execute(
        update(RefreshToken)
        .where(
            RefreshToken.device_registration_id == device_registration_id,
            RefreshToken.revoked_at.is_(None),
        )
        .values(revoked_at=now)
    )
