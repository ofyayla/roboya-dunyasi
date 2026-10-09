# 0028 — Hata izleme ve panolar (kendi sunucumuzda)

- Durum: Kabul edildi
- Tarih: 2026-10-09
- İlgili: F1-19, F1-20; ADR 0007, 0014; CLAUDE.md altın kural 1, 2, §6, §11; PRD "Analitik"

## Bağlam

Mağazaların çocuk kategorisi nedeniyle uygulamaya üçüncü taraf hata izleme veya analitik SDK'sı eklenmez. Yine de sunucudaki hataları görmek ve öğrenme/dönüşüm panolarını izlemek gerekir. Her iki araç da bizim sunucumuzda çalışır.

## Karar

### Hata izleme (F1-19)

1. **Sunucu tarafı:** API, Sentry uyumlu bir hata sunucusuna (GlitchTip, kendi barındırdığımız) `sentry-sdk` ile rapor gönderebilir. `ROBOYA_ERROR_DSN` boşsa hiçbir şey dışarı çıkmaz (varsayılan). Bu SDK **yalnız Python sunucusundadır**; oyun istemcisinde hata izleme SDK'sı yoktur. Yeni bağımlılık gerekçesi: kendi Sentry protokolü istemcimizi yazmak yerine küçük, yaygın ve bakımlı bir kütüphane.
2. **Kişisel veri yok:** `before_send` ile olaydan istek verisi, kullanıcı, çerez, üstbilgi, ekstra, günlük izleri (breadcrumb) ve **istisna iletileri** çıkarılır (ileti e-posta veya takma ad taşıyabilir); yalnız istisna türü, yığın çerçeveleri (yerel değişken ve kod bağlamı olmadan) ve sürüm gider. `send_default_pii=False`, gövde boyutu `never`, yerel değişkenler kapalı, izleme örneklemesi sıfır. Bu kurallar testle korunur.
3. **İstemci hataları:** Unity hataları cihazda günlüklenir; otomatik istemci hata raporu yoktur (altın kural 2).
4. **Altyapı:** `infra/monitoring/docker-compose.yml` (GlitchTip + Valkey + ayrı Postgres). Geliştirme parolaları önemsizdir; gerçek kurulum sırlarını ortamdan alır ve barındırma kararına (ADR 0007) göre yapılır. GlitchTip ve Metabase imaj sürümleri gerçek dağıtımdan önce güncel kararlı sürüme sabitlenip doğrulanmalıdır (burada yalnız `docker compose config` ile sözdizimi doğrulandı).

### Panolar (F1-20)

5. **Metabase** (kendi sunucumuzda) panoları gösterir. Veritabanına yalnız **salt okunur `roboya_bi` rolüyle** bağlanır (`infra/bi/create-readonly-role.sql`); bu rol yalnız `bi` şemasını görür, `public` tablolarına (hesap, profil, e-posta) erişemez (yerelde doğrulandı).
6. **`bi` şemasındaki görünümler** (migrasyon 0006) anonim toplulaştırmadır: `level_stats` (başlama, bitirme, terk oranı, ortalama deneme/süre/yıldız/ipucu), `hint_usage`, `daily_activity`, `b2c_funnel`, `subscriptions` (mağaza bildirimlerinden, hesap bağı yok). Hiçbir görünüm e-posta, takma ad, hesap veya profil kimliği içermez (testle korunur).
7. **Kavram bazında ustalık eğrisi** için bölüm→kavram eşlemesi (içerikten) panolara ayrı bir tabloyla yüklenecek; olaylar zaten yalnız bölüm kimliği taşır.

## Sonuçlar

- Panolar ham olay tablosunu değil görünümleri okur; ham olaylar 24 ay sonra bölüm düşürülerek silinir (ADR 0014).
- Rolün parolası dağıtımda rastgele üretilir ve repoya girmez. Şema yeniden oluşturulursa (yalnız testlerde) rol betiği yeniden çalıştırılır.
- B2B panosu (okul) F2'de; B2C panosunun abonelik hunisi mağaza bildirimlerinin gerçek sandbox akışıyla doğrulanınca tamamlanır.
