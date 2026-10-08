"""store subscriptions and processed store events (F1-17)

Revision ID: 0003
Revises: 0002
Create Date: 2026-10-08
"""

import sqlalchemy as sa
from alembic import op
from sqlalchemy.dialects import postgresql

revision = "0003"
down_revision = "0002"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "store_subscriptions",
        sa.Column("id", postgresql.UUID(as_uuid=True), primary_key=True),
        sa.Column("store", sa.String(16), nullable=False),
        sa.Column("original_transaction_id", sa.String(128), nullable=False),
        sa.Column(
            "account_id",
            postgresql.UUID(as_uuid=True),
            sa.ForeignKey("accounts.id", ondelete="SET NULL"),
            nullable=True,
        ),
        sa.Column("product_id", sa.String(128), nullable=False),
        sa.Column("status", sa.String(16), nullable=False),
        sa.Column("expires_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("is_trial", sa.Boolean, nullable=False, server_default=sa.false()),
        sa.Column("last_event_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False),
        sa.UniqueConstraint("store", "original_transaction_id", name="uq_subscription_store_tx"),
    )
    op.create_index("ix_store_subscriptions_account_id", "store_subscriptions", ["account_id"])
    op.create_table(
        "store_events",
        sa.Column("event_id", sa.String(128), primary_key=True),
        sa.Column("store", sa.String(16), nullable=False),
        sa.Column("received_at", sa.DateTime(timezone=True), nullable=False),
    )


def downgrade() -> None:
    op.drop_table("store_events")
    op.drop_table("store_subscriptions")
