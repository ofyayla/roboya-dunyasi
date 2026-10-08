import uuid
from typing import Any

from httpx import AsyncClient

from tests.conftest import FakeEmailSender


def device(platform: str = "android") -> dict[str, str]:
    return {"device_id": str(uuid.uuid4()), "platform": platform}


async def sign_in(
    client: AsyncClient,
    mailbox: FakeEmailSender,
    email: str = "veli@example.com",
    dev: dict[str, str] | None = None,
) -> dict[str, Any]:
    """Runs the whole code flow and returns the token response body."""
    r = await client.post("/v1/auth/code", json={"email": email})
    assert r.status_code == 202
    r = await client.post(
        "/v1/auth/verify",
        json={"email": email, "code": mailbox.last_code, "device": dev or device()},
    )
    assert r.status_code == 200, r.text
    body: dict[str, Any] = r.json()
    return body


def bearer(tokens: dict[str, Any]) -> dict[str, str]:
    return {"Authorization": "Bearer " + tokens["access_token"]}
