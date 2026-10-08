from fastapi import APIRouter, status

from app.core import clock
from app.routers.deps import SessionDep
from app.schemas.events import EventBatchIn, EventBatchOut
from app.services import events

# Public by design: it works without an account (free play) and carries only an anonymous random
# profile id and allow-listed numbers. Abuse limits (rate, size) are also applied at the gateway.
router = APIRouter(prefix="/v1/events", tags=["analytics"])


@router.post(
    "",
    status_code=status.HTTP_202_ACCEPTED,
    response_model=EventBatchOut,
    operation_id="sendEvents",
)
async def send_events(batch: EventBatchIn, session: SessionDep) -> EventBatchOut:
    accepted, dropped = await events.ingest(session, batch, clock.now())
    return EventBatchOut(accepted=accepted, dropped=dropped)
