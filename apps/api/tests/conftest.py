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

import pytest
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
                "TRUNCATE progress_entries, child_profiles, refresh_tokens, login_codes, "
                "device_registrations, accounts "
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


@pytest.fixture
async def client(
    mailbox: FakeEmailSender, fake_clock: FakeClock, settings_override: dict[str, object]
) -> AsyncIterator[AsyncClient]:
    app = create_app()
    app.dependency_overrides[get_email_sender] = lambda: mailbox
    if settings_override:
        custom: Settings = get_settings().model_copy(update=settings_override)
        app.dependency_overrides[get_settings] = lambda: custom
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as c:
        yield c


DeviceFactory = Callable[[], dict[str, str]]
