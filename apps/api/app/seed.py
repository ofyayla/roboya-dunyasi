"""Load synthetic seed data (ADR 0001). Idempotent; extended as models are added."""

import asyncio
import logging

from app.core.config import get_settings
from app.core.logging import configure_logging

logger = logging.getLogger(__name__)


async def seed() -> None:
    settings = get_settings()
    if settings.env == "production":
        raise RuntimeError("Seed data must never be loaded into production")
    logger.info("seed.done", extra={"env": settings.env})


if __name__ == "__main__":
    configure_logging("INFO")
    asyncio.run(seed())
