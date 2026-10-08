import uuid

import jwt
import pytest
from httpx import AsyncClient

from app.core.config import get_settings
from tests.conftest import FakeClock, FakeEmailSender
from tests.helpers import bearer, device, sign_in


async def test_me_withoutToken_is401(client: AsyncClient):
    r = await client.get("/v1/me")

    assert r.status_code == 401 and r.json() == {"code": "invalid_token"}


async def test_me_returnsOnlyTheCallersAccount(client: AsyncClient, mailbox: FakeEmailSender):
    a = await sign_in(client, mailbox, "a@example.com")
    b = await sign_in(client, mailbox, "b@example.com")

    me_a = (await client.get("/v1/me", headers=bearer(a))).json()
    me_b = (await client.get("/v1/me", headers=bearer(b))).json()

    assert me_a["email"] == "a@example.com" and me_b["email"] == "b@example.com"
    assert me_a["id"] != me_b["id"]


@pytest.mark.parametrize("bad", ["not-a-jwt", "", "a.b.c"])
async def test_me_malformedToken_is401(client: AsyncClient, bad: str):
    r = await client.get("/v1/me", headers={"Authorization": "Bearer " + bad})

    assert r.status_code == 401


async def test_me_expiredToken_is401(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)
    fake_clock.advance(minutes=31)

    assert (await client.get("/v1/me", headers=bearer(tokens))).status_code == 401


async def test_me_tamperedOrForeignSignedToken_is401(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)
    forged = jwt.encode(
        {"sub": tokens["account_id"], "typ": "access", "iat": 1, "exp": 9999999999},
        "another-secret-another-secret-0123456789",
        algorithm="HS256",
    )

    assert (
        await client.get("/v1/me", headers={"Authorization": "Bearer " + forged})
    ).status_code == 401


async def test_me_refreshTokenIsNotAnAccessToken(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)

    r = await client.get("/v1/me", headers={"Authorization": "Bearer " + tokens["refresh_token"]})

    assert r.status_code == 401


async def test_me_wrongTokenType_is401(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)
    settings = get_settings()
    other = jwt.encode(
        {"sub": tokens["account_id"], "typ": "refresh", "iat": 1, "exp": 9999999999},
        settings.jwt_secret,
        algorithm="HS256",
    )

    assert (
        await client.get("/v1/me", headers={"Authorization": "Bearer " + other})
    ).status_code == 401


async def test_me_tokenForDeletedAccountId_is401(client: AsyncClient, fake_clock: FakeClock):
    settings = get_settings()
    ghost = jwt.encode(
        {"sub": str(uuid.uuid4()), "typ": "access", "iat": 1, "exp": 9999999999},
        settings.jwt_secret,
        algorithm="HS256",
    )

    assert (
        await client.get("/v1/me", headers={"Authorization": "Bearer " + ghost})
    ).status_code == 401


async def test_devices_listsOwnDevicesOnly(client: AsyncClient, mailbox: FakeEmailSender):
    a = await sign_in(client, mailbox, "a@example.com", device("android"))
    await sign_in(client, mailbox, "a@example.com", device("ios"))
    b = await sign_in(client, mailbox, "b@example.com")

    mine = (await client.get("/v1/me/devices", headers=bearer(a))).json()
    theirs = (await client.get("/v1/me/devices", headers=bearer(b))).json()

    assert {d["platform"] for d in mine} == {"android", "ios"}
    assert len(theirs) == 1
    assert set(mine[0]) == {"id", "platform", "created_at", "last_seen_at"}, (
        "no device id leaks out"
    )


async def test_devices_cannotRemoveAnotherParentsDevice(
    client: AsyncClient, mailbox: FakeEmailSender
):
    a = await sign_in(client, mailbox, "a@example.com")
    b = await sign_in(client, mailbox, "b@example.com")
    b_device = (await client.get("/v1/me/devices", headers=bearer(b))).json()[0]["id"]

    r = await client.delete(f"/v1/me/devices/{b_device}", headers=bearer(a))

    assert r.status_code == 404 and r.json() == {"code": "not_found"}
    assert len((await client.get("/v1/me/devices", headers=bearer(b))).json()) == 1


async def test_devices_removeOwnDevice_signsItOut(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox, dev=device())
    phone = (await client.get("/v1/me/devices", headers=bearer(tokens))).json()[0]["id"]

    r = await client.delete(f"/v1/me/devices/{phone}", headers=bearer(tokens))

    assert r.status_code == 204
    assert (await client.get("/v1/me/devices", headers=bearer(tokens))).json() == []
    again = await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})
    assert again.status_code == 401


async def test_devices_requireAuth(client: AsyncClient):
    assert (await client.get("/v1/me/devices")).status_code == 401
    assert (await client.delete(f"/v1/me/devices/{uuid.uuid4()}")).status_code == 401
