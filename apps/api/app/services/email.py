import asyncio
import json
import os
from pathlib import Path
from typing import Protocol

from app.core import clock


class EmailSender(Protocol):
    async def send_login_code(self, email: str, code: str) -> None: ...


class OutboxEmailSender:
    """Development sender: appends the message to a local file. Nothing leaves the machine.

    The address and code go to the file only, never to logs (CLAUDE.md §6). A real provider
    needs the foreign-processor process in CLAUDE.md §11 first, so this is the only backend.
    """

    def __init__(self, path: str) -> None:
        self._path = Path(path)

    async def send_login_code(self, email: str, code: str) -> None:
        line = json.dumps({"to": email, "code": code, "at": clock.now().isoformat()})
        await asyncio.to_thread(self._append, line)

    def _append(self, line: str) -> None:
        self._path.parent.mkdir(parents=True, exist_ok=True)
        fd = os.open(self._path, os.O_WRONLY | os.O_APPEND | os.O_CREAT, 0o600)
        with os.fdopen(fd, "a", encoding="utf-8") as f:
            f.write(line + "\n")
