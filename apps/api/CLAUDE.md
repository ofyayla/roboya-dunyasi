# apps/api — FastAPI sunucusu

Kök `CLAUDE.md` §8 geçerlidir.

- Paket yöneticisi `uv`; Python 3.12. `uv run pytest`, `uv run ruff check .`, `uv run mypy`.
- Yapı: `app/routers` → `app/services` → `app/repositories`; `app/models` (SQLAlchemy), `app/schemas` (Pydantic).
- Her yeni uç nokta: kimlik doğrulama bağımlılığı, `response_model`, kapsam filtresi testi, `/v1` öneki, ardından `make gen`.
- Hak hesaplama yalnız `app/services/entitlements.py`; kapsam ≥ %95.
- Testler gerçek PostgreSQL ile çalışır (`infra/docker-compose.yml` veya CI servisi). `ROBOYA_DATABASE_URL` test veritabanını gösterir.
- Loglara e-posta, takma ad, IP yazılmaz. Bu yüzden uvicorn erişim günlüğü kapalı çalışır (`--no-access-log`).
- Veli hesabı: e-posta ile tek kullanımlık kod, kısa ömürlü JWT ve dönen yenileme belirteci (ADR 0011). Geliştirmede e-posta `tmp/outbox.jsonl` dosyasına yazılır; gerçek sağlayıcı CLAUDE.md §11 sürecinden geçmeden bağlanmaz.
- Hak hesabı ve mağaza: premium yalnız doğrulanmış mağaza aboneliğinden gelir (ADR 0013); `SignedDevVerifier` yalnız geliştirme ve testlerde, staging/üretimde başlatma reddedilir. Testlerde `store_signer` fixture'ı mağazayı oynar.
- Analitik olayları: `POST /v1/events`, anonim kimlik, sabit olay ve özellik listesi (ADR 0014). `events` tablosu aylık bölümlüdür; `python -m app.maintenance` zamanlanmış çalışır.
- Rıza ve silme: çocuk verisi yazımı rıza olmadan 403; aydınlatma metni `content/legal` dosyasındadır ve `status: final` olmadan üretim başlamaz; silme talebi 7 gün bekler, `python -m app.maintenance` siler (ADR 0015).
- Testler `tests/conftest.py` ile göçleri gerçek veritabanında aşağı ve yukarı çalıştırır; sahte e-posta ve sahte saat fixture'ları vardır.
