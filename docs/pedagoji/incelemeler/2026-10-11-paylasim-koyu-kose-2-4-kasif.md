# Paylaşım Köyü Köşe 2–4 (Kaşif, 7–24) ve köy sistem sesleri — pedagoji ön incelemesi

> Yapay zekâ ön incelemesidir; pedagoji ekibi onayının yerine geçmez.

- Tarih: 2026-10-11
- Kapsam:
  - `content/levels/paylasim-koyu/bal-pesinde/07.json`–`24.json`; 1–6 yalnız yolculuk ve önceki incelemeden bu yana değişenler için (dal `content/koy-ses`)
  - `content/voice/script.csv`: `bal_pesinde.koy.l07`–`l24`, `bal_pesinde.nothing_to_drop`, `hive_overflow`, `hive_forgotten`, `not_equal`, `table_leftover`, `bal_pesinde.kac_kaldi.*`, `map.welcome.paylasim-koyu`, `map.region_locked`, `value.sharing`, `card.drop.intro`, `roboya.hint.koy.*`
  - Çalışma anındaki ses ve görüntüyü anlamak için okunanlar: `GameController.cs`, `HiveFeedback.cs`, `HiveReport.cs`, `HiveCompareView.cs`, `BoardView.cs`, `BasketView.cs`, `RemainingView.cs`, `RemainingChoices.cs`, `RegionArt.cs`
- İlgili: F2-13, BAL-01, BAL-03, OYN-03, OYN-04, YZ-01, YZ-03; PK-0001–PK-0005; önceki rapor `2026-10-11-paylasim-koyu-kose-1-minik.md`
- Sınır: Oyun çalıştırılmadı, `make validate-content` koşturulmadı. En kısa çözümler elle izlendi ve JSON'daki `solution.shortestLength` ile karşılaştırıldı; hepsi tutuyor. Kod okumasına dayanan tespitler "doğrulanmalı" diye işaretlendi.

## Kısa sonuç

18 Kaşif bölümü PK-0003'ün sayısal sınırlarının hepsine uyuyor:
- toplam bal ≤ 7;
- `need` ≤ 4;
- "Bırak" ≤ 6, dönüş ya da engel varsa ≤ 4;
- en kısa çözüm ≤ 10 ve şerit +1/+2.

Kart eğrisi köşe başlarında düşüp köşe sonuna doğru çıkıyor (5→10, 5→9, 5→10). Ceza dili, yasaklayıcı ifade ve §13 ihlali yok. Köşe 1 incelemesindeki öneriler büyük ölçüde uygulanmış (5–6. bölüm, ızgaralar, "çiçek", eksik sesler).

En önemli üç bulgu:
1. **Paylaşma peteklerinde `need` = 4.** PK-0001'in harfini koruyor: rakam yok ve yuva sayısı cevabı ele vermiyor. Ama yuvalar 12 bölüm boyunca "bu kadar istiyor" demişti; çocuk 4 yuvayı "4'er ver" diye değil, büyük olasılıkla "ilk peteği doldur" diye okur. Başarı anında da petekler boş petek resmiyle ve yarı boş yuvalarla görünüyor, ses ise "eşit doldu" diyor (bulgu 1–2).
2. **19–22'de `carryAtEnd` hiçbir zaman ayrıca bağlayıcı değil.** Bu bölümlerde petek ihtiyacı teslim sayısına eşit; fazla "Bırak" taşmayla duruyor. Bu yüzden kalan bal teslimden kendiliğinden çıkıyor. "Kaç kaldı?" bayrakla kapalı olduğu için Köşe 4'te çocuk Köşe 2'den farklı bir hesap yapmıyor. PK-0002'nin "ölçülen çıkarma becerisi bu hedeftir" varsayımı içerikte gerçekleşmiyor (bulgu 3).
3. **Geri bildirim sesleri paylaşma ve kalan balda yanlış yöne işaret ediyor.** `hive_overflow` olmayan bir "başka peteği" öneriyor. Eşitsizlikte `not_equal` "bal ekle" yönüne itiyor. 14. bölümün "Bir ona, bir ona" ipucu kelimesi kelimesine uygulanınca 1-1 dağılıyor ve çocuk ilgisiz "daha varamadık" sesini duyuyor (bulgu 5–7).

`Engel` bulgusu yok. Bütün bölümler çözülebilir ve hiçbir hata cezalandırılmıyor.

## Bulgular

