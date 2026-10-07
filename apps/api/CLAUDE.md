# apps/api — FastAPI sunucusu

Kök `CLAUDE.md` §8 geçerlidir.

- Paket yöneticisi `uv`; Python 3.12. `uv run pytest`, `uv run ruff check .`, `uv run mypy`.
- Yapı: `app/routers` → `app/services` → `app/repositories`; `app/models` (SQLAlchemy), `app/schemas` (Pydantic).
- Her yeni uç nokta: kimlik doğrulama bağımlılığı, `response_model`, kapsam filtresi testi, `/v1` öneki, ardından `make gen`.
- Hak hesaplama yalnız `app/services/entitlements.py`; kapsam ≥ %95.
- Testler gerçek PostgreSQL ile çalışır (`infra/docker-compose.yml` veya CI servisi). `ROBOYA_DATABASE_URL` test veritabanını gösterir.
- Loglara e-posta, takma ad, IP yazılmaz.
