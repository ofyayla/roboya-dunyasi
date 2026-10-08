# 0021 — İstemci hak önbelleği

- Durum: Kabul edildi (insan incelemesi gerekli: hak hesaplama)
- Tarih: 2026-10-09
- İlgili: GLR-03; ADR 0009, 0013, 0019, 0020; CLAUDE.md altın kural 3, §12

## Karar

1. `EntitlementService` (`IEntitlementSource`) sunucudan `GET /v1/me/entitlement` ile hakkı sorar; yanıtı `entitlement.json` içinde sunucunun verdiği `cache_until` anına kadar saklar ve çevrimdışıyken ondan okur.
2. `HasPremium` yalnız önbellekte `tier=premium` ve `şimdi < cache_until` iken doğrudur. Süre dolunca veya dosya bozuksa ücretsiz katmana dönülür; istemci hiçbir koşulda hak üretmez.
3. Eşitleme akışında, profil eşitlenmeden önce sorulur; böylece sunucunun uyguladığı profil sınırı ile cihazın gösterdiği sınır aynı olur. Çıkışta önbellek silinir (hak hesaba bağlıdır).
4. Editörde `ROBOYA_DEV_PREMIUM=1` hâlâ derlemeye dahil değildir (oyuncu sürümlerinden çıkarılır).

## Sonuçlar

- Köklenmiş bir cihazda `entitlement.json` elle değiştirilirse yerelde ücretli bölümler açılabilir. Bu bilinçli kabul edildi: içerik sunucuda korunmuyor (bölümler uygulamayla gelir), sunucu tarafında profil sınırı ve abonelik kayıtları yine doğru kalır. İçerik sunucudan akıtılırsa yeniden değerlendirilir.
- Satın alma köprüsü (F1-16) bu servise dokunmaz; satın alma sonrası `POST /v1/store/receipts` yanıtı aynı önbelleğe yazılır.
