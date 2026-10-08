"""child profiles and merged progress (F1-15)

Revision ID: 0002
Revises: 0001
Create Date: 2026-10-08
"""

import sqlalchemy as sa
from alembic import op
from sqlalchemy.dialects import postgresql

revision = "0002"
down_revision = "0001"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "child_profiles",
        sa.Column("id", postgresql.UUID(as_uuid=True), primary_key=True),
        sa.Column(
            "account_id",
            postgresql.UUID(as_uuid=True),
            sa.ForeignKey("accounts.id", ondelete="CASCADE"),
            nullable=False,
        ),
        sa.Column("nickname", sa.String(24), nullable=False),
        sa.Column("avatar_id", sa.String(32), nullable=False),
        sa.Column("age_band", sa.String(16), nullable=False),
        sa.Column(
            "created_at", sa.DateTime(timezone=True), server_default=sa.func.now(), nullable=False
        ),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False),
    )
    op.create_index("ix_child_profiles_account_id", "child_profiles", ["account_id"])
    op.create_table(
        "progress_entries",
        sa.Column(
            "profile_id",
            postgresql.UUID(as_uuid=True),
            sa.ForeignKey("child_profiles.id", ondelete="CASCADE"),
            primary_key=True,
        ),
        sa.Column("level_id", sa.String(80), primary_key=True),
        sa.Column("stars", sa.Integer, nullable=False),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False),
    )


def downgrade() -> None:
    op.drop_table("progress_entries")
    op.drop_table("child_profiles")
