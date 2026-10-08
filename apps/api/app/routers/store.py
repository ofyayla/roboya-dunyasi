from typing import Any

from fastapi import APIRouter, Response, status

from app.core import clock
from app.routers.deps import AccountDep, SessionDep, SettingsDep, StoreVerifierDep
from app.schemas.auth import ErrorOut
from app.schemas.store import EntitlementOut, NotificationIn, ReceiptIn, Store
from app.services import store

router = APIRouter(prefix="/v1/store", tags=["store"])

_receipt_errors: dict[int | str, dict[str, Any]] = {
    401: {"model": ErrorOut},
    409: {"model": ErrorOut},
}


@router.post(
    "/receipts",
    response_model=EntitlementOut,
    responses=_receipt_errors,
    operation_id="linkPurchase",
)
async def link_purchase(
    body: ReceiptIn,
    account: AccountDep,
    session: SessionDep,
    settings: SettingsDep,
    verifier: StoreVerifierDep,
) -> EntitlementOut:
    result = await store.link_purchase(
        session, account, body.store, body.proof, verifier, settings, clock.now()
    )
    return EntitlementOut(**result.__dict__)


# Public by design: the store calls it. Authenticity comes from the signature, not the caller.
@router.post(
    "/notifications/{store_name}",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses={401: {"model": ErrorOut}},
    operation_id="receiveStoreNotification",
)
async def receive_notification(
    store_name: Store, body: NotificationIn, session: SessionDep, verifier: StoreVerifierDep
) -> Response:
    await store.handle_notification(session, store_name, body.signed_payload, verifier, clock.now())
    return Response(status_code=status.HTTP_204_NO_CONTENT)
