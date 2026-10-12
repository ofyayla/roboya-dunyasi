# Paylaşım Köyü — 24 bölüm taslağı (pedagoji incelemesi için)

Durum: **2026-10-12: ızgara tasarımı geçersiz (ADR 0031, yönsüz Köy Meydanı); aşağıdaki mekanik ve bölüm tabloları tarihçedir, yeni tasarım pedagoji ekibinin önünde (bekleyen K8–K12).** Eski durum: onaylandı (2026-10-11); 24 bölüm yazıldı (plan E1, `content/levels/paylasim-koyu/`). Ürün sahibi pedagoji danışmanının önerilerini kabul etti: PK-0001–PK-0005. Aşağıdaki tablolar bu kararlara göre güncellendi. Uzman onayı (G2) yayından önce alınır. Mekanik (ADR 0030) yapıldı; bölüm tabloları artık dosyalardan üretilmiş **gerçek en kısa çözüm** sayılarıyla yazılı.

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
5. **Artan bal köy sofrasına.** Bal eşit bölünemiyorsa artan, herkesin kullandığı ortak sofraya bırakılır. Ses dili: "Artan bal sofraya, herkes birlikte yesin." (PK-0001)
6. **"Kaç kaldı?" adımı (çıkarma, öneri).** Kaşif bölümlerinde bal verildikten sonra Roboya sorar: "Sepette kaç bal kaldı?" Çocuk üç nokta kartından birine dokunur (rakam yok ya da isteğe bağlı rakam). Yanlış seçimde Ari sepeti açar ve birlikte sayarlar; ceza yok. Bu adım Kodlama Kutusu'ndaki "tahmin et" adımına benzer ve **pedagoji kararı** gerektirir.

Bellek ve dünya (PK-0005): her çalıştırmada dünya başa döner (arı başlangıçta, sepet ve petekler ilk hâlinde). Kaşif'te bellek görünür ve Temizle'ye kadar korunur; Temizle unutulursa eski komutlar da baştan çalışır. Minik'te bellek gizlidir ve her çalıştırmadan sonra kendiliğinden temizlenir. Hata cezalandırılmaz; can, süre baskısı veya yıldız kaybı yok.

## Dizilim kuralı

