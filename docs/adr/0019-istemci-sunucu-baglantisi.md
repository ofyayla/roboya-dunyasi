# 0019 — İstemci–sunucu bağlantısı ve veli hesabı oturumu

- Durum: Kabul edildi (insan incelemesi gerekli: belirteç saklama, madde 4)
- Tarih: 2026-10-09
- İlgili: F1-14 (istemci), GLR-03; ADR 0011, 0012; CLAUDE.md altın kural 2, 3, §11

## Bağlam

Sunucu uçları hazır (hesap, profil eşitleme, mağaza hakkı, olaylar, gizlilik). Oyun hesapsız oynanır; hesap yalnız satın alma ve cihazlar arası eşitleme için gerekir. Üçüncü taraf SDK yok (altın kural 2); ağ katmanı Unity'nin kendi `UnityWebRequest` sınıfıyla yazılır.

## Karar

1. **Katman:** `Roboya.Services` (ağ, hesap, ileride eşitleme, olay kuyruğu, satın alma) yalnız Unity ve Newtonsoft'a bağlıdır; `Roboya.Core` ona bağlanır (bileşim kökü kurar), yani Services Core'u tanımaz.
2. **Ağ tek noktada:** `IHttpTransport` (gerçek: `UnityHttpTransport`). Ulaşılamayan sunucu istisna değil `status 0` → `ApiException("network")`. Testler sahte taşıyıcıyla çalışır; `Bootstrap.TransportOverride` PlayMode içindir.
3. **Sunucu adresi koda gömülmez:** `ROBOYA_API_URL` ortam değişkeni veya ilerleme klasöründeki `server.json` (`{"apiUrl": "..."}`). İkisi de yoksa oyun tamamen çevrimdışıdır ve hesap bölümü görünmez. Barındırma sağlayıcısı henüz seçilmedi; üretim adresi sağlayıcı kararından sonra yapılandırılır.
4. **Oturum saklama:** `account.json` (uygulamanın özel klasörü): rastgele cihaz kimliği, e-posta (ekranda göstermek için), erişim ve yenileme belirteci. Android ve iOS uygulama klasörünü işletim sistemi şifreler; ek olarak Android Keystore / iOS Keychain'e taşımak yerel eklenti gerektirir ve **ayrı karar** olarak bırakıldı. Reklam kimliği veya donanım kimliği kullanılmaz.
5. **Yenileme:** erişim belirteci bitmeye 60 sn kala tek seferde yenilenir (dönen belirteç olduğu için paralel iki yenileme çalınma sayılır). Sunucu belirteci reddederse oturum yerelde kapanır; ağ yoksa oturum korunur.
6. **Çıkış** yerelde hemen yapılır, sunucu çağrısı en iyi çaba ile yapılır.
7. **Arayüz:** hesap bölümü veli alanındadır (ebeveyn kilidinin arkasında); e-posta → 6 haneli kod → giriş. Hata iletileri `tr.json` tablosundan.

## Sonuçlar

- Üretim sürümü `server.json`/ortam değişkeni olmadan çevrimdışı kalır; bu bilinçli, hosting kararına bağlı.
- Sonraki PR'lar bu katmanın üstüne biner: rıza yükleme, ilerleme eşitleme, olay kuyruğu, hak sorgusu, gizlilik merkezi, abonelik.
- Elle yazılmış istemci, `packages/api-contract` üretilmiş istemciyle değiştirilecek (CLAUDE.md §9 web için; Unity tarafı için üretim ayrı iş).
