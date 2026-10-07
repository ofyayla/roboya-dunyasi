import os

# Tests always use the dedicated test database (ADR 0001); set before app import.
os.environ.setdefault(
    "ROBOYA_DATABASE_URL", "postgresql+asyncpg://roboya:roboya@localhost:5432/roboya_test"
)
os.environ["ROBOYA_ENV"] = "test"

from collections.abc import AsyncIterator

import pytest
from httpx import ASGITransport, AsyncClient

from app.main import create_app


@pytest.fixture
async def client() -> AsyncIterator[AsyncClient]:
    app = create_app()
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as c:
        yield c
