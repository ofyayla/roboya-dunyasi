import json
import uuid

import pytest
from httpx import AsyncClient
from sqlalchemy import func, select, text

from app import maintenance
from app.core.config import Settings
from app.core.db import get_sessionmaker
from app.main import check_settings
from app.models.privacy import ConsentRecord, DeletionLog
from app.services import legal
from tests.conftest import FakeClock, FakeEmailSender, StoreSigner
from tests.helpers import bearer, sign_in, sign_in_with_consent

PROFILE = {"nickname": "Elif", "avatar_id": "robot-mavi", "age_band": "minik"}
LEVEL = "sabir-ormani.yon-avcisi.01"


async def _count(model) -> int:
    async with get_sessionmaker()() as s:
        return int(await s.scalar(select(func.count()).select_from(model)) or 0)


async def _scalar(sql: str) -> int:
    async with get_sessionmaker()() as s:
        return int((await s.execute(text(sql))).scalar_one())


async def test_notice_isPublicAndVersioned(client: AsyncClient):
    r = await client.get("/v1/legal/notice")

    assert r.status_code == 200
    body = r.json()
    assert body["version"] and body["status"] in ("draft", "final")
    assert "KVKK" in body["text"] and "<!--" not in body["text"]
    assert "yurt dışına aktarılır" in body["text"], "UYM-04: the transfer is told in the notice"


async def test_consent_profileNeedsConsentFirst(client: AsyncClient, mailbox: FakeEmailSender):
    tokens = await sign_in(client, mailbox)

    blocked = await client.put(
        f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens)
    )

    assert blocked.status_code == 403 and blocked.json() == {"code": "consent_required"}


async def test_consent_givenForTheCurrentVersionOpensProfiles(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in_with_consent(client, mailbox)

    ok = await client.put(f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens))
    status = (await client.get("/v1/me/consents", headers=bearer(tokens))).json()

    assert ok.status_code == 200
    assert status["consented"] is True and len(status["records"]) == 1
    assert status["records"][0]["notice_version"] == status["current_version"]


