from datetime import datetime
from typing import Annotated, Literal

from pydantic import BaseModel, ConfigDict, Field

Store = Literal["apple", "google"]


class ReceiptIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    store: Store
    # Apple: the signed transaction (JWS). Google: the purchase token. Checked on the server only.
    proof: Annotated[str, Field(min_length=10, max_length=8000)]


class NotificationIn(BaseModel):
    model_config = ConfigDict(extra="forbid")

    signed_payload: Annotated[str, Field(min_length=10, max_length=8000)]


class EntitlementOut(BaseModel):
    """What the app may cache until `cache_until` and show locks from; it never decides itself."""

    tier: Literal["free", "premium"]
    source: Literal["none", "store"]
    status: Literal["free", "trial", "active", "grace"]
    expires_at: datetime | None
    cache_until: datetime
