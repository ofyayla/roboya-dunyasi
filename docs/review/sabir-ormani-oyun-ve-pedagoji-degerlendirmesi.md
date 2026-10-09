# Sabır Ormanı — oyun tasarımı ve pedagoji değerlendirmesi

Tarih: 2026-10-09. Kapsam: Sabır Ormanı'nın 36 bölümü (Yön Avcısı, Kodlama Kutusu, Bal Peşinde), harita, oyun ekranı, hikâye sahneleri. Kaynak: kod, bölüm verileri ve PlayMode ekran görüntüleri; tabletteki gözlem (veli geri bildirimi: "arı, çiçek deniyor ama ekranda yok; 36 bölüm birbirinin aynı; görsel yönlendirme eksik").

## Kısa sonuç

Motor ve akış sağlam (36 bölüm çözülebilir, ipucu merdiveni, rehberli mod, yıldız, ödül, süre sınırı çalışıyor). Ama **oyunun görsel dili tek oyuna göre kurulmuş**: tahta, karakter, nesne ve hikâye sahnesi Yön Avcısı'nın orman sahnesi. Kodlama Kutusu ve Bal Peşinde yalnız ses ve kuralla ayrışıyor, ekranda ayrışmıyor. Sesin söylediği şey ekranda yoksa 3,5–7 yaş çocuğu yönergeyi takip edemez; bu, kendi bölüm kuralımızı da bozuyor ("Ses metninde adı geçen nesne tahtada da görünmelidir", `content/levels/README.md`).

Kök nedenler:

1. **Sanat seti yalnız Yön Avcısı için var.** `RegionArt.ItemFor` her nesneyi elma ya da armuta çeviriyor; arı, çiçek, bal, kutu görseli yok. (`Scripts/UI/RegionArt.cs`)
2. **Oyuna özgü görünüm katmanı yok.** ADR 0025 oyunları tek ekranda birleştirdi; bu doğruydu, ama "oyun teması" (tahta, oyuncu karakteri, hedef, kontrol paneli) genişletme noktası olarak eklenmedi.
3. **İçerik çözücüyle üretildi, tasarımcı elinden geçmedi.** 36 bölümün 21'inde engel yok; yalnız 1 bölümde kenar süsü, 1 bölümde engel görünüşü var; Kodlama Kutusu ve Bal Peşinde'nin hiçbir hikâye sahnesinde nesne yok. Zorluk yalnız kart sayısıyla artıyor.

## Öncelik özeti

| # | Sorun | Öncelik | İş türü |
| --- | --- | --- | --- |
| 1 | Bal Peşinde'de arı yok; oyuncu Roboya | P0 | Sanat + kod (S) |
| 2 | Çiçek ve bal görünmüyor; mor, mavi, turuncu her şey kırmızı elma | P0 | Sanat + kod (S) |
| 3 | Toplama bölümlerinde hedef görünmüyor (ne, kaç tane, hangi renk) | P0 | Kod (M) |
| 4 | Robotun baktığı yön okunmuyor (sağ/sol öğretiminin temeli) | P0 | Kod (S) + sanat |
| 5 | Kodlama Kutusu'nun "kutu/masa" kimliği yok; hata avcısı ekranda belli değil | P0 | Sanat + kod (M) |
| 6 | Kaşif başlangıcı (10. bölüm) Kodlama Kutusu ve Bal Peşinde tanıtımlarını atlıyor | P0 | İçerik + kod (S) |
| 7 | Hikâye sahnesinde arı, çiçek, kutu yok; nesne listesi yetersiz | P1 | Sanat + şema (S) |
| 8 | Haritada 36 taş birbirinin aynı; oyun, bölüm, ilerleme bölgesi yok | P1 | Kod (M) + sanat |
| 9 | Kart etkisinin önizlemesi yok (Minik için kartın ne yaptığını görmek) | P1 | Kod (M) |
| 10 | Bal Peşinde kontrolleri PRD'deki "arının sırtındaki tuşlar" değil | P1 | Sanat + kod (M) |
| 11 | Sayma desteği yok (toplanınca 1-2-3 sayılmıyor) | P1 | Kod (S) + ses |
| 12 | Hata sonrası görsel geri bildirim zayıf (neresi yanlış, ne eksik) | P1 | Kod (M) |
| 13 | Minik ilerlemesi çok yoğun: 9 bölümde 3 mekanik + hata ayıklama | P1 | Pedagoji kararı + içerik |
| 14 | Bölüm çeşitliliği düşük (açık ızgaralar, süs yok, aynı sahneler) | P1 | İçerik (L) |
| 15 | Ses efekti ve müzik yok | P1 | Ses (M) |
| 16 | Sabır değeri yalnız seste; değer kartı anı yok (F1-24) | P2 | Sanat + kod (M) |
| 17 | Planlanan/gerçekleşen karşılaştırması (KUT-02) yarım: hayalet yol çocuğun planı değil çözüm | P2 | Kod (M) + pedagoji |

