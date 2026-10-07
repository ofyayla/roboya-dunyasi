# 0001 — Üretim dışında gerçek kullanıcı verisi yok

- Durum: Kabul edildi
- Tarih: 2026-10-07
- İlgili: plan "Kapı 1 öncesi" güvenlik listesi, CLAUDE.md altın kural 5

## Bağlam

Ürün çocuklara yönelik; çocuk verisi en hassas veri türüdür. Yerel, CI ve staging ortamları geliştiricilere ve üçüncü taraf altyapıya (GitHub Actions, staging bulutu) açıktır. Repo herkese açıktır.

## Karar

- Yerel, CI ve staging ortamlarında yalnız `app.seed` ile üretilen sentetik veri kullanılır.
- Üretim veritabanının dökümü (dump) hiçbir koşulda üretim dışına taşınmaz; hata ayıklama için anonimleştirilmiş bile olsa kopya alınmaz.
- Sentetik veride gerçek e-posta alan adı kullanılmaz (`@example.test`).
- Staging e-postaları gerçek alıcılara gitmez (yakalayıcı/sink).

## Sonuçlar

- Üretime özgü hatalar yapılandırılmış ve kişisel veri içermeyen loglarla incelenir.
- Tohum verisi gerçekçi senaryoları (çok çocuklu aile, okul sınıfı, süresi dolan abonelik) kapsamalıdır.
