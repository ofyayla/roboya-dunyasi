"""Parent sign-in with an emailed one-time code (F1-14, GLR-03)."""

import logging
import uuid
from dataclasses import dataclass
from datetime import datetime, timedelta

from sqlalchemy.ext.asyncio import AsyncSession

from app.core import security
from app.core.config import Settings
from app.models.account import Account
from app.repositories import accounts, auth
from app.services.email import EmailSender
from app.services.errors import (
    DeviceLimitError,
    InvalidCodeError,
    InvalidTokenApiError,
    TooManyRequestsError,
)

logger = logging.getLogger(__name__)


@dataclass(frozen=True)
class TokenPair:
    access_token: str
    refresh_token: str
    expires_in: int
    account_id: uuid.UUID


async def request_code(
    session: AsyncSession, email: str, sender: EmailSender, settings: Settings, now: datetime
) -> None:
    """Sends a code. The caller answers the same way, so emails cannot be probed."""
    address = security.normalize_email(email)
    digest = security.email_digest(address, settings.jwt_secret)
    recent = await auth.count_recent_codes(session, digest, now - timedelta(hours=1))
    if recent >= settings.login_codes_per_hour:
        raise TooManyRequestsError

    code = security.new_login_code()
    code_id = uuid.uuid4()
    await auth.add_code(
        session,
        code_id,
        digest,
        security.code_digest(code_id, code, settings.jwt_secret),
        now,
        now + timedelta(minutes=settings.login_code_ttl_minutes),
    )
    # Commit first: a failed send only means the parent asks again, never a code nobody holds.
    await session.commit()
    await sender.send_login_code(address, code)
    logger.info("auth.code_sent")


async def verify_code(
    session: AsyncSession,
    email: str,
    code: str,
    device_id: uuid.UUID,
    platform: str,
    settings: Settings,
    now: datetime,
) -> TokenPair:
    address = security.normalize_email(email)
    digest = security.email_digest(address, settings.jwt_secret)
    row = await auth.latest_open_code(session, digest, now)
    if row is None or row.attempts >= settings.login_code_max_attempts:
        raise InvalidCodeError

    row.attempts += 1
    if not security.codes_match(row.code_digest, row.id, code, settings.jwt_secret):
        await session.commit()  # keep the attempt count even though we fail
        raise InvalidCodeError

    row.consumed_at = now
    account = await accounts.get_by_email(session, address) or await accounts.create(
        session, address
    )
    device = await accounts.find_device(session, account.id, device_id)
    if device is None:
        known = await accounts.list_devices(session, account.id)
        if len(known) >= settings.max_devices_per_account:
            await session.rollback()
            raise DeviceLimitError
        device = await accounts.add_device(session, account.id, device_id, platform, now)
    device.last_seen_at = now
    device.platform = platform

    pair = await _issue(session, account.id, device.id, settings, now)
    await session.commit()
    logger.info("auth.signed_in")
    return pair


async def refresh(
    session: AsyncSession, refresh_token: str, settings: Settings, now: datetime
) -> TokenPair:
    row = await auth.find_refresh_token(session, security.token_digest(refresh_token))
    if row is None:
        raise InvalidTokenApiError
    if row.revoked_at is not None:
        # A rotated token came back: someone copied it. Sign this device out everywhere.
        await auth.revoke_device_tokens(session, row.device_registration_id, now)
        await session.commit()
        logger.warning("auth.refresh_reuse")
        raise InvalidTokenApiError
    if row.expires_at <= now:
        raise InvalidTokenApiError

    row.revoked_at = now
    device = await accounts.get_device(session, row.account_id, row.device_registration_id)
    if device is None:
        raise InvalidTokenApiError
    device.last_seen_at = now
    pair = await _issue(session, row.account_id, row.device_registration_id, settings, now)
    await session.commit()
    return pair


async def logout(session: AsyncSession, refresh_token: str, now: datetime) -> None:
    """Idempotent: an unknown or already used token simply does nothing."""
    row = await auth.find_refresh_token(session, security.token_digest(refresh_token))
    if row is not None and row.revoked_at is None:
        row.revoked_at = now
        await session.commit()


async def current_account(
    session: AsyncSession, token: str, settings: Settings, now: datetime
) -> Account:
    try:
        account_id = security.decode_access_token(token, settings, now)
    except security.InvalidTokenError as e:
        raise InvalidTokenApiError from e
    account = await accounts.get_by_id(session, account_id)
    if account is None:
        raise InvalidTokenApiError
    return account


async def _issue(
    session: AsyncSession,
    account_id: uuid.UUID,
    device_registration_id: uuid.UUID,
    settings: Settings,
    now: datetime,
) -> TokenPair:
    refresh_token = security.new_refresh_token()
    await auth.add_refresh_token(
        session,
        security.token_digest(refresh_token),
        account_id,
        device_registration_id,
        now,
        now + timedelta(days=settings.refresh_token_days),
    )
    return TokenPair(
        access_token=security.create_access_token(account_id, now, settings),
        refresh_token=refresh_token,
        expires_in=settings.access_token_minutes * 60,
        account_id=account_id,
    )
