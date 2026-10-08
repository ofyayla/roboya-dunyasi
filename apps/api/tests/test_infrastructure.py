import json
import stat
from pathlib import Path

import pytest

from app.core import clock
from app.core.config import Settings
from app.main import check_settings
from app.services.email import OutboxEmailSender


async def test_outbox_appendsMessageToPrivateFile(tmp_path: Path):
    path = tmp_path / "mail" / "outbox.jsonl"
    sender = OutboxEmailSender(str(path))

    await sender.send_login_code("veli@example.com", "123456")
    await sender.send_login_code("veli@example.com", "654321")

    lines = [json.loads(line) for line in path.read_text().splitlines()]
    assert [m["code"] for m in lines] == ["123456", "654321"]
    assert lines[0]["to"] == "veli@example.com" and "at" in lines[0]
    assert stat.S_IMODE(path.stat().st_mode) == 0o600


def test_clock_nowIsTimezoneAwareUtc():
    assert clock.now().utcoffset().total_seconds() == 0


@pytest.mark.parametrize("env", ["staging", "production"])
def test_checkSettings_developmentDefaultsAreRefusedOutsideLocal(env: str):
    with pytest.raises(RuntimeError, match="JWT_SECRET"):
        check_settings(Settings(env=env))
    with pytest.raises(RuntimeError, match="JWT_SECRET"):
        check_settings(Settings(env=env, jwt_secret="short"))  # noqa: S106
    with pytest.raises(RuntimeError, match="outbox"):
        check_settings(Settings(env=env, jwt_secret="x" * 40))


@pytest.mark.parametrize("env", ["local", "test"])
def test_checkSettings_localAndTestAreFine(env: str):
    check_settings(Settings(env=env))
