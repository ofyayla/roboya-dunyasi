# 0030 — Paylaşım Köyü: bal taşıma, bırakma ve dağıtma

- Durum: Kabul edildi (2026-10-11; pedagoji kararları PK-0001–PK-0005)
- Tarih: 2026-10-11
- İlgili: F2-13; BAL-01, BAL-03; ADR 0018, 0025, 0026; `docs/pedagoji/paylasim-koyu-taslagi.md`

## Bağlam

Paylaşım Köyü'nde Ari balı toplayıp köyün peteklerine götürür; bölümler toplama, çıkarma ve eşit paylaşma öğretir (BAL-03). Motor bugün yalnız "şuraya var" (`goal.reach`) ve "şunları topla" (`goal.collect`) hedeflerini biliyor. Robot durumu konum, yön ve toplanan nesnelerin bit maskesinden ibaret (`RobotState`); bir nesne bir kez toplanır, bir yere bırakılamaz. `ActionCommand` var ama dünyayı değiştirmez, yalnız görünüm yorumlar. Bu yüzden dağıtma, içerikle değil motor genişletmesiyle mümkün.

Ürün kararı (2026-10-11): bırakma **Minik'te otomatik**, **Kaşif'te "Bırak" kartıyla**; bölge Minik ve Kaşif'i kapsar.

## Karar (önerilen)

Bal Peşinde'nin ortak motoru genişletilir; motor çatallanmaz (CLAUDE.md §7).

1. **Dünya: petek ve sofra.** Bölüme `drops` listesi eklenir: `{ id, x, y, kind: "hive" | "table", need? }`. Hücre yürünebilirdir; arı üstüne gelince bırakma mümkün olur. `hive` bir arı ailesinin peteği, `table` ortak köy sofrası. Ari'nin kovanı (`home`) yoktur: kalan bal sepette kalır (PK-0001).
2. **Taşıma.** `honey` nesnesine `amount` (varsayılan 1) eklenir; toplanınca miktar sepete girer. Bölüm `robot.carrying` ile sepette balla başlayabilir (çıkarma bölümleri).
3. **Robot durumu.** `RobotState`'e sepet sayısı ve bırakma noktası başına teslim sayısı eklenir (küçük sayılar; tek bir `ulong` içinde 4 bit × en çok 8 nokta + sepet). Eşitlik ve karma bu alanları içerir; böylece çözücü (BFS) yeni hedefleri de arar.
4. **Bırakma.**
   - `cards.palette` içinde yeni `drop` kartı (`CardType.Drop`, gerçek bir komut; `ActionCommand` değil). Bulunduğu noktaya bir bal bırakır. Sepet boşsa ya da nokta yoksa yürütme o kartta nazikçe durur (`NothingToDrop`), ceza yok.
   - `options.autoDrop: true` (yalnız Minik): arı bir `hive` hücresine girince petek `need` dolana kadar sepetten alır; fazlası verilemez. Minik bölümlerinde toplam bal toplam ihtiyaca eşittir (PK-0001).
   - Manuel bırakmada bir bal peteğin `need`'ini aşarsa bal görünür biçimde taşar, yürütme o kartta durur ve `HiveOverflow` olayıyla açıklanır (PK-0001).
5. **Hedefler (`goal`).**
   - `deliver: [{ to, count }]` — noktaya tam bu kadar bal.
   - `share: { among: [...], leftoverTo? }` — listedeki petekler eşit, tüm bal dağıtılmış ve `leftoverTo`'daki artan petek sayısından az (en büyük eşit pay tek doğru dağıtımdır). `leftoverTo` yalnız `table` olabilir. Hedef denetimi bunu oyun sırasında da uygular (PK-0001).
   - `deliver` hedefinde sahnede sofra yoksa sepette bal kalabilir.
   - `carryAtEnd` — sepette kalması gereken bal (çıkarma).
   - Mevcut `reach` ve `collect` aynen kalır ve birlikte kullanılabilir.
6. **"Kaç kaldı?" adımı** (`options.askRemaining`), Kodlama Kutusu'ndaki `predict` adımı gibi başarı sonrası bir görünüm adımıdır; motor hedefi değildir, yıldızı etkilemez, deneme sayılmaz. Üç seçenek ("başta vardı", "verdiğim", "kalan") motordan türetilir ve birbirinden farklı olmalıdır. Çocuk testine kadar özellik bayrağıyla kapalıdır (PK-0002).
7. **Bellek ve dünya (PK-0005):** köy bölümlerinde dünya her çalıştırmada başa döner (arı, sepet, petekler); bellek ise Kaşif'te Temizle'ye kadar korunur, Minik'te her çalıştırmadan sonra kendiliğinden temizlenir. Davranış bölüm verisinde açık bir seçenekle tanımlanır (`options.resetEachRun`, `options.clearMemoryEachRun`). Kart düzeyindeki ipucu bu bölümlerde açıktır. Sabır Ormanı'nın `keepsState` davranışı değişmez.
8. **Şema v3:** yeni alanlar isteğe bağlıdır; v2 bölümler için dönüştürme betiği yalnız `schemaVersion`'ı artırır. `make gen` ile C# ve TS yeniden üretilir. Köşe başlarını açıkça işaretlemek için `options.intro` tanıtım bayrağı eklenir (ADR 0018'in tanıtım kuralı bunu da okur; PK-0005). Yükleyici, motor desteği (B2) gelene kadar bu alanlı bölümleri açıkça reddeder. Doğrulayıcıya kurallar: `autoDrop` yalnız Minik bölümünde ve toplam bal = toplam ihtiyaç; `drop` kartı varsa en az bir bırakma noktası; `deliver`/`share` hedefi için toplam bal yeterli; `leftoverTo` yalnız `table`; toplam bal ≤ 7, `need` ≤ 4 (köşe finalinde ≤ 5), `drop` sayısı ≤ 6; Kaşif'te en kısa çözüm ≤ 10 kart; `askRemaining` seçenekleri birbirinden farklı (PK-0002, PK-0003).
9. **Hikâye ve sanat:** `story.props`'a köylü arı, petek (yuvalı), sofra; bölge karakteri olarak köy arısı. `RegionArt` Paylaşım Köyü seti; eksikse kodla çizilmiş yedek.
10. **Beceri modeli:** yeni bölümler `counting` kavramıyla etiketlenir; yeni kavram yok. Veli raporundaki görünen ad "Sayma ve hesap" olur (PK-0004).

## Sonuçlar

- Motor, çözücü ve yorumlayıcıda değişiklik: `make test-engine` kapsamı (≥ %90) yeni olay ve hedefler için testlerle korunur. Çözücünün durum uzayı büyür, ama bölümlerde sayılar ≤ 7 ve nokta sayısı ≤ 4 olduğundan BFS küçük kalır; doğrulayıcıda süre ölçülür.
- Arı tuş paneline "Bırak" tuşu eklenir (UI Toolkit); dokunma hedefi ≥ 64 dp.
- Sabır Ormanı bölümleri etkilenmez (yalnız şema sürümü).
- Gizlilik, ödeme ve hak etkisi yok; bölge premium içeriktir ve kilidi mevcut kurallarla açılır.
- Pedagoji kararları: PK-0001 (bırakma, paylaşma, artan), PK-0002 ("Kaç kaldı?"), PK-0003 (sınırlar), PK-0004 (kavram), PK-0005 (erişim ve çalıştırma durumu). Uzman onayı yayından önce alınır.
