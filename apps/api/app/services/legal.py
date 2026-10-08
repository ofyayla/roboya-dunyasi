"""The privacy notice a parent consents to (UYM-01). The text lives in content/legal as a file."""

import re
from dataclasses import dataclass
from pathlib import Path
from typing import Literal

from app.core.config import Settings

NOTICE_FILE = "aydinlatma-metni.tr.md"
_VERSION = re.compile(r"<!--\s*version:\s*(\S+)\s*-->")
_STATUS = re.compile(r"<!--\s*status:\s*(\S+)\s*-->")


class NoticeError(Exception):
    """The notice file is missing or has no version marker."""


@dataclass(frozen=True)
class Notice:
    version: str
    status: Literal["draft", "final"]  # "draft" until a lawyer approved it
    text: str


def legal_dir(settings: Settings) -> Path:
    if settings.legal_dir:
        return Path(settings.legal_dir)
    return Path(__file__).resolve().parents[4] / "content" / "legal"


def current_notice(settings: Settings) -> Notice:
    path = legal_dir(settings) / NOTICE_FILE
    try:
        raw = path.read_text(encoding="utf-8")
    except OSError as e:
        raise NoticeError(f"notice file not found: {path.name}") from e
    version = _VERSION.search(raw)
    if version is None:
        raise NoticeError("the notice has no version marker")
    marker = _STATUS.search(raw)
    # Anything other than an explicit "final" counts as a draft, so a typo cannot ship a draft.
    status: Literal["draft", "final"] = (
        "final" if marker and marker.group(1) == "final" else "draft"
    )
    text = _STATUS.sub("", _VERSION.sub("", raw)).strip()
    return Notice(version.group(1), status, text)
