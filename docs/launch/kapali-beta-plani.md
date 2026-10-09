# Kapalı beta planı (F1-27)

Durum: **yapay zekâ taslağı**; tarihler, sayılar ve sahipler ürün ekibiyle netleştirilir. Hedef: PRD MVP kanıtı — çocuklar kendiliğinden geri dönüyor mu, veliler ödemeye istekli mi?

## Ön koşullar (kapıdan geçmeden başlanmaz)

- [ ] Aydınlatma metni hukukçu onaylı (`status: final`); üretim API'si taslak metinle başlamaz.
- [ ] Barındırma sağlayıcısı seçildi, KVKK standart sözleşmesi imzalı, Kurum bildirimi yapıldı, ADR 0007 envanteri güncel (CLAUDE.md §11).
- [ ] Gerçek e-posta sağlayıcısı (aynı süreçten geçmiş) ve mağaza doğrulayıcıları (Apple/Google) bağlı; sandbox testleri geçti.
- [ ] 36 bölümün pedagoji onayı ve profesyonel ses kaydı kararı (en az Minik bölümleri için; TTS ücretli planla yeniden üretilmiş).
- [ ] Çocuk deneyimi testleri her yeni bölge için en az 8–10 çocukla yapıldı (`docs/child-testing`).
- [ ] Güvenlik: sızma testi ya da en azından bağımsız gözden geçirme, olay müdahale prosedürü.

## Kapsam

- 50–100 aile; Minik ve Kaşif karışımı; iOS (TestFlight) ve Android (Play kapalı test).
- Süre: 4–6 hafta; ilk hafta destek yoğunluğu yüksek.
- Alım: ürün ekibinin ağından davet; her aileye kısa bilgilendirme ve aydınlatma metni (uygulama içinde de gösterilir).

## Ölçüt ve kapılar (PRD §Başarı ölçütleri)

| Soru | Ölçüt | Kaynak |
| --- | --- | --- |
| Çocuk geri dönüyor mu? | Haftalık geri dönüş; 2. hafta elde tutma | `bi.daily_activity` (anonim) |
| Öğreniyor mu? | Bölüm tamamlama, deneme/ipucu eğrisi, terk oranı | `bi.level_stats`, `bi.hint_usage` |
| Ücretsiz içeriği bitirip abonelik ekranını görüyor mu? | Abonelik ekranı görüntüleme | `bi.b2c_funnel` |
| Deneme başlatıyor mu? | ≥ %15 (ekranı görenlerden) | mağaza bildirimleri → `bi.subscriptions` |
| Veli memnun mu? | Kısa form puanı + serbest yorum | geri bildirim formu |

## Geri bildirim formu (taslak sorular)

1. Çocuğunuz uygulamayı kendiliğinden açıyor mu? Haftada kaç kez?
2. Hangi bölümlerde takıldı? Hangi anlarda güldü/şaşırdı?
3. Roboya'nın sesi ve yönergeleri anlaşılır mı? (1–5)
4. Süre sınırı ve veli alanı kullanışlı mı? Eksik olan ne?
5. Ücretsiz bölümlerin sayısı (3) sizce yeterli mi?
6. **Fiyat anketi (Van Westendorp):** aylık abonelik için "çok ucuz / ucuz / pahalı ama düşünürüm / çok pahalı" fiyatları.
7. Yıllık plan (7 gün deneme) cazip mi?

Form yanıtları çocuk verisi içermez; ad ve e-posta yalnız iletişim için ayrı ve isteğe bağlıdır.

## Takvim (öneri)

| Hafta | İş |
| --- | --- |
| −2 | Ön koşullar, TestFlight/Play kanalları, destek kanalı |
| 0 | Davetler, ilk oturum gözlemleri (5–10 aile ile görüntülü) |
| 1–2 | Hata düzeltme, bölüm sırası ayarları |
| 3–4 | Abonelik deneyi (sandbox değil gerçek mağaza), fiyat anketi |
| 5–6 | Değerlendirme: ölçütler, karar toplantısı (devam / değiştir / dur) |

## Riskler

- Taslak metinle gerçek veli verisi toplamak (kapı: hukukçu onayı).
- Mağaza onay gecikmesi; sandbox ile gerçek farkları.
- Küçük örneklem: ölçütleri yorumlarken güven aralığını belirt.
