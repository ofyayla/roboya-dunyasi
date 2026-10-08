import uuid
from datetime import datetime

from sqlalchemy import Boolean, DateTime, ForeignKey, String, UniqueConstraint
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class StoreSubscription(Base):
    """A store subscription, keyed by the store's own transaction family id.

    The row exists as soon as a store notification arrives; it is tied to a parent account when that
    parent presents the purchase (GLR-03). Payment details never reach us, only these facts.
    """

    __tablename__ = "store_subscriptions"
    __table_args__ = (
        UniqueConstraint("store", "original_transaction_id", name="uq_subscription_store_tx"),
    )

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    store: Mapped[str] = mapped_column(String(16), nullable=False)
    original_transaction_id: Mapped[str] = mapped_column(String(128), nullable=False)
    account_id: Mapped[uuid.UUID | None] = mapped_column(
        ForeignKey("accounts.id", ondelete="SET NULL"), nullable=True, index=True
    )
    product_id: Mapped[str] = mapped_column(String(128), nullable=False)
    # trial | active | grace | expired | revoked
    status: Mapped[str] = mapped_column(String(16), nullable=False)
    expires_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    is_trial: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    last_event_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    updated_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)


class StoreEventRecord(Base):
    """Every store notification id we have processed, so a retried delivery changes nothing."""

    __tablename__ = "store_events"

    event_id: Mapped[str] = mapped_column(String(128), primary_key=True)
    store: Mapped[str] = mapped_column(String(16), nullable=False)
    received_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
