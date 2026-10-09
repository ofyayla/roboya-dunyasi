import sentry_sdk

from app.core.config import Settings
from app.core.error_tracking import init_error_tracking, scrub_event


def _event() -> dict:
    return {
        "message": "failed for ayse@example.com",
        "request": {
            "url": "http://x/v1/auth/code",
            "data": {"email": "ayse@example.com"},
            "headers": {"authorization": "Bearer x"},
        },
        "user": {"ip_address": "1.2.3.4", "email": "ayse@example.com"},
        "server_name": "host",
        "breadcrumbs": [{"message": "q"}],
        "extra": {"nickname": "Elif"},
        "exception": {
            "values": [
                {
                    "type": "ValueError",
                    "value": "bad nickname Elif",
                    "stacktrace": {
                        "frames": [
                            {
                                "filename": "app/x.py",
                                "lineno": 3,
                                "function": "f",
                                "vars": {"email": "ayse@example.com"},
                                "context_line": "email = 'ayse@example.com'",
                                "pre_context": ["a"],
                                "post_context": ["b"],
                            }
                        ]
                    },
                }
            ]
        },
    }


def test_scrub_event_removes_everything_that_could_identify_a_person() -> None:
    out = scrub_event(_event())

    assert out is not None
    for key in ("request", "user", "server_name", "breadcrumbs", "extra", "message"):
        assert key not in out
    value = out["exception"]["values"][0]
    assert value["value"] == "ValueError"
    frame = value["stacktrace"]["frames"][0]
    assert frame["filename"] == "app/x.py" and frame["function"] == "f"
    assert all(k not in frame for k in ("vars", "context_line", "pre_context", "post_context"))
    assert "ayse" not in str(out) and "Elif" not in str(out)


def test_init_without_dsn_does_nothing() -> None:
    assert init_error_tracking(Settings(error_dsn="")) is False


def test_init_with_dsn_starts_the_client_without_pii() -> None:
    try:
        assert init_error_tracking(Settings(error_dsn="http://key@localhost:9/1")) is True
        options = sentry_sdk.get_client().options
        assert options["send_default_pii"] is False
        assert options["include_local_variables"] is False
        assert options["max_request_body_size"] == "never"
    finally:
        sentry_sdk.get_client().close()
