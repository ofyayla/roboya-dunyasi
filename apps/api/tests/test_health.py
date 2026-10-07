from collections.abc import AsyncIterator

from httpx import ASGITransport, AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from app.core.db import get_session
from app.main import create_app


async def test_health_database_up_returns_ok(client: AsyncClient) -> None:
    response = await client.get("/v1/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok", "database": "ok", "version": "0.1.0"}


async def test_health_database_down_returns_degraded() -> None:
    # Port 1 is never listening; simulates an unreachable database.
    engine = create_async_engine("postgresql+asyncpg://x:x@127.0.0.1:1/x")
    maker = async_sessionmaker(engine)

    async def broken_session() -> AsyncIterator[AsyncSession]:
        async with maker() as session:
            yield session

    app = create_app()
    app.dependency_overrides[get_session] = broken_session
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as c:
        response = await c.get("/v1/health")
    await engine.dispose()

    assert response.status_code == 200
    assert response.json()["status"] == "degraded"
    assert response.json()["database"] == "unavailable"


async def test_openapi_health_route_is_versioned(client: AsyncClient) -> None:
    spec = (await client.get("/openapi.json")).json()

    assert "/v1/health" in spec["paths"]
