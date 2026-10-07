Yeni bir API uç noktası ekle: $ARGUMENTS

1. `apps/api/CLAUDE.md` ve ilgili PRD gereksinimini oku. Kimliği yoksa dur ve sor.
2. Katmanlar: schema (Pydantic) → repository → service → router (`/v1`). Router'da veritabanı erişimi yok.
3. Kimlik doğrulama bağımlılığı, açık `response_model`, tipli hata kodları.
4. Testler: mutlu yol, yetkisiz erişim, başka kullanıcının/sınıfın verisine erişim (kapsam filtresi).
5. `make gen`, `make test-api`, `make lint-api` çalıştır ve sonucu raporla.
