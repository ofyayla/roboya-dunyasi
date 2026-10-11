# 0030 — Paylaşım Köyü: bal taşıma, bırakma ve dağıtma

- Durum: Önerildi (tasarım; pedagoji onayından sonra uygulanacak)
- Tarih: 2026-10-11
- İlgili: F2-13; BAL-01, BAL-03; ADR 0018, 0025, 0026; `docs/pedagoji/paylasim-koyu-taslagi.md`

## Bağlam

Paylaşım Köyü'nde Ari balı toplayıp köyün peteklerine götürür; bölümler toplama, çıkarma ve eşit paylaşma öğretir (BAL-03). Motor bugün yalnız "şuraya var" (`goal.reach`) ve "şunları topla" (`goal.collect`) hedeflerini biliyor. Robot durumu konum, yön ve toplanan nesnelerin bit maskesinden ibaret (`RobotState`); bir nesne bir kez toplanır, bir yere bırakılamaz. `ActionCommand` var ama dünyayı değiştirmez, yalnız görünüm yorumlar. Bu yüzden dağıtma, içerikle değil motor genişletmesiyle mümkün.

Ürün kararı (2026-10-11): bırakma **Minik'te otomatik**, **Kaşif'te "Bırak" kartıyla**; bölge Minik ve Kaşif'i kapsar.

## Karar (önerilen)

Bal Peşinde'nin ortak motoru genişletilir; motor çatallanmaz (CLAUDE.md §7).

1. **Dünya: petek ve sofra.** Bölüme `drops` listesi eklenir: `{ id, x, y, kind: "hive" | "table" | "home", need? }`. Hücre yürünebilirdir; arı üstüne gelince bırakma mümkün olur. `hive` bir arı ailesinin peteği, `table` ortak köy sofrası, `home` Ari'nin kovanı (kalan bal).
2. **Taşıma.** `honey` nesnesine `amount` (varsayılan 1) eklenir; toplanınca miktar sepete girer. Bölüm `robot.carrying` ile sepette balla başlayabilir (çıkarma bölümleri).
3. **Robot durumu.** `RobotState`'e sepet sayısı ve bırakma noktası başına teslim sayısı eklenir (küçük sayılar; tek bir `ulong` içinde 4 bit × en çok 8 nokta + sepet). Eşitlik ve karma bu alanları içerir; böylece çözücü (BFS) yeni hedefleri de arar.
4. **Bırakma.**
   - `cards.palette` içinde yeni `drop` kartı (`CardType.Drop`, gerçek bir komut; `ActionCommand` değil). Bulunduğu noktaya bir bal bırakır. Sepet boşsa ya da nokta yoksa yürütme nazikçe durur (`NothingToDrop`), ceza yok.
   - `options.autoDrop: true` (yalnız Minik): arı bir `hive` noktasına varınca petek `need` dolana kadar sepetten alır; fazlası verilemez.
   - Manuel bırakmada petek `need`'i aşabilir; bu hedefi bozar ve `HiveOverflow` olayıyla açıklanır.
5. **Hedefler (`goal`).**
   - `deliver: [{ to, count }]` — noktaya tam bu kadar bal.
   - `share: { among: [...], leftoverTo? }` — listedeki petekler eşit, tüm bal dağıtılmış; artan varsa `leftoverTo` noktasında.
   - `carryAtEnd` — sepette kalması gereken bal (çıkarma).
   - Mevcut `reach` ve `collect` aynen kalır ve birlikte kullanılabilir.
6. **"Kaç kaldı?" adımı** (`options.askRemaining`), Kodlama Kutusu'ndaki `predict` adımı gibi başarı sonrası bir görünüm adımıdır; motor hedefi değildir, yıldızı etkilemez. Pedagoji onayına bağlıdır.
7. **Bellek (ADR 0026) aynen geçerli:** `keepsState` kipinde sepet ve teslimler de çalışmalar arasında korunur; Temizle yalnız belleği siler.
8. **Şema v3:** yeni alanlar isteğe bağlıdır; v2 bölümler için dönüştürme betiği yalnız `schemaVersion`'ı artırır. `make gen` ile C# ve TS yeniden üretilir. Doğrulayıcıya kurallar: `autoDrop` yalnız Minik bölümünde; `drop` kartı varsa en az bir bırakma noktası; `deliver`/`share` hedefi için toplam bal yeterli; `amount` ve `need` 1–7.
9. **Hikâye ve sanat:** `story.props`'a köylü arı, petek (yuvalı), sofra; bölge karakteri olarak köy arısı. `RegionArt` Paylaşım Köyü seti; eksikse kodla çizilmiş yedek.
10. **Beceri modeli:** yeni bölümler `counting` kavramıyla etiketlenir; ayrı bir "toplama-çıkarma / eşit paylaşma" kavramı pedagoji kararına bırakıldı.

## Sonuçlar

- Motor, çözücü ve yorumlayıcıda değişiklik: `make test-engine` kapsamı (≥ %90) yeni olay ve hedefler için testlerle korunur. Çözücünün durum uzayı büyür, ama bölümlerde sayılar ≤ 7 ve nokta sayısı ≤ 4 olduğundan BFS küçük kalır; doğrulayıcıda süre ölçülür.
- Arı tuş paneline "Bırak" tuşu eklenir (UI Toolkit); dokunma hedefi ≥ 64 dp.
- Sabır Ormanı bölümleri etkilenmez (yalnız şema sürümü).
- Gizlilik, ödeme ve hak etkisi yok; bölge premium içeriktir ve kilidi mevcut kurallarla açılır.
- Açık: "Kaç kaldı?" adımı, Minik'te fazla bal verilebilmesi ve kavram etiketi pedagoji kararını bekliyor.
