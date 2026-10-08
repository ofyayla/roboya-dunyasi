import uuid
from datetime import datetime

from sqlalchemy import DateTime, ForeignKey, Integer, String, func
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class ChildProfile(Base):
    """A child profile. Only the allowed fields (CLAUDE.md §11): nickname, avatar, age band.

    The id is made by the app, so a profile created offline keeps its id when it first syncs.
    """

    __tablename__ = "child_profiles"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True)
    account_id: Mapped[uuid.UUID] = mapped_column(
        ForeignKey("accounts.id", ondelete="CASCADE"), nullable=False, index=True
    )
    nickname: Mapped[str] = mapped_column(String(24), nullable=False)
    avatar_id: Mapped[str] = mapped_column(String(32), nullable=False)
    age_band: Mapped[str] = mapped_column(String(16), nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), nullable=False
    )
    updated_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)


class ProgressEntry(Base):
    """Best stars for one level of one profile. Stars only ever go up (ILR-02)."""

    __tablename__ = "progress_entries"

    profile_id: Mapped[uuid.UUID] = mapped_column(
        ForeignKey("child_profiles.id", ondelete="CASCADE"), primary_key=True
    )
    level_id: Mapped[str] = mapped_column(String(80), primary_key=True)
    stars: Mapped[int] = mapped_column(Integer, nullable=False)
    updated_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
