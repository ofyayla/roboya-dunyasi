# CLAUDE.md — Roboya Dünyası

3.5–10 yaş çocuklara hikâye içinde kodlama ve değerler öğreten, Türkçe seslendirmeli mobil oyun (iOS + Android). Bileşenler: Unity istemcisi (`apps/game`), FastAPI (`apps/api`), React panelleri (`apps/web`), bölüm editörü (`apps/level-editor`). Çalıştığın katmanın kendi `CLAUDE.md` dosyasına da bak.

Ayrıntılı kurallar (Unity, API, web, içerik, ödeme, çocuk deneyimi, Git/PR, tamamlanma tanımı): [docs/gelistirme-kurallari.md](docs/gelistirme-kurallari.md). Yalnız ilgili bölümü, gerektiğinde oku. Ürün için `docs/prd.md`, `docs/plan.md`, `docs/adr/`.

## Altın kurallar (esnemez)

1. **Çocuk verisi en aza indirilir.** Çocuk profilinde yalnız `nickname`, `avatar_id`, `age_band`, `level` ve ilerleme tutulur. Yeni alan: ADR + insan onayı.
2. **Üçüncü taraf SDK yok** (analitik, reklam, takip, hata izleme). Önce sor.
3. **Haklar (entitlement) yalnız sunucuda hesaplanır**; istemci yalnız önbelleğe alır.
4. **Sırlar repoya girmez.** Yalnız `.env.example`.
5. **Staging/yerelde gerçek kullanıcı verisi yok**, yalnız sentetik veri.
6. **Unity `.unity`, `.prefab`, `.asset` dosyalarına** yalnız açıkça istenirse dokun.
7. **Kullanıcıya görünen metin/ses yerelleştirme tablosundan gelir**, kodda sabit Türkçe yok.
8. **Geçmeyen testi geçirmek için testi değiştirme**; gerekiyorsa nedenini yaz.
9. **Gizlilik, ödeme, hak hesaplama ve çocuk deneyiminde belirsizlik varsa sor.**
10. **Yıkıcı işlem yok** (DB silme, `push --force`, geçmiş yeniden yazma, toplu silme) açık onay olmadan.

## Çalışma biçimi

- Her iş bir PRD/plan kimliğine bağlıdır (ör. `YON-02`, `F1-16`). Kimlik belirsizse sor, ama küçük düzeltmelerde kimlik aramak için işi durdurma.
- **Plan modu yalnız** gizlilik, ödeme, kimlik doğrulama, hak hesaplama veya geniş (çok dosyalı, katman aşan) değişikliklerde gerekir. Küçük ve yerel değişiklikleri doğrudan uygula.
- Bir PR tek amaçlı olsun; ilgisiz yeniden düzenleme yapma. Yeni bağımlılık eklemeden önce gerekçeyi yaz. Mimari karar → kısa ADR.
- Kod, değişken ve commit mesajları İngilizce (Conventional Commits). Yorumlar "neden"i anlatır.
- Bulamadığın dosya, fonksiyon veya uç nokta adını uydurma; önce ara.

## Test ve doğrulama (hızlı tut)

Unity batch modu her çalıştırmada editörü baştan açar (dakikalar sürer), bu yüzden en dar doğrulamayı seç:

1. **Saf mantık** (kodlama motoru, geri bildirim, çözücü): `make test-engine` veya yalnız ilgili `dotnet test --filter`. Unity açma.
2. **Unity gerekiyorsa**: yalnız değişen sınıfı çalıştır, ör. `-testFilter <Sınıf>`. Tüm `make unity-test` / `make unity-playmode` koşusu yalnız geniş değişiklikte.
3. **`make unity-e2e`** (tüm bölümler) yalnız iş bitip birleşmeye hazırken veya bölüm içeriği değiştiğinde, bir kez.
4. Yalnız `.uss`/UXML/metin gibi görsel değişikliklerde Unity testi çalıştırma; ne değiştiğini söyle, kullanıcı editörde bakar.
5. Backend/web için ilgili katmanın `make lint` ve testi yeter; `make test` (hepsi) yalnız PR öncesi.
6. Komut çıktısını bekleyerek takılma: uzun koşuları arka planda başlat, sonucu sonra oku.

Bitirirken neyi çalıştırdığını, neyi çalıştırmadığını ve nedenini kısaca raporla.

## Sık kullanılan komutlar

`make setup` · `make api-dev` · `make web-dev` · `make editor-dev` · `make test` · `make test-engine` · `make lint` · `make gen` (şema/OpenAPI değişince) · `make validate-content` (içerik değişince) · `make tts` · `make refresh-art` · `make unity-test` · `make unity-playmode` · `make unity-e2e`

## Alan uyarıları

- **Çocuk deneyimi:** hata cezalandırılmaz; can/süre baskısı, seri cezası, sürpriz kutu, oyun içi para, çocuğa "geri gel" bildirimi yok. Minik seviyede (3.5–5) ekranda yönerge metni yok.
- **Ödeme:** fiyatlar ve ücretsiz bölüm sayısı koda gömülmez; mağazadan/sunucudan gelir. Mağaza dışı ödeme yönlendirmesi yok.
- **İçerik/pedagoji:** bölüm, ses metni veya çocuk akışı değişince `pedagoji-uzmani` ile ön inceleme (`/pedagoji-inceleme`); ekip kararı kaydedilmeden içerik o karara göre değişmez.
- **`services/entitlements.py`:** değişiklik ≥ %95 kapsam ve insan incelemesi ister.
- **Yurt dışı kişisel veri servisi:** KVKK adımları tamamlanmadan kullanılmaz (docs/gelistirme-kurallari.md §11, ADR 0007).
