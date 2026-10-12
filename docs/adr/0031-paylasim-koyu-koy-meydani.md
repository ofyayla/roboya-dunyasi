# 0031 — Paylaşım Köyü: yönsüz "Köy Meydanı" oyunu

- Durum: Önerildi (ürün sahibi yönü onayladı 2026-10-12; pedagoji ekibi onayı bekliyor)
- Tarih: 2026-10-12
- İlgili: F2-13; BAL-01, BAL-03; ADR 0002, 0025, 0030 (yerini alır); `docs/pedagoji/bekleyen-kararlar.md` K8–K12

## Bağlam

ADR 0030 köyü Bal Peşinde'nin ızgara motorunun üstüne kurdu: Ari yön ve dönüş planlıyor, bal ve petek bunun üstüne eklendi. Tablet prototipi (2026-10-12) şunu gösterdi:
- Köyün asıl işi toplama, çıkarma ve eşit paylaşma; yön işi Sabır Ormanı'nda zaten çok çalışıldı ve burada işi gölgeliyor.
- Kodlama çoğu köy bölümünde gerekmiyor: sıra sonucu değiştirmiyor, tekrar hiçbir bölümde zorunlu değil.
- Ekranda sayı dört yerde, dört farklı biçimde gösteriliyor; tahtadaki petek ve sepet resimleri tanınmıyor.

## Karar

1. **Köy ayrı bir oyun olur:** `koy-meydani` (`GameId.KoyMeydani`). Izgara, yön, dönüş, çarpma yok. Bal Peşinde yalnız Sabır Ormanı'nda kalır.
2. **Çocuk Ari'ye bir paylaşım planı verir.** Kartlar: `Çiçekten al`, `Peteğe ver ×n` (sofraya da verilebilir) ve bir `Tekrarla ×k` kutusu. Ari planı uçarak yapar. Sepetteki, çiçeklerdeki ve peteklerdeki bal her an noktalarla görünür.
3. **Dört köşe, dört kavram, yeni bir kodlama fikri her köşede:**
   - Köşe 1 (Minik): birebir eşleme, 3'e kadar sayma; sıralama.
   - Köşe 2: toplama ve eksiği tamamlama; sepet sınırı yüzünden sıra önemli.
   - Köşe 3: çıkarma; çalıştırmadan önce "sepette kaç kalacak?" tahmini.
   - Köşe 4: eşit paylaşma ve artan balın sofraya gitmesi; tekrar kutusu gerçekten gerekli.
4. **Taşan bal yere akar ve çalıştırma sürer;** sonuç sonda değerlendirilir. Sepet doluyken `al`, boşken `ver` ya da boş çiçekten `al` plan o kartta durur ve kart işaretlenir. Ceza yok. (ADR 0030'daki "taşmada o kartta dur" maddesinin yerine.)
5. **Hedefler:** `deliver` (her petek tam `need`) ve `share` (petekler eşit, sepet boş, sofrada artan petek sayısından az).
6. **Saf C# motor** `Roboya.CodingEngine.Sharing` (ADR 0002 gibi .NET ile de test edilir): `SharingLevel`, `SharingState`, `SharingProgram`, `SharingInterpreter`, `SharingRules`, `SharingSolver` (en kısa ve bütün en kısa çözümler), `SharingHint`, `PredictionChoices`. Yıldız mevcut `StarRating`.
7. **Şema:** tek `level.schema.json`; `game = koy-meydani` olunca `sharing` nesnesi zorunlu, `grid/robot/goal/cards` değil. `schemaVersion` 3 kalır (ızgara köy alanları yayınlanmamıştı; kaldırılır). Doğrulayıcıda `SharingChecks`.
8. **Ekran** tek `Game` sahnesinde ve `Roboya.Games.Common` assembly'sindeki `Roboya.Games.Square` ad alanında (`Games/Common/Square/`; yeni asmdef gerekmedi, `GameScreen` ile döngü olmaz): `GameScreen` oyuna göre `SquareController` ya da `GameController` seçer. Sahne ve UXML kalır (yalnız `Sharing.uss` eklenir); ekran kodla kurulur. Ortak akış (analitik, kazanılan en iyi sonuç ve ödül, "sonraki") `Roboya.Core.LevelFlow` içinde paylaşılır, `GameController` de onu kullanır.
9. **Yönlendirme dört katmanlı:** resimle amaç (istek balonu, nefes alan boş hücreler), sesli yönerge ve dinle düğmesi, köşenin ilk bölümünde adım adım el, takılınca 8 sn sonra ya da başarısız çalıştırmadan sonra el. El, çözücünün bütün geçerli çözümlerine göre yönlendirir.
10. **Görsel dil:** sepet büyük ve Ari'nin yanındadır (hasır, kulplu, açık ağızlı; içindeki bal kodla noktalarla çizilir); kovan saman kovan resmidir ve hücre içermez; petek hücreleri **kodla çizilir** (kenar paylaşan altıgenler, boş krem, dolu amber). Böylece hem petek tanınır hem sayı resme bağlı kalmaz. Resimler (`item_basket_empty`, `item_hive_skep`) Higgsfield ile üretildi; resim yoksa aynı şeyler kodla çizilir (`BasketShape`, `SkepShape`).
11. **Eski ızgaralı köy silinir:** 24 bölüm, `VillageChecks`, motor bal kodu, köy görünümleri ve testleri. Yayında değildi; git geçmişinde kalır. Sabır Ormanı etkilenmez.

## Sonuçlar

- ADR 0030 yerini alır. PK-0001, PK-0003 ve PK-0005'in ızgara maddeleri (otomatik bırakma, `Bırak` kartı, yön ve dönüş sınırları, bellek ve dünya sıfırlama, taşmada dur) geçersiz olur; PK-0002'nin "Kaç kaldı?" fikri tahmin adımı olarak sürer. Pedagoji ekibi yeni kararı verene kadar bölümler `[Taslak, pedagoji incelemesi bekliyor]` notuyla gelir (`bekleyen-kararlar.md` K8–K12).
- Köy bölgesi yeni içerik gelene kadar oynanamaz; gemi parçası `engine` köy finaline bağlı olduğundan o zamana kadar kazanılamaz.
- Bölüm editörünün köy araçları kalkar; köy bölümleri JSON olarak elle yazılır, doğrulayıcı güvencedir. Yeni editör ayrı iş.
- Teslim sırası: önce Köşe 1 dikey dilimi (6 Minik bölümü) ve çocuk testi, sonra Köşe 2–4.
