import os

# Tests always use the dedicated test database (ADR 0001); set before app import.
os.environ.setdefault(
    "ROBOYA_DATABASE_URL", "postgresql+asyncpg://roboya:roboya@localhost:5432/roboya_test"
)
os.environ["ROBOYA_ENV"] = "test"

import subprocess
import sys
from collections.abc import AsyncIterator, Callable
from datetime import UTC, datetime, timedelta
from pathlib import Path

import jwt
import pytest
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import ec
from httpx import ASGITransport, AsyncClient
from sqlalchemy import text

from app.core import clock
from app.core.config import Settings, get_settings
from app.core.db import get_engine
from app.main import create_app
from app.routers.deps import get_email_sender

API_ROOT = Path(__file__).resolve().parents[1]


def _alembic(*args: str) -> None:
    subprocess.run(  # noqa: S603 - fixed arguments, no user input
        [sys.executable, "-m", "alembic", *args], cwd=API_ROOT, check=True, capture_output=True
    )


@pytest.fixture(scope="session", autouse=True)
def migrated_database() -> None:
    """Applies the real migrations to the test database, down and up, so they are tested too."""
    _alembic("downgrade", "base")
    _alembic("upgrade", "head")


@pytest.fixture(autouse=True)
async def clean_tables(migrated_database: None) -> AsyncIterator[None]:
    yield
    async with get_engine().begin() as conn:
        await conn.execute(
            text(
                "TRUNCATE deletion_log, consent_records, deletion_requests, events, store_events, "
                "store_subscriptions, progress_entries, child_profiles, refresh_tokens, "
                "login_codes, device_registrations, accounts "
                "RESTART IDENTITY CASCADE"
            )
        )


class FakeEmailSender:
    """Collects messages instead of sending them."""

    def __init__(self) -> None:
        self.sent: list[tuple[str, str]] = []

    async def send_login_code(self, email: str, code: str) -> None:
        self.sent.append((email, code))

    @property
    def last_code(self) -> str:
        return self.sent[-1][1]


class FakeClock:
    def __init__(self) -> None:
        self.current = datetime(2026, 10, 8, 12, 0, tzinfo=UTC)

    def advance(self, **kwargs: float) -> None:
        self.current += timedelta(**kwargs)

    def __call__(self) -> datetime:
        return self.current


@pytest.fixture
def fake_clock(monkeypatch: pytest.MonkeyPatch) -> FakeClock:
    fake = FakeClock()
    monkeypatch.setattr(clock, "now", fake)
    return fake


@pytest.fixture
def mailbox() -> FakeEmailSender:
    return FakeEmailSender()


@pytest.fixture
def settings_override() -> dict[str, object]:
    """Tests put field overrides here before the client is created."""
    return {}


class StoreSigner:
    """Plays the store: signs purchases and notifications with a throwaway ES256 key."""

    def __init__(self) -> None:
        self._private = ec.generate_private_key(ec.SECP256R1())
        self.public_pem = (
            self._private.public_key()
            .public_bytes(
                serialization.Encoding.PEM, serialization.PublicFormat.SubjectPublicKeyInfo
            )
            .decode()
        )
        self._counter = 0

    def _sign(
        self, claims: dict[str, object], key: ec.EllipticCurvePrivateKey | None = None
    ) -> str:
        return jwt.encode(claims, key or self._private, algorithm="ES256")

    def purchase(
        self,
        clock_now: datetime,
        *,
        store: str = "apple",
        otid: str = "tx-1",
        days: float = 30,
        trial: bool = False,
        forged: bool = False,
    ) -> str:
        claims = {
            "typ": "purchase",
            "store": store,
            "otid": otid,
            "pid": "family_premium_monthly",
            "expires": int((clock_now + timedelta(days=days)).timestamp()),
            "trial": trial,
        }
        return self._sign(claims, ec.generate_private_key(ec.SECP256R1()) if forged else None)

    def notification(
        self,
        clock_now: datetime,
        kind: str,
        *,
        store: str = "apple",
        otid: str = "tx-1",
        days: float = 30,
        trial: bool = False,
        event_id: str | None = None,
        at: datetime | None = None,
    ) -> str:
        self._counter += 1
        claims = {
            "typ": "notification",
            "store": store,
            "eid": event_id or f"evt-{self._counter}",
            "kind": kind,
            "at": int((at or clock_now).timestamp()),
            "otid": otid,
            "pid": "family_premium_monthly",
            "expires": int((clock_now + timedelta(days=days)).timestamp()),
            "trial": trial,
        }
        return self._sign(claims)

    def raw(self, claims: dict[str, object]) -> str:
        return self._sign(claims)


@pytest.fixture
def store_signer(settings_override: dict[str, object]) -> StoreSigner:
    signer = StoreSigner()
    settings_override["store_dev_public_key"] = signer.public_pem
    return signer


@pytest.fixture
async def client(
    mailbox: FakeEmailSender,
    fake_clock: FakeClock,
    settings_override: dict[str, object],
    store_signer: StoreSigner,
) -> AsyncIterator[AsyncClient]:
    app = create_app()
    app.dependency_overrides[get_email_sender] = lambda: mailbox
    if settings_override:
        custom: Settings = get_settings().model_copy(update=settings_override)
        app.dependency_overrides[get_settings] = lambda: custom
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as c:
        yield c


DeviceFactory = Callable[[], dict[str, str]]
