"""What an account may do. The single home for access decisions (CLAUDE.md §8, golden rule 3).

Premium comes only from a store subscription the server has verified and tied to the account.
The app never decides: it asks (`GET /v1/me/entitlement`), caches the answer until `cache_until`
and shows locks from it. School licences (v1.0) will add a second source here.
"""

from dataclasses import dataclass
from datetime import datetime, timedelta

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import Settings
from app.models.account import Account
from app.models.store import StoreSubscription
from app.repositories import store as store_repo

ACTIVE_STATUSES = ("trial", "active", "grace")


@dataclass(frozen=True)
class Entitlement:
    tier: str  # "free" | "premium"
    source: str  # "none" | "store"
    status: str  # "free" | "trial" | "active" | "grace"
    expires_at: datetime | None
    cache_until: datetime


def _is_active(sub: StoreSubscription, now: datetime) -> bool:
    return sub.status in ACTIVE_STATUSES and sub.expires_at > now


def best_subscription(subs: list[StoreSubscription], now: datetime) -> StoreSubscription | None:
    """The active subscription that lasts longest (an account may hold more than one)."""
    active = [s for s in subs if _is_active(s, now)]
    return max(active, key=lambda s: s.expires_at) if active else None


async def compute(
    session: AsyncSession, account: Account, settings: Settings, now: datetime
) -> Entitlement:
    cache_limit = now + timedelta(hours=settings.entitlement_cache_hours)
    sub = best_subscription(await store_repo.for_account(session, account.id), now)
    if sub is None:
        return Entitlement("free", "none", "free", None, cache_limit)
    # Never let the app keep premium past the end of the paid period.
    return Entitlement(
        "premium", "store", sub.status, sub.expires_at, min(sub.expires_at, cache_limit)
    )


async def profile_limit(
    session: AsyncSession, account: Account, settings: Settings, now: datetime
) -> int:
    """Child profiles allowed: 1 on the free tier, up to 4 with Family Premium (PRD)."""
    entitlement = await compute(session, account, settings, now)
    return (
        settings.max_profiles_premium
        if entitlement.tier == "premium"
        else settings.max_profiles_free
    )
