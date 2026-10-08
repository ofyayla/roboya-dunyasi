# 0014 — Birinci taraf analitik: anonim, izinli alanlı, aylık bölümlenmiş

- Durum: Kabul edildi (sunucu tarafı; istemci kuyruğu sonraki PR'da)
- Tarih: 2026-10-08
- İlgili: F1-18; UYM-06; CLAUDE.md altın kural 1 ve 2, §11; PRD "Analitik ve ölçümleme"

## Bağlam

Analitik iki soruya cevap verecek: çocuk öğreniyor mu, aile ödemeye devam ediyor mu? Mağazaların çocuk kategorisi nedeniyle üçüncü taraf analitik SDK'sı yok; her şey kendi sunucumuzda toplanır. Veri, takma adla değil rastgele anonim profil kimliğiyle ilişkilendirilir; ham veri 24 ay sonra silinir.

## Karar

1. **`POST /v1/events`** hesapsız çalışır (ücretsiz oyun hesapsız sürer) ve **herkese açıktır**; yalnız anonim veri taşıdığı için kimlik doğrulama yoktur. Kötüye kullanım sınırları (hız, boyut) ağ geçidinde de uygulanacak.
2. **Zarf:** rastgele `anon_id` (cihazda çocuk profili başına üretilen UUID), uygulama sürümü, platform, en çok 100 olay. Takma ad, e-posta, reklam kimliği, IP veya serbest metin alanı yoktur; bilinmeyen her alan 422 ile reddedilir (`extra="forbid"`).
3. **Olay adları sabit liste:** `app_open`, `level_start`, `level_complete`, `level_abandon`, `hint_used`, `session_limit_reached`, `paywall_view`. Deneme, satın alma ve yenileme olayları uygulamadan değil, mağaza bildirimlerinden (ADR 0013) sunucuda üretilir; uygulama bunları gönderemez.
4. **Özellikler sabit ve tiplidir:** küçük tam sayılar (`attempts`, `duration_s`, `stars`, `hints`, `hint_tier`, `code_length`, `last_step`, `limit_minutes`) ve sabit sözcükler (`profile_kind`, `device_class`, `level_band`, `source`). Başka özellik eklemek ADR ve insan onayı gerektirir.
5. **Tekrar güvenliği:** olay kimliğini uygulama üretir; birincil anahtar `(id, occurred_at)`. Ağ hatasında yeniden gönderilen olay bir kez saklanır.
6. **Zaman penceresi:** `occurred_at` şimdiden en çok 30 gün öncesi ve 5 dakika sonrası arasında olmalı; dışındakiler atılır (yanıtta `dropped`). Çevrimdışı oynayan cihaz geç gönderebilir; yanlış saatli cihaz bölümlere dağınık satır üretemez.
7. **Tablo `occurred_at` üzerinden aylık bölümlenir.** Bir varsayılan bölüm eklemenin hiçbir zaman bölüm eksikliğinden başarısız olmamasını sağlar. `python -m app.maintenance` (zamanlanmış iş) önümüzdeki üç ayın bölümünü oluşturur ve 24 aydan eski bölümleri düşürür; ikisi de idempotenttir. Silme tek tek satır değil bölüm düşürmedir.

## Sonuçlar

- Oyun istemcisi (cihazda kuyruk, toplu gönderim, çevrimdışı saklama) sonraki PR'da; o zamana kadar uç nokta kullanılmıyor.
- Panolar (F1-20) bu tablodan ve abonelik tablosundan beslenir.
- IP adresi uygulama tarafından saklanmaz ve loglanmaz; ters vekil veya CDN erişim günlüklerinde IP tutulacaksa kısaltılması veya kapatılması dağıtım işidir (ADR 0007 envanterindeki CDN satırı).
- `anon_id`'nin hesapla eşlenmesi bilinçli olarak yapılmaz; veli raporu (F1-12) için ilerleme verisi `/v1/me/profiles/{id}/progress` üzerinden gelir, olaylardan değil.
