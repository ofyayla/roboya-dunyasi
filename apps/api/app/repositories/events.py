import uuid
from dataclasses import dataclass
from datetime import datetime
from typing import Any

from sqlalchemy import text
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.event import AnalyticsEvent


@dataclass(frozen=True)
class EventRow:
    id: uuid.UUID
    anon_id: uuid.UUID
    name: str
    level_id: str | None
    props: dict[str, Any]
    occurred_at: datetime
    received_at: datetime
    app_version: str
    platform: str


async def insert_events(session: AsyncSession, rows: list[EventRow]) -> int:
    """Inserts the new events and returns how many were new; a retried event changes nothing."""
    if not rows:
        return 0
    statement = (
        insert(AnalyticsEvent)
        .values([row.__dict__ for row in rows])
        .on_conflict_do_nothing(index_elements=["id", "occurred_at"])
        .returning(AnalyticsEvent.id)
    )
    return len((await session.execute(statement)).all())


async def ensure_month_partition(session: AsyncSession, year: int, month: int) -> None:
    """Creates the monthly partition if it does not exist (idempotent)."""
    ny, nm = (year + 1, 1) if month == 12 else (year, month + 1)
    name = f"events_{year}_{month:02d}"
    await session.execute(
        text(
            f"CREATE TABLE IF NOT EXISTS {name} PARTITION OF events "  # noqa: S608 - ints only
            f"FOR VALUES FROM ('{year}-{month:02d}-01 00:00:00+00') "
            f"TO ('{ny}-{nm:02d}-01 00:00:00+00')"
        )
    )


async def partition_names(session: AsyncSession) -> list[str]:
    result = await session.execute(
        text(
            "SELECT c.relname FROM pg_inherits i JOIN pg_class c ON c.oid = i.inhrelid "
            "JOIN pg_class p ON p.oid = i.inhparent WHERE p.relname = 'events' ORDER BY c.relname"
        )
    )
    return [row[0] for row in result.all()]


async def drop_partition(session: AsyncSession, name: str) -> None:
    if not name.startswith("events_20") or len(name) != len("events_2026_10"):
        raise ValueError("not a monthly events partition")
    await session.execute(text(f"DROP TABLE IF EXISTS {name}"))  # noqa: S608 - name validated above
