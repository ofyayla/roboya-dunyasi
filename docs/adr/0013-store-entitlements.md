# 0013 — Mağaza haklarının sunucuda hesaplanması

- Durum: Kabul edildi (sunucu çekirdeği; gerçek mağaza bağdaştırıcıları mağaza hesaplarıyla)
- Tarih: 2026-10-08
- İlgili: F1-17; GLR-01, GLR-02, GLR-03; CLAUDE.md altın kural 3, §8, §12; ADR 0008, 0011, 0012

## Bağlam

Premium hakkı yalnız sunucuda hesaplanır; uygulama kararı kendisi vermez (altın kural 3). Satın alma yerel köprüyle yapılır (ADR 0008), fiş doğrulaması sunucudadır. Mağaza bildirimleri (yenileme, iade, iptal) imza doğrulamasıyla ve olay kimliği üzerinden idempotent işlenmelidir (CLAUDE.md §8).

## Karar

1. **Premium yalnız doğrulanmış bir mağaza aboneliğinden gelir** ve veli hesabına bağlıdır (GLR-03: aynı hesapla açılan her cihazda geçerli). Hak hesabı tek modülde: `services/entitlements.py` (kapsam %100, eşik ≥ %95). Okul lisansı (v1.0) aynı modüle ikinci kaynak olarak eklenecek.
2. **Uygulama sorar, sunucu söyler:** `GET /v1/me/entitlement` `tier`, `status` (`free`, `trial`, `active`, `grace`), bitiş ve `cache_until` döner. Uygulama yanıtı `cache_until`'e kadar saklayıp çevrimdışı kullanır. `cache_until`, ücretli dönemin bitişini asla aşmaz ve en çok `ROBOYA_ENTITLEMENT_CACHE_HOURS` (72 saat) sürer.
3. **Satın alma kanıtı:** `POST /v1/store/receipts` (kimlik doğrulamalı) kanıtı doğrulatır ve aboneliği hesaba bağlar.
   - Bir satın alma tek hesaba aittir; başkasına bağlıysa 409 `purchase_linked`.
   - İade edilmiş (iptal edilmiş) bir satın alma, yeni bir kanıtla geri gelmez.
4. **Mağaza bildirimleri:** `POST /v1/store/notifications/{apple|google}` herkese açıktır; güven imzadan gelir, çağıranın kimliğinden değil.
   - İmza geçersizse 401. Olay kimliği `store_events` tablosunda tutulur; aynı olay ikinci kez gelirse hiçbir şey değişmez.
   - Olaylar sırasız gelebilir: kayıttaki en son olaydan eski bir olay yok sayılır (olay kimliği yine kaydedilir).
   - Bir veli henüz sahiplenmemiş bir abonelik için gelen bildirim saklanır; veli kanıtı sunduğunda bulunur.
   - Olay türleri: abone oldu, yenilendi, ek süre (grace), süresi doldu, iade edildi.
5. **Doğrulama arayüzü (`StoreVerifier`):** gerçek App Store (JWS zinciri, App Store Server API) ve Google Play (Developer API, RTDN) bağdaştırıcıları mağaza kimlik bilgileri gerektirir; hesaplar hazır olunca eklenir. Bugünkü `SignedDevVerifier` yapılandırılmış anahtarla imzalı ES256 belirteçlerini kabul eder; geliştirme ve testler içindir ve staging/üretimde başlatma reddedilir.
6. **Profil sınırı** bu modülden gelir: ücretsizde 1, premiumda 4 (ADR 0012).

## Sonuçlar

- Deneme bitişinden 24 saat önce veliye hatırlatma (GLR-02) e-posta sağlayıcısı gerektirir (CLAUDE.md §11); sağlayıcı seçilince bu abonelik verisinden beslenir.
- Gerçek mağaza testleri (sandbox hesapları, gerçek cihaz) sizde; sunucu mantığı sahte mağaza imzalarıyla tam kapsanıyor.
- Mağaza bildirimi adresleri sunucu dağıtılınca Apple ve Google panellerine tanıtılır.
