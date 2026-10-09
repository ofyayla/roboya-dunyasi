"""anonymous aggregate views for the dashboards (F1-20)

Revision ID: 0006
Revises: 0005
Create Date: 2026-10-09
"""

from alembic import op

revision = "0006"
down_revision = "0005"
branch_labels = None
depends_on = None

# Everything in the `bi` schema is anonymous aggregate data: no account, e-mail, profile or nickname
# ever appears here, so the dashboards' read-only role cannot reach personal data (ADR 0028).
_VIEWS = {
    "level_stats": """
        SELECT level_id,
               count(*) FILTER (WHERE name = 'level_start') AS starts,
               count(*) FILTER (WHERE name = 'level_complete') AS completes,
               count(*) FILTER (WHERE name = 'level_abandon') AS abandons,
               round(count(*) FILTER (WHERE name = 'level_abandon')::numeric
                     / NULLIF(count(*) FILTER (WHERE name = 'level_start'), 0), 3) AS abandon_rate,
               round(avg((props->>'attempts')::int) FILTER (WHERE name = 'level_complete'), 2)
                   AS avg_attempts,
               round(avg((props->>'duration_s')::int) FILTER (WHERE name = 'level_complete'), 1)
                   AS avg_duration_s,
               round(avg((props->>'stars')::int) FILTER (WHERE name = 'level_complete'), 2)
                   AS avg_stars,
               round(avg((props->>'hints')::int) FILTER (WHERE name = 'level_complete'), 2)
                   AS avg_hints
        FROM public.events
        WHERE level_id IS NOT NULL
        GROUP BY level_id
    """,
    "hint_usage": """
        SELECT level_id, (props->>'hint_tier')::int AS hint_tier, count(*) AS uses
        FROM public.events
        WHERE name = 'hint_used' AND level_id IS NOT NULL
        GROUP BY level_id, (props->>'hint_tier')::int
    """,
    "daily_activity": """
        SELECT date_trunc('day', occurred_at)::date AS day,
               count(DISTINCT anon_id) AS active_children,
               count(*) FILTER (WHERE name = 'app_open') AS app_opens,
               count(*) FILTER (WHERE name = 'level_complete') AS levels_completed,
               count(*) FILTER (WHERE name = 'session_limit_reached') AS limits_reached,
               count(*) FILTER (WHERE name = 'paywall_view') AS paywall_views
        FROM public.events
        GROUP BY 1
    """,
    # One row: how far anonymous children get before the subscription screen.
    "b2c_funnel": """
        SELECT count(DISTINCT anon_id) AS seen,
               count(DISTINCT anon_id) FILTER (WHERE name = 'level_start')
                   AS started_a_level,
               count(DISTINCT anon_id) FILTER (WHERE name = 'level_complete')
                   AS completed_a_level,
               count(DISTINCT anon_id) FILTER (WHERE name = 'paywall_view')
                   AS saw_subscription_screen
        FROM public.events
    """,
    # Subscription facts come from verified store notifications and carry no account reference here.
    "subscriptions": """
        SELECT product_id, store, status, is_trial, count(*) AS subscriptions
        FROM public.store_subscriptions
        GROUP BY product_id, store, status, is_trial
    """,
}


def upgrade() -> None:
    op.execute("CREATE SCHEMA IF NOT EXISTS bi")
    for name, sql in _VIEWS.items():
        op.execute(f"CREATE OR REPLACE VIEW bi.{name} AS {sql}")


def downgrade() -> None:
    for name in reversed(list(_VIEWS)):
        op.execute(f"DROP VIEW IF EXISTS bi.{name}")
    op.execute("DROP SCHEMA IF EXISTS bi")