| # | Önem | Bakış | Bulgu | Dayanak | Öneri | İnsan kararı |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Önemli | Kavram ilerleyişi / görsel dil | 13–18, 23, 24'te paylaşma peteklerinin yuvası eşit paydan fazla (13: 2 yuva, pay 1; diğerleri 4 yuva, pay 1–3). Köşe 1–2'de ve 19–22'de yuva = hedef. Çocuk aynı görsele iki ayrı anlam yüklemek zorunda. 14 ve 24'te sepetteki bal sayısı yuva sayısına eşit (4 = 4); "ilk peteği doldur" yanlışı en çekici seçenek. | PK-0001 madde 2 ve gerekçesi; R-GEL-2, R-ILR-1, R-ERS-4 | Seçenek B (paylaşma peteğinde boş yuva gösterilmesin, dolan bal üst üste görünsün) ve hemen yapılabilecek metin düzeltmeleri (bulgu 2). Ayrıntı aşağıda. | Evet |
| 2 | Önemli | Ses ve dil / görsel | Paylaşma bölümü kazanılınca petek `Partial` durumda kalıyor ve tahtada **boş petek resmi** görünüyor (`HiveReport.StateOf`, `RegionArt.HiveFor`; kod okuması). 13, 15, 17 ve 18'in başarı sesi "eşit doldu" diyor. Paylaşma bölümlerindeki ipucu (`hint.koy.listen`) "Boş petek kaldı mı?" diye soruyor, oysa doğru çözümde her petekte boş yuva var. | R-DIL-3, R-MOT-2, R-ERS-3 | Başarı satırlarında "doldu" yerine "aynı oldu" ya da "eşit oldu". Paylaşma için ayrı bir ipucu: "Önce balı sayalım. İki peteğe aynı kadar düşsün." Eşit pay tamamlanınca petek mutlu ya da dolu resimle görünsün (geliştirici). | Hayır |
| 3 | Önemli | Kavram ilerleyişi | 19–22'de her petekte `need` = teslim sayısı. Fazla "Bırak" taşma, yanlış yerde "Bırak" `nothing_to_drop` ile duruyor; bu yüzden `carryAtEnd` (3, 3, 2, 1) teslimden kendiliğinden çıkıyor ve hiçbir çalıştırmayı tek başına başarısız kılmıyor. `askRemaining` kapalıyken Köşe 4'te çocuk "kaç kaldı"yı ne hesaplıyor ne duyuyor; iş yalnız "Bırak"ları saymak, yani Köşe 2'deki iş. 9. bölüm 6 − 4'ü zaten Köşe 2'de gösteriyor. | PK-0002 madde 1 (yeni kanıt), R-ILR-1, R-ILR-4, BAL-01 | Kısa vadede A: kalanı adlandıran, yalnız soru kapalıyken çalan bir satır; veli raporunda çıkarma "ölçülmüş" diye geçmesin. Orta vadede B: kalanı gerçekten bağlayıcı yapan bir pilot bölüm. Ayrıntı aşağıda. | Evet |
| 4 | Önemli | Kavram ilerleyişi / iskele | 23'te ilk petek yoldaki baldan önce geliyor. Çocuk ilk "Bırak"tan önce 3 + 2 = 5 ve 5'i ikiye bölmeyi (2, 2, artan 1) kafasında yapmalı; PK-0001'in öğrettiği tek tek dağıtma tek hatta yapılamıyor. Sayılar 18'le aynı (5 → 2, 2, 1); fark yalnız sıra. Bölgenin tek 5 zorluklu bölümü. İpucu genel `hint.koy.listen`, alternatifi 22 (paylaşma değil teslim, 10 kart, dönüşlü). | PK-0003 madde 6 ("Topla, eşit ver, artanı sofraya"), R-ILR-4, R-MOT-2, R-ILR-7 | Sıra kalsın (23'ü 18'den ayıran tek şey bu), ama bölüme özgü bir ipucu eklensin ("Önce bütün balı sayalım: sepette üç, yolda iki") ve alternatif 18 olsun. Diğer yol: sırayı PK-0003'teki başlıkla uyumlu hâle getirmek (önce topla); bu durumda 23 neredeyse 18'in kopyası olur. | Evet |
| 5 | Önemli | Hata ve motivasyon / ses | `bal_pesinde.hive_overflow` "Fazlası başka bir peteğe yarar mı?" diyor. Taşmanın olabildiği tek petekli bölümlerde (9, 19, 20, 22) başka petek yok; doğru yön "sepette kalsın". 10'da doğru yer sofra. | R-DIL-3, R-MOT-2, PK-0001 madde 5 | Bağlamdan bağımsız metin: "Bu petek doldu! Fazlası sepette kalabilir." Ya da sofralı ve sofrasız iki satır (geliştirici). | Hayır |
| 6 | Önemli | Hata ve motivasyon / ses | Paylaşma bölümlerinde iki boşluk var. (a) Eşitsizlikte (ör. 14'te 3-1) `not_equal` "Bir petek hâlâ bal bekliyor" diyor; çocuğu az alan peteğe bal eklemeye itiyor, oysa sepet boş ve çözüm dağılımı değiştirmek. (b) Petekler eşit ama sepette bal kalmışsa (14'te 1-1, 18'de 2-2 ve sofraya gidilmemiş) hüküm `NotYet` oluyor ve genel `roboya.not_there.01` ("daha varamadık") çalıyor (kod okuması, doğrulanmalı). | R-MOT-2, R-DIL-3; önceki rapor bulgu 1 (anahtar ayrımı önerisi) | Paylaşma için ayrı satırlar. (a) "Petekler aynı değil. Birine çok, birine az gitti." (b) "Petekler aynı, ama sepette bal kaldı. Onu da dağıtalım mı?" | Hayır |
| 7 | Önemli | Hata ve motivasyon / iskele | 14'ün ipucu ("Bir ona, bir ona! Balı tek tek dağıtalım.") tek hatta dizilmiş peteklerde kodla uygulanamıyor: arı her petekten bir kez geçiyor. Kelimesi kelimesine uygulanırsa (İ İ B İ B) 1-1 çıkar, 2 bal sepette kalır ve bulgu 6b'deki ilgisiz ses çalar. PK-0001 bu stratejiyi kod yazmadan önce parmakla dağıtmak için öngörmüştü. | PK-0001 madde 2, R-MOT-2 | Metin: "Önce parmağınla dağıt: bir ona, bir ona... Her peteğe kaç düştü?" | Hayır |
| 8 | Önemli | Kavram ilerleyişi / bölüm finali | 24, PK-0003'ün "kutlama, zorluk zirvesi değil" amacıyla gerilim içinde. Kart sayısı tavanda (10). İki ilk var: üç petek arasında artanlı paylaşma ve paylaşmadan sonra bir yere varma (`reach`). Bunlara bir dönüş ve 4 bal = 4 yuva tuzağı ekleniyor. Kutlama anında üç petek de boş petek resmi ve 3/4 boş yuvayla görünüyor (bulgu 2). Sesteki "şenlik" tahtada çizilmiyor: hedef yaşlı köy arısı (`goalIdle`); `festivalFlags` sprite'ı hiçbir yerde kullanılmıyor. | PK-0003 madde 6, R-ILR-1, R-ILR-4, R-DIL-3 | A: şenlik alanı hattın sonundaki sofrayla aynı yerde, dönüş yok (İ B İ B İ B İ B, 8 kart). B: düzen kalsın, petekler `need` 1 olsun (üç petek dolu, artan sofrada). İkisinde de şenlik bayrakları hedefin yanında çizilsin (sanat/geliştirici). | Evet |
| 9 | Önemli | Ses ve dil (sayma) | Köşe 1 incelemesinin 4. bulgusu (iki ayrı sayma akışı) hâlâ açık ve Kaşif'in toplama bölümlerinde ağırlaşıyor. Toplama sayımı öğe sayısını söylüyor, bal sayısını değil: 8'de 2 + 1 bal toplanırken "Bir, iki" duyuluyor, sonra petekte "bir, iki, üç". 22'de 3 + 2 bal toplanırken "Bir, iki", petekte "bir … dört". Başarı sesleri ise "İki bal ve bir bal, üç bal" diyor (kod okuması, doğrulanmalı). | BAL-01, R-DIL-2; önceki rapor bulgu 4 | Köyde toplama sayımı ya kapatılsın ya da sepetteki bal sayısını söylesin; bir çalıştırmada tek sayı akışı olsun. | Evet (önceki rapor soru 2 ile birlikte) |
| 10 | Öneri | Görsel dil | Bal öğesinin miktarı tahtada hâlâ görünmüyor (önceki rapor bulgu 3). Ses 8, 18, 22 ve 23'te yer başına miktar söylüyor ("bir yerde iki bal, bir yerde bir bal", "yolda iki bal daha var"). Sepetteki boş halkalar toplamı gösteriyor ve 23'te planlamaya yetiyor; ama hangi damlanın kaç bal olduğu yalnız seste. | R-DIL-3, R-GEL-4 | Damla altında nokta ya da yığılı damlalar (sanat/geliştirici). | Hayır |
| 11 | Öneri | İskele (YZ-03) | Beş alternatif ya daha kolay değil ya da başka bir beceriyi çalıştırıyor: 10→9 (8 kart > 7), 16→15 (9 > 8), 18→17 (engel, dönüş, 5 kart türü), 23→22 (teslim, 10 kart > 9), 24→22 (teslim, aynı uzunluk). | R-ILR-7 | 10→8, 16→14, 18→16, 23→18, 24→18. | Hayır |
| 12 | Öneri | Ses ve dil | Küçük metin sorunları. 21'de "İki komşu da doldu!" (komşu değil petek doluyor). 21'de "paylaştıralım" hem anlam olarak tuhaf hem de Köşe 3'ün "eşit" anlamındaki "paylaş"ını eşit olmayan teslime taşıyor. 23'ün başarı sesinde "sofrada da bal kaldı" sofrayı artık gibi gösteriyor (PK-0001 madde 3). 23 ve 24 girişleri dört bilgi taşıyor. 19 ve 20 aynı kapanışı tekrarlıyor ("başka komşulara götüreceğiz"). 11 ve 21'de "biri bir, biri üç". `nothing_to_drop` tek cümlede iki neden sayıyor. `value.sharing` soyut ve dört köşe sonunda (Minik'in 6. bölümü dahil) birebir aynı. | R-DIL-1, R-DIL-2, R-DIL-3, R-DEG-3 | Önerilen metinler aşağıda. | Hayır |
| 13 | Öneri | Belge | Taslak tablosu JSON'un gerisinde kaldı (önceki rapor bulgu 11). 9. bölüm taslakta "farklı yollar" bölümüydü; JSON'da tek hat ve tek yol var, yeni kavram "kalan sepette". 12 ve 17'nin gerçek kart sayıları 10 ve 8. | PK-0003 "Sonuçlar" | Tablo JSON'dan yeniden üretilsin; 9. bölümün amacı "kalan sepette kalır" diye düzeltilsin. | Hayır |

