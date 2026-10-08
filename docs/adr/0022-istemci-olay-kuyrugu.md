# 0022 — İstemci olay kuyruğu (anonim kullanım ölçümü)

- Durum: Kabul edildi (insan incelemesi gerekli: çocuk kullanım verisi)
- Tarih: 2026-10-09
- İlgili: F1-18 (istemci); ADR 0014, 0015, 0019; UYM-01; CLAUDE.md altın kural 1, 2, §11

## Karar

1. **Kapsam:** yalnız sunucunun kabul ettiği sabit olay adları ve özellikler (ADR 0014). `AnalyticsService` bunları gönderim öncesinde de doğrular; listede olmayan ad, özellik veya sözcük istisna fırlatır (testle korunur), yani kod tabanına yanlışlıkla kişisel veri sızamaz.
2. **Koşullar:** olay yalnız (a) sunucu yapılandırılmışsa, (b) etkin çocuk profili varsa ve (c) veli geçerli aydınlatma metnine rıza vermişse kaydedilir. Rıza geri çekilirse bekleyen olaylar silinir, gönderilmez.
3. **Anonim kimlik:** çocuk başına cihazda üretilen rastgele UUID (`analytics.json`). **Profil kimliği değildir**; olaylar hesaptaki profillerle eşlenemez (ADR 0014). Profil silinince kimliği ve bekleyen olayları da silinir.
4. **Kuyruk:** `analytics.json`, en çok 1000 olay (aşınca en eskiler düşer), 20 olayda veya haritaya dönüşte ve uygulama arka plana alınınca toplu gönderim (en çok 100). Ağ yoksa olaylar kalır; sunucunun reddettiği paket (422 vb.) tekrar tekrar denenmemek için atılır.
5. **Olaylar:** `app_open`; `level_start`, `level_complete` (deneme, süre, yıldız, ipucu, kod uzunluğu), `level_abandon`, `hint_used` (kademe), `session_limit_reached` (süre sınırı dakikası). `paywall_view` abonelik ekranıyla birlikte eklenecek. Bölümün yaş seviyesi `level_band` olarak eklenir; takma ad, e-posta, cihaz veya reklam kimliği hiçbir olayda yoktur.
6. **Süre** `Time.realtimeSinceStartup` farkıdır; yalnız saniye olarak (en çok 24 saat) gönderilir.

## Sonuçlar

- Sunucu adresi yoksa (şu an varsayılan) hiçbir olay kaydedilmez ve hiçbir ağ çağrısı yapılmaz.
- `profile_kind`, `device_class`, `source` özellikleri henüz kullanılmıyor; ihtiyaç doğunca eklenir.
