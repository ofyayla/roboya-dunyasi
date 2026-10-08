from datetime import UTC, datetime


def now() -> datetime:
    """Current UTC time. A single seam so tests can move the clock."""
    return datetime.now(UTC)
