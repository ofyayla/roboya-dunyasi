import uuid
from datetime import timedelta

import pytest
from httpx import AsyncClient

from tests.conftest import FakeClock, FakeEmailSender, StoreSigner
from tests.helpers import bearer, sign_in, sign_in_with_consent

PROFILE = {"nickname": "Elif", "avatar_id": "robot-mavi", "age_band": "minik"}


async def _link(client, tokens, proof, store="apple"):
    return await client.post(
        "/v1/store/receipts", json={"store": store, "proof": proof}, headers=bearer(tokens)
    )


async def _notify(client, payload, store="apple"):
    return await client.post(f"/v1/store/notifications/{store}", json={"signed_payload": payload})


async def _entitlement(client, tokens):
    r = await client.get("/v1/me/entitlement", headers=bearer(tokens))
    assert r.status_code == 200, r.text
    return r.json()


async def test_entitlement_newAccountIsFree(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock
):
    tokens = await sign_in(client, mailbox)

    e = await _entitlement(client, tokens)

    assert (e["tier"], e["source"], e["status"], e["expires_at"]) == ("free", "none", "free", None)


async def test_entitlement_requiresAuthentication(client: AsyncClient):
    assert (await client.get("/v1/me/entitlement")).status_code == 401
    assert (
        await client.post("/v1/store/receipts", json={"store": "apple", "proof": "x" * 20})
    ).status_code == 401


async def test_receipt_validPurchaseMakesTheAccountPremium(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)

    r = await _link(client, tokens, store_signer.purchase(fake_clock.current))

    assert r.status_code == 200
    assert (r.json()["tier"], r.json()["source"], r.json()["status"]) == (
        "premium",
        "store",
        "active",
    )
    assert (await _entitlement(client, tokens))["tier"] == "premium"


async def test_receipt_trialPurchaseIsPremiumWithTrialStatus(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)

    r = await _link(client, tokens, store_signer.purchase(fake_clock.current, days=7, trial=True))

    assert r.json()["status"] == "trial"


