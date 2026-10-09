import uuid

from httpx import AsyncClient
from sqlalchemy import text

from app.core.db import get_sessionmaker
from tests.conftest import FakeClock

LEVEL = "sabir-ormani.yon-avcisi.01"


def _event(clock: FakeClock, name: str, props: dict, level: str | None = LEVEL) -> dict:
    return {
        "id": str(uuid.uuid4()),
        "name": name,
        "level_id": level,
        "occurred_at": clock.current.isoformat(),
        "props": props,
    }


async def _post(client: AsyncClient, anon: str, events: list[dict]) -> None:
    r = await client.post(
        "/v1/events",
        json={"anon_id": anon, "app_version": "0.1.0", "platform": "android", "events": events},
    )
    assert r.status_code == 202


async def _all(sql: str) -> list:
    async with get_sessionmaker()() as s:
        return list((await s.execute(text(sql))).mappings().all())


async def test_bi_levelStats_countsStartsCompletesAndAverages(
    client: AsyncClient, fake_clock: FakeClock
):
    a, b = str(uuid.uuid4()), str(uuid.uuid4())
    await _post(
        client,
        a,
        [
            _event(fake_clock, "level_start", {}),
            _event(
                fake_clock,
                "level_complete",
                {"attempts": 1, "duration_s": 20, "stars": 3, "hints": 0},
            ),
        ],
    )
    await _post(
        client,
        b,
        [
            _event(fake_clock, "level_start", {}),
            _event(
                fake_clock,
                "level_complete",
                {"attempts": 3, "duration_s": 40, "stars": 1, "hints": 2},
            ),
            _event(fake_clock, "level_start", {}),
            _event(fake_clock, "level_abandon", {"attempts": 1, "duration_s": 5, "hints": 0}),
        ],
    )

    (row,) = await _all("SELECT * FROM bi.level_stats")

    assert row["level_id"] == LEVEL
    assert (row["starts"], row["completes"], row["abandons"]) == (3, 2, 1)
    assert float(row["abandon_rate"]) == 0.333
    assert float(row["avg_attempts"]) == 2.0
    assert float(row["avg_duration_s"]) == 30.0
    assert float(row["avg_stars"]) == 2.0


async def test_bi_hintUsage_dailyActivityAndFunnel(client: AsyncClient, fake_clock: FakeClock):
    a, b = str(uuid.uuid4()), str(uuid.uuid4())
    await _post(
        client,
        a,
        [
            _event(fake_clock, "app_open", {}, None),
            _event(fake_clock, "level_start", {}),
            _event(fake_clock, "hint_used", {"hint_tier": 1}),
            _event(fake_clock, "hint_used", {"hint_tier": 1}),
            _event(fake_clock, "level_complete", {"stars": 2}),
            _event(fake_clock, "paywall_view", {}, None),
        ],
    )
    await _post(client, b, [_event(fake_clock, "app_open", {}, None)])

    hints = await _all("SELECT * FROM bi.hint_usage")
    assert [(h["hint_tier"], h["uses"]) for h in hints] == [(1, 2)]

    (day,) = await _all("SELECT * FROM bi.daily_activity")
    assert day["active_children"] == 2 and day["app_opens"] == 2
    assert day["levels_completed"] == 1 and day["paywall_views"] == 1

    (funnel,) = await _all("SELECT * FROM bi.b2c_funnel")
    assert (funnel["seen"], funnel["started_a_level"]) == (2, 1)
    assert (funnel["completed_a_level"], funnel["saw_subscription_screen"]) == (1, 1)


async def test_bi_viewsExposeNoPersonalColumns():
    columns = await _all(
        "SELECT table_name, column_name FROM information_schema.columns WHERE table_schema = 'bi'"
    )
    names = {c["column_name"] for c in columns}

    assert names
    assert not names & {"email", "nickname", "account_id", "profile_id", "anon_id", "id"}
    assert {c["table_name"] for c in columns} == {
        "level_stats",
        "hint_usage",
        "daily_activity",
        "b2c_funnel",
        "subscriptions",
    }
