"""First-party analytics ingestion (F1-18). Public by design; it holds only anonymous data."""

import logging
from datetime import datetime, timedelta

from sqlalchemy.ext.asyncio import AsyncSession

from app.repositories import events as repo
from app.schemas.events import EventBatchIn

logger = logging.getLogger(__name__)

MAX_AGE = timedelta(days=30)
MAX_FUTURE = timedelta(minutes=5)


async def ingest(session: AsyncSession, batch: EventBatchIn, now: datetime) -> tuple[int, int]:
    """Stores the batch; returns (new events, events dropped for a time far from now).

    A phone that played offline may send events up to 30 days old; anything older or from the future is
    dropped, so a wrong device clock cannot scatter rows across partitions.
    """
    rows: list[repo.EventRow] = []
    dropped = 0
    for e in batch.events:
        if e.occurred_at.tzinfo is None or not (now - MAX_AGE <= e.occurred_at <= now + MAX_FUTURE):
            dropped += 1
            continue
        rows.append(
            repo.EventRow(
                id=e.id,
                anon_id=batch.anon_id,
                name=e.name,
                level_id=e.level_id,
                props=e.props.model_dump(exclude_none=True),
                occurred_at=e.occurred_at,
                received_at=now,
                app_version=batch.app_version,
                platform=batch.platform,
            )
        )
    inserted = await repo.insert_events(session, rows)
    await session.commit()
    logger.info("events.ingested", extra={"count": inserted, "dropped": dropped})
    return inserted, dropped