Bu bakışlarda sorun bulunmadı:
- **§13 sınırları:** Can, süre, yıldız kaybı, satış ya da "geri gel" dili yok. `map.region_locked` satış dili taşımıyor.
- **PK-0003 sayısal sınırları:** Hepsi tutuyor (ayrıntı aşağıda).
- **Değer gömülmesi:** Paylaşma bölümleri ancak bütün bal dağıtılıp petekler eşit olunca bitiyor; değer kuralın içinde (R-DEG-1). Artan bal her zaman sofraya gidiyor, "en küçüğe" ya da kovana gitmiyor (PK-0001 madde 3–4). `table_leftover` kararın cümlesini birebir kullanıyor. "Kimse fazla almasın" türünden yasaklayıcı bir ifade yok.
- **Ceza ve suçlama:** Bütün sistem satırları suçlamasız. `kac_kaldi.wrong` ("Olsun, birlikte sayalım!") PK-0002 madde 6'ya uyuyor.

## Ayrıntılar

### Kararlara uyum

| Karar maddesi | Durum |
| --- | --- |
| PK-0001/2: peteklerde sayı yok, petekler yan yana, canlı dolum | Rakam yok; ama yuvalar ikinci bir miktar gösteriyor (bulgu 1) |
| PK-0001/2: 13 rehberli, 2 petek × 1 bal; 14 ipucu "bir ona, bir ona"; 16 gözlemi soru değil | Uyuyor (13'te yuva 2, bulgu 1; 14 ipucunun metni, bulgu 7) |
| PK-0001/3: artan bal yalnız sofraya, yasaklayıcı dil yok | Uyuyor (10, 18, 23, 24) |
| PK-0001/4: kovan yok; teslim bölümünde kalan bal sepette | Uyuyor (9, 19–22) |
| PK-0002: `askRemaining` yalnız Kaşif, teslim + `carryAtEnd`, üç seçenek farklı | Uyuyor: 19 (4/1/3), 20 (5/2/3), 21 (5/3/2), 22 (5/4/1). `carryAtEnd` bağlayıcı değil (bulgu 3) |
| PK-0003/1–3: bal ≤ 7, `need` ≤ 4 (finalde ≤ 5), "Bırak" ≤ 6 | Uyuyor (en çok 6 bal: 9, 12, 15, 16; `need` 5 hiç kullanılmamış; 6 "Bırak": 12, 15, 16) |
| PK-0003/4: Kaşif ≤ 10, şerit +1/+2 | Uyuyor |
| PK-0003/5: dönüş ya da engel varsa "Bırak" ≤ 4 | Uyuyor (11: 4, 17: 4, 20: 2, 21: 3, 22: 4, 24: 4) |
| PK-0003/6: 12, 17, 23, 24'ün kurgusu | Uyuyor. 23'te sıra "önce ver, sonra topla" (bulgu 4) |
| PK-0005/2: köşe başları 7, 13, 19 `intro` + `guided`, alternatifsiz | Uyuyor |
| PK-0005/3: `resetEachRun` her yerde; 7+ `beeMemoryVisible` | Uyuyor |

### Önceki raporun (Köşe 1) durumu

| Önceki bulgu | Durum |
| --- | --- |
| 1: `hive_forgotten` / `not_equal` yok | Yazıldı. Minik için "eşit" sözcüğü kullanılmamış. Paylaşmada yetersiz kalıyor (bulgu 6). |
| 2: 5–6. bölümde doğuya bakan dönüş | A seçeneği uygulandı (5 düz yol, 6 kuzeyden İ S İ İ) |
| 3: "çiçek", miktar tahtada yok | "Çiçek" kalktı; miktar hâlâ görünmüyor (bulgu 10) |
| 4: iki ayrı sayma akışı | Açık; Kaşif toplamasında ağırlaştı (bulgu 9) |
| 5, 6: 4–6 tekrarı, 6'nın alternatifi | Çözüldü (6 → 4; 5 → 3) |
| 7: kenarda biten bölümler | İçerikte çözüldü (son petekten sonra bir kare) |
| 8, 9: ipucu ve metinler | Uygulandı (`hint.koy.listen`, `hint.koy.direction`, 3/4/6 metinleri) |
| 10: 4–6'ya `direction` etiketi | Uygulanmadı (Öneri olarak açık) |

### Çocuğun yolculuğu (Kaşif: 1. bölüm, sonra 7'den)

Kısaltmalar: İ ileri, B Bırak, S sağa dön, L sola dön. R = rehberli. Bütün Kaşif bölümlerinde bellek görünür ve dünya her çalıştırmada başa dönüyor. "Pay/yuva", paylaşma bölümlerinde peteğe düşen eşit payın yuva sayısına oranı.

| Sıra | Kart / şerit | Palet | Kurgu | Destek | Zorluk | Yenilik |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 3 / 4 | İ | 1 bal → ● (otomatik) | R, tanıtım | 1 | Köy, sepet, yuva, başa dönen dünya |
| 7 | 5 / 6 | İ B | sepette 2 → ●● | R, tanıtım, kart tanıtımı | 2 | "Bırak" kartı; dolu sepetle başlama |
| 8 | 7 / 8 | İ B | 2 + 1 topla → ●●● | — | 2 | Toplayıp bırakma (toplama) |
| 9 | 8 / 9 | İ B | 1 + 2 + 3 topla → ●●●●, 2 sepette | — | 3 | Kalan bal sepette; 4 "Bırak" arka arkaya |
| 10 | 7 / 8 | İ B | sepette 3 → ●● + sofra | — | 3 | Sofra |
| 11 | 9 / 10 | İ S L B | sepette 4 → ● ve ●●● | — | 3 | Köyde ilk dönüş; iki farklı ihtiyaç |
| 12 | 10 / 11 | İ B | 6 topla → ●, ●●, ●●● | — | 4 | Köşe finali; 6 "Bırak" |
| 13 | 5 / 6 | İ B | sepette 2 → 2 petek, pay 1/2 | R, tanıtım | 3 | Eşit paylaşma; yuva artık hedef değil |
| 14 | 7 / 8 | İ B | sepette 4 → 2 petek, pay 2/4 | ipucu "bir ona, bir ona" | 3 | Desteksiz ilk paylaşma |
| 15 | 9 / 10 | İ B | sepette 6 → 3 petek, pay 2/4 | — | 4 | Üç petek |
| 16 | 8 / 9 | İ B | sepette 6 → 2 petek, pay 3/4 | — | 4 | Aynı bal, büyük pay |
| 17 | 8 / 9 | İ S L Geri B | sepette 4 → 2 petek, çalılar | — | 4 | Engel + dönüş; köyde tek "geri" kartı |
| 18 | 9 / 10 | İ B | 5 topla → 2 petek + sofra, pay 2/4 | — | 4 | Paylaşmada artan |
| 19 | 5 / 7 | İ B | sepette 4 → ●, 3 kalır | R, tanıtım | 3 | (Soru kapalıyken) yalnız anlatı: "kalan" |
| 20 | 7 / 9 | İ S L B | sepette 5 → ●●, 3 kalır | — | 3 | Dönüşlü teslim |
| 21 | 9 / 11 | İ S L B | sepette 5 → ●● ve ●, 2 kalır | — | 4 | İki petek, kalan |
| 22 | 10 / 11 | İ S L B | 3 + 2 topla → ●●●●, 1 kalır | — | 4 | Toplama + kalan |
| 23 | 9 / 10 | İ B | sepette 3, petek, yolda 2, petek, sofra; pay 2/4 | — | 5 | Bal elde olmadan plan |
| 24 | 10 / 11 | İ S L B | sepette 4 → 3 petek + sofra → şenlik; pay 1/4 | — | 4 | Üçe artanlı paylaşma + `reach` |

**Oturum.** Kaşif oturumu 15–20 dakika. Bölüm başına 2–3,5 dakika varsayılırsa (çocuk testinde ölçülmeli) her köşe yaklaşık bir oturum. Köşe 2'de her bölüm tek yenilik getiriyor. Köşe 3'ün asıl yükü 13–14'te: hedef türü değişiyor, yuvanın anlamı da sessizce değişiyor (bulgu 1). Köşe 4'te soru kapalıyken 19–21'in yeniliği yalnız seste (bulgu 3).

**Eğri.** Zorluk 2-2-3-3-3-4 | 3-3-4-4-4-4 | 3-3-4-4-5-4; kartlar 5-7-8-7-9-10 | 5-7-9-8-8-9 | 5-7-9-10-9-10. Testere dişi düzgün. İki not: 23 bölgenin tek 5'i (bulgu 4); 24 kartta tavan, aritmetikte en kolaylardan (bulgu 8).

### Bulgu 1–2: Paylaşmada `need` = 4 ve "sayı gösterilmez" (soru 1)

**PK-0001'i koruyor mu?**
- **Harfi korunuyor.** Peteklerde rakam yok ve yuva sayısı doğru payı vermiyor; "şu kadar teslim et" diye okunabilecek cevap ekranda yok.
- **Amacı kısmen korunuyor.** Kararın gerekçesi: "Sayı göstermek hedefi 'eşit'ten 'şu kadar teslim et'e çevirirdi." Yuvalar yine bir miktar gösteriyor. Kaşif çocuk 1 ve 7–12'de, yani köye girdiği andan beri, "boş yuva = bu petek daha bal istiyor, hepsi dolunca başarı" kuralını öğrendi. Bunu sesler pekiştirdi ("Petek doldu!": 1, 7, 8, 11, 12) ve üzgün arı da öyle (eksik alan petek). Paylaşmada bu miktar artık hedef değil, kapasite. Ama hiçbir ses ya da görsel bunu söylemiyor.

**Çocuk 4 yuvayı nasıl okur?**
- "4'er ver" okuması zayıf, çünkü hiçbir bölümde 4'er verecek bal yok (en çok 6).
- Beklenen okuma "ilk peteği doldur, kalanı ötekine". Bu, bölüm bölüm şu öngörülebilir yanlışları üretir:

| Bölüm | Olası ilk dağılım | Hüküm | Duyulan ses |
| --- | --- | --- | --- |
| 14 | 4-0 | Forgotten | `hive_forgotten` |
| 15 | 4-2-0 | Forgotten | `hive_forgotten` |
| 16 | 4-2 | NotEqual | `not_equal` ("hâlâ bal bekliyor") |
| 17 | 4-0 | Forgotten | `hive_forgotten` |
| 18 | 4-1, sofra 0 | NotEqual | `not_equal` |
| 23 | 3-2 | NotEqual | `not_equal` |
| 24 | 4-0-0 | Forgotten | `hive_forgotten` |

**Bu yanlış öğretici olabilir mi?** Evet: eşitsizliği görünür kılar. Ama ancak geri bildirim dağılımı yeniden düşündürürse. Bugün durum şöyle:
- `not_equal` az alan peteğe eklemeyi öneriyor (bulgu 6).
- Başarı anında tahta "boş petek" resmi gösteriyor: `Partial` durumu `HiveFor` içinde boş sprite'a düşüyor.
- Başarı sesi "doldu" diyor.

Çocuk aynı ekranda iki ayrı miktarı (kapasite ve pay) birbirinden ayırmak zorunda kalıyor. Çalışma belleği yükü için genel ilke: bu yaşta aynı anda iki sayı kuralını tutmak zordur (R-GEL-2).

**13. bölüm.** Rehberli olduğu için çocuk çatışmayı yaşamıyor: doğru kart nabız atıyor. Desteksiz ilk karşılaşma 14 ve orada bal = yuva = 4.

**Seçenekler:**

| Seçenek | Ne değişir | Artı | Eksi |
| --- | --- | --- | --- |
| **A** | `need` 4 kalır. 13'ün girişi kuralı söyler ("Bu sefer petekleri doldurmuyoruz; ikisine aynı kadar veriyoruz"). Başarı satırları "aynı oldu" der. Paylaşmaya özgü ipucu ve `not_equal` yazılır. Eşit pay tamamlanınca petek mutlu ya da dolu resimle çizilir. | En az iş; yuva cevabı vermemeye devam eder. | Ekranda iki miktar kalır; "doldur" alışkanlığı sesle bastırılmaya çalışılır. |
| **B** | Paylaşma bölümlerinde (`goal.share`) peteğin boş yuvaları çizilmez; dolan bal üst üste görünür, petekler yan yana boy karşılaştırır. Taşma sınırı motor için kalır. | PK-0001'in "yan yana karşılaştırma" niyetine en yakın; yanıltıcı sayı yok; "aynı boy" doğrudan görülür. | Geliştirici ve sanat işi. Köşe 3'te petek farklı görünür; 13'te bu fark kısaca tanıtılmalı. |
| **C** | `need` bölümden bölüme değişir (3, 4) ve hiçbir bölümde sepetteki bal yuva sayısına eşit olmaz (14 ve 24 değişir). | Yalnız içerik işi; en çekici tuzağı kaldırır. | Yuva anlamı çatışması sürer; PK-0003'e göre 5 yalnız finallerde. |

Ön tercih: hemen A'nın metin kısmı (bulgu 2), orta vadede B. C yalnız B yapılamazsa.

### Bulgu 3: 19–22'de `carryAtEnd` ve "Kaç kaldı?" (soru 2)

**Sayılar:** 19: 4 − 1 = 3. 20: 5 − 2 = 3. 21: 5 − (2 + 1) = 2. 22: (3 + 2) − 4 = 1.
- Yaş aralığına uygun ve 5'in altında.
- Üç seçenek her bölümde farklı (PK-0002 madde 4).
- 19 ile 20'nin cevabı aynı (3); 20'de "başta" ve "verdiğim" değişiyor. Bu kabul edilebilir, ama çocuğun "hep üç" diye ezberleyip ezberlemediği gözlenmeli.

**Bağlayıcılık:**
- Bölümlerde sofra ya da hedef dışı petek yok ve her petekte `need` = `count`.
- Fazla bir "Bırak" `HiveOverflow` ile, yanlış yerde "Bırak" `NothingToDrop` ile duruyor.
- Sonuç: teslim tutuyorsa kalan bal zorunlu olarak `carryAtEnd`'e eşit. Doğrulayıcı bunu yakalamaz, çünkü kural ihlal edilmiyor; yalnız etkisiz.

**Neden önemli:**
- PK-0002 madde 1 çıkarmanın mekanikle taşınacağını ve ölçülen becerinin bu hedef olduğunu söylüyor. İçerikte ölçülen şey yalnız "Bırak"ları doğru saymak.
- Soru bayrakla kapalıyken çocuk 19–21'de kalanı ne hesaplıyor ne duyuyor. Başarı satırları "sepette bal kaldı" diyor ama sayı söylemiyor; bu da doğru, çünkü soru açıldığında cevabı vermemeli.
- Köşe 4'ün öğretim katkısı şimdilik sepetin görünür azalmasından ibaret. Bunu 9. ve 10. bölüm Köşe 2'de zaten gösteriyor.

**Seçenekler:**

| Seçenek | Ne değişir | Artı | Eksi |
| --- | --- | --- | --- |
| **A** | Kabul. Köşe 4 çıkarmayı "adlandırır", ölçmez. Soru kapalıyken başarıdan sonra kalanı sayarak söyleyen bir satır çalar ("Sepette bir, iki, üç bal kaldı!"; yalnız `askRemaining` kapalıyken). PK-0002 madde 1'in "ölçülen beceri" cümlesi düzeltilir; veli raporu çıkarmayı ölçülmüş göstermez. | Küçük iş; "kaldı" sözcüğü ve azalma somut olarak adlandırılır. | Çıkarma hesabı yine yapılmaz. |
| **B** | Bir pilot bölümde kalan gerçekten bağlayıcı olur. Örnek: petekte 4 yuva var ama teslim sayısı verilmez; Ari'nin sonraki komşu için 3 bal ayırması gerekir (sepette 5, `carryAtEnd` 3, petek "kalanı" alır). Çocuk kodlamadan önce 5 − 3'ü bulmalıdır. | Gerçek bir çıkarma (parça-bütün) koda girer. | Şema ya da doğrulayıcı değişikliği gerekebilir (hedefsiz petek; geliştirici). "Sonraki komşu" ekranda görünmeli (R-GEL-4). Zorluk Kaşif'in üst ucunda; çocuk testinden sonra. |
| **C** | Soru çalıştırmadan önce sorulur (tahmin). | Zihinden çıkarma. | PK-0002 bunu bilinçli olarak dışarıda bıraktı ve P3'e bağladı; şimdi önerilmez. |

Ön tercih: A şimdi; B'yi F4 çocuk testinden sonra tek bir bölümde denemek. Bu bir PK-0002 güncellemesi gerektirir; ekip kararı.

### Bulgu 4: 23'te önce ver, sonra topla (soru 3)

- **Çözüm:** İ B B İ İ B B İ B (9 kart). İlk petek, yoldaki 2 baldan önce geliyor.
- **Çocuk ne yapmalı:** İlk "Bırak"tan önce 3 + 2 = 5'i, sonra 5'in iki peteğe 2'şer ve sofraya 1 dağılacağını bulmalı.
- **Destek ne:** Sepetteki 3 dolu ve 2 boş halka toplamı gösteriyor. Ama tahtadaki damla tek bal gibi çizildiği için "yolda iki bal" bilgisi yalnız seste ve boş halkalarda (bulgu 10).
- **Tek tek dağıtma neden yok:** Hat düz, her petek bir kez ziyaret ediliyor. PK-0001'in öğrettiği strateji burada kodla yapılamıyor.
- **İlginç bir yan durum:** Çocuk tahtada görünenle düşünüp "3 + 1 = 4, ikişer" derse yine 2'yi ilk peteğe verir; sonra 3 balı olur, 2'sini verir, 1'i sofraya bırakır. Yani yanlış toplamla da doğru sonuca varılabiliyor. Bu ölçümü bulandırır; çocuk testinde sorulmalı ("Kaç bal vardı?").
- **Zirve olması doğru mu:** PK-0003 finali kutlama olarak tanımladığı için zirvenin 23'te olması tutarlı. Sayılar 18'le aynı olduğundan aritmetik yük artmıyor; artan şey planlama. Kaşif'in çekirdeği "önce şu, sonra bu" planı olduğu için bu yük yaşa uygun bir zirve. Ama desteği bölüme özgü olmalı.
- **Öneri:**
  - İpucu 1: "Önce bütün balı sayalım: sepette üç, yolda iki."
  - Alternatif 18 olsun (aynı sayılar, önce topla).
  - Sıra değiştirilirse (önce topla) 23 neredeyse 18'in kopyası olur; o zaman sayılar değişmeli (ör. sepette 2, yolda 3, iki petek, artan 1).

### Bulgu 8: 24. bölüm, kutlama mı zirve mi (soru 4)

- **Çözüm:** İ B İ B İ B S İ B İ (10 kart).
- **Aritmetik kolay:** her peteğe 1, artan 1.
- **Kodu uzun:** kart tavanda. Ritmik "İ B" deseni yükü azaltıyor, ama PK-0003'ün gözlem maddesi tam da 9–10 kartta yolu kaybetmeyi soruyor.
- **İki ilk:** üç petek arasında artanlı paylaşma (15 artansız, 18 iki petekli) ve paylaştıktan sonra bir yere varma.
- **4 yuva / 4 bal:** "İlk peteği doldur" tuzağını en güçlü biçimde kuruyor.
- **Kutlama görseli:** üç boş petek resmi ve her birinde 3 boş yuva. "Herkesin balı var" sesiyle görüntü çelişiyor.
- **Sonuç:** Bölüm kart sayısıyla zirve, sayıyla kutlama, görüntüsüyle kıtlık.
- **Seçenekler:**
  - **A:** Petekler (1,0), (2,0), (3,0); sofra ve şenlik alanı hattın sonunda (4,0). Çözüm İ B İ B İ B İ B, 8 kart, dönüş yok. Hedef sofranın karesiyse `reach` ile bırakma noktasının aynı karede olmasına doğrulayıcının izin verip vermediğine bakılmalı (geliştirici).
  - **B:** Düzen kalır, petekler `need` 1 olur. Üç petek de "doldu", artan sofrada. Cevap görünür hâle gelir, ama PK-0003 finali zaten kutlama sayıyor.
  - **Ön tercih:** A + B birlikte (8 kart, dolu petekler). Şenlik bayrakları (`festivalFlags`) hedefin yanında çizilmeli; bugün hedef yaşlı arı.

### Bulgu 5–7: Önerilen satırlar

| Anahtar | Şu an | Öneri |
| --- | --- | --- |
| `hive_overflow` | "Bu petek doldu! Fazlası başka bir peteğe yarar mı?" | "Bu petek doldu! Fazlası sepette kalabilir." |
| `not_equal` (paylaşma için yeni anahtar) | "Bir petek hâlâ bal bekliyor. Birlikte bakalım mı?" | "Petekler aynı değil. Birine çok, birine az gitti." |
| Paylaşmada eşit ama sepette bal var (yeni) | "Hmm, daha varamadık…" | "Petekler aynı, ama sepette bal kaldı. Onu da dağıtalım mı?" |
| `hint.koy.l14.listen` | "Bir ona, bir ona! Balı tek tek dağıtalım." | "Önce parmağınla dağıt: bir ona, bir ona… Her peteğe kaç düştü?" |
| Paylaşma ipucu (yeni; 13, 15–18, 23) | `hint.koy.listen`: "…Boş petek kaldı mı?" | "Önce balı sayalım. İki peteğe aynı kadar düşsün." |

Minik'teki `not_equal` olduğu gibi kalmalı; orada doğru çalışıyor.

### Bulgu 12: Metin önerileri

| Anahtar | Şu an | Öneri | Neden |
| --- | --- | --- | --- |
| `koy.l13.success` | "İki petek eşit doldu!" | "İki peteğe aynı kadar bal düştü!" | Petek dolmadı (bulgu 2) |
| `koy.l15.success`, `l17`, `l18` | "… eşit doldu" | "… aynı oldu" / "… eşit oldu" | Aynı |
| `koy.l21.intro` | "… Sepetteki balı paylaştıralım." | "… Sepetteki baldan ikisine de verelim." | "Paylaş" Köşe 3'te "eşit" demekti |
| `koy.l21.success` | "İki komşu da doldu! …" | "İki komşunun peteği de doldu! …" | Dolan petek |
| `koy.l23.success` | "Eşit verdik, sofrada da bal kaldı. Afiyet olsun!" | "Eşit verdik, sofrada da herkese bal var. Afiyet olsun!" | PK-0001: sofra artık değil, ortak yer |
| `koy.l23.intro` | Dört bilgi tek satırda | "Sepetimde üç bal var, yolda iki bal daha. İki peteğe aynı kadar verelim." (sofrayı başarısızlıkta ya da ipucunda söyle) | R-DIL-2 |
| `koy.l20.success` | "… Kalan balı başka komşulara götüreceğiz." | "Petek doldu! Kalan bal sepette bizi bekliyor." | 19 ile aynı kapanış |
| `koy.l11.intro`, `l21.intro` | "biri bir, biri üç" | "bir petek bir bal, öbürü üç bal" | Sesli okumada dil dolaşıyor |
| `nothing_to_drop` | "… Sepet boş ya da petek yok. …" | Aynı kalabilir; ileride iki ayrı satır (sepet boş / burada petek yok) | Çocuk hangisi olduğunu kendisi çıkarıyor |
| `value.sharing` | Dört köşe sonunda aynı; "paylaşmak, herkesin payını düşünmektir" | Köşe başına kısa ve somut bir değişke, ör. Minik 6: "Herkesin peteğine bal geldi. Paylaşınca hepimiz mutluyuz." | R-DEG-3, R-DIL-1; tanım cümlesi 4–5 yaş için soyut |

`card.drop.intro` ("Onu koyunca sepetimden bir bal bırakırım") "bir kart = bir bal" kuralını açıkça söylüyor, iyi. Sepetin sahibi seste değişiyor: 7, 10, 19, 20 ve 23'te "sepetim", 14, 16 ve 18'de "balımız". Sepeti taşıyan Ari, konuşan Roboya. Küçük bir tutarlılık meselesi; önceki rapordaki `card.turn_right.intro` notuyla birlikte ele alınabilir.

## Ekibe sorular

1. **Paylaşma peteğinin görünüşü (bulgu 1):** A, B ya da C? B seçilirse bu PK-0001 madde 2'nin uygulama ayrıntısı olarak karar kaydına eklensin mi?
2. **Köşe 4'te çıkarma (bulgu 3):** Köşe 4 çıkarmayı yalnız adlandırsın mı (A), yoksa çocuk testinden sonra kalanın bağlayıcı olduğu bir bölüm mü denensin (B)? Her iki durumda PK-0002 madde 1'in "ölçülen beceri" cümlesi güncellenmeli mi?
3. **23. bölümün sırası (bulgu 4):** "Önce ver, sonra topla" köyün planlama zirvesi olarak kalsın mı? PK-0003'teki "Topla, eşit ver" başlığı sırayı bağlıyor muydu?
4. **24. bölüm (bulgu 8):** Final kart sayısıyla tavanda kalsın mı, yoksa 8 kartlık, dolu petekli bir kutlamaya mı insin?
5. **Toplama sayımı (bulgu 9):** Köyde toplama anında ne sayılsın: toplanan öğe mi, sepetteki bal mı, hiçbiri mi? (Önceki raporun 2. sorusuyla birlikte.)

## Çocuk testinde gözlenecekler

- **13–14:** Çocuk ilk peteğin bütün yuvalarını doldurmaya mı çalışıyor? 14'te 4-0 dağılımından sonra ne değiştiriyor: ilk petekteki "Bırak"ları mı azaltıyor, ikinci peteğe "Bırak" mı ekliyor?
- **Başarı anında (13–18):** Yarı boş yuvaları görünce "ama dolmadı" diyor mu? Başarıdan emin mi?
- **14 ipucu:** "Bir ona, bir ona" sonrasında parmakla mı dağıtıyor, yoksa doğrudan İ B İ B mi diziyor?
- **16:** "Az kişi paylaşınca daha çok düşüyor" cümlesine tepki veriyor mu, 15 ile karşılaştırıyor mu (sorulmadan)?
- **19–22:** Bölüm bitince sepete bakıyor mu? Kalanı kendiliğinden söylüyor mu? Soru açıldığında boş halkaları da sayıp "başta vardı" kartını mı seçiyor? 19 ve 20'de "hep üç" diye ezberliyor mu?
- **23:** İlk peteğe kaç bal vereceğine nasıl karar veriyor? Yoldaki balı hesaba katıyor mu? "Kaç bal vardı?" sorusuna 4 mü, 5 mi diyor?
- **24:** 10 kartı dizerken sayıyı mı, yolu mu kaybediyor? Kutlama sahnesinde petekleri dolu mu, boş mu algılıyor? Hedefteki yaşlı arıyı "şenlik" olarak tanıyor mu?
- **Taşma (9, 10, 19, 20):** "Başka bir petek" sesinden sonra tahtada petek arıyor mu?
- **8 ve 22:** Toplarken duyduğu "bir, iki" ile peteğe bırakırken duyduğu "bir, iki, üç" arasında şaşırıyor mu?
- **Genel:** Köşe başına süre (Kaşif oturumuna sığıyor mu); Köşe 4'te Köşe 2'ye göre sıkılma belirtisi.
