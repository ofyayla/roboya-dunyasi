"""What an account may do. The single home for access decisions (CLAUDE.md §8, golden rule 3).

Today every account is on the free tier. The store-backed entitlement (F1-17) will replace the
constants below; callers already go through these functions, so nothing else changes.
"""

from app.core.config import Settings
from app.models.account import Account


def profile_limit(account: Account, settings: Settings) -> int:
    """Child profiles allowed: 1 on the free tier, up to 4 with Family Premium."""
    return settings.max_profiles_free
