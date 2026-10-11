# Paylaşım Köyü — 24 bölüm taslağı (pedagoji incelemesi için)

Durum: **taslak, tasarım aşaması**. Henüz bölüm dosyası ve kod yok. Bu belge önce ürün sahibi, sonra pedagoji ekibi tarafından onaylanır; ardından mekanik (ADR 0030) yapılır ve bölümler editörle yazılıp çözücüyle doğrulanır. Aşağıdaki kart sayıları **tahminidir**; kesin en kısa çözüm, mekanik yapıldıktan sonra çözücüyle hesaplanır.

İlgili: F2-13, BAL-01, BAL-03; PRD "Ürün konsepti" (Paylaşım Köyü satırı); ADR 0026 (bellekli arı), ADR 0030 (taşıma ve dağıtma).

## Hikâye

Köylü arıların hasadı herkese yetmiyor: bazı peteklerde bal var, bazıları bomboş. Ari (Sabır Ormanı'ndan tanıdığımız arı robot) çiçeklerden balı toplar ve köyün peteklerine götürür. Çocuk Ari'yi programlayarak herkesin balı olmasını sağlar. Bölge sonunda köyde hasat şenliği olur; Roboya bir gemi parçası ve "paylaşma" değer kartını kazanır.

Bölge karakteri: **Köylü arılar** (her petekte bir arı ailesi). Sabır Ormanı'ndaki Bilge Kaplumbağa'nın yerini, sorunu getiren ve teşekkür eden yaşlı bir köy arısı alır (adı ve görünüşü sanat ekibiyle).

## Neden bu bölge

- Sabır Ormanı'ndaki Bal Peşinde bölümleri yalnız **sayma ve renk** içeriyor; PRD'deki toplama-çıkarma (BAL-01) eksik. Sabır Ormanı taslağının 4. sorusu da bunu soruyor. Paylaşım Köyü bu boşluğu doldurur.
- "Paylaşma" değeri bir kural olarak oyuna girer (değer mekaniktir, ders değildir): bölüm ancak herkes payını alınca biter.

## Yeni mekanik (çocuğun gözünden)

1. **Ari balı taşır.** Çiçekteki bala uğrayınca bal Ari'nin sepetine girer. Sepetteki bal sayısı ekranda noktalarla görünür.
2. **Petekler ne kadar bal istediğini gösterir.** Peteğin üstünde boş yuvalar vardır (●● = 2 bal ister). Bal konunca yuva dolar ve sesle sayılır ("bir, iki!").
3. **Balı bırakmanın iki yolu var** (ürün kararı: ikisi de):
   - **Minik (otomatik):** Ari peteğe varınca petek, ihtiyacı kadar balı kendisi alır. Çocuk yalnız rotayı planlar. Fazla bal verilemez, eksik kalan yuva boş görünür.
   - **Kaşif ("Bırak" kartı/tuşu):** Her "Bırak" bir bal bırakır. Çocuk kaç bal bırakacağını kodla söyler, yani sayma ve dağıtma koda girer. Fazla bırakılırsa petek "taşar" (ceza yok, Roboya "Bu petek doldu, fazlası başka birine yarar mı?" der).
4. **Eşit paylaşma bölümlerinde petekler sayı göstermez.** Kural "herkese aynı ve hepsi dağıtılsın"dır. Bittiğinde petekler yan yana dolu yuvalarıyla karşılaştırılır.
5. **Artan bal köy sofrasına.** Bal eşit bölünemiyorsa artan, herkesin kullandığı ortak sofraya bırakılır. Bu, paylaşmanın "kimse fazla almasın" yanını gösterir.
6. **"Kaç kaldı?" adımı (çıkarma, öneri).** Kaşif bölümlerinde bal verildikten sonra Roboya sorar: "Sepette kaç bal kaldı?" Çocuk üç nokta kartından birine dokunur (rakam yok ya da isteğe bağlı rakam). Yanlış seçimde Ari sepeti açar ve birlikte sayarlar; ceza yok. Bu adım Kodlama Kutusu'ndaki "tahmin et" adımına benzer ve **pedagoji kararı** gerektirir.

Değişmeyenler: Ari'nin belleği (ADR 0026) aynen kalır. Temizle unutulursa eski komutlar da çalışır; Minik'te bellek görünmez, Kaşif'te görünür (Sabır Ormanı'ndaki karar neyse o). Hata cezalandırılmaz; can, süre baskısı veya yıldız kaybı yok.

## Dizilim kuralı

