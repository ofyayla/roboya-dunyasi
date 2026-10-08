from typing import Annotated

from fastapi import Depends
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.ext.asyncio import AsyncSession

from app.core import clock
from app.core.config import Settings, get_settings
from app.core.db import get_session
from app.models.account import Account
from app.services import auth
from app.services.email import EmailSender, OutboxEmailSender
from app.services.errors import InvalidTokenApiError

_bearer = HTTPBearer(auto_error=False)

SessionDep = Annotated[AsyncSession, Depends(get_session)]
SettingsDep = Annotated[Settings, Depends(get_settings)]


def get_email_sender(settings: SettingsDep) -> EmailSender:
    return OutboxEmailSender(settings.outbox_path)


EmailDep = Annotated[EmailSender, Depends(get_email_sender)]


async def get_current_account(
    session: SessionDep,
    settings: SettingsDep,
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(_bearer)],
) -> Account:
    """Authentication dependency for every account endpoint."""
    if credentials is None:
        raise InvalidTokenApiError
    return await auth.current_account(session, credentials.credentials, settings, clock.now())


AccountDep = Annotated[Account, Depends(get_current_account)]
