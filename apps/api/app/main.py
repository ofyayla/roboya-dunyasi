from fastapi import FastAPI

from app.core.config import get_settings
from app.core.logging import configure_logging
from app.routers import health
from app.services.health import API_VERSION


def create_app() -> FastAPI:
    settings = get_settings()
    configure_logging(settings.log_level)
    app = FastAPI(
        title="Roboya Dünyası API",
        version=API_VERSION,
        # Interactive docs only outside production.
        docs_url=None if settings.env == "production" else "/docs",
        redoc_url=None,
    )
    app.include_router(health.router)
    return app


app = create_app()