async def test_consent_isRecordedOnceAndOnlyForTheCurrentVersion(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in(client, mailbox)
    current = (await client.get("/v1/me/consents", headers=bearer(tokens))).json()[
        "current_version"
    ]

    first = await client.post(
        "/v1/me/consents", json={"notice_version": current}, headers=bearer(tokens)
    )
    again = await client.post(
        "/v1/me/consents", json={"notice_version": current}, headers=bearer(tokens)
    )
    old = await client.post(
        "/v1/me/consents", json={"notice_version": "1999-old"}, headers=bearer(tokens)
    )

    assert first.status_code == again.status_code == 204
    assert old.status_code == 409 and old.json() == {"code": "notice_outdated"}
    assert await _count(ConsentRecord) == 1


async def test_consent_withdrawalStopsChildDataWritesButKeepsReads(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in_with_consent(client, mailbox)
    pid = uuid.uuid4()
    await client.put(f"/v1/me/profiles/{pid}", json=PROFILE, headers=bearer(tokens))

    assert (await client.delete("/v1/me/consents", headers=bearer(tokens))).status_code == 204

    sync = await client.post(
        f"/v1/me/profiles/{pid}/progress/sync", json={"stars": {LEVEL: 3}}, headers=bearer(tokens)
    )
    update = await client.put(f"/v1/me/profiles/{pid}", json=PROFILE, headers=bearer(tokens))
    read = await client.get(f"/v1/me/profiles/{pid}/progress", headers=bearer(tokens))
    status = (await client.get("/v1/me/consents", headers=bearer(tokens))).json()
    assert sync.status_code == update.status_code == 403
    assert read.status_code == 200, "the parent can still see what we hold"
    assert status["consented"] is False and status["records"][0]["withdrawn_at"]


async def test_consent_endpointsRequireAuth(client: AsyncClient):
    assert (await client.get("/v1/me/consents")).status_code == 401
    assert (await client.post("/v1/me/consents", json={"notice_version": "x"})).status_code == 401
    assert (await client.delete("/v1/me/consents")).status_code == 401
    assert (await client.get("/v1/me/data")).status_code == 401
    assert (await client.get("/v1/me/data/export")).status_code == 401
    assert (await client.post("/v1/me/deletion-request")).status_code == 401


async def test_myData_showsEverythingWeHold_andOnlyTheCallers(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    a = await sign_in_with_consent(client, mailbox, "a@example.com")
    b = await sign_in_with_consent(client, mailbox, "b@example.com")
    pid = uuid.uuid4()
    await client.put(f"/v1/me/profiles/{pid}", json=PROFILE, headers=bearer(a))
    await client.post(
        f"/v1/me/profiles/{pid}/progress/sync", json={"stars": {LEVEL: 2}}, headers=bearer(a)
    )
    await client.post(
        "/v1/store/receipts",
        json={"store": "apple", "proof": store_signer.purchase(fake_clock.current)},
        headers=bearer(a),
    )

    mine = (await client.get("/v1/me/data", headers=bearer(a))).json()
    theirs = (await client.get("/v1/me/data", headers=bearer(b))).json()

    assert mine["account"]["email"] == "a@example.com"
    assert mine["profiles"][0]["nickname"] == "Elif" and mine["profiles"][0]["progress"] == {
        LEVEL: 2
    }
    assert mine["subscriptions"][0]["status"] == "active" and mine["devices"] and mine["consents"]
    assert theirs["profiles"] == [] and theirs["subscriptions"] == []
    assert "a@example.com" not in json.dumps(theirs)


async def test_export_isADownloadableJsonFileWithTheSameData(
    client: AsyncClient, mailbox: FakeEmailSender
):
    tokens = await sign_in_with_consent(client, mailbox)

    r = await client.get("/v1/me/data/export", headers=bearer(tokens))

    assert r.status_code == 200 and r.headers["content-type"].startswith("application/json")
    assert "attachment" in r.headers["content-disposition"]
    assert r.json() == (await client.get("/v1/me/data", headers=bearer(tokens))).json()


async def test_deletionRequest_isScheduledAfterTheCoolingPeriodAndRepeatsSafely(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)

    first = await client.post("/v1/me/deletion-request", headers=bearer(tokens))
    second = await client.post("/v1/me/deletion-request", headers=bearer(tokens))
    status = await client.get("/v1/me/deletion-request", headers=bearer(tokens))

    assert first.status_code == second.status_code == 202
    assert first.json() == second.json() == status.json()
    requested = first.json()["requested_at"][:10]
    assert requested == "2026-10-08" and first.json()["scheduled_for"][:10] == "2026-10-15"
    assert (await client.get("/v1/me/data", headers=bearer(tokens))).json()["deletion_request"]


async def test_deletionRequest_canBeCancelled(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)
    assert (await client.get("/v1/me/deletion-request", headers=bearer(tokens))).status_code == 404
    assert (
        await client.delete("/v1/me/deletion-request", headers=bearer(tokens))
    ).status_code == 404
    await client.post("/v1/me/deletion-request", headers=bearer(tokens))

    assert (
        await client.delete("/v1/me/deletion-request", headers=bearer(tokens))
    ).status_code == 204

    fake_clock.advance(days=8)
    deleted, _ = await maintenance.run_privacy()
    assert deleted == 0
    assert await _scalar("SELECT count(*) FROM accounts") == 1


async def test_deletion_afterTheWaitRemovesEverythingTiedToTheAccount(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    keep = await sign_in_with_consent(client, mailbox, "keep@example.com")
    gone = await sign_in_with_consent(client, mailbox, "gone@example.com")
    for tokens, pid in ((keep, uuid.uuid4()), (gone, uuid.uuid4())):
        await client.put(f"/v1/me/profiles/{pid}", json=PROFILE, headers=bearer(tokens))
        await client.post(
            f"/v1/me/profiles/{pid}/progress/sync",
            json={"stars": {LEVEL: 3}},
            headers=bearer(tokens),
        )
    await client.post(
        "/v1/store/receipts",
        json={"store": "apple", "proof": store_signer.purchase(fake_clock.current)},
        headers=bearer(gone),
    )
    await client.post("/v1/me/deletion-request", headers=bearer(gone))

    fake_clock.advance(days=6)
    early = await maintenance.run_privacy()
    assert early[0] == 0, "still inside the waiting period"
    fake_clock.advance(days=2)
    deleted, _ = await maintenance.run_privacy()
    again, _ = await maintenance.run_privacy()

    assert (deleted, again) == (1, 0)
    for table in (
        "accounts",
        "device_registrations",
        "refresh_tokens",
        "child_profiles",
        "progress_entries",
    ):
        remaining = await _scalar(f"SELECT count(*) FROM {table}")  # noqa: S608 - fixed names
        assert remaining >= 1, table  # the other parent's rows are still there
    assert await _scalar("SELECT count(*) FROM accounts WHERE email = 'gone@example.com'") == 0
    assert await _scalar("SELECT count(*) FROM child_profiles") == 1
    assert await _scalar("SELECT count(*) FROM progress_entries") == 1
    assert (
        await _scalar("SELECT count(*) FROM store_subscriptions WHERE account_id IS NOT NULL") == 0
    )
    assert await _scalar("SELECT count(*) FROM store_subscriptions") == 1, (
        "opaque store facts stay unlinked"
    )
    assert await _count(DeletionLog) == 1
    log = await _scalar(
        "SELECT extract(day FROM completed_at - requested_at)::int FROM deletion_log"
    )
    assert log <= 30, "UYM-03: completed within 30 days"
    assert await _scalar("SELECT count(*) FROM consent_records") == 2, (
        "proof of consent is kept, unlinked"
    )


async def test_deletion_signedOutTokensOfADeletedAccountStopWorking(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)
    await client.post("/v1/me/deletion-request", headers=bearer(tokens))
    fake_clock.advance(days=7, minutes=1)
    await maintenance.run_privacy()

    assert (await client.get("/v1/me", headers=bearer(tokens))).status_code == 401
    refresh = await client.post("/v1/auth/refresh", json={"refresh_token": tokens["refresh_token"]})
    assert refresh.status_code == 401


async def test_maintenance_purgesExpiredLoginCodes(client: AsyncClient, fake_clock: FakeClock):
    await client.post("/v1/auth/code", json={"email": "a@example.com"})
    fake_clock.advance(days=2)

    _, purged = await maintenance.run_privacy()

    assert purged == 1
    assert await _scalar("SELECT count(*) FROM login_codes") == 0


def test_checkSettings_productionRefusesADraftNotice(tmp_path):
    base = tmp_path / "legal"
    base.mkdir()
    (base / legal.NOTICE_FILE).write_text(
        "<!-- version: v1 -->\n<!-- status: draft -->\n# Metin\n", encoding="utf-8"
    )
    settings = Settings(env="production", jwt_secret="x" * 40, legal_dir=str(base))

    with pytest.raises(RuntimeError, match="draft"):
        check_settings(settings)


def test_notice_loader_rejectsMissingFileAndMissingVersion(tmp_path):
    with pytest.raises(legal.NoticeError):
        legal.current_notice(Settings(legal_dir=str(tmp_path)))
    (tmp_path / legal.NOTICE_FILE).write_text("# no version here", encoding="utf-8")
    with pytest.raises(legal.NoticeError):
        legal.current_notice(Settings(legal_dir=str(tmp_path)))


def test_notice_loader_anythingButFinalCountsAsDraft(tmp_path):
    path = tmp_path / legal.NOTICE_FILE
    path.write_text("<!-- version: v2 -->\n<!-- status: finall -->\ntext", encoding="utf-8")
    assert legal.current_notice(Settings(legal_dir=str(tmp_path))).status == "draft"
    path.write_text("<!-- version: v2 -->\ntext", encoding="utf-8")
    assert legal.current_notice(Settings(legal_dir=str(tmp_path))).status == "draft"
    path.write_text("<!-- version: v2 -->\n<!-- status: final -->\ntext", encoding="utf-8")
    assert legal.current_notice(Settings(legal_dir=str(tmp_path))).status == "final"
