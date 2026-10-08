import uuid
from datetime import datetime, timedelta

import pytest
from httpx import AsyncClient
from sqlalchemy import text

from app import maintenance
from app.core.db import get_sessionmaker
from app.repositories import events as events_repo
from tests.conftest import FakeClock

ANON = str(uuid.uuid4())


def _event(clock: FakeClock, name: str = "level_complete", **extra) -> dict:
    return {
        "id": str(uuid.uuid4()),
        "name": name,
        "level_id": "sabir-ormani.yon-avcisi.01",
        "occurred_at": clock.current.isoformat(),
        "props": {"stars": 3, "attempts": 1},
        **extra,
    }


def _batch(events: list[dict], **extra) -> dict:
    return {
        "anon_id": ANON,
        "app_version": "0.1.0",
        "platform": "android",
        "events": events,
        **extra,
    }


async def _rows() -> list:
    async with get_sessionmaker()() as s:
        return list((await s.execute(text("SELECT name, props, anon_id FROM events"))).all())


async def test_events_needNoAccountAndAreStoredAnonymously(
    client: AsyncClient, fake_clock: FakeClock
):
    r = await client.post(
        "/v1/events",
        json=_batch([_event(fake_clock), _event(fake_clock, "app_open", level_id=None, props={})]),
    )

    assert r.status_code == 202 and r.json() == {"accepted": 2, "dropped": 0}
    rows = await _rows()
    assert {row[0] for row in rows} == {"level_complete", "app_open"}
    assert all(str(row[2]) == ANON for row in rows)
    assert {k for row in rows for k in row[1]} <= {"stars", "attempts"}


async def test_events_aRetriedEventIsStoredOnce(client: AsyncClient, fake_clock: FakeClock):
    batch = _batch([_event(fake_clock)])

    first = await client.post("/v1/events", json=batch)
    retry = await client.post("/v1/events", json=batch)

    assert first.json()["accepted"] == 1 and retry.json()["accepted"] == 0
    assert len(await _rows()) == 1


async def test_events_propsKeepOnlyWhatWasSent(client: AsyncClient, fake_clock: FakeClock):
    props = {"duration_s": 42, "profile_kind": "family", "device_class": "tablet", "source": "path"}
    await client.post("/v1/events", json=_batch([_event(fake_clock, props=props)]))

    assert (await _rows())[0][1] == props


@pytest.mark.parametrize(
    "bad",
    [
        {"name": "purchase"},
        {"name": "trial_start"},
        {"props": {"nickname": "Elif"}},
        {"props": {"email": "veli@example.com"}},
        {"props": {"stars": 4}},
        {"props": {"stars": "3"}},
        {"props": {"profile_kind": "alien"}},
        {"level_id": "Not A Level"},
        {"nickname": "Elif"},
        {"ip": "10.0.0.1"},
    ],
)
async def test_events_unknownNamesAndFreeFormFieldsAreRejected(
    client: AsyncClient, fake_clock: FakeClock, bad: dict
):
    r = await client.post("/v1/events", json=_batch([{**_event(fake_clock), **bad}]))

    assert r.status_code == 422
    assert await _rows() == []


@pytest.mark.parametrize(
    "extra",
    [
        {"nickname": "Elif"},
        {"email": "a@b.co"},
        {"advertising_id": "x"},
        {"platform": "web"},
        {"app_version": "a b"},
    ],
)
async def test_events_batchEnvelopeRejectsPersonalFields(
    client: AsyncClient, fake_clock: FakeClock, extra: dict
):
    r = await client.post("/v1/events", json=_batch([_event(fake_clock)], **extra))

    assert r.status_code == 422


async def test_events_emptyBatchIsRejected(client: AsyncClient):
    assert (await client.post("/v1/events", json=_batch([]))).status_code == 422


async def test_events_batchOfOver100IsRejected(client: AsyncClient, fake_clock: FakeClock):
    r = await client.post("/v1/events", json=_batch([_event(fake_clock) for _ in range(101)]))

    assert r.status_code == 422


async def test_events_timesFarFromNowAreDroppedNotStored(
    client: AsyncClient, fake_clock: FakeClock
):
    now = fake_clock.current
    ok_old = _event(fake_clock, occurred_at=(now - timedelta(days=29)).isoformat())
    too_old = _event(fake_clock, occurred_at=(now - timedelta(days=31)).isoformat())
    future = _event(fake_clock, occurred_at=(now + timedelta(hours=2)).isoformat())
    naive = _event(fake_clock, occurred_at="2026-10-08T12:00:00")

    r = await client.post("/v1/events", json=_batch([ok_old, too_old, future, naive]))

    assert r.json() == {"accepted": 1, "dropped": 3}


async def test_events_eventsLandInTheirMonthlyPartition(client: AsyncClient, fake_clock: FakeClock):
    await client.post("/v1/events", json=_batch([_event(fake_clock)]))

    async with get_sessionmaker()() as s:
        where = (await s.execute(text("SELECT tableoid::regclass::text FROM events"))).scalar_one()
    assert where == "events_2026_10"


async def test_maintenance_createsUpcomingPartitionsAndDropsOldOnes(fake_clock: FakeClock):
    async with get_sessionmaker()() as s:
        await s.execute(
            text(
                "CREATE TABLE IF NOT EXISTS events_2020_01 PARTITION OF events "
                "FOR VALUES FROM ('2020-01-01 00:00:00+00') TO ('2020-02-01 00:00:00+00')"
            )
        )
        await s.commit()

    ensured, dropped = await maintenance.run(
        datetime(2026, 11, 15, tzinfo=fake_clock.current.tzinfo)
    )
    again_ensured, again_dropped = await maintenance.run(
        datetime(2026, 11, 15, tzinfo=fake_clock.current.tzinfo)
    )

    assert ensured == ["events_2026_11", "events_2026_12", "events_2027_01", "events_2027_02"]
    assert dropped == ["events_2020_01"]
    assert again_dropped == [] and again_ensured == ensured
    async with get_sessionmaker()() as s:
        names = await events_repo.partition_names(s)
    assert "events_2020_01" not in names and "events_2027_02" in names and "events_default" in names


def test_maintenance_monthArithmeticCrossesYears():
    assert maintenance.month_after(2026, 11, 3) == (2027, 2)
    assert maintenance.month_after(2026, 1, -24) == (2024, 1)
    assert maintenance.month_after(2026, 12, 1) == (2027, 1)


async def test_repository_refusesToDropAnythingButMonthlyPartitions():
    async with get_sessionmaker()() as s:
        for bad in ("accounts", "events_default", "events_2026_1", "events_20xx_10;DROP"):
            with pytest.raises(ValueError):
                await events_repo.drop_partition(s, bad)
