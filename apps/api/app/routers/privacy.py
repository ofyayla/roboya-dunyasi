import json
from typing import Any

from fastapi import APIRouter, Response, status

from app.core import clock
from app.repositories import privacy as privacy_repo
from app.routers.deps import AccountDep, SessionDep, SettingsDep
from app.schemas.auth import ErrorOut
from app.schemas.privacy import (
    ConsentIn,
    ConsentOut,
    ConsentStatusOut,
    DeletionRequestOut,
    MyDataOut,
    NoticeOut,
)
from app.services import legal, privacy

_auth: dict[int | str, dict[str, Any]] = {401: {"model": ErrorOut}}

# Public by design: a parent reads the notice before consenting, and before having an account.
legal_router = APIRouter(prefix="/v1/legal", tags=["privacy"])
router = APIRouter(prefix="/v1/me", tags=["privacy"])


@legal_router.get("/notice", response_model=NoticeOut, operation_id="getPrivacyNotice")
async def get_notice(settings: SettingsDep) -> NoticeOut:
    notice = legal.current_notice(settings)
    return NoticeOut(version=notice.version, status=notice.status, text=notice.text)


@router.get(
    "/consents", response_model=ConsentStatusOut, responses=_auth, operation_id="getConsents"
)
async def get_consents(
    account: AccountDep, session: SessionDep, settings: SettingsDep
) -> ConsentStatusOut:
    notice = legal.current_notice(settings)
    records = await privacy_repo.consents_of(session, account.id)
    return ConsentStatusOut(
        current_version=notice.version,
        consented=await privacy.has_consent(session, account, settings),
        records=[ConsentOut.model_validate(r) for r in records],
    )


@router.post(
    "/consents",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses={**_auth, 409: {"model": ErrorOut}},
    operation_id="giveConsent",
)
async def give_consent(
    body: ConsentIn, account: AccountDep, session: SessionDep, settings: SettingsDep
) -> Response:
    await privacy.give_consent(session, account, body.notice_version, settings, clock.now())
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.delete(
    "/consents",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses=_auth,
    operation_id="withdrawConsent",
)
async def withdraw_consent(account: AccountDep, session: SessionDep) -> Response:
    await privacy.withdraw_consent(session, account, clock.now())
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.get("/data", response_model=MyDataOut, responses=_auth, operation_id="getMyData")
async def get_my_data(account: AccountDep, session: SessionDep) -> MyDataOut:
    return MyDataOut(**await privacy.my_data(session, account))


@router.get(
    "/data/export",
    responses={**_auth, 200: {"content": {"application/json": {}}}},
    response_class=Response,
    operation_id="exportMyData",
)
async def export_my_data(account: AccountDep, session: SessionDep) -> Response:
    payload = json.dumps(await privacy.my_data(session, account), ensure_ascii=False, indent=2)
    return Response(
        content=payload,
        media_type="application/json",
        headers={"Content-Disposition": 'attachment; filename="roboya-verilerim.json"'},
    )


@router.post(
    "/deletion-request",
    status_code=status.HTTP_202_ACCEPTED,
    response_model=DeletionRequestOut,
    responses=_auth,
    operation_id="requestDeletion",
)
async def request_deletion(
    account: AccountDep, session: SessionDep, settings: SettingsDep
) -> DeletionRequestOut:
    request = await privacy.request_deletion(session, account, settings, clock.now())
    return DeletionRequestOut.model_validate(request)


@router.get(
    "/deletion-request",
    response_model=DeletionRequestOut,
    responses={**_auth, 404: {"model": ErrorOut}},
    operation_id="getDeletionRequest",
)
async def get_deletion_request(account: AccountDep, session: SessionDep) -> DeletionRequestOut:
    return DeletionRequestOut.model_validate(await privacy.deletion_status(session, account))


@router.delete(
    "/deletion-request",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses={**_auth, 404: {"model": ErrorOut}},
    operation_id="cancelDeletion",
)
async def cancel_deletion(account: AccountDep, session: SessionDep) -> Response:
    await privacy.cancel_deletion(session, account)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
