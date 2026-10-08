"""Upkeep for the events table: create upcoming monthly partitions, drop old ones (PRD: 24 months).

Run from a job (cron / worker): `python -m app.maintenance`. Both steps are idempotent.
"""

import asyncio
import logging
from datetime import UTC, datetime, timedelta

from app.core import clock
from app.core.db import get_sessionmaker
from app.core.logging import configure_logging
from app.repositories import events as repo
from app.repositories import privacy as privacy_repo
from app.services import privacy

logger = logging.getLogger(__name__)

RETENTION_MONTHS = 24
MONTHS_AHEAD = 3


def month_after(year: int, month: int, delta: int) -> tuple[int, int]:
    index = (year * 12 + (month - 1)) + delta
    return index // 12, index % 12 + 1


async def run(now: datetime | None = None) -> tuple[list[str], list[str]]:
    """Returns (partitions ensured, partitions dropped)."""
    moment = now or clock.now()
    ensured: list[str] = []
    dropped: list[str] = []
    async with get_sessionmaker()() as session:
        for ahead in range(MONTHS_AHEAD + 1):
            y, m = month_after(moment.year, moment.month, ahead)
            await repo.ensure_month_partition(session, y, m)
            ensured.append(f"events_{y}_{m:02d}")
        cutoff_y, cutoff_m = month_after(moment.year, moment.month, -RETENTION_MONTHS)
        cutoff = f"events_{cutoff_y}_{cutoff_m:02d}"
        for name in await repo.partition_names(session):
            if name.startswith("events_20") and name < cutoff:
                await repo.drop_partition(session, name)
                dropped.append(name)
        await session.commit()
    logger.info("events.maintenance", extra={"ensured": len(ensured), "dropped": len(dropped)})
    return ensured, dropped


async def run_privacy(now: datetime | None = None) -> tuple[int, int]:
    """Deletes accounts whose waiting period is over and purges expired login codes (UYM-03).

    Returns (accounts deleted, login codes purged). Idempotent; run at least daily.
    """
    moment = now or clock.now()
    async with get_sessionmaker()() as session:
        deleted = await privacy.process_due_deletions(session, moment)
        purged = await privacy_repo.purge_login_codes(session, moment - timedelta(days=1))
        await session.commit()
    return deleted, purged


if __name__ == "__main__":
    configure_logging("INFO")
    asyncio.run(run(datetime.now(UTC)))
    asyncio.run(run_privacy(datetime.now(UTC)))
