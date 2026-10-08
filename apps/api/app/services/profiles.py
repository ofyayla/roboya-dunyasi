"""Child profiles and conflict-free progress sync (F1-15, ILR-01/02)."""

import uuid
from dataclasses import dataclass
from datetime import datetime

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import Settings
from app.models.account import Account
from app.models.profile import ChildProfile
from app.repositories import profiles
from app.services import entitlements, privacy
from app.services.errors import NotFoundError, ProfileLimitError


@dataclass(frozen=True)
class ProfileFields:
    nickname: str
    avatar_id: str
    age_band: str


async def list_profiles(session: AsyncSession, account: Account) -> list[ChildProfile]:
    return await profiles.list_for_account(session, account.id)


async def save_profile(
    session: AsyncSession,
    account: Account,
    profile_id: uuid.UUID,
    fields: ProfileFields,
    settings: Settings,
    now: datetime,
) -> ChildProfile:
    """Creates the profile or updates the parent's own; the app makes the id (offline-first)."""
    # Child data is written only with the parent's consent (UYM-01).
    await privacy.require_consent(session, account, settings)
    existing = await profiles.get_owned(session, account.id, profile_id)
    if existing is not None:
        existing.nickname = fields.nickname
        existing.avatar_id = fields.avatar_id
        existing.age_band = fields.age_band
        existing.updated_at = now
        await session.commit()
        return existing

    # An id that exists under another account must not reveal that: treat it as not found.
    if await profiles.id_exists(session, profile_id):
        raise NotFoundError
    limit = await entitlements.profile_limit(session, account, settings, now)
    if await profiles.count_for_account(session, account.id) >= limit:
        raise ProfileLimitError
    created = await profiles.add(
        session, account.id, profile_id, fields.nickname, fields.avatar_id, fields.age_band, now
    )
    await session.commit()
    return created


async def delete_profile(session: AsyncSession, account: Account, profile_id: uuid.UUID) -> None:
    profile = await profiles.get_owned(session, account.id, profile_id)
    if profile is None:
        raise NotFoundError
    await profiles.delete(session, profile)
    await session.commit()


async def get_progress(
    session: AsyncSession, account: Account, profile_id: uuid.UUID
) -> dict[str, int]:
    if await profiles.get_owned(session, account.id, profile_id) is None:
        raise NotFoundError
    return await profiles.stars_of(session, profile_id)


async def sync_progress(
    session: AsyncSession,
    account: Account,
    profile_id: uuid.UUID,
    stars: dict[str, int],
    settings: Settings,
    now: datetime,
) -> dict[str, int]:
    """Merges the device's stars into the server's, keeping the higher value per level.

    Taking the maximum makes the merge order-independent, repeatable and safe for several devices.
    """
    await privacy.require_consent(session, account, settings)
    if await profiles.get_owned(session, account.id, profile_id) is None:
        raise NotFoundError
    await profiles.merge_stars(session, profile_id, stars, now)
    merged = await profiles.stars_of(session, profile_id)
    await session.commit()
    return merged
