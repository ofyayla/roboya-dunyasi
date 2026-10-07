import json
import logging

from app.core.logging import JsonFormatter


def test_json_formatter_extra_fields_included() -> None:
    record = logging.LogRecord("t", logging.INFO, __file__, 1, "evt", (), None)
    record.profile_id = "anon-123"

    out = json.loads(JsonFormatter().format(record))

    assert out == {"level": "INFO", "logger": "t", "msg": "evt", "profile_id": "anon-123"}
