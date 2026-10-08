import uuid

import pytest
from httpx import AsyncClient

from tests.conftest import FakeEmailSender
from tests.helpers import bearer, sign_in

L1 = "sabir-ormani.yon-avcisi.01"
L2 = "sabir-ormani.yon-avcisi.02"
L3 = "sabir-ormani.yon-avcisi.03"
PROFILE = {"nickname": "Elif", "avatar_id": "robot-mavi", "age_band": "minik"}


async def _put(client: AsyncClient, tokens: dict, profile_id: uuid.UUID | str, body=None):
    return await client.put(
        f"/v1/me/profiles/{profile_id}", json=body or PROFILE, headers=bearer(tokens)
    )


async def _sync(client: AsyncClient, tokens: dict, profile_id, stars: dict):
    return await client.post(
        f"/v1/me/profiles/{profile_id}/progress/sync", json={"stars": stars}, headers=bearer(tokens)
    )


async def test_saveProfile_createsThenUpdatesWithTheAppsOwnId(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()

    created = await _put(client, tokens, pid)
    updated = await _put(
        client, tokens, pid, {**PROFILE, "nickname": "  Ela  ", "age_band": "kasif"}
    )

    assert created.status_code == 200 and created.json()["id"] == str(pid)
    assert updated.json()["nickname"] == "Ela" and updated.json()["age_band"] == "kasif"
    assert len((await client.get("/v1/me/profiles", headers=bearer(tokens))).json()) == 1


async def test_saveProfile_freeTierAllowsOneProfile(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)
    await _put(client, tokens, uuid.uuid4())

    second = await _put(client, tokens, uuid.uuid4())

    assert second.status_code == 409 and second.json() == {"code": "profile_limit"}


@pytest.mark.parametrize(
    "body",
    [
        {**PROFILE, "nickname": ""},
        {**PROFILE, "nickname": "   "},
        {**PROFILE, "nickname": "x" * 25},
        {**PROFILE, "avatar_id": "Robot Mavi!"},
        {**PROFILE, "age_band": "baby"},
        {**PROFILE, "birth_date": "2021-01-01"},
        {**PROFILE, "real_name": "Elif Yılmaz"},
        {"nickname": "Elif"},
    ],
)
async def test_saveProfile_onlyTheAllowedFieldsAreAccepted(
    client: AsyncClient, mailbox: FakeEmailSender, body: dict
):
    tokens = await sign_in(client, mailbox)

    assert (await _put(client, tokens, uuid.uuid4(), body)).status_code == 422


async def test_profiles_requireAuthentication(client: AsyncClient):
    pid = uuid.uuid4()

    assert (await client.get("/v1/me/profiles")).status_code == 401
    assert (await client.put(f"/v1/me/profiles/{pid}", json=PROFILE)).status_code == 401
    assert (await client.delete(f"/v1/me/profiles/{pid}")).status_code == 401
    assert (await client.get(f"/v1/me/profiles/{pid}/progress")).status_code == 401
    assert (
        await client.post(f"/v1/me/profiles/{pid}/progress/sync", json={"stars": {}})
    ).status_code == 401


async def test_scope_anotherParentsProfileIsInvisibleEverywhere(
    client: AsyncClient, mailbox: FakeEmailSender
):
    a = await sign_in(client, mailbox, "a@example.com")
    b = await sign_in(client, mailbox, "b@example.com")
    pid = uuid.uuid4()
    await _put(client, a, pid)
    await _sync(client, a, pid, {L1: 3})

    assert (await client.get("/v1/me/profiles", headers=bearer(b))).json() == []
    assert (
        await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(b))
    ).status_code == 404
    assert (await _sync(client, b, pid, {L1: 1})).status_code == 404
    assert (await client.delete(f"/v1/me/profiles/{pid}", headers=bearer(b))).status_code == 404
    hijack = await _put(client, b, pid, {**PROFILE, "nickname": "Ele Geçirdi"})
    assert hijack.status_code == 404, "an id owned by another account looks like it does not exist"
    own = (await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(a))).json()
    assert own == {"stars": {L1: 3}}


async def test_sync_firstSyncStoresStarsAndReturnsThem(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)

    r = await _sync(client, tokens, pid, {L1: 3, L2: 1})

    assert r.status_code == 200 and r.json() == {"stars": {L1: 3, L2: 1}}


async def test_sync_keepsTheHigherStarsPerLevel_inAnyOrder(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)
    await _sync(client, tokens, pid, {L1: 3, L2: 1})

    # A second device that played level 2 better and level 1 worse.
    r = await _sync(client, tokens, pid, {L1: 1, L2: 3, L3: 2})

    assert r.json() == {"stars": {L1: 3, L2: 3, L3: 2}}


async def test_sync_isIdempotentAndNeverLowersStars(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)

    first = await _sync(client, tokens, pid, {L1: 2})
    again = await _sync(client, tokens, pid, {L1: 2})
    lower = await _sync(client, tokens, pid, {L1: 0})
    empty = await _sync(client, tokens, pid, {})

    assert first.json() == again.json() == lower.json() == empty.json() == {"stars": {L1: 2}}


@pytest.mark.parametrize(
    "stars",
    [
        {L1: 4},
        {L1: -1},
        {"not a level": 1},
        {"UPPER.case.01": 1},
        {L1: "3"},
        {"a." * 50 + "b.c": 1},
    ],
)
async def test_sync_badStarsOrLevelIdsAreRejected(
    client: AsyncClient, mailbox: FakeEmailSender, stars: dict
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)

    assert (await _sync(client, tokens, pid, stars)).status_code == 422


async def test_sync_tooManyEntriesInOneRequestIsRejected(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)
    many = {f"r.g.{i}": 1 for i in range(501)}

    assert (await _sync(client, tokens, pid, many)).status_code == 422


async def test_getProgress_returnsMergedStarsAndEmptyForNewProfile(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)

    empty = await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(tokens))
    await _sync(client, tokens, pid, {L2: 2})
    filled = await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(tokens))

    assert empty.json() == {"stars": {}} and filled.json() == {"stars": {L2: 2}}


async def test_deleteProfile_removesItsProgress(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)
    pid = uuid.uuid4()
    await _put(client, tokens, pid)
    await _sync(client, tokens, pid, {L1: 3})

    assert (
        await client.delete(f"/v1/me/profiles/{pid}", headers=bearer(tokens))
    ).status_code == 204

    assert (
        await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(tokens))
    ).status_code == 404
    # The slot is free again and the new profile starts with no stars.
    await _put(client, tokens, pid)
    assert (await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(tokens))).json() == {
        "stars": {}
    }


@pytest.mark.parametrize("settings_override", [{"max_profiles_free": 2}])
async def test_saveProfile_limitComesFromConfiguration(
    client: AsyncClient, mailbox: FakeEmailSender, settings_override: dict[str, object]
):
    tokens = await sign_in(client, mailbox)

    assert (await _put(client, tokens, uuid.uuid4())).status_code == 200
    assert (await _put(client, tokens, uuid.uuid4())).status_code == 200
    assert (await _put(client, tokens, uuid.uuid4())).status_code == 409
