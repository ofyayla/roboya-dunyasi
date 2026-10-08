"""first-party analytics events, partitioned by month (F1-18)

Revision ID: 0004
Revises: 0003
Create Date: 2026-10-08
"""

from alembic import op

revision = "0004"
down_revision = "0003"
branch_labels = None
depends_on = None

# Monthly partitions for the launch window; app/maintenance.py adds later months. The default
# partition guarantees an insert never fails for want of a partition.
_MONTHS = [(2026, m) for m in range(9, 13)] + [(2027, m) for m in range(1, 7)]


def upgrade() -> None:
    op.execute(
        """
        CREATE TABLE events (
            id uuid NOT NULL,
            anon_id uuid NOT NULL,
            name varchar(32) NOT NULL,
            level_id varchar(80),
            props jsonb NOT NULL DEFAULT '{}'::jsonb,
            occurred_at timestamptz NOT NULL,
            received_at timestamptz NOT NULL,
            app_version varchar(16) NOT NULL,
            platform varchar(16) NOT NULL,
            CONSTRAINT pk_events PRIMARY KEY (id, occurred_at)
        ) PARTITION BY RANGE (occurred_at)
        """
    )
    op.execute("CREATE INDEX ix_events_name_occurred ON events (name, occurred_at)")
    op.execute("CREATE INDEX ix_events_anon_occurred ON events (anon_id, occurred_at)")
    op.execute("CREATE TABLE events_default PARTITION OF events DEFAULT")
    for year, month in _MONTHS:
        ny, nm = (year + 1, 1) if month == 12 else (year, month + 1)
        op.execute(
            f"CREATE TABLE events_{year}_{month:02d} PARTITION OF events "
            f"FOR VALUES FROM ('{year}-{month:02d}-01 00:00:00+00') "
            f"TO ('{ny}-{nm:02d}-01 00:00:00+00')"
        )


def downgrade() -> None:
    op.execute("DROP TABLE events")