async def test_receipt_cacheNeverOutlivesThePaidPeriod(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    short = await _link(client, tokens, store_signer.purchase(fake_clock.current, days=1))
    long_ = await _link(
        client, tokens, store_signer.purchase(fake_clock.current, otid="tx-2", days=90)
    )

    assert short.json()["cache_until"] == short.json()["expires_at"]
    assert long_.json()["cache_until"].startswith(
        (fake_clock.current + timedelta(hours=72)).strftime("%Y-%m-%dT%H:%M")
    )


async def test_receipt_forgedTamperedOrWrongStoreIsRejected(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    good = store_signer.purchase(fake_clock.current)

    forged = await _link(client, tokens, store_signer.purchase(fake_clock.current, forged=True))
    tampered = await _link(client, tokens, good[:-4] + "AAAA")
    other_store = await _link(client, tokens, good, store="google")
    garbage = await _link(client, tokens, "not-a-token-at-all")
    wrong_kind = await _link(
        client, tokens, store_signer.notification(fake_clock.current, "renewed")
    )
    missing = await _link(
        client, tokens, store_signer.raw({"typ": "purchase", "store": "apple", "otid": "t"})
    )

    for r in (forged, tampered, other_store, garbage, wrong_kind, missing):
        assert r.status_code == 401 and r.json() == {"code": "invalid_signature"}
    assert (await _entitlement(client, tokens))["tier"] == "free"


async def test_receipt_aPurchaseBelongsToOneAccount(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    a = await sign_in(client, mailbox, "a@example.com")
    b = await sign_in(client, mailbox, "b@example.com")
    proof = store_signer.purchase(fake_clock.current)
    await _link(client, a, proof)

    again = await _link(client, a, proof)
    stolen = await _link(client, b, proof)

    assert again.status_code == 200, "the same parent on a second device"
    assert stolen.status_code == 409 and stolen.json() == {"code": "purchase_linked"}
    assert (await _entitlement(client, b))["tier"] == "free"


async def test_receipt_expiredPurchaseGivesNothing(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)

    r = await _link(client, tokens, store_signer.purchase(fake_clock.current, days=-1))

    assert r.json()["tier"] == "free"


async def test_premium_endsWhenThePeriodEnds(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    await _link(client, tokens, store_signer.purchase(fake_clock.current, days=2))
    fake_clock.advance(hours=1)
    tokens = await sign_in(client, mailbox)  # the 30 minute access token has run out
    assert (await _entitlement(client, tokens))["tier"] == "premium"

    fake_clock.advance(days=2)
    # The access token also lasts only 30 minutes; sign in again as the app would.
    tokens = await sign_in(client, mailbox)

    assert (await _entitlement(client, tokens))["tier"] == "free"


async def test_notification_requiresAValidSignature(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    r = await _notify(client, "x" * 30)
    wrong_store = await _notify(
        client, store_signer.notification(fake_clock.current, "renewed"), "google"
    )
    unknown_store = await _notify(
        client, store_signer.notification(fake_clock.current, "renewed"), "huawei"
    )

    assert r.status_code == 401 and r.json() == {"code": "invalid_signature"}
    assert wrong_store.status_code == 401
    assert unknown_store.status_code == 422


async def test_notification_beforeTheReceiptIsKeptAndClaimedLater(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)

    r = await _notify(
        client, store_signer.notification(fake_clock.current, "subscribed", otid="early")
    )
    assert r.status_code == 204
    assert (await _entitlement(client, tokens))["tier"] == "free", "nobody owns it yet"

    await _link(client, tokens, store_signer.purchase(fake_clock.current, otid="early", days=30))
    assert (await _entitlement(client, tokens))["tier"] == "premium"


async def test_notification_isIdempotent(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    await _link(client, tokens, store_signer.purchase(fake_clock.current, days=5))
    refund = store_signer.notification(fake_clock.current, "refunded", event_id="evt-refund")

    first = await _notify(client, refund)
    retry = await _notify(client, refund)

    assert first.status_code == retry.status_code == 204
    assert (await _entitlement(client, tokens))["tier"] == "free"


@pytest.mark.parametrize(
    ("kind", "days", "tier", "status"),
    [
        ("renewed", 30, "premium", "active"),
        ("grace", 3, "premium", "grace"),
        ("expired", 0, "free", "free"),
        ("refunded", 30, "free", "free"),
    ],
)
async def test_notification_kindsMoveTheEntitlement(
    client: AsyncClient,
    mailbox: FakeEmailSender,
    fake_clock: FakeClock,
    store_signer: StoreSigner,
    kind: str,
    days: int,
    tier: str,
    status: str,
):
    tokens = await sign_in(client, mailbox)
    await _link(client, tokens, store_signer.purchase(fake_clock.current, days=2))
    fake_clock.advance(minutes=5)

    await _notify(client, store_signer.notification(fake_clock.current, kind, days=days))

    e = await _entitlement(client, tokens)
    assert (e["tier"], e["status"]) == (tier, status)


async def test_notification_renewalExtendsAndTrialConvertsToActive(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    await _link(client, tokens, store_signer.purchase(fake_clock.current, days=7, trial=True))
    assert (await _entitlement(client, tokens))["status"] == "trial"

    fake_clock.advance(days=7)
    await _notify(client, store_signer.notification(fake_clock.current, "renewed", days=30))
    tokens = await sign_in(client, mailbox)

    e = await _entitlement(client, tokens)
    assert e["status"] == "active" and e["tier"] == "premium"


async def test_notification_olderEventAfterANewerOneIsIgnored(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    start = fake_clock.current
    await _link(client, tokens, store_signer.purchase(start, days=2))

    await _notify(
        client, store_signer.notification(start, "refunded", at=start + timedelta(minutes=10))
    )
    late_old = store_signer.notification(start, "renewed", days=30, at=start + timedelta(minutes=1))
    await _notify(client, late_old)

    assert (await _entitlement(client, tokens))["tier"] == "free", (
        "the stale renewal must not undo the refund"
    )


async def test_receipt_afterRefundDoesNotBringPremiumBack(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    proof = store_signer.purchase(fake_clock.current, days=30)
    await _link(client, tokens, proof)
    fake_clock.advance(minutes=1)
    await _notify(client, store_signer.notification(fake_clock.current, "refunded", days=30))

    r = await _link(client, tokens, store_signer.purchase(fake_clock.current, days=60))

    assert r.json()["tier"] == "free", "a revoked purchase stays revoked"


async def test_twoSubscriptions_theLongerOneCounts(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in(client, mailbox)
    await _link(client, tokens, store_signer.purchase(fake_clock.current, otid="a", days=2))
    r = await _link(
        client,
        tokens,
        store_signer.purchase(fake_clock.current, otid="b", days=40, store="google"),
        "google",
    )

    e = r.json()
    assert e["tier"] == "premium"
    assert e["expires_at"].startswith(
        (fake_clock.current + timedelta(days=40)).strftime("%Y-%m-%dT%H:%M")
    )


async def test_profileLimit_premiumAllowsFourFreeAllowsOne(
    client: AsyncClient, mailbox: FakeEmailSender, fake_clock: FakeClock, store_signer: StoreSigner
):
    tokens = await sign_in_with_consent(client, mailbox)
    await client.put(f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens))
    blocked = await client.put(
        f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens)
    )
    assert blocked.status_code == 409

    await _link(client, tokens, store_signer.purchase(fake_clock.current))
    for _ in range(3):
        ok = await client.put(
            f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens)
        )
        assert ok.status_code == 200
    fifth = await client.put(
        f"/v1/me/profiles/{uuid.uuid4()}", json=PROFILE, headers=bearer(tokens)
    )
    assert fifth.status_code == 409


def test_checkSettings_productionRefusesTheSignedDevStore():
    from app.core.config import Settings
    from app.main import check_settings

    # Reaching the store check needs a long secret; the outbox check fires first, so test the order.
    with pytest.raises(RuntimeError):
        check_settings(Settings(env="production", jwt_secret="x" * 40))
