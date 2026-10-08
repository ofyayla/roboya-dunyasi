# 0005 — Unity paketleri ve motor düzeyi gizlilik ayarları

- Durum: Kabul edildi (satın alma yöntemi ADR 0008 ile kapandı)
- Tarih: 2026-10-08
- İlgili: F0-02, F0-20; UYM-06; CLAUDE.md altın kural 2; ADR 0008

## Bağlam

Uygulama mağazaların çocuk kategorisinde yayınlanacak. Unity'nin bazı paketleri ve modülleri (Analytics, Cloud Diagnostics, Unity Gaming Services) cihazdan Unity sunucularına veri gönderebilir. Paket listesi küçük, gerekçeli ve denetlenebilir olmalı.

## Karar

**Eklenen paketler** (sürümleri Unity 6.6 paket yöneticisi seçti, `Packages/manifest.json`):

| Paket | Neden |
| --- | --- |
| `com.unity.render-pipelines.universal` 17.6 | 2D URP; düşük seviye cihazda MSAA ve HDR kapalı |
| `com.unity.2d.sprite` | Sprite düzenleme |
| `com.unity.inputsystem` | Dokunma ve sürükle-bırak |
| `com.unity.localization` | Metin ve ses tabloları (CLAUDE.md kural 7) |
| `com.unity.addressables` | Bölge içerik paketleri, CDN |
| `com.unity.nuget.newtonsoft-json` | Bölüm JSON'u (ADR 0002) |
| `com.unity.test-framework` | EditMode / PlayMode testleri |

**Çıkarılan modüller:** `unityanalytics` (gizlilik), `ai`, `cloth`, `physics` (3D), `terrain`, `terrainphysics`, `tetgen`, `umbra`, `vehicles`, `wind`, `xr`. Eklenen paketlerin hiçbiri `unityanalytics` veya `com.unity.services.*` paketlerini geri getirmiyor (`packages-lock.json` ile doğrulandı).

**Motor ayarları** (`Assets/_Project/Editor/ProjectSetup.cs`, YAML elle düzenlenmez):
Unity Analytics, Cloud Diagnostics (çökme raporlama), Performance Reporting ve `enableCrashReportAPI` kapalı; kamera, mikrofon ve konum kullanım açıklamaları boş; Unity açılış ekranı kapalı; paket kimliği `com.roboyakids.roboyadunyasi`; yalnız yatay ekran; Android 9+ (API 28), IL2CPP, ARMv7 + ARM64; Android grafik arayüzü önce OpenGL ES 3, sonra Vulkan (Unity 6 varsayılanı Vulkan'sız cihazlarda ve emülatörde açılmadı; 2026-10-08); iOS 16+.

**Mağaza içi satın alma:** Unity IAP kullanılmaz. Yerine iOS'ta StoreKit 2, Android'de Play Billing Library üzerine ince bir yerel köprü yazılır ([ADR 0008](0008-in-app-purchase-bridge.md)). Nedeni: IAP 5.4+ Unity'ye kapatılamayan kişisel veri gönderiyor ve `com.unity.services.core` paketini getiriyor. `com.unity.services.*` paketleri projeye eklenmez.

## Android grafik gereksinimi (2026-10-08)

Unity 6.6 Android'de **en az OpenGL ES 3.1** istiyor; ES 3.0 desteği kaldırıldı (Unity'ye göre oyuncuların ~%0,4'ü, çoğunlukla Adreno 300 GPU'lu çok eski cihazlar). macOS'taki Android emülatörü yalnız ES 3.0 sunduğu için oyun emülatörde açılmıyor; doğrulama gerçek cihazda yapılır. Pilot okulların tablet modelleri (F1-28) bu gereksinime göre kontrol edilmeli; ES 3.0'a mahkûm bir tablet filosu çıkarsa 6.3 LTS'ye dönüş bu ADR ile yeniden değerlendirilir.

## Sonuçlar

- Yeni bir Unity paketi eklemek bu ADR'nin güncellenmesini gerektirir.
- Hata izleme için Unity Cloud Diagnostics yerine kendi sunucumuzdaki hata izleme kullanılacak (F1-19).
- Unity Personal lisansının koşulları (son 12 ayda gelir ve fon eşiği) yıllık olarak kontrol edilmelidir; eşik aşılırsa Pro lisansa geçilir.
