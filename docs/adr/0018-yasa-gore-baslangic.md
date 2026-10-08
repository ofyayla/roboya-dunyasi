# 0018 — Yaşa göre başlangıç bölümü ve ücretsiz bölümler

- Durum: Kabul edildi (ürün sahibi onayı bekleyen nokta: madde 3)
- Tarih: 2026-10-08
- İlgili: F1-08; PRD "Yaş seviyeleri"; GLR-01; ADR 0009, 0016

## Bağlam

Seviye yaşla başlar (PRD). Bölüm verisinde her bölümün `ageLevels` alanı vardır. Ücretsiz katman "ilk 3 bölüm"dür; Kaşif bir çocuğun ilk 3 bölümü kendisine uygun olmayabilir.

## Karar

1. **Başlangıç:** profilin yaş aralığını adlandıran ilk bölüm (`PathRules.StartIndex`). Aralığı adlandıran bölüm yoksa bir alt aralığa düşülür (Mucit içeriği gelene kadar Mucit → Kaşif); hiçbiri yoksa ilk bölüm.
2. **Sıra:** "sıradaki" bölüm başlangıçtan sayılır. Başlangıçtan önceki bölümler premiumda `Open` (oynanabilir, sıradaki değil) olur.
3. **Ücretsiz bölümler başlangıçtan sayılır:** ücretsizde çocuğun başlangıcından itibaren `FreeLevelCount` bölüm açıktır, öncesi ve sonrası büyük kilidin (veli) arkasındadır. Böylece Kaşif çocuk kendi seviyesinde ücretsiz oynar. **Ürün sahibi karar versin:** veli yaş aralığını değiştirerek farklı bir ücretsiz pencere açabilir (yıldızlar korunur); istenmezse ücretsiz pencere sabitlenir.
4. İstemci hak vermez; premium bilgisi yine sunucudan (ADR 0013). Ücretsiz bölüm sayısı yapılandırmadan gelir.

## Sonuçlar

- Minik için davranış değişmedi (başlangıç 0).
- Kaşif başlangıcında kart tanıtan önceki bölümler (sağa/sola dön) atlanır; çocuk kartları bölüm içinde kullanır. Pedagoji gözden geçirmesi F1-21'de.
