from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

from app.core.config import DEFAULT_SECRET, Settings, get_settings
from app.core.logging import configure_logging
from app.routers import auth, events, health, me, privacy, profiles, store
from app.services import legal
from app.services.errors import ApiError
from app.services.health import API_VERSION


def check_settings(settings: Settings) -> None:
    """Refuses to start with development defaults outside local and test."""
    if settings.env in ("staging", "production"):
        if settings.jwt_secret == DEFAULT_SECRET or len(settings.jwt_secret) < 32:
            raise RuntimeError("ROBOYA_JWT_SECRET must be a long random value outside local/test")
        if settings.env == "production" and legal.current_notice(settings).status != "final":
            raise RuntimeError("The privacy notice is still a draft: get the lawyer's approval")
        if settings.email_backend == "outbox":
            raise RuntimeError("The outbox email backend is for development only")
        if settings.store_backend == "signed-dev":
            raise RuntimeError("The signed-dev store backend is for development only")


async def _api_error(_: Request, exc: Exception) -> JSONResponse:
    assert isinstance(exc, ApiError)
    return JSONResponse(status_code=exc.status_code, content={"code": exc.code})


def create_app() -> FastAPI:
    settings = get_settings()
    check_settings(settings)
    configure_logging(settings.log_level)
    app = FastAPI(
        title="Roboya Dünyası API",
        version=API_VERSION,
        # Interactive docs only outside production.
        docs_url=None if settings.env == "production" else "/docs",
        redoc_url=None,
    )
    app.add_exception_handler(ApiError, _api_error)
    app.include_router(health.router)
    app.include_router(auth.router)
    app.include_router(me.router)
    app.include_router(profiles.router)
    app.include_router(store.router)
    app.include_router(events.router)
    app.include_router(privacy.router)
    app.include_router(privacy.legal_router)
    return app


app = create_app()
