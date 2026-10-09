"""Optional error reporting to a self-hosted Sentry-compatible server (F1-19, ADR 0028).

Off unless ``ROBOYA_ERROR_DSN`` is set. Only the exception type, the stack frames (no local
variables) and the release go out: request data, users, cookies, headers, breadcrumbs and
exception messages are removed, because messages can carry an e-mail address or a nickname
(CLAUDE.md §6, §11).
"""

from typing import Any

import sentry_sdk
from sentry_sdk.integrations.fastapi import FastApiIntegration
from sentry_sdk.integrations.starlette import StarletteIntegration

from app.core.config import Settings
from app.services.health import API_VERSION

# Event fields that are dropped entirely.
_DROP = (
    "request",
    "user",
    "server_name",
    "extra",
    "contexts",
    "breadcrumbs",
    "modules",
    "tags",
    "logentry",
)


def scrub_event(event: dict[str, Any], hint: dict[str, Any] | None = None) -> dict[str, Any] | None:
    """Keeps the exception type and stack frames; removes whatever could identify a person."""
    for key in _DROP:
        event.pop(key, None)
    for exc in (event.get("exception") or {}).get("values", []):
        # The message may contain personal data; the type and the traceback are enough to fix a bug.
        exc["value"] = exc.get("type", "error")
        for frame in (exc.get("stacktrace") or {}).get("frames", []):
            frame.pop("vars", None)
            frame.pop("pre_context", None)
            frame.pop("context_line", None)
            frame.pop("post_context", None)
    event.pop("message", None)
    return event


def init_error_tracking(settings: Settings) -> bool:
    """Starts reporting when a DSN is configured; returns whether it did."""
    if not settings.error_dsn:
        return False
    sentry_sdk.init(
        dsn=settings.error_dsn,
        environment=settings.env,
        release=API_VERSION,
        send_default_pii=False,
        include_local_variables=False,
        max_request_body_size="never",
        traces_sample_rate=0.0,
        before_send=scrub_event,  # type: ignore[arg-type]
        integrations=[StarletteIntegration(), FastApiIntegration()],
    )
    return True
