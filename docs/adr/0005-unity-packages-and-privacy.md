# 0005 — Unity paketleri ve motor düzeyi gizlilik ayarları

- Durum: Kabul edildi (IAP kararı açık; aşağıya bakın)
- Tarih: 2026-10-08
- İlgili: F0-02, F0-20; UYM-06; CLAUDE.md altın kural 2

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
Unity Analytics, Cloud Diagnostics (çökme raporlama), Performance Reporting ve `enableCrashReportAPI` kapalı; kamera, mikrofon ve konum kullanım açıklamaları boş; Unity açılış ekranı kapalı; paket kimliği `com.roboyakids.roboyadunyasi`; yalnız yatay ekran; Android 9+ (API 28), IL2CPP, ARMv7 + ARM64; iOS 16+.

**Mağaza içi satın alma: karar açık.** Unity IAP'nin güncel sürümü Unity Gaming Services çekirdeğini (`com.unity.services.core`) başlatıyor. F1-16'dan önce şu iki seçenek karşılaştırılacak ve bu ADR güncellenecek:
1. Unity IAP: ağ trafiği gözlenerek UGS'nin hangi veriyi gönderdiği ölçülür; kapatılabiliyorsa kullanılır.
2. İnce yerel köprü: iOS'ta StoreKit 2, Android'de Play Billing Library; fiş doğrulaması zaten sunucuda (GLR-03).

## Sonuçlar

- Yeni bir Unity paketi eklemek bu ADR'nin güncellenmesini gerektirir.
- Hata izleme için Unity Cloud Diagnostics yerine kendi sunucumuzdaki hata izleme kullanılacak (F1-19).
- Unity Personal lisansının koşulları (son 12 ayda gelir ve fon eşiği) yıllık olarak kontrol edilmelidir; eşik aşılırsa Pro lisansa geçilir.
