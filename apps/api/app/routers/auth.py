from fastapi import APIRouter, Response, status

from app.core import clock
from app.routers.deps import EmailDep, SessionDep, SettingsDep
from app.schemas.auth import CodeRequestIn, CodeVerifyIn, ErrorOut, RefreshIn, TokensOut
from app.services import auth

# Public by design: these endpoints are how a parent obtains a token.
router = APIRouter(prefix="/v1/auth", tags=["auth"])

_errors = {
    401: {"model": ErrorOut},
    429: {"model": ErrorOut},
    409: {"model": ErrorOut},
}


@router.post(
    "/code",
    status_code=status.HTTP_202_ACCEPTED,
    response_class=Response,
    responses={429: _errors[429]},
    operation_id="requestLoginCode",
)
async def request_code(
    body: CodeRequestIn, session: SessionDep, settings: SettingsDep, sender: EmailDep
) -> Response:
    await auth.request_code(session, body.email, sender, settings, clock.now())
    return Response(status_code=status.HTTP_202_ACCEPTED)


@router.post(
    "/verify",
    response_model=TokensOut,
    responses={401: _errors[401], 409: _errors[409]},
    operation_id="verifyLoginCode",
)
async def verify_code(body: CodeVerifyIn, session: SessionDep, settings: SettingsDep) -> TokensOut:
    pair = await auth.verify_code(
        session,
        body.email,
        body.code,
        body.device.device_id,
        body.device.platform,
        settings,
        clock.now(),
    )
    return TokensOut(**pair.__dict__)


@router.post(
    "/refresh",
    response_model=TokensOut,
    responses={401: _errors[401]},
    operation_id="refreshTokens",
)
async def refresh(body: RefreshIn, session: SessionDep, settings: SettingsDep) -> TokensOut:
    pair = await auth.refresh(session, body.refresh_token, settings, clock.now())
    return TokensOut(**pair.__dict__)


@router.post(
    "/logout",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    operation_id="logout",
)
async def logout(body: RefreshIn, session: SessionDep) -> Response:
    await auth.logout(session, body.refresh_token, clock.now())
    return Response(status_code=status.HTTP_204_NO_CONTENT)
