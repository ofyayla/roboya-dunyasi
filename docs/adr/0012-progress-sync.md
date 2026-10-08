# 0012 — İlerleme eşitleme: çakışmasız "en yüksek yıldız" birleştirmesi

- Durum: Kabul edildi (sunucu tarafı; oyun istemcisi sonraki PR'da)
- Tarih: 2026-10-08
- İlgili: F1-15; ILR-01, ILR-02; CLAUDE.md altın kural 1 ve 3, §8, §11; ADR 0009, 0011

## Bağlam

İlerleme cihazda saklanıyor (ADR 0009). Hesap varsa sunucuyla eşitlenmeli; aynı veli birden çok cihaz kullanabilir, cihazlar çevrimdışı oynayıp sonra eşitleyebilir. Kural: yıldız yalnız artar (ILR-02).

## Karar

1. **Birleştirme "bölüm başına yüksek olan kazanır"dır.** Sunucu, gelen yıldızı kayıtlıyla karşılaştırıp yükseğini tutar (`INSERT … ON CONFLICT DO UPDATE SET stars = GREATEST(…)`, tek deyimde). Bu birleştirme sıradan bağımsızdır, tekrarlanabilir ve birden çok cihazda güvenlidir; ayrıca çakışma çözme mantığı gerekmez. En ileri bölüm yıldızlardan türetilir.
2. **Uç noktalar** (hepsi kimlik doğrulamalı, hesap kapsamlı):
   - `GET /v1/me/profiles`, `PUT /v1/me/profiles/{id}`, `DELETE /v1/me/profiles/{id}`
   - `GET /v1/me/profiles/{id}/progress`, `POST /v1/me/profiles/{id}/progress/sync` (gönder, birleşmiş sonucu al)
3. **Profil kimliğini uygulama üretir** (UUID); çevrimdışı oluşturulan profil ilk eşitlemede aynı kimliği korur. Başka hesabın sahip olduğu bir kimlik "yok" gibi 404 döner.
4. **Çocuk verisi yalnız izinli alanlardır:** takma ad (en çok 24 karakter), avatar kimliği, yaş aralığı (`minik`, `kasif`, `mucit`) ve bölüm başına yıldız. Başka alan gönderilirse 422 döner (`extra="forbid"`); doğum tarihi veya gerçek ad kabul edilmez. Yeni alan ADR ve insan onayı gerektirir.
5. **Girdi sınırları:** yıldız 0–3 tam sayı, bölüm kimliği `<bölge>.<oyun>.<nn>` biçiminde, tek istekte en çok 500 bölüm.
6. **Profil sınırı** `services/entitlements.py` içindeki `profile_limit` ile verilir: bugün ücretsiz katman (1). Aile Premium'da 4'e mağaza hakkı (F1-17) çıkarınca yükselir; çağıranlar zaten bu fonksiyondan geçiyor.

## Sonuçlar

- Profil silinince ilerlemesi de silinir (`ON DELETE CASCADE`); hesap silinince profiller ve ilerleme de gider (F1-13 silme akışının temeli).
- İstemci tarafı (oturum açma ekranı, belirteç saklama, eşitleme kuyruğu, ağ hatalarında yeniden deneme) ayrı bir PR'da, veli alanı içinde yapılacak. O zamana kadar sunucu hazır ama kullanılmıyor.
- Rıza kaydı (F1-25) gelene kadar bu uç noktalar gerçek çocuk verisiyle kullanılmamalı.
