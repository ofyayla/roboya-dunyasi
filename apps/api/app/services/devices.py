import uuid
from datetime import datetime

from sqlalchemy.ext.asyncio import AsyncSession

from app.models.account import Account, DeviceRegistration
from app.repositories import accounts, auth
from app.services.errors import NotFoundError


async def list_devices(session: AsyncSession, account: Account) -> list[DeviceRegistration]:
    return await accounts.list_devices(session, account.id)


async def remove_device(
    session: AsyncSession, account: Account, registration_id: uuid.UUID, now: datetime
) -> None:
    """Signs a device out. Another parent's device id answers 404, the same as a missing one."""
    device = await accounts.get_device(session, account.id, registration_id)
    if device is None:
        raise NotFoundError
    await auth.revoke_device_tokens(session, device.id, now)
    await accounts.delete_device(session, device)
    await session.commit()
