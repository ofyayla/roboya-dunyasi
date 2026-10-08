import uuid
from datetime import datetime

from sqlalchemy import func, select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.profile import ChildProfile, ProgressEntry


async def list_for_account(session: AsyncSession, account_id: uuid.UUID) -> list[ChildProfile]:
    result = await session.execute(
        select(ChildProfile)
        .where(ChildProfile.account_id == account_id)
        .order_by(ChildProfile.created_at, ChildProfile.id)
    )
    return list(result.scalars())


async def count_for_account(session: AsyncSession, account_id: uuid.UUID) -> int:
    result = await session.execute(
        select(func.count()).select_from(ChildProfile).where(ChildProfile.account_id == account_id)
    )
    return int(result.scalar_one())


async def get_owned(
    session: AsyncSession, account_id: uuid.UUID, profile_id: uuid.UUID
) -> ChildProfile | None:
    """Scoped by account: another parent's profile is never visible (CLAUDE.md §8)."""
    result = await session.execute(
        select(ChildProfile).where(
            ChildProfile.account_id == account_id, ChildProfile.id == profile_id
        )
    )
    return result.scalar_one_or_none()


async def id_exists(session: AsyncSession, profile_id: uuid.UUID) -> bool:
    result = await session.execute(select(ChildProfile.id).where(ChildProfile.id == profile_id))
    return result.scalar_one_or_none() is not None


async def add(
    session: AsyncSession,
    account_id: uuid.UUID,
    profile_id: uuid.UUID,
    nickname: str,
    avatar_id: str,
    age_band: str,
    now: datetime,
) -> ChildProfile:
    profile = ChildProfile(
        id=profile_id,
        account_id=account_id,
        nickname=nickname,
        avatar_id=avatar_id,
        age_band=age_band,
        updated_at=now,
    )
    session.add(profile)
    await session.flush()
    return profile


async def delete(session: AsyncSession, profile: ChildProfile) -> None:
    await session.delete(profile)
    await session.flush()


async def merge_stars(
    session: AsyncSession, profile_id: uuid.UUID, stars: dict[str, int], now: datetime
) -> None:
    """Keeps the higher of the stored and sent stars per level, in one statement (ILR-02)."""
    if not stars:
        return
    statement = insert(ProgressEntry).values(
        [
            {"profile_id": profile_id, "level_id": level_id, "stars": value, "updated_at": now}
            for level_id, value in stars.items()
        ]
    )
    statement = statement.on_conflict_do_update(
        index_elements=[ProgressEntry.profile_id, ProgressEntry.level_id],
        set_={
            "stars": func.greatest(ProgressEntry.stars, statement.excluded.stars),
            "updated_at": func.greatest(ProgressEntry.updated_at, statement.excluded.updated_at),
        },
    )
    await session.execute(statement)


async def stars_of(session: AsyncSession, profile_id: uuid.UUID) -> dict[str, int]:
    result = await session.execute(
        select(ProgressEntry.level_id, ProgressEntry.stars).where(
            ProgressEntry.profile_id == profile_id
        )
    )
    return {level_id: stars for level_id, stars in result.all()}
