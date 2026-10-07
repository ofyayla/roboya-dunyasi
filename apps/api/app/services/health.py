import logging

from sqlalchemy.exc import SQLAlchemyError
from sqlalchemy.ext.asyncio import AsyncSession

from app.repositories.health import ping_database
from app.schemas.health import HealthOut

logger = logging.getLogger(__name__)

API_VERSION = "0.1.0"


async def check_health(session: AsyncSession) -> HealthOut:
    try:
        await ping_database(session)
    except (SQLAlchemyError, OSError):
        # Expected when the database is down; reported as degraded, not raised.
        logger.warning("health.database_unavailable", exc_info=True)
        return HealthOut(status="degraded", database="unavailable", version=API_VERSION)
    return HealthOut(status="ok", database="ok", version=API_VERSION)
