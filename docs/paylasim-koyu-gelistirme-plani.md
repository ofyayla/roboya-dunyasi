# Paylaşım Köyü — geliştirme planı (F2-13)

Durum: **taslak plan**. Tasarım: `docs/pedagoji/paylasim-koyu-taslagi.md`; mekanik: ADR 0030. Boyutlar `docs/plan.md` ile aynı ölçektedir (S ≈ 1–2 gün, M ≈ 3–5 gün, L ≈ 1–2 hafta).

## Kısa özet

Paylaşım Köyü yalnız 24 yeni bölüm değil. Bugünkü istemci tek bölgeli yazılmış (harita patikası, müzik ve değer filmi Sabır Ormanı'na sabit) ve motor balı bırakamıyor. Bu yüzden iş beş akışa ayrılır:

| Akış | Ne | Toplam boyut |
| --- | --- | --- |
| A. Kararlar | Pedagoji onayı ve 4 ürün kararı | (dış) |
| B. Motor ve içerik araçları | Taşıma/bırakma/dağıtma, şema v3, çözücü, doğrulayıcı, bölüm editörü | 2 L + 2 M |
| C. Çok bölgeli istemci | Bölge kilidi, patika, müzik, değer anı, gemi parçaları | 2 M + 3 S |
| D. Oyun ekranı | Sepet, petek, Bırak tuşu, eşit karşılaştırma, "Kaç kaldı?" | 1 L + 2 M |
| E. İçerik, ses, sanat | 24 bölüm, seslendirme, köy sanatı, değer filmi | 2 L + 2 M |
| F. Rapor ve test | Veli raporu, olaylar, PlayMode, çocuk testi | 2 M + 1 S |

Kabaca 3 sprint geliştirme + paralel sanat; çocuk testi ve düzeltme için 1 sprint tampon.

## A. Önce verilmesi gereken kararlar

A1–A4 ürün sahibince kabul edildi (2026-10-11). A1, pedagoji danışmanının önerileriyle karara bağlandı: PK-0001–PK-0005. B ve E başlayabilir. Uzman onayı (G2) yayından önce alınır; köşe 1'in Minik'e erken açılması P1/P6 ile birlikte açık.

| # | Karar | Kimde | Etkisi | Önerim |
| --- | --- | --- | --- | --- |
| A1 | Pedagoji taslağının onayı (7 soru) | Pedagoji ekibi | E1, D3, D4 | **Kabul (2026-10-11):** PK-0001–PK-0005; ADR 0030 güncellendi |
| A2 | Bölge ne zaman açılır? Bugün içeriği olan her bölge açık (PRD: "bölgeler sırayla açılır") | Ürün | C1 | **Kabul (2026-10-11):** Sabır Ormanı'nın son bölümü (bölge finali) bitince |
| A3 | Kaşif çocuk Sabır Ormanı'nı bitirmeden köye geçebilir mi? | Ürün + pedagoji | C1 | **Kabul (2026-10-11):** Hayır; ama Kaşif başlangıcı sayesinde ormanda Minik bölümleri zaten atlanıyor |
| A4 | Gemi parçası sayısı: bugün 7 parça var; Sabır Ormanı 36 bölümle 7 + 1 bölge = 8 parça kazandırıyor, köyle 4 + 1 daha eklenir. PRD hem "her 5 bölümde bir" hem "beş parça tamamlandığında gemi onarılır" diyor | Ürün | C5, E4 | **Kabul (2026-10-11):** Bölüm başına parça yerine bölge başına 1 büyük + köşe başına 1 küçük parça; gemi 5 bölgede tamamlanır |

## B. Motor ve içerik araçları

| ID | İş | Boyut | Bağımlılık | Not |
| --- | --- | --- | --- | --- |
| B1 | **Şema v3:** `drops`, `honey.amount`, `robot.carrying`, `goal.deliver/share/carryAtEnd`, `drop` kartı, `options.autoDrop/askRemaining`, tanıtım işareti ve çalıştırma durumu seçeneği (PK-0005), yeni hikâye nesneleri; `home` yok (PK-0001); v2 → v3 dönüştürme betiği; `make gen` (C# + TS) | M | ADR 0030 kabulü | Sabır Ormanı dosyaları yalnız sürüm numarası değişir. **Yapıldı:** şema v3, `migrate-v3.mjs`, üretilmiş kod; B2'de yükleyici korumasının yerini motor aldı |
| B2 | **Motor:** `RobotState`'e sepet ve teslim sayıları; `DropCommand`; otomatik bırakma; yeni olaylar (`Dropped`, `NothingToDrop`, `HiveOverflow`); hedef denetimi (eşit paylaşmada artan < petek sayısı); taşmada ve boş sepette o kartta durma; köyde dünyanın her çalıştırmada başa dönmesi, Minik'te belleğin kendiliğinden temizlenmesi (PK-0001, PK-0005) | L | B1 | Saf C#, .NET ile test; kapsam ≥ %90 korunur. **Yapıldı:** `HoneyRules`, `RobotState` sepeti ve teslimleri, `DropCommand`, olaylar ve sonuçlar, hedefler, oturum; çözücünün BFS'i de "Bırak" adımını biliyor (bal geçişi tek yerde olsun diye B3'ten öne alındı) |
| B3 | **Çözücü ve ipucu:** yeni durum alanlarıyla BFS, en kısa çözüm, ipucu merdiveni (yanlış/eksik bırakma için kart ipucu), rehberli mod | M | B2 | Süre ölçümü: tüm köy bölümleri doğrulayıcıda < 10 sn. BFS ve Bırak adımı B2'de yapıldı. **Yapıldı (B3):** maliyet ölçümü ve durum sayısı üst sınırları (önbellek gerekmedi, ADR 0030), `RemainingQuestion`, köyde ipucu sesi, bırakma için ipucu ve rehberli mod testleri |
| B4 | **Doğrulayıcı kuralları:** `autoDrop` yalnız Minik; `drop` kartı varsa bırakma noktası; bal yeterli mi; sayılar 1–7; ses anahtarları; "Kaç kaldı?" cevap satırları | S | B1, B3 | `make validate-content`. **Yapıldı:** PK-0001–PK-0005 kuralları (`VillageChecks.cs`, tablo `content/levels/README.md`'de). **Kalan:** "Kaç kaldı?" soru ve cevap satırlarının ses anahtarı kontrolü, satırlar E2'de yazılınca eklenir |
| B5 | **Bölüm editörü:** petek ve sofra yerleştirme, bal miktarı, hedef türleri, yeni kart; editör içi çözücü önizlemesi | L | B1–B3 | `apps/level-editor`; içerik yazımı (E1) bunu bekler. **Yapıldı:** petek/sofra/bal araçları, bal hedefi paneli, köy şablonları, "Yeni köy bölümü", çözüm rotasında bırakma işaretleri; PK kuralları `/solve` yanıtından geliyor |

## C. Çok bölgeli istemci

Bugün `PathView` Sabır Ormanı'na sabit, adaya dokununca hangi bölge seçilirse seçilsin orman patikası açılıyor, müzik tek dosya, değer filmi sabır filmi. Bu işler köy içeriğinden bağımsızdır ve A2 dışında karar beklemez.

| ID | İş | Boyut | Bağımlılık | Not |
| --- | --- | --- | --- | --- |
| C1 | **Bölge kilidi:** sıralı açılma kuralı (`CodingEngine/Progress`, saf C#, testli); kilitli bölgede Roboya'nın sesli açıklaması; premium yoksa ebeveyn kapısı (hak sunucudan, mevcut kural) | M | A2, A3 | Ücretsiz katman değişmez (Sabır Ormanı'nın ilk 3 bölümü) **Yapıldı** (#80 sıralı açılma); C-a: ücretsiz katman yalnız ilk bölgede, ücretsiz sayılan tanıtım bölümü muafiyeti de. |
| C2 | **Patika bölgeye göre:** `PathView` seçilen bölgenin bölümlerini çizer; köşe sayısı ve köşe işaretleri bölge verisinden; "sonraki bölüm" bölge içinde, bölge bitince adaya dönüş | M | — | `MapController.OnRegion` bölge kimliğini kullanır |
| C3 | **Bölge sanatı ve müziği:** `RegionArt` köy seti (bölge karakteri, zemin, süsler); müzik bölgeye göre (kayıt yoksa kodla üretilen yedek) | S | E3 (geçici sanat yeterli) | Lisans: Suno ücretli planı veya özgün (docs/art/music.md) **Yapıldı (C-b):** `RegionArtLibrary` (Resources/Art), köy `RegionArt` seti (geçici sanat), bölgeye göre patika/tahta/hikâye sahnesi sanatı, köşe kütüğünde köy arısı, `IMusicPlayer.SetRegion` (köy kaydı yok: orman döngüsü çalar, lisans kararı `docs/art/music.md`'de) |
| C4 | **Değer anı bölgeye göre:** `ValueCard` bölgenin değer filmini oynatır (paylaşma); köşe sonu anları | S | E4 | **Yapıldı (C-c):** değer anı bölümün `meta.value`'suna göre (`ValueMoments`: `video/value_<değer>.mp4`, `value.<değer>`, `value.<değer>.1…`), sakin kartta bölgenin dostu. Köy filmi ve satırları E2/E4'te; yoksa kart kısa ve sessiz kalır |
| C5 | **Gemi parçaları:** parça kataloğu ve kazanma kuralı A4'e göre; mevcut ilerlemesi olan çocukların parçaları geri alınmaz | S | A4 | Sunucu eşitlemesi yıldızdan türer, değişiklik gerekmez **Yapıldı** (#82 kural); C-a: 8. parça `engine` kataloğa eklendi (köy finali). |

## D. Oyun ekranı (Bal Peşinde genişletmesi)

| ID | İş | Boyut | Bağımlılık | Not |
| --- | --- | --- | --- | --- |
| D1 | **Tahta:** petek (boş/dolu/taşmış yuvalar, şekille), sofra; Ari'nin sepeti (nokta sayacı); bırakma animasyonu ve sesli sayma | L | B2 | Renk tek başına anlam taşımaz; 30 fps, `Update` içinde bellek ayırma yok |
| D2 | **Bırak tuşu:** arının tuş paneline büyük "Bırak" tuşu (≥ 64 dp); kart tanıtım anı; Minik'te gizli (otomatik bırakma) | M | B1 | UI Toolkit |
| D3 | **Eşit paylaşma ve geri bildirim:** bitişte peteklerin yan yana karşılaştırılması; unutulan petek üzgün, taşan petek uyarısı; hedef rozeti yeni hedef türlerini gösterir | M | B2, A1 | Ceza yok |
| D4 | **"Kaç kaldı?" adımı:** başarı sonrası üç nokta kartından seçim; yanlışta birlikte sayma; yıldızı etkilemez | M | A1 (onaylanırsa) | `predict` adımının kalıbı |

## E. İçerik, ses ve sanat

| ID | İş | Boyut | Bağımlılık | Not |
| --- | --- | --- | --- | --- |
| E1 | **24 bölüm:** editörle yazım, çözücüyle doğrulama, `alternativeLevelId`, tanıtım bölümleri, MEB kazanım eşleştirmesi (`meta.mebOutcomes`) | L | A1, B5 | Pedagoji taslağı gerçek kart sayılarıyla güncellenir |
| E2 | **Seslendirme:** ~48 bölüm satırı + sayma (1–7), Bırak kartı tanıtımı, "Kaç kaldı?" soru ve cevapları, taşan/unutulan petek, artan bal, bölge karşılaması ve kilit sesi; önce TTS, sonra profesyonel kayıt (F2-14) | M | E1 | Yalnız senaryo metni TTS'e gider |
| E3 | **Sanat:** köy arka planı, köylü arı ailesi (mutlu/üzgün/teşekkür), yaşlı köy arısı (bölge karakteri), petek durumları, sepet, sofra, şenlik alanı, ada haritasında köyün açılmış hâli; önce geçici set | L | — | F1-23 gibi dış kaynak; stil `docs/art/style-guide.md` |
| E4 | **Paylaşma değer filmi** (30–60 sn) ve köşe sonu anları | M | E3 | Sabır filmiyle aynı üretim yolu (F1-24) |

## F. Rapor, ölçüm ve test

| ID | İş | Boyut | Bağımlılık | Not |
| --- | --- | --- | --- | --- |
| F1 | **Veli raporu ve beceri modeli:** köy bölümlerinin kavram etiketi (A1 soru 7); rapor cümleleri yerelleştirme tablosundan | S | A1 | Çocuk verisine yeni alan yok |
| F2 | **Olaylar ve panolar:** mevcut sabit olay listesi bölge kimliğini zaten taşıyorsa değişiklik yok; değilse olay şeması ve BI görünümleri | S | — | Önce kontrol; kişisel veri yok |
| F3 | **PlayMode akışları:** köyün kilidi → açılması → bölüm aç–oyna–bitir → değer anı → gemi parçası; tüm köy patikasının arayüzden çözülmesi; ekran görüntüleri | M | C, D, E1 | `make unity-playmode` |
| F4 | **Çocuk testi:** 8–10 çocukla gözlem (Minik ve Kaşif), özellikle eşit paylaşma ve "Kaç kaldı?" | M | F3 | `docs/child-testing/protocol.md` |

## Sıra ve sprintler

```
Sprint 1   A1–A4 kararları ......... C1 C2 (karar beklemeyen kısımlar) ... E3 geçici sanat başlar
           B1 şema v3 → B2 motor
Sprint 2   B3 çözücü, B4 doğrulayıcı → B5 editör ... D1 tahta, D2 Bırak tuşu ... C3 C5
Sprint 3   E1 24 bölüm → E2 ses (TTS) ... D3 D4 ... C4 ... F1 F2
Sprint 4   F3 PlayMode, düşük seviye Android tablette deneme, F4 çocuk testi, düzeltmeler
Sonra      E2 profesyonel kayıt (F2-14), E3/E4 kalıcı sanat ve film
```

Her satır ayrı PR'dır (CLAUDE.md §5: bir PR tek amaç). Paralel yürüyebilecekler: C akışı B'den bağımsız; E3 sanat ilk günden başlayabilir.

## Riskler

- **Pedagoji onayı gecikirse** B ve E bekler; C ve E3 yine ilerler. Sabır Ormanı onayındaki kararlar (Minik sırası, bellek görünürlüğü) köyü de etkiler.
- **Çözücü yavaşlaması:** sepet ve teslim sayıları durum uzayını büyütür; sayılar ≤ 7 ve nokta ≤ 4 sınırı doğrulayıcıda zorunlu tutulur.
- **Sanat en uzun kalem:** geçici setle geliştirme sürer, kalıcı sanat lansmanı bekletmemeli.
- **Gemi parçası değişikliği** (A4) mevcut oyuncuların ilerlemesini etkilememeli; kazanılmış parça geri alınmaz.

## Etki değerlendirmesi

- Gizlilik: çocuk profiline alan eklenmiyor; olaylarda kişisel veri yok.
- Ödeme ve hak: köy premium içerik; hak sunucudan, mevcut kural. Ücretsiz katman değişmiyor.
- Performans: yeni tahta öğeleri ve animasyonlar 2 GB tablette 30 fps hedefiyle denenir (D1, F3).
