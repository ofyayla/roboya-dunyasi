import re
import uuid
from datetime import datetime
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field, field_validator

_EMAIL = re.compile(r"^[^@\s]+@[^@\s]+\.[^@\s]+$")


class ErrorOut(BaseModel):
    """Every expected failure has a stable `code` the app can switch on."""

    code: str


class DeviceIn(BaseModel):
    # A random id the app makes once; never an advertising id (CLAUDE.md §1).
    device_id: uuid.UUID
    platform: Literal["ios", "android"]


class CodeRequestIn(BaseModel):
    email: str = Field(max_length=254)

    @field_validator("email")
    @classmethod
    def looks_like_email(cls, v: str) -> str:
        if not _EMAIL.match(v.strip()):
            raise ValueError("invalid email")
        return v


class CodeVerifyIn(CodeRequestIn):
    code: str = Field(pattern=r"^\d{6}$")
    device: DeviceIn


class RefreshIn(BaseModel):
    refresh_token: str = Field(min_length=20, max_length=200)


class TokensOut(BaseModel):
    access_token: str
    refresh_token: str
    expires_in: int
    account_id: uuid.UUID


class MeOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    email: str
    created_at: datetime


class DeviceOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    platform: str
    created_at: datetime
    last_seen_at: datetime
