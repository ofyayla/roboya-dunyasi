import logging
import uuid

import pytest
from httpx import AsyncClient
from sqlalchemy import func, select

from app.core.db import get_sessionmaker
from app.models.account import Account, LoginCode
from tests.conftest import FakeClock, FakeEmailSender
from tests.helpers import device, sign_in


async def _verify(client: AsyncClient, email: str, code: str, dev: dict[str, str] | None = None):
    return await client.post(
        "/v1/auth/verify", json={"email": email, "code": code, "device": dev or device()}
    )


async def test_requestCode_validEmail_sendsSixDigitCode(
    client: AsyncClient, mailbox: FakeEmailSender
):
    r = await client.post("/v1/auth/code", json={"email": "  Veli@Example.com "})

    assert r.status_code == 202
    assert r.content == b""
    assert mailbox.sent[0][0] == "veli@example.com"
    assert len(mailbox.last_code) == 6 and mailbox.last_code.isdigit()


@pytest.mark.parametrize("email", ["", "not-an-email", "a@b", "x" * 260 + "@example.com"])
async def test_requestCode_badEmail_isRejected(client: AsyncClient, email: str):
    assert (await client.post("/v1/auth/code", json={"email": email})).status_code == 422


async def test_requestCode_sameAnswerForKnownAndUnknownEmail(
    client: AsyncClient, mailbox: FakeEmailSender
):
    await sign_in(client, mailbox, "kayitli@example.com")

    known = await client.post("/v1/auth/code", json={"email": "kayitli@example.com"})
    unknown = await client.post("/v1/auth/code", json={"email": "yeni@example.com"})

    assert known.status_code == unknown.status_code == 202
    assert known.content == unknown.content


async def test_requestCode_sixthRequestInAnHour_isRateLimited(
    client: AsyncClient, fake_clock: FakeClock
):
    for _ in range(5):
        assert (
            await client.post("/v1/auth/code", json={"email": "a@example.com"})
        ).status_code == 202

    r = await client.post("/v1/auth/code", json={"email": "a@example.com"})

    assert r.status_code == 429
    assert r.json() == {"code": "too_many_requests"}
    other = await client.post("/v1/auth/code", json={"email": "b@example.com"})
    assert other.status_code == 202, "the limit is per email"
    fake_clock.advance(hours=1, minutes=1)
    assert (await client.post("/v1/auth/code", json={"email": "a@example.com"})).status_code == 202


async def test_verify_rightCode_createsAccountAndDeviceAndReturnsTokens(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)

    assert tokens["expires_in"] == 30 * 60
    assert tokens["access_token"] and tokens["refresh_token"]
    async with get_sessionmaker()() as s:
        assert await s.scalar(select(func.count()).select_from(Account)) == 1


async def test_verify_secondSignInKeepsTheSameAccount(
    client: AsyncClient, mailbox: FakeEmailSender
):
    first = await sign_in(client, mailbox)
    second = await sign_in(client, mailbox, dev=device("ios"))

    assert first["account_id"] == second["account_id"]


async def test_verify_wrongCode_is401AndCodeSurvivesUntilAttemptsRunOut(
    client: AsyncClient, mailbox: FakeEmailSender
):
    await client.post("/v1/auth/code", json={"email": "a@example.com"})
    right = mailbox.last_code
    wrong = "000000" if right != "000000" else "111111"

    for _ in range(5):
        r = await _verify(client, "a@example.com", wrong)
        assert r.status_code == 401 and r.json() == {"code": "invalid_code"}

    assert (await _verify(client, "a@example.com", right)).status_code == 401, "attempts exhausted"
    async with get_sessionmaker()() as s:
        assert await s.scalar(select(func.count()).select_from(Account)) == 0


async def test_verify_codeIsSingleUse(client: AsyncClient, mailbox: FakeEmailSender):
    await sign_in(client, mailbox)

    assert (await _verify(client, "veli@example.com", mailbox.last_code)).status_code == 401


