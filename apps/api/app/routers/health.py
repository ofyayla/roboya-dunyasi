from typing import Annotated

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.db import get_session
from app.schemas.health import HealthOut
from app.services.health import check_health

# Public by design: used by load balancers and uptime checks; returns no user data.
router = APIRouter(prefix="/v1", tags=["system"])


@router.get("/health", response_model=HealthOut, operation_id="getHealth")
async def health(session: Annotated[AsyncSession, Depends(get_session)]) -> HealthOut:
    return await check_health(session)
