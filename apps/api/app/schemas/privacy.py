import uuid
from datetime import datetime
from typing import Annotated, Any, Literal

from pydantic import BaseModel, ConfigDict, Field


class NoticeOut(BaseModel):
    version: str
    status: Literal["draft", "final"]
    text: str


class ConsentIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    notice_version: Annotated[str, Field(min_length=1, max_length=64)]


class ConsentOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    notice_version: str
    accepted_at: datetime
    withdrawn_at: datetime | None


class ConsentStatusOut(BaseModel):
    current_version: str
    consented: bool
    records: list[ConsentOut]


class DeletionRequestOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    requested_at: datetime
    scheduled_for: datetime


class MyDataOut(BaseModel):
    """Everything we hold about this parent account (UYM-03); shown and downloaded as one."""

    account: dict[str, Any]
    consents: list[dict[str, Any]]
    devices: list[dict[str, Any]]
    profiles: list[dict[str, Any]]
    subscriptions: list[dict[str, Any]]
    deletion_request: dict[str, Any] | None
