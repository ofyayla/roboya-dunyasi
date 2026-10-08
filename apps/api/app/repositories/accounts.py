import uuid
from datetime import datetime

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.account import Account, DeviceRegistration


async def get_by_id(session: AsyncSession, account_id: uuid.UUID) -> Account | None:
    return await session.get(Account, account_id)


async def get_by_email(session: AsyncSession, email: str) -> Account | None:
    result = await session.execute(select(Account).where(Account.email == email))
    return result.scalar_one_or_none()


async def create(session: AsyncSession, email: str) -> Account:
    account = Account(email=email)
    session.add(account)
    await session.flush()
    return account


async def list_devices(session: AsyncSession, account_id: uuid.UUID) -> list[DeviceRegistration]:
    result = await session.execute(
        select(DeviceRegistration)
        .where(DeviceRegistration.account_id == account_id)
        .order_by(DeviceRegistration.created_at)
    )
    return list(result.scalars())


async def find_device(
    session: AsyncSession, account_id: uuid.UUID, device_id: uuid.UUID
) -> DeviceRegistration | None:
    result = await session.execute(
        select(DeviceRegistration).where(
            DeviceRegistration.account_id == account_id, DeviceRegistration.device_id == device_id
        )
    )
    return result.scalar_one_or_none()


async def get_device(
    session: AsyncSession, account_id: uuid.UUID, registration_id: uuid.UUID
) -> DeviceRegistration | None:
    """Scoped by account: another parent's device is never visible (CLAUDE.md §8)."""
    result = await session.execute(
        select(DeviceRegistration).where(
            DeviceRegistration.account_id == account_id, DeviceRegistration.id == registration_id
        )
    )
    return result.scalar_one_or_none()


async def add_device(
    session: AsyncSession, account_id: uuid.UUID, device_id: uuid.UUID, platform: str, now: datetime
) -> DeviceRegistration:
    device = DeviceRegistration(
        account_id=account_id, device_id=device_id, platform=platform, last_seen_at=now
    )
    session.add(device)
    await session.flush()
    return device


async def delete_device(session: AsyncSession, device: DeviceRegistration) -> None:
    await session.delete(device)
    await session.flush()
