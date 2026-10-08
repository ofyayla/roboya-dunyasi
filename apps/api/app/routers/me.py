import uuid
from typing import Any

from fastapi import APIRouter, Response, status

from app.core import clock
from app.routers.deps import AccountDep, SessionDep, SettingsDep
from app.schemas.auth import DeviceOut, ErrorOut, MeOut
from app.schemas.store import EntitlementOut
from app.services import devices, entitlements

router = APIRouter(prefix="/v1/me", tags=["account"])

_unauthorized: dict[int | str, dict[str, Any]] = {401: {"model": ErrorOut}}


@router.get("", response_model=MeOut, responses=_unauthorized, operation_id="getMe")
async def get_me(account: AccountDep) -> MeOut:
    return MeOut.model_validate(account)


@router.get(
    "/devices", response_model=list[DeviceOut], responses=_unauthorized, operation_id="listDevices"
)
async def list_devices(account: AccountDep, session: SessionDep) -> list[DeviceOut]:
    return [DeviceOut.model_validate(d) for d in await devices.list_devices(session, account)]


@router.delete(
    "/devices/{registration_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses={401: {"model": ErrorOut}, 404: {"model": ErrorOut}},
    operation_id="removeDevice",
)
async def remove_device(
    registration_id: uuid.UUID, account: AccountDep, session: SessionDep
) -> Response:
    await devices.remove_device(session, account, registration_id, clock.now())
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.get(
    "/entitlement",
    response_model=EntitlementOut,
    responses=_unauthorized,
    operation_id="getEntitlement",
)
async def get_entitlement(
    account: AccountDep, session: SessionDep, settings: SettingsDep
) -> EntitlementOut:
    result = await entitlements.compute(session, account, settings, clock.now())
    return EntitlementOut(**result.__dict__)