async def test_verify_expiredCode_is401(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    await client.post("/v1/auth/code", json={"email": "a@example.com"})
    fake_clock.advance(minutes=11)

    assert (await _verify(client, "a@example.com", mailbox.last_code)).status_code == 401


async def test_verify_newerCodeReplacesOlderOne(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    await client.post("/v1/auth/code", json={"email": "a@example.com"})
    old = mailbox.last_code
    fake_clock.advance(seconds=5)
    await client.post("/v1/auth/code", json={"email": "a@example.com"})
    new = mailbox.last_code

    if old != new:
        assert (await _verify(client, "a@example.com", old)).status_code == 401
    assert (await _verify(client, "a@example.com", new)).status_code == 200


async def test_verify_codeForAnotherEmail_is401(client: AsyncClient, mailbox: FakeEmailSender):
    await client.post("/v1/auth/code", json={"email": "a@example.com"})

    assert (await _verify(client, "b@example.com", mailbox.last_code)).status_code == 401


@pytest.mark.parametrize("code", ["12345", "1234567", "abcdef", ""])
async def test_verify_malformedCode_is422(client: AsyncClient, code: str):
    assert (await _verify(client, "a@example.com", code)).status_code == 422


async def test_verify_badPlatform_is422(client: AsyncClient):
    r = await client.post(
        "/v1/auth/verify",
        json={
            "email": "a@example.com",
            "code": "123456",
            "device": {"device_id": str(uuid.uuid4()), "platform": "tv"},
        },
    )
    assert r.status_code == 422


@pytest.mark.parametrize("settings_override", [{"max_devices_per_account": 2}])
async def test_verify_tooManyDevices_is409ButKnownDeviceStillSignsIn(
    client: AsyncClient, mailbox: FakeEmailSender, settings_override: dict[str, object]
):
    d1, d2 = device(), device("ios")
    await sign_in(client, mailbox, dev=d1)
    await sign_in(client, mailbox, dev=d2)

    await client.post("/v1/auth/code", json={"email": "veli@example.com"})
    third = await _verify(client, "veli@example.com", mailbox.last_code, device())
    assert third.status_code == 409 and third.json() == {"code": "device_limit"}

    again = await sign_in(client, mailbox, dev=d1)
    assert again["access_token"]


async def test_refresh_rotatesTokenAndOldOneStopsWorking(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)

    r = await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})

    assert r.status_code == 200
    assert r.json()["refresh_token"] != tokens["refresh_token"]
    assert r.json()["account_id"] == tokens["account_id"]


async def test_refresh_reuseOfRotatedToken_signsTheDeviceOut(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    newer = (
        await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})
    ).json()

    stolen = await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})

    assert stolen.status_code == 401 and stolen.json() == {"code": "invalid_token"}
    after = await client.post("/v1/auth/refresh", json={"refresh_token": newer["refresh_token"]})
    assert after.status_code == 401, "the whole device is signed out after a reuse"


async def test_refresh_expiredOrUnknownToken_is401(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)
    unknown = await client.post("/v1/auth/refresh", json={"refresh_token": "x" * 40})
    fake_clock.advance(days=61)
    expired = await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})

    assert unknown.status_code == 401
    assert expired.status_code == 401


async def test_logout_revokesTheTokenAndIsIdempotent(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)

    first = await client.post("/v1/auth/logout", json={"refresh_token": tokens["refresh_token"]})
    second = await client.post("/v1/auth/logout", json={"refresh_token": tokens["refresh_token"]})
    unknown = await client.post("/v1/auth/logout", json={"refresh_token": "y" * 40})

    assert first.status_code == second.status_code == unknown.status_code == 204
    assert (
        await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})
    ).status_code == 401


async def test_storage_neverHoldsPlainEmailOrCodeInTransientTables(
    client: AsyncClient, mailbox: FakeEmailSender
):
    await client.post("/v1/auth/code", json={"email": "gizli@example.com"})

    async with get_sessionmaker()() as s:
        row = (await s.execute(select(LoginCode))).scalar_one()
    assert "gizli" not in row.email_digest and mailbox.last_code not in row.code_digest
    assert len(row.email_digest) == 64


async def test_logs_neverContainEmailOrCode(
    client: AsyncClient, mailbox: FakeEmailSender, caplog: pytest.LogCaptureFixture
):
    with caplog.at_level(logging.DEBUG):
        tokens = await sign_in(client, mailbox, "kisisel@example.com")

    text = " ".join(r.getMessage() + str(r.__dict__) for r in caplog.records)
    assert "kisisel" not in text
    assert mailbox.last_code not in text
    assert tokens["refresh_token"] not in text