- Bölge Sabır Ormanı'ndan sonra açılır. Tamamı premium içeriktir (ücretsiz katman Sabır Ormanı'nın ilk 3 bölümü).
- Patika 4 köşe × 6 bölüm. Her köşe bir matematik fikrini tanıtır ve kısa bir değer anıyla biter (Sabır Ormanı'ndaki köşe kaplumbağaları gibi).
- **Minik:** 1–6. bölümler, 2–4 kart, otomatik bırakma, sayılar en çok 3.
- **Kaşif:** 1. bölümü tanıtım olarak oynar, sonra 7. bölümden başlar; 5–10 kart (finaller dahil), "Bırak" kartı, toplam bal en çok 7, peteğin ihtiyacı en çok 4 (köşe finalinde 5), bölüm başına en çok 6 "Bırak" (PK-0003, PK-0005).
- Her köşenin ilk bölümü (1, 7, 13, 19) bölüm verisinde açıkça tanıtım olarak işaretlenir ve rehberlidir; Kaşif çocuğun başlangıcından önce kalan tek tanıtım 1. bölümdür (ADR 0018'deki tanıtım kuralı, PK-0005).
- Bölümler yalnız Bal Peşinde'dir (PRD: "Bal Peşinde (genişletilmiş)").

## Bölüm listesi

Gösterim: ● = bir bal. **Bu tablolar `content/levels/paylasim-koyu/bal-pesinde/NN.json` dosyalarından üretildi** (plan E1); kart sayıları çözücünün en kısa çözümüdür. Köşe başları 1, 7, 13, 19.

### Köşe 1 — Herkese bir (Minik, otomatik bırakma)

| Sıra | Ne öğrenir | Kurgu | Arı bakışı | En kısa | Not |
| --- | --- | --- | --- | --- | --- |
| 1 | Taşı ve ver | yerde 1 bal; petek ● | doğu | 3 | tanıtım, rehberli |
| 2 | Birebir eşleme | yerde 1 bal; yerde 1 bal; petek ●, ● | doğu | 4 | kolay alternatif: 1 |
| 3 | 3'e kadar sayma | yerde 3 bal; petek ●, ●, ● | doğu | 4 | kolay alternatif: 2 |
| 4 | Kimse unutulmasın | yerde 2 bal; petek ●, ● | kuzey | 4 | hayalet yol, kolay alternatif: 3 |
| 5 | Toplama: 1 + 1 | yerde 1 bal; yerde 1 bal; petek ●● | doğu | 3 | kolay alternatif: 3 |
| 6 | Köşe finali: ● ve ●● | yerde 3 bal; petek ●, ●● | kuzey | 4 | kolay alternatif: 4 |

### Köşe 2 — Ne kadar, nereye? (Kaşif başlangıcı, "Bırak" kartı)

| Sıra | Ne öğrenir | Kurgu | Arı bakışı | En kısa | Not |
| --- | --- | --- | --- | --- | --- |
| 7 | "Bırak" kartı | sepette 2 bal; petek ●● | doğu | 5 | tanıtım, rehberli, kart tanıtımı |
| 8 | Toplama: 2 + 1 | yerde 2 bal; yerde 1 bal; petek ●●● | doğu | 7 | kolay alternatif: 7 |
| 9 | Sepette kalan bal sepette kalır | yerde 1 bal; yerde 2 bal; yerde 3 bal; petek ●●●● | doğu | 8 | kolay alternatif: 8 |
| 10 | Fazlası sofraya | sepette 3 bal; petek ●●; sofra | doğu | 7 | kolay alternatif: 8 |
| 11 | İki farklı ihtiyaç | sepette 4 bal; petek ●, ●●● | doğu | 9 | kolay alternatif: 10 |
| 12 | Köşe finali: topla, üç petek | yerde 6 bal; petek ●, ●●, ●●● | doğu | 10 | kolay alternatif: 11 |

### Köşe 3 — Eşit paylaş (Kaşif)

| Sıra | Ne öğrenir | Kurgu | Arı bakışı | En kısa | Not |
| --- | --- | --- | --- | --- | --- |
| 13 | "Eşit" ne demek | sepette 2 bal; 2 petek eşit paylaşır (sayı yok) | doğu | 5 | tanıtım, rehberli |
| 14 | 4'ü ikiye böl | sepette 4 bal; 2 petek eşit paylaşır (sayı yok) | doğu | 7 | kolay alternatif: 13 |
| 15 | 6'yı üçe böl | sepette 6 bal; 3 petek eşit paylaşır (sayı yok) | doğu | 9 | kolay alternatif: 14 |
| 16 | 6'yı ikiye böl | sepette 6 bal; 2 petek eşit paylaşır (sayı yok) | doğu | 8 | kolay alternatif: 14 |
| 17 | Engelli paylaşma | sepette 4 bal; 2 petek eşit paylaşır (sayı yok) | doğu | 8 | kolay alternatif: 16 |
| 18 | Köşe finali: artan sofraya | yerde 5 bal; 2 petek eşit paylaşır (sayı yok); sofra | doğu | 9 | kolay alternatif: 16 |

### Köşe 4 — Ver ve say (Kaşif, çıkarma ve karışık)

| Sıra | Ne öğrenir | Kurgu | Arı bakışı | En kısa | Not |
| --- | --- | --- | --- | --- | --- |
| 19 | Çıkarma: 4 − 1 | sepette 4 bal; petek ●; 3 bal sepette kalır | doğu | 5 | tanıtım, rehberli, "Kaç kaldı?" |
| 20 | Çıkarma: 5 − 2 | sepette 5 bal; petek ●●; 3 bal sepette kalır | kuzey | 7 | "Kaç kaldı?", kolay alternatif: 19 |
| 21 | İki komşuya ver | sepette 5 bal; petek ●●, ●; 2 bal sepette kalır | kuzey | 9 | "Kaç kaldı?", kolay alternatif: 20 |
| 22 | Topla, sonra ver | yerde 3 bal; yerde 2 bal; petek ●●●●; 1 bal sepette kalır | kuzey | 10 | "Kaç kaldı?", kolay alternatif: 21 |
| 23 | Ver, topla, eşit ver, artanı sofraya | sepette 3 bal; yerde 2 bal; 2 petek eşit paylaşır (sayı yok); sofra | doğu | 9 | kolay alternatif: 18 |
| 24 | Bölge finali: hasat şenliği | sepette 4 bal; 3 petek eşit paylaşır (sayı yok); sofra; sonra şenlik alanına var | doğu | 10 | kolay alternatif: 18 |

Paylaşma bölümlerinde petek `need` değeri kapasitedir (aşılırsa petek taşar), eşit payın üstünde (4) seçildi; 24. bölüme kadar uzanan geçişler, 19–22'deki kalan bal ve 23'ün sırası için pedagoji ön incelemesi (`docs/pedagoji/incelemeler/2026-10-11-paylasim-koyu-kose-2-4-kasif.md`) açık sorular listeler.

## Ses ve görsel ihtiyacı (özet)

- **Ses:** Bölüm başına giriş ve başarı satırları (yaklaşık 48), sayma ("bir, iki, üç..." 7'ye kadar), "Bırak" kartı tanıtımı, "Kaç kaldı?" sorusu ve cevap satırları, taşan petek, unutulan petek, artan bal. Metinler `content/voice/script.csv`'ye yazılır; önce TTS, sonra profesyonel kayıt (F2-14).
- **Görsel:** Köy arka planı, köylü arı ailesi (mutlu, üzgün, teşekkür), yuvalı petek (boş, dolu, taşmış), Ari'nin bal sepeti, köy sofrası, şenlik alanı; Ari'nin tuş paneline "Bırak" tuşu. Renk tek başına anlam taşımaz: dolu ve boş yuva şekille de ayrışır.

## İncelemede sorulacaklar

Hepsi karara bağlandı (2026-10-11): 1 → PK-0005, 2, 5 ve 6 → PK-0001, 3 → PK-0002, 4 → PK-0003, 7 → PK-0004. Sorular kayıt için aşağıda duruyor.

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