- Bölge Sabır Ormanı'ndan sonra açılır. Tamamı premium içeriktir (ücretsiz katman Sabır Ormanı'nın ilk 3 bölümü).
- Patika 4 köşe × 6 bölüm. Her köşe bir matematik fikrini tanıtır ve kısa bir değer anıyla biter (Sabır Ormanı'ndaki köşe kaplumbağaları gibi).
- **Minik:** 1–6. bölümler, 2–4 kart, otomatik bırakma, sayılar en çok 3.
- **Kaşif:** 7. bölümden başlar, 5–12 kart, "Bırak" kartı, sayılar en çok 7.
- Her köşenin ilk bölümü tanıtımdır (`guided`) ve Kaşif çocuk bölgeye geç başlasa bile önce oynanır (ADR 0018'deki tanıtım kuralı).
- Bölümler yalnız Bal Peşinde'dir (PRD: "Bal Peşinde (genişletilmiş)").

## Bölüm listesi

Gösterim: ● = bir bal; "petek ●●" = 2 bal isteyen petek. Kart sayıları tahminidir.

### Köşe 1 — Herkese bir (Minik, otomatik bırakma)

| Sıra | Ne öğrenir | Kurgu | Kart (tahmini) | Not |
| --- | --- | --- | --- | --- |
| 1 | Taşı ve ver | 1 bal, 1 petek ●. Köy arısı "Peteğimiz boş" der. | 2–3 | Tanıtım, rehberli. Sepet ve petek yuvası ilk kez. |
| 2 | Birebir eşleme | 2 bal, 2 petek ● ●. Hepsi aynı yol üstünde. | 3 | |
| 3 | 3'e kadar sayma | 3 bal tek çiçekte, 3 petek ● ● ● yan yana. | 3–4 | Her peteğe varışta sesli sayma. |
| 4 | Kimse unutulmasın | 2 petek; biri yolun kenarında, dönüş gerekir. | 4 | Unutulan petek üzgün görünür, Roboya "Bu arının hiç balı yok" der. |
| 5 | Toplama: 1 + 1 | İki ayrı çiçekte birer bal, tek petek ●●. | 4 | Rozet: ● + ● = ●●. |
| 6 | Köşe finali | 3 bal, 2 petek ● ve ●●. | 4 | Köşe sonu değer anı: ilk mutlu arı ailesi. |

### Köşe 2 — Ne kadar, nereye? (Kaşif başlangıcı, "Bırak" kartı)

| Sıra | Ne öğrenir | Kurgu | Kart (tahmini) | Not |
| --- | --- | --- | --- | --- |
| 7 | "Bırak" kartı | Sepette 2 bal, petek ●●. İki kez "Bırak". | 5 | Tanıtım, rehberli; kart tanıtım anı. Kaşif başlangıcı. |
| 8 | Toplama: 2 + 1 | Bir çiçekte 2, diğerinde 1 bal; petek ●●●. | 7 | |
| 9 | Toplama, farklı yollar | Üç çiçekte 1, 2, 3 bal; petek ●●●●. 1 + 3 de olur, 2 + 3 alıp 4'ünü bırakmak da. | 8 | Birden çok doğru yol var; en kısa yol 3. yıldız. |
| 10 | Fazlası nereye? | Sepette 3 bal, petek ●●; artan 1 köy sofrasına. | 7 | Çıkarmaya hazırlık. Petek taşarsa nazik uyarı. |
| 11 | İki farklı ihtiyaç | Petekler ● ve ●●●, 4 bal. | 8 | |
| 12 | Köşe finali | Petekler ●, ●● ve ●●●; 6 bal (1 + 2 + 3). | 10 | Köşe sonu değer anı. |

### Köşe 3 — Eşit paylaş (Kaşif)

| Sıra | Ne öğrenir | Kurgu | Kart (tahmini) | Not |
| --- | --- | --- | --- | --- |
| 13 | "Eşit" ne demek | 2 bal, 2 petek; petekler sayı göstermez. | 5 | Tanıtım, rehberli. Bitişte petekler yan yana karşılaştırılır. |
| 14 | 4'ü ikiye böl | 4 bal, 2 petek → 2'şer. | 7 | |
| 15 | 6'yı üçe böl | 6 bal, 3 petek → 2'şer. | 9 | |
| 16 | 6'yı ikiye böl | 6 bal, 2 petek → 3'er. | 8 | 15 ile aynı bal, farklı pay: "Kaç kişi paylaşırsa o kadar az düşer." |
| 17 | Engelli paylaşma | 6 bal, 3 petek, çalı ve kayalar arasında. | 10 | |
| 18 | Köşe finali: artan | 5 bal, 2 petek → 2'şer, 1 sofraya. | 10 | "Bir tane arttı, ne yapalım?" Değer anı: ortak sofra. |

### Köşe 4 — Ver ve say (Kaşif, çıkarma ve karışık)

| Sıra | Ne öğrenir | Kurgu | Kart (tahmini) | Not |
| --- | --- | --- | --- | --- |
| 19 | Çıkarma: kaç kaldı? | Sepette 4 bal; petek ●. Sonra "Kaç kaldı?" adımı. | 5 + soru | Tanıtım, rehberli. |
| 20 | 5 − 2 | Sepette 5 bal; petek ●●; kalan 3 Ari'nin kovanına. | 7 + soru | |
| 21 | İki komşuya ver | Sepette 5 bal; petekler ●● ve ●. Kaç kaldı? | 9 + soru | |
| 22 | Topla, sonra ver | 3 + 2 bal topla, petek ●●●● ister. Kaç kaldı? | 10 + soru | Toplama ve çıkarma bir arada. |
| 23 | Eşit ve artan | 7 bal, 3 petek → 2'şer, 1 sofraya. | 11 | |
| 24 | Bölge finali: hasat şenliği | 3 petek eşit pay ve sofraya artan; şenlik alanına var. | 12 | Gemi parçası ve "paylaşma" değer kartı. |

## Ses ve görsel ihtiyacı (özet)

- **Ses:** Bölüm başına giriş ve başarı satırları (yaklaşık 48), sayma ("bir, iki, üç..." 7'ye kadar), "Bırak" kartı tanıtımı, "Kaç kaldı?" sorusu ve cevap satırları, taşan petek, unutulan petek, artan bal. Metinler `content/voice/script.csv`'ye yazılır; önce TTS, sonra profesyonel kayıt (F2-14).
- **Görsel:** Köy arka planı, köylü arı ailesi (mutlu, üzgün, teşekkür), yuvalı petek (boş, dolu, taşmış), Ari'nin bal sepeti, köy sofrası, şenlik alanı; Ari'nin tuş paneline "Bırak" tuşu. Renk tek başına anlam taşımaz: dolu ve boş yuva şekille de ayrışır.

## İncelemede sorulacaklar

1. **Minik payı:** Bölgede yalnız 6 Minik bölümü var (Sabır Ormanı'nda 9). Minik'e 2–3 bölüm daha eklensin mi, yoksa toplama-çıkarma 3.5–5 yaş için zaten erken mi?
2. **Otomatik bırakma kuralı:** Minik'te petek "ihtiyacı kadar alır" diye kurduk; fazla bal verilemez. Bu, birebir eşlemeyi öğretmek için yeterli mi, yoksa Minik de fazla verip görmeli mi?
3. **"Kaç kaldı?" adımı:** Çıkarmayı bir soru adımıyla mı ölçelim, yoksa yalnız hareket ve sepetin görünür azalmasıyla mı öğretelim? Seçimlerde rakam gösterilsin mi (Kaşif)?
4. **Sayı aralığı:** Kaşif için en çok 7 yeterli mi? MEB okul öncesi kazanımlarıyla (sayma, toplama-çıkarma, eşit paylaştırma) eşleştirme yapılmalı (`meta.mebOutcomes` boş).
5. **Eşit paylaşma:** Peteklerde sayı göstermemek (yalnız "eşit olsun") 5–7 yaş için anlaşılır mı, yoksa ilk bölümlerde sayıyla başlayıp sonra kaldıralım mı?
6. **Artanı ne yapalım:** Ortak sofra fikri paylaşma değerini doğru mu anlatıyor? Alternatif: artanı kendine değil, en küçük arıya vermek.
7. **Kavram etiketi:** Veli raporundaki kavram listesine "toplama-çıkarma / eşit paylaşma" eklenmeli mi, yoksa mevcut "sayma" kavramı altında mı raporlansın?

## Sonraki adımlar

1. Bu taslağın ürün sahibi ve pedagoji ekibince onayı.
2. Mekanik (ADR 0030): taşıma, bırakma, petek hedefi, eşit paylaşma, sofra; şema v3 ve çözücü; testler.
3. 24 bölümün editörle yazılması ve `make validate-content` ile doğrulanması; bu belgenin gerçek kart sayılarıyla güncellenmesi.
4. Ses (TTS) ve geçici görseller; ardından çocuklarla gözlemli test (`docs/child-testing`).
