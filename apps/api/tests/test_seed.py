import pytest

from app import seed as seed_module
from app.core.config import Settings


async def test_seed_production_env_raises(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(seed_module, "get_settings", lambda: Settings(env="production"))

    with pytest.raises(RuntimeError):
        await seed_module.seed()


async def test_seed_test_env_completes() -> None:
    await seed_module.seed()
