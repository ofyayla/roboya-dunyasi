# 0008 — Mağaza içi satın alma: ince yerel köprü (StoreKit 2 + Play Billing)

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F0-20, F1-16, F1-17; GLR-01, GLR-02, GLR-03; UYM-06; CLAUDE.md altın kural 2 ve 3; ADR 0005

## Bağlam

ADR 0005 satın alma yöntemini açık bırakmıştı: Unity IAP mı, ince bir yerel köprü mü?

Unity belgeleri (2026-10-08 itibarıyla):

- **Unity IAP 5.4 ve üstü varsayılan olarak kişisel veri toplar:** Player ID (Unity Authentication), Unity Installation ID, cihaz bilgisi, oturum kimlikleri, ülke.
  - Belge bunları "ürünün çalışması için her zaman toplanır" diye tanımlıyor; kapatma yolu belgelenmemiş.
  - IAP'nin onay servisi yok. Velinin onayını almak ve devre dışı bırakma yolu sunmak geliştiriciye bırakılmış.
  - Kaynak: [Unity IAP gizlilik özeti](https://docs.unity.com/en-us/iap/privacy-and-consent/overview).
- **5.4 öncesi sürümler** belgeye göre kişisel veri toplamıyor. Ancak Google Play, Billing Library sürümünü düzenli olarak zorunlu yükseltiyor; 2025-08-31'den beri en az 7. Eski bir IAP sürümüne kilitlenmek, ileride veri toplayan sürüme geçmeyi zorunlu kılar.
  - Kaynaklar: [IAP değişiklik günlüğü](https://docs.unity3d.com/Packages/com.unity.purchasing@5.3/changelog/CHANGELOG.html), [Unity: neden IAP v5](https://support.unity.com/hc/articles/47757890052372-Why-you-should-upgrade-to-Unity-In-App-Purchasing-IAP-v5-x).
- **IAP, `com.unity.services.core` paketini getirir.** Paketin telemetri ve tanı verisi gönderdiği belgelerde geçiyor; pakete özel bir kapatma ayarı belgelenmemiş.
  - Kaynak: [Services Core değişiklik günlüğü](https://docs.unity3d.com/Packages/com.unity.services.core@1.14/changelog/CHANGELOG.html).
- **Kurallarla çatışıyor:**
  - Altın kural 2 (üçüncü taraf SDK yok).
  - ADR 0005 (`com.unity.services.*` yok).
  - Apple Kids kategorisi ve Google Families: üçüncü tarafa cihaz kimliği gönderilmemeli.
- **IAP'nin bize ek değeri az:**
  - Fiş doğrulaması ve hak hesabı zaten sunucuda (GLR-03, altın kural 3).
  - StoreKit 2'de istemci tarafı doğrulama işlevsiz; Apple'ın imzalı işlemi (JWS) sunucuda doğrulanmalı.

## Karar

**Mağazaların kendi kütüphaneleri üzerine ince bir yerel köprü:**
- iOS: StoreKit 2.
- Android: Google Play Billing Library 8.x.

Bunlar mağazanın zorunlu kütüphaneleridir, üçüncü taraf değildir. Unity IAP ve `com.unity.services.*` paketleri projeye eklenmez.

### Köprünün tasarımı (uygulama F1-16)

- **C# arayüzü:** `Roboya.Services` içinde `IStoreBridge`, bileşim kökü `Bootstrap` kurar.
  - `GetProductsAsync(ids)`: mağazanın yerelleştirilmiş fiyat metnini döner. Fiyat kodda tutulmaz (CLAUDE.md §6, §12).
  - `PurchaseAsync(productId, accountToken)` → `PurchaseProof`: iOS'ta `signedTransaction` (JWS), Android'de `purchaseToken`. Kanıt sunucuya gönderilir; hak sunucuda hesaplanır.
  - `RestoreAsync()` ve `TransactionUpdated` olayı: yenileme, iade, aile paylaşımı.
  - `FinishAsync(proof)`: sunucu hakkı onayladıktan sonra çağrılır. iOS'ta `transaction.finish()`; Android'de onay (acknowledge) sunucudan Play Developer API ile yapılır.
- **Hesap bağı (GLR-03):**
  - iOS `appAccountToken` ve Android `obfuscatedAccountId`, veli hesabının rastgele UUID'sinden türetilir.
  - Mağazaya e-posta veya başka kişisel veri gitmez.
- **Platform kodu:**
  - Android: `Assets/Plugins/Android/` altında küçük Kotlin modülü. `billing-ktx` bağımlılığı, `ProjectSetup` ile yönetilen Gradle şablonundan eklenir.
  - iOS: `Assets/Plugins/iOS/` altında Swift dosyası; `@_cdecl` C girişleri, C# tarafında `DllImport("__Internal")`.
- **Editör ve testler:** `FakeStoreBridge`, satın alma, iptal, iade, geri yükleme ve deneme bitişi akışlarını üretir. Her akış için test yazılır (CLAUDE.md §12).
- **Sunucu (F1-17):**
  - Apple: App Store Server API ve App Store Server Notifications V2.
  - Google: Play Developer API ve Real-time Developer Notifications.
  - Bildirimler imza doğrulamasıyla ve olay kimliği üzerinden idempotent işlenir.
  - Hak yalnız `services/entitlements.py`'de hesaplanır.

## Sonuçlar

- **Bedel:**
  - İki küçük yerel kod tabanı. Unity IAP'ye göre tahminen 2–3 iş günü fazla.
  - Satın alma testleri mağaza sandbox hesaplarıyla, gerçek cihazda yapılır.
- **Bakım:**
  - Google'ın Billing Library takvimini kendimiz izleriz; her yıl yükseltme planlanır.
  - StoreKit 2 ve Play Billing'in yeni sürümleri bu köprüde güncellenir.
- **Gizlilik:**
  - Satın alma için cihazdan yalnız mağazaya veri gider.
  - Mağaza bildirimlerinin aktarım envanteri ADR 0007'de.
- **Yeniden değerlendirme koşulu:** Unity, IAP'nin kişisel veri toplamasını ve Services Core telemetrisini kapatma seçeneği sunar ve bunu belgelerse bu karar yeniden değerlendirilir.
