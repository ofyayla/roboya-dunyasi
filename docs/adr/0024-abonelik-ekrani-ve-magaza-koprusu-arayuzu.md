# 0024 — Abonelik ekranı ve mağaza köprüsü arayüzü

- Durum: Kabul edildi (insan incelemesi gerekli: ödeme, hak; mağaza metni hukukçu onayı)
- Tarih: 2026-10-09
- İlgili: F1-16; GLR-01, GLR-02, GLR-03; ADR 0008, 0013, 0021; CLAUDE.md altın kural 2, 3, §12

## Karar

1. **Arayüz önce:** ADR 0008'deki `IStoreBridge` (`GetProductsAsync`, `PurchaseAsync`, `RestoreAsync`, `FinishAsync`, `TransactionUpdated`) `Roboya.Services` içinde tanımlandı. Yerel köprüler (StoreKit 2 Swift, Play Billing Kotlin) mağaza hesapları ve gerçek cihaz gerektirir ve **bu PR'da yok**; o zamana kadar `NoStoreBridge` kullanılır (ekran "Mağaza bağlantısı bu sürümde henüz yok" der, satın alma düğmesi gösterilmez). Test ve editör için `FakeStoreBridge` satın alma, iptal, bekleyen onay, geri yükleme ve yenileme akışlarını üretir.
2. **Satın alma akışı:** hesap şart (GLR-03) → köprü satın alır (mağazaya yalnız hesaptan türetilmiş rastgele bir kimlik gider, e-posta veya hesap kimliği gitmez) → kanıt `POST /v1/store/receipts` ile sunucuya gider → sunucunun verdiği hak `EntitlementService` önbelleğine yazılır → **ancak sonra** `FinishAsync` ile mağazaya bildirilir. Sunucuya ulaşılamazsa işlem bitirilmez; uygulama açılışında `RecoverAsync` (geri yükleme) onu bağlar. Ödenmiş satın alma kaybolmaz.
3. **Fiyat koda yazılmaz:** ekran fiyatı mağazadan alır. Ürün kimlikleri ve deneme gün sayısı `content/store/products.json` içindedir (uygulamayla paketlenir).
4. **Ekran** veli alanındadır (ebeveyn kilidi, GLR-01): durum (ücretsiz katman / etkin / deneme / ek süre), aylık ve yıllık düğmeler (mağaza fiyatıyla), deneme notu, geri yükleme ve GLR-02 koşul metni (ücretlendirme, otomatik yenileme, iptal, deneme). Satış dili çocuğa yönelmez; çocuk ekranlarında abonelik görünmez.
5. **Olay:** veli alanı açıldığında ve premium değilse `paywall_view` (ADR 0022). Deneme/satın alma/yenileme olayları sunucuda mağaza bildirimlerinden üretilir.
6. İstemci hak vermez: ekran yalnız `EntitlementService` durumunu gösterir.

## Sonuçlar

- **Mağaza koşul metni taslaktır** ve Apple/Google yönergelerine ve tüketici mevzuatına uygunluk için hukukçu onayı gerekir.
- Gerçek satın almanın çalışması için: App Store Connect ve Play Console'da ürünler, yerel köprü kodları, sunucuda `AppleVerifier`/`GoogleVerifier` ve gerçek cihaz sandbox testi gerekir (dış bağımlılık).
- Yıllık fiyat = aylık × 12 × 0,85 kuralı mağaza panelinde tanımlanır (CLAUDE.md §12); kodda değil.
