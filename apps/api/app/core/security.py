"""Hashing, code generation and JWT helpers for parent accounts (F1-14)."""

import hashlib
import hmac
import secrets
import uuid
from datetime import datetime, timedelta
from typing import Any

import jwt

from app.core.config import Settings

ALGORITHM = "HS256"
ACCESS = "access"


class InvalidTokenError(Exception):
    """The access token is missing, expired, tampered with or of the wrong kind."""


def normalize_email(email: str) -> str:
    return email.strip().lower()


def email_digest(email: str, secret: str) -> str:
    """Keyed hash of the email, so transient tables never hold the address itself."""
    return hmac.new(secret.encode(), normalize_email(email).encode(), hashlib.sha256).hexdigest()


def new_login_code() -> str:
    return f"{secrets.randbelow(1_000_000):06d}"


def code_digest(code_id: uuid.UUID, code: str, secret: str) -> str:
    return hmac.new(secret.encode(), f"{code_id}:{code}".encode(), hashlib.sha256).hexdigest()


def codes_match(expected_digest: str, code_id: uuid.UUID, code: str, secret: str) -> bool:
    return hmac.compare_digest(expected_digest, code_digest(code_id, code, secret))


def new_refresh_token() -> str:
    return secrets.token_urlsafe(48)


def token_digest(token: str) -> str:
    return hashlib.sha256(token.encode()).hexdigest()


def create_access_token(account_id: uuid.UUID, issued_at: datetime, settings: Settings) -> str:
    payload: dict[str, Any] = {
        "sub": str(account_id),
        "typ": ACCESS,
        "iat": issued_at,
        "exp": issued_at + timedelta(minutes=settings.access_token_minutes),
    }
    return jwt.encode(payload, settings.jwt_secret, algorithm=ALGORITHM)


def decode_access_token(token: str, settings: Settings, now: datetime) -> uuid.UUID:
    try:
        claims = jwt.decode(
            token,
            settings.jwt_secret,
            algorithms=[ALGORITHM],
            options={
                "require": ["sub", "exp", "iat", "typ"],
                "verify_exp": False,
                "verify_iat": False,
            },
        )
        if claims["typ"] != ACCESS:
            raise InvalidTokenError("wrong token type")
        # Time is checked here, against our clock seam, instead of PyJWT's wall clock.
        if datetime.fromtimestamp(claims["exp"], tz=now.tzinfo) <= now:
            raise InvalidTokenError("expired")
        return uuid.UUID(claims["sub"])
    except (jwt.PyJWTError, ValueError, KeyError) as e:
        raise InvalidTokenError("invalid token") from e
