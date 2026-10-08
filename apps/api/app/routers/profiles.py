import uuid
from typing import Any

from fastapi import APIRouter, Response, status

from app.core import clock
from app.routers.deps import AccountDep, SessionDep, SettingsDep
from app.schemas.auth import ErrorOut
from app.schemas.profile import ProfileIn, ProfileOut, ProgressIn, ProgressOut
from app.services import profiles

router = APIRouter(prefix="/v1/me/profiles", tags=["profiles"])

_auth: dict[int | str, dict[str, Any]] = {401: {"model": ErrorOut}}
_scoped: dict[int | str, dict[str, Any]] = {**_auth, 404: {"model": ErrorOut}}


@router.get("", response_model=list[ProfileOut], responses=_auth, operation_id="listProfiles")
async def list_profiles(account: AccountDep, session: SessionDep) -> list[ProfileOut]:
    return [ProfileOut.model_validate(p) for p in await profiles.list_profiles(session, account)]


@router.put(
    "/{profile_id}",
    response_model=ProfileOut,
    responses={**_scoped, 409: {"model": ErrorOut}},
    operation_id="saveProfile",
)
async def save_profile(
    profile_id: uuid.UUID,
    body: ProfileIn,
    account: AccountDep,
    session: SessionDep,
    settings: SettingsDep,
) -> ProfileOut:
    profile = await profiles.save_profile(
        session,
        account,
        profile_id,
        profiles.ProfileFields(body.nickname, body.avatar_id, body.age_band),
        settings,
        clock.now(),
    )
    return ProfileOut.model_validate(profile)


@router.delete(
    "/{profile_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    response_class=Response,
    responses=_scoped,
    operation_id="deleteProfile",
)
async def delete_profile(
    profile_id: uuid.UUID, account: AccountDep, session: SessionDep
) -> Response:
    await profiles.delete_profile(session, account, profile_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.get(
    "/{profile_id}/progress",
    response_model=ProgressOut,
    responses=_scoped,
    operation_id="getProgress",
)
async def get_progress(
    profile_id: uuid.UUID, account: AccountDep, session: SessionDep
) -> ProgressOut:
    return ProgressOut(stars=await profiles.get_progress(session, account, profile_id))


@router.post(
    "/{profile_id}/progress/sync",
    response_model=ProgressOut,
    responses=_scoped,
    operation_id="syncProgress",
)
async def sync_progress(
    profile_id: uuid.UUID, body: ProgressIn, account: AccountDep, session: SessionDep
) -> ProgressOut:
    merged = await profiles.sync_progress(session, account, profile_id, body.stars, clock.now())
    return ProgressOut(stars=merged)
