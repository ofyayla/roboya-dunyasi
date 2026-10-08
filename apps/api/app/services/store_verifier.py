"""Verifying store purchases and notifications (GLR-03, CLAUDE.md §8).

The server never trusts the app or a webhook body: a purchase proof and a store notification are
accepted only when their signature verifies. `StoreVerifier` is the seam; the App Store (JWS chain
to Apple's root, App Store Server API) and Google Play (Developer API, RTDN) adapters need store
credentials and arrive with the store accounts. `SignedDevVerifier` accepts ES256 tokens signed with
a configured key so every flow is testable end to end without a store.
"""

from dataclasses import dataclass
from datetime import UTC, datetime
from enum import StrEnum
from typing import Any, Protocol

import jwt

STORES = ("apple", "google")


class InvalidStoreSignatureError(Exception):
    """The proof or notification is unsigned, tampered with, malformed or for another store."""


class EventKind(StrEnum):
    SUBSCRIBED = "subscribed"
    RENEWED = "renewed"
    GRACE = "grace"
    EXPIRED = "expired"
    REFUNDED = "refunded"


@dataclass(frozen=True)
class VerifiedPurchase:
    store: str
    original_transaction_id: str
    product_id: str
    expires_at: datetime
    is_trial: bool


@dataclass(frozen=True)
class StoreEvent:
    event_id: str
    kind: EventKind
    occurred_at: datetime
    purchase: VerifiedPurchase


class StoreVerifier(Protocol):
    def verify_purchase(self, store: str, proof: str) -> VerifiedPurchase: ...

    def verify_notification(self, store: str, signed_payload: str) -> StoreEvent: ...


class SignedDevVerifier:
    """Accepts ES256 tokens signed with the configured development key."""

    def __init__(self, public_key_pem: str) -> None:
        self._key = public_key_pem

    def verify_purchase(self, store: str, proof: str) -> VerifiedPurchase:
        claims = self._decode(proof, store, "purchase")
        return self._purchase(store, claims)

    def verify_notification(self, store: str, signed_payload: str) -> StoreEvent:
        claims = self._decode(signed_payload, store, "notification")
        try:
            return StoreEvent(
                event_id=str(claims["eid"]),
                kind=EventKind(claims["kind"]),
                occurred_at=datetime.fromtimestamp(int(claims["at"]), tz=UTC),
                purchase=self._purchase(store, claims),
            )
        except (KeyError, ValueError, TypeError) as e:
            raise InvalidStoreSignatureError("malformed notification") from e

    def _decode(self, token: str, store: str, kind: str) -> dict[str, Any]:
        if store not in STORES or not self._key:
            raise InvalidStoreSignatureError("unknown store or no key configured")
        try:
            claims: dict[str, Any] = jwt.decode(
                token,
                self._key,
                algorithms=["ES256"],
                options={"require": ["typ", "store"], "verify_exp": False, "verify_iat": False},
            )
        except jwt.PyJWTError as e:
            raise InvalidStoreSignatureError("bad signature") from e
        if claims.get("typ") != kind or claims.get("store") != store:
            raise InvalidStoreSignatureError("wrong kind or store")
        return claims

    @staticmethod
    def _purchase(store: str, claims: dict[str, Any]) -> VerifiedPurchase:
        try:
            return VerifiedPurchase(
                store=store,
                original_transaction_id=str(claims["otid"])[:128],
                product_id=str(claims["pid"])[:128],
                expires_at=datetime.fromtimestamp(int(claims["expires"]), tz=UTC),
                is_trial=bool(claims.get("trial", False)),
            )
        except (KeyError, ValueError, TypeError, OverflowError, OSError) as e:
            raise InvalidStoreSignatureError("malformed purchase") from e
