from functools import lru_cache
from typing import Literal

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Runtime configuration. Values come from environment / .env, never from code."""

    model_config = SettingsConfigDict(env_prefix="ROBOYA_", env_file=".env", extra="ignore")

    env: Literal["local", "test", "staging", "production"] = "local"
    database_url: str = "postgresql+asyncpg://roboya:roboya@localhost:5432/roboya"
    redis_url: str = "redis://localhost:6379/0"
    jwt_secret: str = "change-me-local-only"  # noqa: S105 - overridden outside local
    free_level_count: int = 3
    log_level: str = "INFO"


@lru_cache
def get_settings() -> Settings:
    return Settings()
