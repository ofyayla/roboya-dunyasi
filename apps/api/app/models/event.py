import uuid
from datetime import datetime
from typing import Any

from sqlalchemy import DateTime, PrimaryKeyConstraint, String
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class AnalyticsEvent(Base):
    """A first-party analytics event (F1-18). The table is partitioned by month on `occurred_at`.

    It holds an anonymous random profile id and fixed, allow-listed properties; never a nickname, an
    email or an advertising id (CLAUDE.md §11). Rows older than 24 months are dropped with their
    partition (PRD "Analitik" rules).
    """

    __tablename__ = "events"
    __table_args__ = (
        PrimaryKeyConstraint("id", "occurred_at", name="pk_events"),
        {"postgresql_partition_by": "RANGE (occurred_at)"},
    )

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), nullable=False)
    anon_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), nullable=False)
    name: Mapped[str] = mapped_column(String(32), nullable=False)
    level_id: Mapped[str | None] = mapped_column(String(80), nullable=True)
    props: Mapped[dict[str, Any]] = mapped_column(JSONB, nullable=False, server_default="{}")
    occurred_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    received_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    app_version: Mapped[str] = mapped_column(String(16), nullable=False)
    platform: Mapped[str] = mapped_column(String(16), nullable=False)
