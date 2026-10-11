# Paylaşım Köyü Köşe 1 (Minik, 1–6. bölümler) — pedagoji ön incelemesi

> Yapay zekâ ön incelemesidir; pedagoji ekibi onayının yerine geçmez.

- Tarih: 2026-10-11
- Kapsam:
  - `content/levels/paylasim-koyu/bal-pesinde/01.json`–`06.json` (dal `content/paylasim-koyu`, commit'lenmemiş)
  - `content/voice/script.csv`: `bal_pesinde.koy.l01`–`l06` (`intro`, `success`) ve `roboya.hint.koy.listen`
  - Karşılaştırma için okunanlar: Sabır Ormanı Minik bölümleri (yön ve başlangıç bakışı); `GameController.cs`, `HiveFeedback.cs`, `BasketView.cs`, `RegionArt.cs` (yalnız çalışma anındaki ses ve görüntüyü anlamak için)
- İlgili: F2-13, BAL-01, BAL-03, YZ-01, YZ-03, OYN-01, OYN-03; PK-0001–PK-0005; `docs/pedagoji/paylasim-koyu-taslagi.md`
- Sınır: Oyun çalıştırılmadı, `make validate-content` koşturulmadı. Ses ve görüntüyle ilgili tespitler kod okumasına dayanıyor ve "doğrulanmalı" diye işaretlendi.

## Kısa sonuç

Altı bölüm kabul edilmiş kararlara (PK-0001–PK-0005) harfiyen uyuyor:
- Toplam bal toplam ihtiyaca eşit.
- Paletlerde yalnız ileri ve sağa dön var.
- 1. bölüm tanıtım ve rehberli, 4. bölümde hayalet yol açık.
- 4. bölümde arı yukarı bakarak başlıyor ve tek bir sağa dönüş yapıyor.
- En kısa çözümler 3–4 kart, şerit +1.
- Ses kısa ve sıcak, ceza dili yok.

4. bölümde `cards.introduces` olmaması doğru. Sağa dön ormanın 3. bölümünde tanıtılıyor; PK-0005 madde 1'in "yalnız ormanın 1–9. bölümlerinde öğrenilen kartlar" koşulu sağlanıyor.

En önemli üç bulgu:
1. **Unutulan petek için ses yok.** Minik'te en sık beklenen hata, bir peteği boş bırakmak. Bu durumda çalınması gereken `bal_pesinde.hive_forgotten` ve `bal_pesinde.not_equal` satırları senaryoda yok. Kod da genel "daha varamadık" satırına düşmüyor, yani çocuk sessizlikle karşılaşıyor.
2. **5. ve 6. bölümde ters yön.** Arı doğuya bakarak başlıyor; sağa dönünce aşağı gidiyor. Ormanın Minik koridorunda (1–9) her dönüş yukarı bakan robotla yapılıyor. Bu ilk "robotun sağı ≠ çocuğun sağı" anı 5. bölümde 1 + 1 yeniliğiyle ve desteğin kalkmasıyla aynı yere düşüyor.
3. **Ses ile tahta uyuşmuyor.** Ses "çiçek" ve "üç bal" diyor, ama tahtada çiçek yok ve tek bal damlası görünüyor. Canlı sayma da iki ayrı sayıyı karıştırıyor ("bir, bir, iki, bir").

`Engel` bulgusu yok.

## Bulgular

| # | Önem | Bakış | Bulgu | Dayanak | Öneri | İnsan kararı |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Önemli | Hata ve motivasyon | Bir petek boş kaldığında (Forgotten) veya eksik dolduğunda (NotEqual) kod `bal_pesinde.hive_forgotten` / `bal_pesinde.not_equal` satırını çalıyor. Bu satırlar `script.csv`'de yok. Bu dal genel `not_there` / `missing_items` satırlarının yerine geçtiği için çocuk hiçbir ses duymuyor. Taslaktaki 4. bölüm satırı ("Bu arının hiç balı yok") yazılmamış. | R-MOT-1, R-GEL-3, OYN-01, OYN-03; taslak 4. bölüm notu | Çocuk testinden önce iki satır yazılsın. `not_equal` Minik'in `deliver` bölümlerinde de çalıyor; metni "eşit" demeden eksik yuvayı göstermeli, ya da anahtar ikiye ayrılmalı (geliştirici). | Hayır |
| 2 | Önemli | Gelişimsel uygunluk / iskele | 5. ve 6. bölümde arı doğuya bakarak başlıyor; sağa dön onu aşağı çeviriyor. Ormanın Minik koridorunda (1–9) bütün dönüşler kuzeye bakan robotla. Doğuya bakan ilk başlangıç YA06 (12. bölüm), ve orada ayrı bir yenilik olarak tanıtılıyor. 5. bölüm bunu 1 + 1 yeniliğiyle ve desteksiz (hayalet yol yok, bellek gizli) birleştiriyor. | R-GEL-5, R-ILR-1, R-ILR-3; PK-0005 madde 1 (erken açma için 1–9 bilgisi) ve madde 5'in gerekçesi | Seçenek A: 5. bölüm düz yol (3 kart), 6. bölüm kuzeyden başlasın. Seçenek B: düzen kalsın, 5. bölüme hayalet yol eklensin. Ayrıntılar aşağıda. | Evet |
| 3 | Önemli | Ses ve dil / görsel | Bal öğesi tahtada tek bal damlası olarak çiziliyor (`RegionArt.ItemFor`). Ses 1, 3 ve 5. bölümde "çiçek" diyor. `amount` (3. bölümde 3, 4'te 2, 6'da 3) tahtada görünmüyor; yalnız sepetteki boş halkalar toplamı gösteriyor. 3. bölümün dersi "üçe kadar sayma", ama ekranda tek damla var. | R-DIL-3, R-ERS-3, çerçeve "Seste adı geçen her şey ekranda görünür" | Seste "çiçek" yerine "bal" denmeli. Miktar tahtada görünmeli (yığılı damlalar ya da damla altında nokta); bu çizim işi geliştirici ve sanat ekibine kalır. | Hayır |
| 4 | Önemli | Ses ve dil (sayma) | Canlı sayma iki ayrı sayıyı karıştırıyor (kod okuması, doğrulanmalı). Toplanan öğe sayısı ile peteğe düşen bal sayısı ayrı ayrı söyleniyor. 2. bölümde duyulan sıra "Bir (bal) – Bir (petek 1) – İki (bal) – Bir (petek 2)". 3. bölümde "Bir – Bir – Bir" duyuluyor, ama başarı sesi "Bir, iki, üç!" diyor. | BAL-01, R-DIL-2, çerçeve: Minik "somut, anlık neden-sonuç" | Otomatik bırakmalı Minik bölümlerinde tek bir şey sayılsın. Ön tercih: dolan petek sayısı ("bir, iki, üç") ya da teslim edilen balın toplamı. | Evet |
| 5 | Öneri | Kavram ilerleyişi | Çözümler İİİ, İİİİ, İİİİ, İİSİ, İİSİ, İİSİ; zorluk 1-1-1-2-2-2. 4–6. bölümler tuş dizisi olarak birebir aynı. Çocuk sayıya bakmadan son kodu tekrarlayabilir. | R-ILR-4, R-ILR-6 | 6. bölüm kuzeyden başlasın ve rota İ S İ İ olsun (bulgu 2'nin A seçeneğiyle birlikte). | Hayır |
| 6 | Öneri | Kavram ilerleyişi | 6. bölümün "daha kolay" alternatifi 5. bölüm. Aynı tuş dizisi, aynı başlangıç bakışı, aynı zorluk; gerçekten kolay değil. | R-ILR-7, YZ-03 | Alternatif 4. bölüm olsun (hayalet yollu, kuzeyden dönüş). | Hayır |
| 7 | Öneri | Hata ve motivasyon | 2. ve 3. bölümde son petek tahtanın kenarında (x = 4). Bir fazla "ileri" basılırsa bütün petekler dolu olsa bile arı çarpıyor ve çalıştırma başarısız sayılıyor. 1, 4, 5 ve 6. bölümde aynı fazla adım başarı sayılıyor. Aynı hata bölümden bölüme farklı sonuç veriyor. | Çerçeve "somut neden-sonuç", R-MOT-1 | 2–3. bölümde ızgara bir sütun genişlesin. Ya da kural düzeyinde "teslim hedefi tamamlanınca çalıştırma biter" seçilsin (geliştirici ve ürün). | Evet |
| 8 | Öneri | Ses ve dil (ipucu) | `roboya.hint.koy.listen` iki sorun taşıyor. "Hangi peteğe gidiyoruz?" tekil; 2, 3, 4 ve 6. bölümde cevap "hepsine". Ormandaki karşılığının yön sorusu ("Ben hangi yöne bakıyorum?") düşmüş, oysa 4–6. bölümde asıl güçlük dönüş. | R-MOT-2, R-DIL-2 | Örnek: "Bal nerede? Boş petek kaldı mı?" Dönüşlü bölümler için ayrı bir satır: "Arı hangi yöne bakıyor?" | Hayır |
| 9 | Öneri | Ses ve dil | Küçük metin düzeltmeleri. 3. bölümün başarı sesinde çift olumsuz var ("Hiçbir petek boş kalmadı"). 4. bölüm "yolun kenarında" diyor ama tahtada yol çizili değil. 6. bölümdeki "Biri bir, öbürü iki" dil dolaştırıyor. 1. bölüm girişindeki sahne nesneleri (`honey`: kavanoz, `beehive`: kovan) "petek bomboş" sesiyle uyuşmayabilir (doğrulanmalı). | R-DIL-1, R-DIL-3 | Önerilen metinler aşağıda. | Hayır |
| 10 | Öneri | Kavram ilerleyişi | 4–6. bölümler yalnız `counting` etiketli. Dönüş yüzünden yapılan hatalar beceri modelinde (YZ-02) sayma zayıflığı gibi görünür. | YZ-02; PK-0004 (yeni kavramı yasaklıyor, var olan etiketi değil) | 4–6. bölümlere `counting` ile birlikte `direction` eklensin. PK-0004 ile çelişmez, yine de ekip bir bakmalı. | Hayır |
| 11 | Öneri | Belge | `paylasim-koyu-taslagi.md` hâlâ "Henüz bölüm dosyası yok" diyor. Kart sayıları tahmini (2. bölüm 3, JSON'da 4). 5. bölümdeki dönüş taslakta yok. | PK-0003 "Sonuçlar" | Tablo JSON'dan güncellensin; başlangıç bakışı ve destek sütunları eklensin. | Hayır |

Bu bakışlarda sorun bulunmadı:
- **Değer gömülmesi:** Bölüm, her petek dolmadan bitmiyor (R-DEG-1). Her girişte bir ihtiyaç var ("peteği bomboş"). "Kimseyi unutmayalım" kapsayıcı birinci çoğul; vaaz tonu yok.
- **§13 sınırları:** Can, süre, yıldız kaybı veya satış dili yok.

## Ayrıntılar

### Kararlara uyum (PK-0001–PK-0005)

| Karar maddesi | Durum |
| --- | --- |
| PK-0001/1: Minik'te otomatik bırakma, toplam bal = toplam ihtiyaç | Uyuyor (1=1, 2=2, 3=3, 2=2, 2=2, 3=3) |
| PK-0003: toplam bal ≤ 7, ihtiyaç ≤ 4 | Uyuyor (en çok 3 bal, en çok ●●) |
| PK-0004: `counting` etiketi | Uyuyor (bkz. bulgu 10) |
| PK-0005/1: altı bölüm; birebir eşleme, 3'e kadar sayma, 1 + 1; çıkarma ve eşit paylaşma yok; yalnız ileri ve sağa dön | Kart düzeyinde uyuyor. Başlangıç bakışı düzeyinde 5–6. bölüm ormanın 1–9'unda olmayan bir şey istiyor (bulgu 2). |
| PK-0005/2: 1. bölüm `intro` | Uyuyor |
| PK-0005/3: `resetEachRun` her yerde; Minik'te `clearMemoryEachRun`, bellek gizli | Uyuyor |
| PK-0005/4: 1 ve 4'te `guided` ya da `ghostPath` | Uyuyor (1: rehberli; 4: hayalet yol) |
| PK-0005/5: 4. bölüm yalnız sağa dön, arı yukarı bakar, petek sağda, tek dönüş | Uyuyor (İ İ S İ) |
| PK-0002: `askRemaining` Minik'te yok | Uyuyor |

### Çocuğun yolculuğu (Minik, köy 1'den)

Kısaltmalar:
- **R**: rehberli
- **H**: hayalet yol
- **B**: bellek. Altı bölümde de gizli ve her çalıştırmadan sonra temizleniyor; dünya da her çalıştırmada başa dönüyor.

| Sıra | En kısa / şerit | Palet | Başlangıç | Destek | Çözüm | Yenilik |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 3 / 4 | ileri | doğu | R (tanıtım) | İ İ İ | Köy, sepet, petek yuvası, otomatik bırakma, arının her çalıştırmada başa dönmesi |
| 2 | 4 / 5 | ileri | doğu | — | İ İ İ İ | İki petek; bal-petek-bal-petek sırası (birebir) |
| 3 | 4 / 5 | ileri | doğu | — | İ İ İ İ | Tek noktada 3 bal, üç petek; sepetin 3→0 görünür azalması |
| 4 | 4 / 5 | ileri, sağ | **kuzey** | H | İ İ S İ | Köyde ilk dönüş; ikinci petek köşede |
| 5 | 4 / 5 | ileri, sağ | **doğu** | — | İ İ S İ | İki ayrı bal → tek ●● petek (1 + 1); doğuya bakarken sağa dönüş (aşağı) |
| 6 | 4 / 5 | ileri, sağ | doğu | — | İ İ S İ | Farklı ihtiyaçlar (● ve ●●); tek noktada 3 bal |

**Oturum.** Minik oturumu 10–15 dakika. Bölüm başına 1,5–2,5 dakika varsayılırsa (çocuk testinde ölçülmeli) köşenin tamamı tek oturuma sığıyor. Oturumda yaklaşık beş yenilik var; çoğu algısal ve küçük. Asıl yük 1. bölümde (beş yeni öğe; rehber bunu karşılıyor) ve 5. bölümde (bulgu 2).

**Kural değişimi.** Ormanda Minik çocuk Bal Peşinde'yi "arı durduğu yerde kalır" kuralıyla oynadı (BP01, BP02). Köyde arı başa dönüyor. Bu PK-0005 madde 3'ün bilinçli kararı ve uçarak dönme animasyonuyla görselleştiriliyor; burada yalnız gözlem maddesi olarak anılıyor.

**Bal Peşinde'de dönüş tuşu.** Ormanın Minik Bal Peşinde bölümlerinde (BP01, BP02) palet yalnız ileri. Arının sırtında dönüş tuşunu Minik çocuk ilk kez ormanın 11. bölümünde (BP03, Kaşif) görüyor. Köşe 1 erken açılırsa (PK-0005'te açık bırakılan seçenek) köy 4 bu çocuğun arıyla ilk dönüşü olur. Hayalet yol bunu karşılıyor; `cards.introduces` gerekmiyor. Kartın kendisi tanıtılmış. Ayrıca `card.turn_right.intro` Roboya'nın ağzından ("Olduğum yerde sağa dönerim") yazılmış, arıya uymuyor.

### Bulgu 1: Unutulan petekte sessizlik

- **Ne görülüyor:**
  - `GameController` başarısız çalıştırmadan sonra petekleri değerlendiriyor (`HiveFeedback`). "Bir petek bal aldı, öteki almadı" ise `bal_pesinde.hive_forgotten` çalıyor. "Bir petek ihtiyacından az aldı" ise `bal_pesinde.not_equal` çalıyor. Bu dallar genel `roboya.not_there.01` / `roboya.missing_items.01` satırlarının yerine geçiyor.
  - Kod yorumu bu satırların "köy ses işiyle (E2) gelene kadar sessiz" olduğunu söylüyor. Bu dal E2'nin bölüm satırlarını ekliyor ama bu iki satırı eklemiyor.
- **Hangi bölümlerde:**
  - 2, 3, 4 ve 6. bölüm: bir eksik "ileri" bir peteği boş bırakır ve sessizlik olur.
  - 5. bölüm: tek bal alınıp peteğe varılırsa NotEqual olur, yine sessiz.
  - 6. bölüm: ●● peteğe varılmazsa Forgotten olur.
- **Neden önemli:** Minik'te yönerge metni yok (OYN-01). Hata sonrası yön ses ve görüntüyle verilmeli. Görüntü var (üzgün köy arısı, karşılaştırma görünümü, duruş halkası), ama "kimse unutulmasın" 4. bölümün ana fikri ve sesle söylenmesi planlanmıştı.
- **Öneri:**
  - `hive_forgotten`: "Bu petek boş kaldı. Ona da bal götürelim mi?" (suçlamasız; "unuttun" değil).
  - `not_equal`: Minik `deliver` bölümünde de çaldığı için "eşit" sözcüğü kullanılmamalı. Örnek: "Bir yuva hâlâ boş. Bir bal daha lazım." Köşe 3'te gereken "eşit değil" cümlesi ayrı bir anahtar ister. Bu ayrım geliştiricinin işi; pedagojik gereksinim, Minik'e "eşit" sözcüğünün gitmemesi.

### Bulgu 2: 5. ve 6. bölümde doğuya bakan başlangıç

- **Ne görülüyor:**
  - 5. bölüm: arı (0,0)'da doğuya bakıyor. Bal (1,0) ve (2,0)'da, petek (2,1)'de, yani aşağıda. Çözüm İ İ S İ; sağa dön arıyı ekranın aşağısına çeviriyor.
  - 6. bölüm: aynı geometri.
  - Ormanın Minik koridorundaki bütün dönüşlü bölümler kuzeyden başlıyor: YA03, KK01, YA04, YA05, KK02. Kuzeye bakan robotun sağı çocuğun sağı. Doğuya bakan ilk başlangıç YA06 (12. bölüm); orada sesi bile ayrı bir yenilik olarak kuruyor ("Benim gözlerimle düşünelim").
- **Neden önemli:**
  - Çerçeveye göre başka bir varlığın sağını düşünmek Minik için kendi sağından daha zor.
  - PK-0005 4. bölümü bu yüzden kuzeyden kurdu. Ama aynı gerekçe 5. bölümde terk ediliyor; üstelik 5. bölüm 1 + 1 yeniliğini taşıyor, hayalet yol kalkıyor ve bellek gizli.
  - Köşe 1 ormanın 9. bölümünden sonra erken açılırsa çocuk YA06'yı hiç görmemiş olur. PK-0005 madde 1'in amacı bu seçeneği açık tutmaktı.
- **Neden `Engel` değil:** Palette tek dönüş kartı var. Çocuk yanlış dönüş kartı seçemez; hata yalnız dönüşü yanlış yere koymak olabilir. İpucu kademeleri de doğru kartı gösteriyor. Çocuk takılmaz, ama 5. bölümün asıl dersi olan 1 + 1, yön güçlüğünün gölgesinde kalır.
- **Seçenekler:**

| Seçenek | Ne değişir | Artı | Eksi |
| --- | --- | --- | --- |
| **A** | 5. bölüm düz yol: arı doğuya bakıyor, bal (1,0) ve (2,0), petek ●● (3,0); İ İ İ, 3 kart. 6. bölüm kuzeyden: arı (0,2) kuzey, 3 bal (0,1), petek ● (1,1), petek ●● (2,1); İ S İ İ, 4 kart. | 5. bölümde tek yenilik (1 + 1). 6. bölüm 4'teki kuzey dönüşünü farklı bir rotayla pekiştiriyor; bulgu 5'i de çözüyor. PK-0005/1 ile tam uyumlu. | İki dosya ve 5–6. bölüm sesleri hafif değişir. Doğuya bakarken dönüş Minik köyde hiç çalışılmaz (bu zaten Kaşif'e ait). |
| **B** | Düzen kalır; 5. bölüme `ghostPath` eklenir. | En az iş. PK-0005 madde 4 yalnız en az desteği tanımlıyor, eklemek çelişmez. | 5. bölümde yine iki yenilik; 6. bölüm desteksiz aynı güçlüğü taşır ve 4–6 tekrarı sürer. |
| **C** | Düzen kalır, değişiklik yok. | — | Erken açma seçilirse bulgu ağırlaşır. |

Ön tercih: A.

### Bulgu 3 ve 4: Ses, tahta ve sayma

- **"Çiçek":** Köy bölümlerinde öğe türü `honey`; `RegionArt.ItemFor` bunu tek bal damlası olarak çiziyor (`item_honey_drop`). 1. ("Çiçekten bal alıp"), 3. ("Bu çiçekte üç bal var") ve 5. bölüm ("İki çiçekte birer bal") çiçekten söz ediyor. Arka planın kenarındaki ayçiçekleri karışıklığı artırabilir: çocuk oraya bakar.
- **Miktar:** 3. bölümde tahtada tek damla var, ses "üç bal" diyor. Sepette üç boş halka görünüyor (`BasketView`), bu iyi bir dayanak. Ama sayının kaynağı tahtada görünmüyor. Bu, köşenin "üçe kadar sayma" bölümü.
- **Metin önerileri:**
  - 1. bölüm: "Köy arılarının peteği bomboş! Balı alıp peteğe götürelim."
  - 3. bölüm: "Burada üç bal var! Üç peteğe birer bal verelim." Bu metin ancak tahtada üç damla görünürse doğru.
  - 5. bölüm: "İki bal var, petek iki bal istiyor. İkisini de götürelim."
- **Sayma (bulgu 4):** Kod okumasına göre:
  - `Collected` olayı, hedefte en az iki öğe varsa toplanan öğe sayısını söylüyor.
  - `Dropped` olayı o peteğe düşen balın sırasını söylüyor.
  - İki sayı aynı çalıştırmada iç içe geçiyor.
  - 3. bölümde petekler tek tek "Bir!" diyor; başarı sesi ise "Bir, iki, üç!" diyor.
  - Ayrıca `count.NN` satırları yalnız hedefte en az iki öğe varken önceden yükleniyor; 1, 3, 4 ve 6. bölümde petek sayımı hiç duyulmayabilir.
- **Neden önemli:** Sayma öğreniminde sayıların hep aynı sırada ve her nesneye bir sayı düşecek biçimde söylenmesi temel ilkedir. Aynı akışta "bir"in üç kez duyulması bu ilkeyle çatışır.
- **Seçenekler (ekip kararı, bulgu 4):**
  - (a) Dolan petekleri say: "bir, iki, üç". Birebir eşlemeye en yakın seçenek, başarı sesleriyle de uyumlu.
  - (b) Teslim edilen balın toplamını say. 5. ve 6. bölümde ●● peteğe "bir, iki" düşer.
  - (c) Peteğin kendi yuvasını say ("bir, iki"), toplanan öğeleri sayma. Bu durumda başarı sesleri yeniden yazılmalı.
  - Ön tercih: Minik'te (a); toplama (5. bölüm) için petek içinde yuva sayımı ayrıca kalabilir. Hangisi seçilirse seçilsin, bir çalıştırmada tek sayı akışı olmalı.

### Bulgu 7: Kenarda biten bölümler

2. ve 3. bölümde ızgara 5 sütun, son petek x = 4'te, şerit 5. Beş "ileri" ile bütün petekler dolar, sonra arı çarpar ("Ups! Tak diye çarptım"). Motor çarpmayı hedef denetiminden önce başarısızlık sayar. 1. bölümde aynı fazla adım ızgara içinde kalır ve başarı sayılır. Minik için "fazla bastım" hatasının sonucu bölümden bölüme değişmemeli. Izgarayı genişletmek içerik düzeyinde çözer. Motor kuralı (hedef tamamlanınca dur) geniş bir değişiklik ve Sabır Ormanı'nı da etkiler; ekibin ve geliştiricinin kararı.

### Bulgu 9: Metin önerileri

| Anahtar | Şu an | Öneri | Neden |
| --- | --- | --- | --- |
| `koy.l03.success` | "Bir, iki, üç! Hiçbir petek boş kalmadı." | "Bir, iki, üç! Üç petek de doldu." | Olumsuz yapı bu yaşta olumluya göre daha zor işlenir. |
| `koy.l04.intro` | "Dikkat, bir petek yolun kenarında! Kimseyi unutmayalım." | "Bir petek köşede, sağda bekliyor! Kimseyi unutmayalım." | Tahtada yol çizili değil; "sağda" 4. bölümün dönüşünü sezdiriyor ve hayalet yolla tutarlı. |
| `koy.l06.intro` | "Üç bal, iki petek! Biri bir, öbürü iki bal istiyor." | "Üç bal, iki petek! Bir petek bir bal, öbürü iki bal istiyor." | "Biri bir" sesli okumada dil dolaştırıyor. |
| `hint.koy.listen` | "Hmm, birlikte düşünelim. Bal nerede? Hangi peteğe gidiyoruz?" | "Hmm, birlikte düşünelim. Bal nerede? Boş petek kaldı mı?" | Çok petekli bölümlerde tekil soru yanlış yönlendirir. |

1. bölüm giriş sahnesi: `honey` sahne nesnesi kavanoz olarak çiziliyor (`propHoneyJar`). "Peteğimiz bomboş" sesinin yanında dolu kavanoz ve kovan duruyor. Boş petek görünüşü daha uygun olabilir; sanat ekibi baksın.

## Ekibe sorular

1. **5. ve 6. bölümdeki yön (bulgu 2):** A, B ya da C? Köşe 1'in ormanın 9. bölümünden sonra erken açılması (PK-0005'te açık) gündemdeyse A'nın önemi artar.
2. **Canlı sayma (bulgu 4):** Minik'te otomatik bırakmada ne sayılsın: dolan petek mi, teslim edilen bal mı, peteğin yuvası mı?
3. **Fazla adım (bulgu 7):** Teslim hedefli bölümlerde hedef tamamlanınca çalıştırma bitsin mi, yoksa sınırlar bölüm tasarımında mı çözülsün?
4. **Köşe sonu değer anı:** Taslak 6. bölüm için "ilk mutlu arı ailesi" değer anı öngörüyor. JSON'da başarı sesi ve teşekkür pozu dışında bir şey yok. Ormandaki sabır filmine (P12) benzer bir köşe anı planlanıyor mu, yoksa başarı sahnesi yeterli mi?
5. **Ses yönergesinde ipucu derecesi:** 4. bölümün girişinde "sağda" demek (bulgu 9) çözümün bir kısmını söyler. Minik tanıtım bölümünde bu kabul edilebilir mi?

## Çocuk testinde gözlenecekler

- **1. bölüm:** Arının başa uçmasına şaşırıyor mu ("niye geri gitti?")? Ormanda arının durduğu yerde kaldığını hatırlıyor mu? Sepetteki noktalara bakıyor mu?
- **2–3. bölüm:** Bal alınınca sepete, bal bırakılınca peteğe bakıyor mu? Peteğe varışta kendisi sayıyor mu? Sayarken duyduğu sesle (bir, bir, bir) çelişki yaşıyor mu?
- **3. bölüm:** "Üç bal" denince tahtada neyi gösteriyor (tek damla, sepet, ayçiçekleri)?
- **Unutulan petek:** Ses yokken (ya da yazıldıktan sonra) boş peteği fark edip neyi değiştireceğini söylüyor mu? Bir "ileri" ekliyor mu, yoksa baştan mı kuruyor?
- **4. bölüm:** Dönüş tuşuna basmadan önce arının yüzüne bakıyor mu? Hayalet yolu takip ediyor mu?
- **5. bölüm:** Doğuya bakan arıda sağa dönüş için kaç deneme gerekiyor? Kendi sağını gösteriyor mu, ekranı mı çeviriyor, başını mı eğiyor? Hata dönüşün yerinde mi, sayıda mı?
- **2–3. bölümde fazla adım:** Bütün petekler dolduktan sonra çarpınca şaşırıyor ya da üzülüyor mu?
- **4–6. bölüm:** Aynı tuş dizisini ezberden mi tekrarlıyor, tahtaya bakarak mı kuruyor?
- **Genel:** Köşenin tamamı kaç dakika sürüyor? Ormanı bitirmiş yaklaşık 5 yaş çocukta 1–3. bölümde sıkılma belirtisi var mı?