**Durum (2026-10-09):** 1–15 uygulandı (PR #36–#59); #10'daki "Minik'te bellek önce görünür" sırası ve #13'ün Minik sırası pedagoji ekibi onayı bekliyor; #15'te bölge müziği yok (lisanslı/özgün müzik gerekir). #8'deki "orman köşesi" kaplumbağa işaretleriyle karşılandı (köşe başına özel süs yok). #16 ve #17 açık: #16 kalıcı sanat (F1-24) ve değer anı metni, #17 "tahmin et" adımı çocuk deneyimi/pedagoji kararı gerektirdiği için başlatılmadı.

P0 = kapalı betadan önce şart (ekran–ses uyumsuzluğu veya oynanamazlık). P1 = beta sırasında düzelmeli. P2 = beta sonrası.

## Ayrıntılar

### P0 — oynanabilirlik ve ekran–ses uyumu

**1. Bal Peşinde'de arı yok.** Ses "Bu benim arkadaşım, arı!" diyor, tahtada Roboya yürüyor. Çocuk kimi programladığını bilmiyor.
- Öneri: arı karakter seti (ön, arka, yan, yürüme/uçma, mutlu, şaşkın) ve oyuna göre "oyuncu karakteri" seçimi (`GameTheme.Actor`). Roboya Bal Peşinde'de kenarda yönlendirici olarak kalır (aynı Bilge Kaplumbağa gibi).

**2. Çiçek ve bal görünmüyor; renkler yanlış.** `ItemFor` yalnız `yellow → armut`, diğer her renk `→ elma`. Sonuç: B03 "mor çiçeği al, sarıya dokunma" ekranda "elmayı al, armuda dokunma"; B04'te iki kırmızı çiçek ile mavi çeldirici **aynı elma** görünüyor, yani bölüm görsel olarak çözülemez. Renk eşleştirme (BAL-01) şu an fiilen yok.
- Öneri: çiçek seti (6 renk), bal damlası, petek. Renk tek başına anlam taşımamalı (CLAUDE.md §7): her renk ayrı taç yaprağı biçimi (yuvarlak, sivri, yıldız, kalp...). Eksik görsel için kodla çizilmiş yedek çiçek simgesi.

**3. Toplama hedefi görünmüyor.** Bal Peşinde bölümlerinde `goal.reach` yok, bu yüzden tahtada hiçbir hedef işareti yok (kaplumbağa da yok). Çocuk neyi, kaç tane, hangi renkten toplayacağını ancak sesten hatırlarsa bilir; bu Minik için çalışma belleğine fazla yük.
- Öneri: ekranın üst köşesinde **hedef rozeti**: toplanacak nesnelerin küçük resimleri (ör. 3 mor çiçek). Toplandıkça dolar, sesle sayılır (#11). Yön Avcısı ve Kodlama Kutusu'ndaki "önce elmayı al, sonra kaplumbağaya git" bölümlerinde de aynı rozet.

**4. Robotun yönü okunmuyor.** "Sağa dön" robotun kendi sağıdır; çocuk bunu ancak robotun nereye baktığını görürse öğrenir. Kuzeye bakan Roboya arka görünüşüyle boş bir paneldir; yan görünüş küçük ve belirsiz (bkz. `01-level1-start.png`).
- Öneri: robotun önüne yere çizilmiş **yön oku / ayak izi** (bakış yönü), dönüşte okun da dönmesi. Kaşif'te robotun "sağ elinde" küçük bir işaret (renk + şekil), çünkü robot çocuğa doğru bakınca sağ/sol tersine döner.

**5. Kodlama Kutusu'nun kimliği yok.** PRD "masa oyunu görünümü" diyor; ekranda aynı orman tahtası. Hata avcısı bölümünde hazır kod, çocuğun kendi koyduğu kartlarla aynı görünüyor; "bu kodda hata var" diye görsel bir işaret yok, çocuk ne yapacağını bilmiyor.
- Öneri:
  - Ahşap kutu/masa tahtası (çerçeve, ahşap zemin, kart yuvaları kutunun kenarında).
  - Hata avcısı akışı: **önce izle → bul → düzelt**. Bölüm başında hazır kod kendiliğinden bir kez çalışır (iz bırakır, yanlış yerde durur), Roboya "bir hata var" der; şeritteki hazır kartlar "kutudan gelen" farklı arka planla görünür; çocuk büyüteçle şüpheli karta dokunur, sonra doğru kartı koyar.
  - İpucu 2. kademesindeki "hatalı kartı vurgula" bu akışın doğal parçası olur.

**6. Kaşif başlangıcı tanıtımları atlıyor.** Kaşif 10. bölümden başlıyor (ADR 0018). Kodlama Kutusu'nun ilk iki bölümü (4, 7) ve Bal Peşinde'nin ilk iki bölümü (5, 8) bundan önce, yani Kaşif çocuk hata avcısını ve arının belleğini **hiç tanıtılmadan** ilk kez zor bir bölümde görüyor. Sağa ve sola dön kartlarının tanıtımı da atlanıyor.
- Öneri: her oyunun ilk bölümü (ve kart tanıtan bölümler) "tanıtım" olarak işaretlenir; yaş başlangıcından önce kalsa bile çocuk o oyuna ilk kez geldiğinde önce kısa tanıtım oynanır. Daha iyisi: Kaşif için aynı mekaniğin Kaşif uzunluğunda kendi tanıtım bölümleri (PRD'deki "Minik ve Kaşif varyantları").

### P1 — görsel yönlendirme ve öğrenme desteği

**7. Hikâye sahnesi nesneleri.** Şemadaki nesne listesi yalnız `apple, pear, gear, log, tree, bush, rock`. Bal Peşinde sahnesinde arı ve çiçek, Kodlama Kutusu'nda kutu ve büyüteç gösterilemiyor; 24 bölümün sahnesi boş.
- Öneri: şema v3'te `bee, flower, honey, box, magnifier` nesneleri ve arı karakteri; her bölümün sahnesi en az bir nesneyle bölümün sorununu gösterir.

**8. Harita.** 36 taşın hepsi aynı ok simgesi; çocuk hangi oyunu oynayacağını taştan anlamıyor, uzun yatay kaydırma kayboluyor (`08-path.png`).
- Öneri: taş simgesi oyuna göre (pusula / kutu / arı) ve biçimi farklı; patika 4 "orman köşesine" bölünür (9'ar bölüm, her köşenin kendi süsü ve sonunda küçük kutlama); Roboya her zaman sıradaki taşın yanında ve kaydırma oraya odaklanır.

**9. Kart önizlemesi.** Minik çocuk kartın ne yaptığını ancak deneyerek öğreniyor. Paletteki karta dokununca robotun önünde kısa bir hayalet hareket (ok ileri kayar, dönüş oku döner) gösterilebilir; plana eklenen kart tahtada hangi adımı başlattığını gösterir.

**10. Bal Peşinde kontrolleri.** PRD: arının sırtında ileri, geri, sol, sağ, **git** ve **temizle** tuşları. Şu an diğer oyunlarla aynı kart paleti, ortak oynat düğmesi ve küçük bir temizle simgesi var.
- Öneri: arı biçiminde büyük tuş paneli; "Git" (yeşil, ok) ve "Temizle" (biçimi belirgin) ayrı ve büyük. Minik'te bellek arının gövdesinde yanan noktalar olarak gösterilir (şimdiki boş sarı yuvarlaklar yerine).
- Pedagojik ölçek: Minik için bellek **önce görünür, sonra gizlenir** (desteği azaltma). Şu anki sıra tersi: Minik'te gizli, Kaşif'te görünür.

**11. Sayma.** BAL-01 sayma görevleri var ama sayma desteği yok. Her toplamada rozet dolar, Roboya sayar ("bir!", "iki!", "üç!"), bitişte toplam gösterilir.

**12. Hata sonrası geri bildirim.** Çarpınca kart sallanıyor (iyi). Ama "yanlış yerde durdu" ve "eksik topladı" durumlarında yalnız ses var.
- Öneri: hedefe kısa bir ok/parıltı; toplanmamış nesne nabız atar; robotun durduğu kare ile hedef arasında soluk bir işaret. Ceza değil, yön.

**13. Minik ilerlemesi (pedagoji).** İlk 9 bölümde 3,5–5 yaş çocuk üç ayrı mekanikle (yön kartları, hata ayıklama, kalıcı bellek) ve sağa dön kartıyla karşılaşıyor. Hata ayıklama ve görünmeyen bellek bu yaş için ileri beceriler.
- Öneri (pedagoji ekibinin kararı):
  - Minik'te bir mekanik iyice oturmadan diğerine geçmeyin: ör. ilk 6 bölüm yalnız Yön Avcısı, sonra kısa Bal Peşinde (görünür bellek), hata avcısı Minik'in son bölümlerinde ve 2–3 kartlık kodda.
  - Her yeni mekaniğin ilk bölümü `guided` olsun; şu an Kodlama Kutusu ve Bal Peşinde'nin hiçbir bölümü rehberli değil.
  - Desteği azaltma takvimi bölüm verisinde açık olsun (hayalet yol → rehber → yalnız ipucu).

**14. Çeşitlilik (içerik).** 21/36 bölüm engelsiz açık ızgara; boyutlar 4×4–6×6; hikâyeler aynı üç pozla.
- Öneri: her bölüme küçük bir hikâye bağlamı (kim, neden yardım istiyor); engellerin anlatıdan gelmesi (kütük, dere, taş); başlangıç yönünün değişmesi; "önce şunu al sonra oraya git" gibi sıra görevleri; bölge içinde 2–3 yeni zemin öğesi (köprü, dere taşı) — yeni öğe şema ve motor işi gerektirir.

**15. Ses efekti ve müzik yok.** Yürüme, dönme, toplama, çarpma, başarı için kısa efektler ve bölge müziği (PRD "Seslendirme ve ses") yok. Ses efektleri yönlendirmenin parçasıdır (doğru toplama sesi, çarpma sesi). Lisansı temiz kaynak veya özgün üretim gerekir.

### P2 — beta sonrası

**16. Sabır değeri.** Değer şu an yalnız seslendirmede. F1-24 "sabır değer kartı" animasyonu ve her orman köşesinin sonunda kısa bir değer anı (Bilge Kaplumbağa ile) eklenmeli.

**17. Planlanan ve gerçekleşen yol (KUT-02).** Hayalet yol çocuğun planını değil doğru çözümü gösteriyor (cevabı veriyor). Kaşif için "tahmin et" adımı: çocuk robotun nereye varacağını önce parmağıyla işaretler, sonra çalıştırır; iz ile tahmin karşılaştırılır. Bu, KUT-02'nin pedagojik amacına daha yakın.

## Önerilen iş sırası

1. **Görsel temel (P0, 1 sprint):** oyun teması genişletme noktası (`GameTheme`: tahta görünümü, oyuncu karakteri, hedef, kontrol paneli); arı ve çiçek setleri; hedef rozeti; yön oku; Kodlama Kutusu ahşap tahtası ve hata avcısı "önce izle" akışı; tanıtım bölümlerinin yaş başlangıcında atlanmaması.
2. **Yönlendirme (P1, 1 sprint):** harita taşları ve orman köşeleri, kart önizlemesi, sayma ve hata geri bildirimi, arı tuş paneli, ses efektleri.
3. **İçerik geçişi (pedagoji ile):** Minik/Kaşif sırası, rehberli bölümler, hikâye bağlamları ve nesneler, çeşitlilik; ardından 8–10 çocukla gözlemli test (`docs/child-testing`).

Sanat üretimi (F1-23) bu listenin en uzun kalemi. Kalıcı sanat gelene kadar Higgsfield ile üretilmiş geçici setler kullanılabilir; stil rehberi `docs/art/style-guide.md`.
