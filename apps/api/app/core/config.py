from functools import lru_cache
from typing import Literal

from pydantic_settings import BaseSettings, SettingsConfigDict

# Long enough for HS256; never accepted outside local and test (see app.main.check_settings).
DEFAULT_SECRET = "change-me-local-only-change-me-local-only"  # noqa: S105


class Settings(BaseSettings):
    """Runtime configuration. Values come from environment / .env, never from code."""

    model_config = SettingsConfigDict(env_prefix="ROBOYA_", env_file=".env", extra="ignore")

    env: Literal["local", "test", "staging", "production"] = "local"
    database_url: str = "postgresql+asyncpg://roboya:roboya@localhost:5432/roboya"
    redis_url: str = "redis://localhost:6379/0"
    jwt_secret: str = DEFAULT_SECRET  # overridden outside local
    free_level_count: int = 3
    log_level: str = "INFO"

    # Parent accounts (F1-14): email one-time codes, short access tokens, rotating refresh tokens.
    access_token_minutes: int = 30
    refresh_token_days: int = 60
    login_code_ttl_minutes: int = 10
    login_code_max_attempts: int = 5
    login_codes_per_hour: int = 5
    max_devices_per_account: int = 10
    # Child profiles per account: 1 free, 4 with Family Premium (PRD). The premium value is applied
    # by services/entitlements.py once store entitlements exist.
    max_profiles_free: int = 1
    max_profiles_premium: int = 4
    max_sync_entries: int = 500

    # Store entitlements (F1-17). "signed-dev" accepts purchases and notifications signed with
    # the key below (ES256); it is for development and tests. The App Store and Google Play
    # adapters need store credentials and come with the store accounts; production refuses it.
    store_backend: Literal["signed-dev"] = "signed-dev"
    store_dev_public_key: str = ""
    # How long an app may keep a granted entitlement without asking again (offline use).
    entitlement_cache_hours: int = 72
    # "outbox" writes codes to a local file for development; a real provider needs the transfer
    # process in CLAUDE.md §11 (contract, notification, inventory) before it is switched on.
    email_backend: Literal["outbox"] = "outbox"
    outbox_path: str = "tmp/outbox.jsonl"


@lru_cache
def get_settings() -> Settings:
    return Settings()
