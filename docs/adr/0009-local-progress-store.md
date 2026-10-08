# 0009 — Yerel ilerleme deposu: profil başına JSON dosyası

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F1-05, F1-06, ILR-01, ILR-02, ILR-03; CLAUDE.md altın kural 1 ve 3; ADR 0008

## Bağlam

- Plan F1-06 yerel ilerleme için SQLite öngörüyordu. Unity'de SQLite yerel bir eklenti (iOS ve Android için ayrı kütüphaneler) ve yeni bir bağımlılık gerektirir.
- Bugün saklanan veri küçük: bölüm başına en iyi yıldız ve Roboya'nın taktığı parçalar.
- Ücretsiz bölüm kilidi bir hak kararıdır; istemci premium hakkını kendisi hesaplamamalı (altın kural 3).

## Karar

1. **İlerleme yerel profil başına tek bir JSON dosyasında tutulur:** `persistentDataPath/progress/<profil-kimliği>.json`.
   - **Profil kimliği:** ilk açılışta üretilen rastgele GUID; takma ad, cihaz kimliği veya başka kişisel veri tutulmaz.
   - **Yazma:** atomiktir; önce geçici dosyaya yazılır, sonra eskisinin yerine konur.
   - **Bozuk dosya:** yanına `.corrupt-<zaman>` olarak kaydedilir; oyun boş ilerlemeyle devam eder, çökmez.
2. **Kurallar saf C#'ta, `Roboya.CodingEngine/Progress` altındadır (.NET'te de test edilir):**
   - `ProgressBook`: yıldızlar yalnız artar.
   - `PathRules`: patikadaki taşın durumu (bitti, sıradaki, kilitli, büyüklerine sor).
   - `RewardRules`: her 5 bölümde ve her bölge sonunda bir robot parçası, sabit sırayla.
   - **Kazanılan parçalar saklanmaz;** her seferinde ilerlemeden hesaplanır.
3. **Premium hakkı `IEntitlementSource`'tan okunur.**
   - Sunucu hak servisi (F1-17) gelene kadar tek uygulama `FreeTierEntitlements`: her zaman ücretsiz katman.
   - Ücretsiz bölüm sayısı `ProgressRules`'tan gelir (varsayılan 3); ileride sunucu yapılandırmasından okunacak.
4. **Geliştirici anahtarı:** `DevEntitlements` yalnız Unity Editor'da derlenir (`#if UNITY_EDITOR`) ve `ROBOYA_DEV_PREMIUM=1` ile tüm bölümleri açar. Oyuncu derlemelerine girmez; "10 bölüm" PlayMode testi bunu kullanır.

## Sonuçlar

- Yeni bağımlılık yok. Depo `IProgressStore` arayüzünün arkasında; olay kuyruğu (F1-18) veya sunucu eşitlemesi (F1-15) daha güçlü bir depo isterse yalnız uygulama değişir.
- **Şifreleme:** dosya şifrelenmiyor, çünkü kişisel veri taşımıyor. Çocuk profillerine takma ad eklendiğinde (F1-10) dosya cihazda şifrelenecek (Android Keystore / iOS Keychain ile anahtar). Bu ADR o zaman güncellenir.
- **Premium süresi biterse:** ücretsiz katmanın ötesindeki bitmiş bölümler de "büyüklerine sor" durumuna döner; yıldızlar kaybolmaz.
- Profil başına dosya yapısı, F1-10'daki çoklu çocuk profiline doğrudan genişler.
