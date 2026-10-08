import re
import uuid
from datetime import datetime
from typing import Annotated, Literal

from pydantic import BaseModel, ConfigDict, Field

# PRD "Temel olaylar". Purchase facts (trial, purchase, renewal) come from the store notifications on
# the server, never from the app, so they are not accepted here.
EventName = Literal[
    "app_open",
    "level_start",
    "level_complete",
    "level_abandon",
    "hint_used",
    "session_limit_reached",
    "paywall_view",
]

_LEVEL_ID = re.compile(r"^[a-z0-9-]+\.[a-z0-9-]+\.[a-z0-9-]+$")


class EventProps(BaseModel):
    """The only properties an event may carry: small numbers and fixed words, nothing free-form."""

    model_config = ConfigDict(extra="forbid")

    attempts: Annotated[int, Field(ge=0, le=1000, strict=True)] | None = None
    duration_s: Annotated[int, Field(ge=0, le=86_400, strict=True)] | None = None
    stars: Annotated[int, Field(ge=0, le=3, strict=True)] | None = None
    hints: Annotated[int, Field(ge=0, le=50, strict=True)] | None = None
    hint_tier: Annotated[int, Field(ge=0, le=3, strict=True)] | None = None
    code_length: Annotated[int, Field(ge=0, le=64, strict=True)] | None = None
    last_step: Annotated[int, Field(ge=0, le=64, strict=True)] | None = None
    limit_minutes: Annotated[int, Field(ge=0, le=720, strict=True)] | None = None
    profile_kind: Literal["family", "school"] | None = None
    device_class: Literal["phone", "tablet"] | None = None
    level_band: Literal["minik", "kasif", "mucit"] | None = None
    source: Literal["map", "path", "level_end", "parent"] | None = None


class EventIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    # Made by the app; a retried event keeps its id and occurred_at, so it is stored once.
    id: uuid.UUID
    name: EventName
    level_id: Annotated[str, Field(max_length=80, pattern=_LEVEL_ID.pattern)] | None = None
    occurred_at: datetime
    props: EventProps = Field(default_factory=EventProps)


class EventBatchIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    # A random id per child profile on the device; never a nickname, email or advertising id.
    anon_id: uuid.UUID
    app_version: Annotated[str, Field(pattern=r"^[0-9A-Za-z.\-+]{1,16}$")]
    platform: Literal["ios", "android"]
    events: Annotated[list[EventIn], Field(min_length=1, max_length=100)]


class EventBatchOut(BaseModel):
    accepted: int
    dropped: int
