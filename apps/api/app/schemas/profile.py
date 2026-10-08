import re
import uuid
from datetime import datetime
from typing import Annotated, Literal

from pydantic import BaseModel, ConfigDict, Field, field_validator

_LEVEL_ID = re.compile(r"^[a-z0-9-]+\.[a-z0-9-]+\.[a-z0-9-]+$")

AgeBand = Literal["minik", "kasif", "mucit"]


class ProfileIn(BaseModel):
    # The only child fields we accept (CLAUDE.md §11); a new field needs an ADR and human approval.
    model_config = ConfigDict(extra="forbid")

    nickname: Annotated[str, Field(min_length=1, max_length=24)]
    avatar_id: Annotated[str, Field(pattern=r"^[a-z0-9-]{1,32}$")]
    age_band: AgeBand

    @field_validator("nickname")
    @classmethod
    def not_blank(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("nickname is blank")
        return v.strip()


class ProfileOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    nickname: str
    avatar_id: str
    age_band: str
    created_at: datetime


class ProgressIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    stars: dict[str, Annotated[int, Field(ge=0, le=3, strict=True)]] = Field(max_length=500)

    @field_validator("stars")
    @classmethod
    def level_ids_are_wellformed(cls, v: dict[str, int]) -> dict[str, int]:
        for level_id in v:
            if len(level_id) > 80 or not _LEVEL_ID.match(level_id):
                raise ValueError("bad level id")
        return v


class ProgressOut(BaseModel):
    stars: dict[str, int]
