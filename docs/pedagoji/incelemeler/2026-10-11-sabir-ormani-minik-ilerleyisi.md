# Sabır Ormanı: Minik ilerleyişi, bellek görünürlüğü ve "tahmin et" adımı (pedagoji ön incelemesi)

> Bu bir yapay zekâ ön incelemesidir; pedagoji ekibinin onayının yerine geçmez.

- Tarih: 2026-10-11
- Kapsam: `content/levels/sabir-ormani/**` altındaki 36 bölüm, `order` sırasıyla; `content/voice/script.csv`; `docs/plan.md` notları ("Görsel temel P1 - 2", "İçerik düzeni"); `docs/review/sabir-ormani-oyun-ve-pedagoji-degerlendirmesi.md` #10, #13, #17; ADR 0018 (2026-10-09 güncellemesi dahil)
- İlgili: F1-21; YON-01, YON-02, KUT-01, KUT-02, BAL-01, BAL-02, YZ-02, YZ-03; PRD "Yaş seviyeleri ve pedagojik çerçeve"
- Karar kaydı: `docs/pedagoji/kararlar/` altında kabul edilmiş karar yok; bu rapordaki hiçbir öneri mevcut bir kararla çelişmiyor.
- Sınır: Bu inceleme bölüm verisine, ses senaryosuna ve belgelere dayanıyor. Oyun kodu okunmadı. Gizli bellekte şeridin nasıl göründüğü, yol gölgesinin tahmin adımında açık olup olmadığı ve çarpmadan sonra arının belleği gibi çalışma anı davranışları plan notlarından ve değerlendirmeden alındı. Bunlar "doğrulanmalı" diye işaretlendi.

## Kısa sonuç

Minik çocuk ilk 9 bölümde üç oyun, üç kart ve dört ayrı mekanikle karşılaşıyor: sıralama, toplama ve sayma, kalıcı ve görünmez bellek, hata ayıklama. Bunların ikisi (sayma ve hata ayıklama) PRD'nin yaş tablosunda Kaşif kavramı olarak geçiyor. En ağır nokta Bal Peşinde. Minik'te (5. ve 8. bölüm) arının belleği görünmüyor, plan notuna göre yol gölgesi de gizli, 8. bölümde rehber ve hayalet yol da yok. Arı bellek ve konumunu da koruyor. Böylece Minik'in en çok destek alması gereken yerde ekrandaki bütün plan bilgisi kalkıyor. Bu, çerçevenin "bilgi ekranda olmalıdır" kuralına aykırı; raporda tek `Engel` bulgusu bu. Bellek sırası gerçekten ters: Bal Peşinde'nin 1–6. bölümlerinde bellek gizli, 7–12. bölümlerde görünür. Yani destek zamanla azalmıyor, artıyor. "Tahmin et" adımı ise artık karar bekleyen bir fikir değil. Dört Kodlama Kutusu bölümünde (`options.predict`) ve ses senaryosunda uygulanmış durumda. Yayından önce ekip onayı gerekiyor. Ayrıca Kaşif'te varsayılan açık olan yol gölgesi tahminin cevabını ekranda gösteriyor olabilir.

## Bulgular

| # | Önem | Bakış | Bulgu | Dayanak | Öneri | İnsan kararı |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Engel | Gelişimsel uygunluk | Minik'te Bal Peşinde (5. ve 8. bölüm): bellek görünmez, plan notuna göre yol gölgesi gizli, 8. bölümde rehber ve hayalet yol da yok. Arı durduğu yerde kalıyor ve belleği silinmiyor (BAL-02). Temizle tuşu Minik'te hiç tanıtılmıyor. | R-GEL-4, R-ILR-3, çerçeve: Minik "Bilgi ekranda olmalıdır" | Karar 2'de B veya C seçeneği. En azından Minik'te bellek görünür olmalı ve yol gölgesi açık kalmalı. | Evet |
| 2 | Önemli | Kavram ilerleyişi | Minik ilk 9 bölümde 3 oyun, 3 kart ve 4 mekanik görüyor. 5. bölümde tek seferde en az 4 yenilik var: arı karakteri, sırt tuşları, kalıcı bellek, toplama hedefi. Sayma (8) ve hata ayıklama (9) PRD'de Kaşif kavramı. PRD'de Kodlama Kutusu ve Bal Peşinde "4+" yaşta. | R-ILR-1, R-GEL-6, PRD yaş tablosu ve oyun tablosu | Karar 1'de C seçeneği (iç içe dizilim kalsın, Minik'te mekanik değişmesin). Uzun vadede D (varyantlar). | Evet |
| 3 | Önemli | Kavram ilerleyişi | KK02 (9. bölüm), Minik'in ilk hata avcısı, rehberli değil. Yanlış kart sola dön/sağa dön ayrımına dayanıyor ve sola dön ancak 2 bölüm önce (7. bölüm) tanıtıldı. Giriş sesiyle "hunt" satırı art arda iki çok adımlı yönerge veriyor. | R-ILR-3, R-GEL-5, R-DIL-2, çerçeve: hata ayıklama "iyi tanıtılmışsa" | KK02'yi Kaşif'in rehberli hata avcısı tanıtımına taşıyın (bkz. #6). Minik'te kalırsa rehberli olsun ve yanlış kart ileri/dön farkına dayansın. | Evet |
| 4 | Önemli | Kavram ilerleyişi | YON-01 Minik'te "yalnız ileri ve tek yönde dönüş" diyor, ama YA05 (7), KK02 (9) ve YA06 (12) Minik bölümlerinin paletinde iki dönüş kartı birlikte var. Plan F1-03 notu bu noktayı açık bırakmış. | YON-01, R-ILR-1 | YON-01'in nasıl okunacağına ekip karar versin (bkz. Ekibe sorular 2). | Evet |
| 5 | Önemli | Kavram ilerleyişi | Bal Peşinde'de destek sırası ters. BP01–BP06'da (Minik ve ilk dört Kaşif bölümü) bellek gizli, BP07–BP12'de görünür. BP07'nin başarı sesi bunu kendisi söylüyor: "Belleği görünce planlamak kolaylaştı." BP07'nin "daha kolay" alternatifi BP06, belleği yine gizliyor. | R-ILR-3, R-ILR-7, çerçeve "Destekli ilerleme" | Karar 2 seçenekleri. Hangi seçenek seçilirse seçilsin, alternatifler desteği azaltmamalı. | Evet |
| 6 | Önemli | Kavram ilerleyişi | Kaşif yolculuğu: tanıtım bölümleri yalnız "oyunun ilk bölümü" ve "kart tanıtan bölüm" olarak sayılıyor. Bu yüzden Kaşif çocuk hata avcısını ilk kez KK04'te (13. bölüm, 5 kartlık kod, rehber ve hayalet yol yok), tahmin adımını ilk kez KK05'te tanıtımsız görüyor. | R-ILR-5, R-ILR-3, ADR 0018 güncellemesinin amacı | Tanıtım kavramı yeni mekaniğin ilk bölümünü de kapsasın: ilk hata avcısı, ilk tahmin, ilk görünür veya gizli bellek. Bu bir geliştirme işi. | Evet |
| 7 | Önemli | Kavram ilerleyişi | BP03 (11. bölüm, Kaşif'in kendi 2. bölümü) dört şeyi birlikte getiriyor: ilk renk çeldiricisi, ilk Temizle, gizli bellek ve dönüş. Giriş sesi üç kuralı tek seferde söylüyor. | R-ILR-1, R-DIL-2 | Temizle ve renk çeldiricisi ayrı bölümlerde gelsin. Önce görünür bellekle Temizle tanıtılsın. | Hayır |
| 8 | Önemli | Kavram ilerleyişi | "Tahmin et" (KUT-02) KK05, KK07, KK10 ve KK12'de ve `predict.*` ses satırlarında uygulanmış. 2026-10-09 değerlendirmesi bu adımı "başlatılmadı" diye gösteriyor; pedagoji onayından önce uygulanmış. İlk tahmin bölümü (KK05) en kısa çözümü 5'ten 8'e çıkarıyor. Kaşif'te yol gölgesi varsayılan açık; tahmin sırasında da açıksa cevap ekranda görünüyor (doğrulanmalı). | KUT-02, R-ILR-1, R-ILR-3 | Karar 3 seçenekleri. Hangisi seçilirse seçilsin, tahmin anında yol gölgesi kapansın ve ilk tahmin bölümü tanıtımlı olsun. | Evet |
| 9 | Önemli | Kavram ilerleyişi | Yol gölgesi (planın basacağı hücrelerdeki ayak izleri) Minik ve Kaşif'te varsayılan açık, bölüm verisinde kademelendirilmiyor. Kaşif'in çekirdeği olan zihinde canlandırma ve tahmin bu yüzden hiç desteksiz kalmıyor. Destek azaltma takvimi bölüm verisinde görünmüyor. | Çerçeve "Destek azaltma takvimi bölüm verisinde açık olmalıdır", YON-02, R-ILR-3 | Yol gölgesi de hayalet yol gibi bölüm veya seviye bazında azaltılsın: Minik'te açık, Kaşif'in ortasından sonra kapalı. Veli ayarı üst sınır olarak kalabilir. | Evet |
| 10 | Önemli | Kavram ilerleyişi | `meta.concepts` alanında hata avcısı bölümleri `debugging`, sayma bölümleri `counting` etiketi taşımıyor; 36 bölümün hepsi `sequencing` veya `direction`. Beceri modeli, YZ-02 desteği azaltma ve veli raporu bu iki kavramı göremiyor. | YZ-02, R-ILR-3 | Etiketleri düzeltin: KK02, 04, 06, 08, 09, 11'e `debugging`; BP02, 05, 07, 09, 10, 11, 12'ye `counting`. | Hayır |
| 11 | Önemli | Kavram ilerleyişi | Kodlama Kutusu'nda "daha kolay alternatif" çoğunlukla mekanik değiştiriyor. Sıralamanın alternatifi hata avcısı: KK03→KK02, KK05→KK04, KK07→KK06, KK10→KK09, KK12→KK11. Hata avcısının alternatifi sıralama: KK04→KK03, KK08→KK07. 10. bölümde takılan Minik çocuğa "kolay" diye ilk hata avcısı öneriliyor. | R-ILR-7, YZ-03 | Alternatif aynı mekanikte olsun. Gerekiyorsa her mekaniğe kolay bir yedek bölüm yazılsın. | Hayır |
| 12 | Önemli | Gelişimsel uygunluk | Minik koridoru 9. bölümde bitiyor. 10. ve 11. bölüm yalnız Kaşif'e ait (en kısa çözüm 5), 12. bölüm ortak, sonrası hep Kaşif. Minik çocuk muhtemelen ikinci oturumda 5 kartlık bölümlere geçiyor. | R-GEL-1, R-ILR-4 | Karar 1 ile birlikte: Minik için 9'dan sonra ne olacağı açıkça tanımlansın (varyant, Minik sonu kutlaması veya tekrar havuzu). | Evet |
| 13 | Öneri | Gelişimsel uygunluk | Beş Kaşif bölümünde en kısa çözüm 11–12; bu, Kaşif'in 5–10 aralığının dışında: KK10, BP11, KK12, BP12, YA12. | R-GEL-1 | Finallerin uzunluğu bilinçli bir seçimse not edilsin, değilse 10'a çekilsin. | Evet |
| 14 | Öneri | Erişilebilirlik | Sırt tuşlarında büyük kırmızı Temizle ile yeşil Git yan yana. Kırmızı/yeşil, renk körlüğünde en sık karışan çift. Minik'te yan yana iki büyük tuş yanlışlıkla Temizle'ye basmayı kolaylaştırır; bellek gizliyse çocuk belleğin silindiğini fark etmez. | R-ERS-1, R-ERS-2 | Simge ve biçim farkının ekranda olduğunu doğrulayın (ok ve silgi). Tuşlar arasına boşluk bırakın. Temizle'ye basınca görünür bir boşalma olsun. | Hayır |
| 15 | Öneri | Ses ve dil | Minik'e de çalan satırlarda soyut sözcükler var: "komut", "hatalı", "kutudan gelen kartlar", "bellek". `predict.ask` satırı Roboya'ya "Robot" diyor. `predict.same` satırındaki "hedef biraz daha ileride", hedef yanda veya geride olduğunda yanlış oluyor. | R-DIL-1, R-DIL-4 | Örnekler: "Arı eski kartları da hatırlıyor", "Roboya nereye varacak?", "Ama kaplumbağaya daha varamadık". | Hayır |
| 16 | Öneri | Belge | `sabir-ormani-36-bolum-taslagi.md` JSON'un gerisinde (ayrıntı aşağıda). | — | Tablo JSON'dan yeniden üretilsin ve destek sütunları eklensin. | Hayır |

Bu kapsamda değer gömülmesi ile hata ve motivasyon bakışlarında sorun bulunmadı. Hiçbir bölümde can, süre veya yıldız kaybı yok. Hata sesleri ("Ups! ... bir daha deneyelim") ve `predict.differ` satırı suçlamıyor. Tek açık soru, yanlış tahminin deneme sayılıp sayılmadığı (Ekibe sorular 6).

## Ayrıntılar

### Çocuğun yolculuğu: Minik (1. bölümden başlar)

Destek kısaltmaları:
- **R**: rehberli (`options.guided`)
- **H**: hayalet yol (`options.ghostPath`, doğru rota)
- **G**: yol gölgesi (çocuğun planı). Plan notuna göre Minik ve Kaşif'te açık, arının belleği gizliyken kapalı; kodda doğrulanmadı.
- **B**: arının belleği (görünür veya gizli)

| Sıra | Bölüm | En kısa / şerit | Palet | Destek | Bu bölümdeki yenilik |
| --- | --- | --- | --- | --- | --- |
| 1 | YA01 | 2 / 3 | ileri | R, H, G | Oyunun kendisi, ileri kartı (tanıtım) |
| 2 | YA02 | 3 / 4 | ileri | H, G | Daha uzun düz yol |
| 3 | YA03 | 3 / 4 | ileri, sağ | R, H, G | Sağa dön kartı (tanıtım) |
| 4 | KK01 | 4 / 5 | ileri, sağ | R, H, G | Kodlama Kutusu tahtası, iz bırakma; ilk 4 kartlık plan |
| 5 | BP01 | 2 / 3 | ileri | R, **G yok, B gizli** | Arı karakteri, sırt tuşları (Git, Temizle), kalıcı bellek ve konum, "topla" hedefi (kaplumbağaya varma yok) |
| 6 | YA04 | 4 / 5 | ileri, sağ | G | Sağa dönüş ilk kez rehbersiz ve hayalet yolsuz |
| 7 | YA05 | 3 / 4 | ileri, **sol, sağ** | R, G | Sola dön kartı (tanıtım); iki dönüş kartı ilk kez birlikte |
| 8 | BP02 | 3 / 4 | ileri | **Destek yok, B gizli** | Sayma (3 çiçek) |
| 9 | KK02 | 3 / 4 | ileri, sol, sağ | H, G | Hata avcısı: hazır kod, izle → bul → düzelt, büyüteç |
| 10 | KK03 | 5 / 6 | ileri, sağ | H, G | (Yalnız Kaşif) 5 kart |
| 11 | BP03 | 5 / 6 | ileri, sağ | B gizli | (Yalnız Kaşif) Renk çeldiricisi, Temizle |
| 12 | YA06 | 4 / 6 | ileri, sol, sağ | G | Robot doğuya bakıyor: Roboya'nın gözünden düşünme |

Oturum: Minik için önerilen oturum 10–15 dakika. Bölüm başına giriş sahnesi, oyun ve bitiş sahnesi birlikte yaklaşık 1,5–2,5 dakika sürerse (bu bir varsayım, çocuk testinde ölçülmeli) çocuk ilk oturumda aşağı yukarı 1–6. bölümleri oynar. Bu oturumda iki yeni oyun (Kodlama Kutusu ve Bal Peşinde), bir yeni kart ve Bal Peşinde'nin bellek kuralı var. İkinci oturum (7–12) sola dönü, saymayı, hata ayıklamayı ve iki Kaşif bölümünü getiriyor. Her iki oturumda da 3–4 yenilik düşüyor. Çerçevedeki "bir bölümde tek yenilik" kuralı çoğu bölümde tutuluyor; 4., 5. ve 9. bölüm istisna. Ama oturum başına yenilik sayısı Minik için yüksek.

Kural değişimi: Yön Avcısı ve Kodlama Kutusu'nda her oynatma baştan başlıyor (bölüm verisinde böyle bir ayar yok; davranış bu yönde varsayıldı, doğrulanmalı). Bal Peşinde'de ise arı durduğu yerde kalıyor ve eski kartlar yeniden çalışıyor. 3,5–5 yaş çocuğun öğrendiği bir kuralı bırakıp yenisine geçmesi genel ilke olarak zordur; çocuk aynı tahtaya benzeyen bir ekranda aynı kartları başka bir sonuçla görüyor. Kural değişimi, bellek görünmezken nedeni ekranda hiç görünmeyen bir şaşırtmaya dönüşüyor.

### Çocuğun yolculuğu: Kaşif (ADR 0018'e göre)

ADR 0018 güncellemesine göre başlangıçtan önce kalan tanıtım bölümleri önce oynanıyor. Tanıtım bölümü, oyunun ilk bölümü ya da kart tanıtan bölüm demek. Buna göre Kaşif çocuğun sırası şöyle:

1. **Tanıtımlar:** YA01 (1), YA03 (3), KK01 (4), BP01 (5, bellek gizli), YA05 (7). Hepsi 2–4 kartlık Minik bölümleri. Isınma olarak uygun.
2. **Kendi başlangıcı:**
   - KK03 (10), BP03 (11: renk çeldiricisi, Temizle, bellek gizli), YA06 (12)
   - **KK04 (13): ilk hata avcısı.** Kod 5 kartlık; rehber ve hayalet yol yok.
   - BP04 (14), YA07 (15: ilk engel)
   - **KK05 (16): ilk tahmin.** En kısa çözüm 8; bir önceki Kodlama Kutusu bölümünde 5'ti.
   - BP05 (17: Kaşif'in ilk saymaları), YA08 (18), KK06 (19), BP06 (20), YA09 (21), KK07 (22: tahmin)
   - **BP07 (23): bellek ilk kez görünür**
   - YA10 (24), KK08 (25: iki yanlış kart), BP08 (26: geri kartı tanıtımı), YA11 (27)
   - KK09–KK12 ve BP09–BP12 (28–35), YA12 (36)

Atlanan bölümler YA02, YA04, BP02 ve KK02. Kaşif çocuk için en önemli kayıp KK02, çünkü o bölüm hata avcısının en kısa, tek yanlışlı ilk örneği. Değerlendirme #6'nın amacı ("Kaşif çocuk hata avcısını tanıtımsız görmesin") bu yüzden tam karşılanmamış: Kodlama Kutusu'nun ilk bölümü (KK01) bir sıralama bölümü, hata avcısı değil. Bulgu #6 bu yüzden var.

### Bulgu 1: Minik'te görünmez bellek

- **Ne görülüyor:**
  - BP01 ve BP02'de `beeMemoryVisible` yok, yani bellek gizli. Plan notu (Görsel temel P1 - 2), yol gölgesinin "arının hafızası gizliyken görünmez" olduğunu söylüyor.
  - BP02'de rehber ve hayalet yol da yok. Yön Avcısı'nda Minik çocuk her bölümde en az yol gölgesini görüyordu; Bal Peşinde'ye geçince bütün plan desteği birden kalkıyor.
  - Değerlendirme #10'a göre gizli bellek şeritte "boş sarı yuvarlaklar" olarak görünüyor. Çocuk kaç tuşa bastığını görebilir ama hangilerine bastığını göremez (kodda doğrulanmalı).
  - `forgot_clear` satırı yardımcı oluyor, ama Temizle tuşu Minik'te hiçbir girişte anlatılmıyor. İlk kez BP03'te, yani Kaşif'te adı geçiyor.
- **Neden önemli:** Çerçevenin Minik notu açık: görünmez bellek, görünmez hedef ve yalnız seste kalan bilgi Minik için yüktür; bilgi ekranda olmalıdır. 4 yaşındaki bir çocuğun daha önce bastığı tuşları akılda tutması ve bunları yeni basacaklarıyla birleştirmesi gerekiyor. Arı yerinde kaldığı için çocuğun bir de yeni başlangıç noktasını hesaba katması gerekiyor. Bu, iki görünmeyen bilgiyi aynı anda akılda tutmak demek. İlk denemede başaramayan Minik çocuğun ikinci denemesi bu yüzden sonucu öngörülemez hale geliyor.
- **Neden `Engel`:** Rubrik `Engel`i "yaşa açıkça uygunsuz" durum için kullanıyor ve bu durum projenin kendi çerçevesine doğrudan aykırı. Ekip PRD'deki Bee-Bot ifadesini ("kod arının belleğinde saklanır ve görünmez") Minik için de bağlayıcı sayarsa bulgu `Önemli`ye iner. Bu bir ekip kararı.

### Bulgu 2 ve Karar 1: Minik sırası

PRD'deki yaş tablosu Minik kavramlarını "yön, neden-sonuç, basit sıralama" olarak sayıyor; "hata ayıklama" ve "sayma" Kaşif satırında. Oyun tablosunda Yön Avcısı 3.5+, Kodlama Kutusu ve Bal Peşinde 4+. Şu anki dizilimde 3,5 yaşında başlayan bir çocuk 4. ve 5. bölümde 4+ oyunlara geliyor. 8. bölümde sayıyor, 9. bölümde hata ayıklıyor.

| Seçenek | Ne değişir | Artı | Eksi |
| --- | --- | --- | --- |
| **1A** Mevcut sıra, yalnız düzeltmeler (KK02 rehberli, BP02 en az yol gölgeli) | Sıra aynı kalır, destek eklenir | En az içerik işi; çeşitlilik erken | Oturum başına 3–4 yenilik sürer; PRD'nin Minik kavram listesinin dışına çıkılır; Bulgu 1 ancak Karar 2 ile çözülür |
| **1B** Değerlendirmedeki öneri: ilk ~6 bölüm yalnız Yön Avcısı, sonra görünür bellekli kısa Bal Peşinde, hata avcısı Minik'in sonunda 2–3 kartla | Minik bölümleri yeniden dizilir; yeni Minik Yön Avcısı bölümleri gerekir (şu an Minik Yön Avcısı yalnız YA01–05 ve ortak YA06) | Bir mekanik iyice oturmadan diğerine geçilmez; sağ ve sol, hata ayıklamadan önce pekişir | Daha uzun tek oyunlu başlangıç sıkıcı olabilir; ücretsiz 3 bölüm yalnız Yön Avcısı kalır; yeni bölüm yazımı ve ses kaydı gerekir |
| **1C** Oyun değişir, mekanik değişmez: iç içe dizilim kalır ama Minik'te Kodlama Kutusu yalnız sıralama, Bal Peşinde yalnız görünür bellek (gerekirse kendiliğinden temizlenen bellek); hata avcısı ve sayma Kaşif'e geçer | KK02 Kaşif'in ilk ve rehberli hata avcısı olur (Bulgu 6'yı da kapatır); BP02 sayma yerine bir sıralama bölümüne döner ya da Kaşif'e geçer; Minik'e 1–2 yeni bölüm eklenir | Çeşitlilik korunur, mekanik yükü düşer; PRD'nin Minik kavram listesine uyar; bir taşla iki sorun (Minik yükü ve Kaşif'in tanıtımsız hata avcısı) | Minik çocuk hata avcısını hiç görmez (PRD buna izin veriyor); Bal Peşinde Minik'te Yön Avcısı'nın görünüşünden ibaret kalma riski taşır; içerik ve kural işi gerekir |
| **1D** PRD'deki "Minik ve Kaşif varyantları": her bölümün iki uzunlukta sürümü | Yol yaşa göre ayrışır | Uzun vadede en temiz yol; Bulgu 12'yi de çözer | En büyük içerik ve kod işi; MVP takvimine sığmayabilir |

Ön tercih: 1C kısa vadede, 1D uzun vadede. 1C'nin Bal Peşinde'yi "görünüş değişikliğinden ibaret" bırakma riski Karar 2'deki görünür bellekle azalır. O durumda Minik'in Bal Peşinde'deki yeni deneyimi tuşa basınca arının sırtında bir ışığın yanması olur; bu, neden-sonuç ilişkisini somut gösterir ve Minik kavramları arasında yer alır.

### Bulgu 5 ve Karar 2: Bal Peşinde'de bellek görünürlüğü

PRD şöyle diyor: "Bee-Bot'taki gibi kod arının belleğinde saklanır ve görünmez; Kaşif seviyede bellek görünür hale getirilebilir." Bu cümle şu anki sırayı destekliyor: önce gizli, sonra görünür. Çerçeve ise tersini istiyor: önce destek, sonra kademeli azaltma. Görsel temel P1 - 2 bu dengeyi iki yönden değiştirdi:

1. **Yol gölgesi** gizli bellekte kapanıyor. Bu yüzden bellek görünürlüğü artık yalnız şeridi değil, plan desteğinin tamamını belirliyor.
2. **Sırt tuşları** (Git ve Temizle) gerçek Bee-Bot'a benziyor. Bellek görünürse tuşa basınca bir ışığın yanması ve Temizle'ye basınca ışıkların sönmesi, Temizle'yi somut bir eyleme çeviriyor. Bellek gizliyse Temizle'nin ekranda görünür hiçbir etkisi yok.

| Seçenek | Minik | Kaşif | Artı | Eksi |
| --- | --- | --- | --- | --- |
| **2A** Mevcut | Gizli (yol gölgesi de yok) | BP03–06 gizli, BP07–12 görünür | PRD cümlesine harfiyen uyar; ek iş yok | Destek zamanla artar, azalmaz; Bulgu 1 kalır; BP07'nin alternatifi desteği kaldırır |
| **2B** Ters çevir | Görünür ve yol gölgesi açık; Minik'te bellek her çalıştırmadan sonra kendiliğinden temizlenebilir (BAL-02 Kaşif'ten başlar) | BP03–BP08 görünür; son 3–4 bölümde (ör. BP09–12) gizli ve yol gölgesi kapalı, Bee-Bot gibi bir meydan okuma | Desteği azaltma ilkesine uyar; sonunda gizli belleğe varmak yüz yüze Bee-Bot atölyesine iyi bir hazırlık olur | PRD cümlesi güncellenmeli (ürün sahibi); `beeMemoryVisible` 12 dosyada değişir, sesler (BP07 girişi ve başarısı) yeniden yazılır; Minik'te kendiliğinden temizleme kod işi gerektirir |
| **2C** Kademeli gizleme | Görünür | Bellek plan kurulurken görünür, Git'e basınca kapanır; arıya dokununca kısa süre gösterilir; finallerde tamamen gizli | En yumuşak geçiş; "hatırla" becerisini adım adım kurar | Yeni bir mekanik demek: yeni kod, ses ve test; kendisi de bir yenilik olarak tanıtılmalı |
| **2D** Minik'te Bal Peşinde yok (1B ile birlikte) | — | 2B veya 2C | Minik yükü en aza iner | Minik'in oyun çeşitliliği azalır; PRD oyun tablosu (Bal Peşinde 4+) Minik'in üst yarısını kapsıyor |

Ön tercih: 2B. Desteği azaltma ilkesine uyuyor, mevcut veri alanıyla (`beeMemoryVisible`) yapılabiliyor ve sonunda Bee-Bot'un gizli belleğine ulaşıyor. Minik'te kendiliğinden temizleme ayrı bir alt karar. BAL-02'nin öğrettiği "unutulan Temizle" hatası, çerçevedeki hatadan öğrenme ilkesine uygun ve değerli. Ama 4 yaş için iyi bir ilk deneyim değil.

### Bulgu 8 ve 9 ve Karar 3: "Tahmin et" adımı

- **Durum:**
  - Şemada `options.predict` var ("ilk çalıştırmadan önce çocuk robotun nereye varacağına dokunur, sonuç tahminle karşılaştırılır").
  - KK05 (16), KK07 (22), KK10 (30) ve KK12 (34) bölümlerinde açık. Bunların hepsi Kodlama Kutusu'nun sıralama bölümleri, hata avcısı değil.
  - Ses satırları: `predict.ask`, `predict.same`, `predict.differ`.
  - Tahmin adımı çocuk deneyimini değiştiren bir mekanik ve pedagoji onayı olmadan yayına girmemeli (PRD "Uzman onayı").
- **Yol gölgesiyle çakışma:** Yol gölgesi çocuğun planının basacağı hücreleri ayak iziyle gösteriyor ve Kaşif'te varsayılan açık. Tahmin anında da görünüyorsa çocuk son ayak izine dokunur; tahmin bir zihin işi olmaktan çıkar, kopyalamaya döner. Kodda doğrulanmalı.
- **Kaşif'in çekirdeği:** Çerçeveye göre "planlanan ile gerçekleşen yolu karşılaştırma ve tahmin etme bu seviyenin çekirdeğidir". Tahmin adımı bu ilkeye uygun. Ama yol gölgesi sürekli açıkken bu beceri hiçbir bölümde desteksiz çalıştırılmıyor (Bulgu 9).

| Seçenek | Ne | Artı | Eksi |
| --- | --- | --- | --- |
| **3A** Mevcut uygulama kalsın, düzeltilsin | Tahmin KK05/07/10/12'de kalır; tahmin anında yol gölgesi gizlenir; KK05'teki ilk tahmin rehberli bir kısa tanıtım ister (veya tahmin 5–6 kartlık bir bölümde başlar); `predict.same` düzeltilir | Uygulama hazır, iş az; KUT-02'nin "karşılaştırma" amacına yakın | Tahmin, sıralama bölümlerinde hata ayıklamadan kopuk bir ek adım olarak kalır; 8–11 kartlık planlarda son noktayı zihinde canlandırmak 5–7 yaş için ağır olabilir |
| **3B** Tahmini hata avcısına taşı | Hata avcısının "izle" adımından önce: "Bu kod Roboya'yı nereye götürecek?" Çocuk dokunur, hazır kod çalışır, tahmin ve gerçek iz karşılaştırılır, sonra hata bulunur | Tahmin ile hata ayıklama aynı amaca hizmet eder; yanlış tahmin, hatayı aramak için doğal bir merak yaratır; hazır kodun ayak izi zaten gösterilmediği için kopya riski azdır | Hata avcısı akışı 3 adımdan 4 adıma çıkar; ses ve akış işi gerekir; KK04/06/08/09/11'in uzunluğu yeniden gözden geçirilmeli (8 kartlık kodda tahmin çok zor) |
| **3C** Kısa planlarda ve azalan destekle | Tahmin yalnız 5–7 kartlık planlarda olur; ilk tahmin bölümü rehberli; sonraki tahminlerde yol gölgesi kapalı | Tahmin becerisi önce kısa kodda oturur | Hangi bölümlerin seçileceği içerik kararı ister |
| **3D** Çocuk testine kadar bayrakla kapat | `predict` veride kalır ama yayında kapalıdır | Onaysız mekanik yayına çıkmaz | KUT-02'nin karşılaştırma amacı açık kalır |

Ön tercih: kısa vadede 3D, çocuk testinde 3B ile 3C'nin birleşimi denensin. 3B'yi öne çıkaran şey, tahmini hata ayıklamaya bağlaması. Bu, PRD'nin Kodlama Kutusu tanımına (karşılaştır, sonra düzelt) en yakın seçenek. Hangi seçenek seçilirse seçilsin: tahmin yıldızı etkilememeli (OYN-04 ölçütlerinin dışında kalsın) ve yanlış tahmin "deneme" sayılmamalı.

### Bulgu 3: KK02 (Minik'te ilk hata avcısı)

Robot (0,3) noktasında kuzeye bakıyor; hedef (1,2). Doğru kod: ileri, sağa dön, ileri. Hazır kodda tek yanlış var: sağa dön yerine sola dön. Kod kısa ve tek yanlışlı, yani çerçevenin "çok kısa kod, tek yanlış" koşulu sağlanıyor. "İyi tanıtılmış" koşulu ise sağlanmıyor: `guided` yok. Yanlışı görmek için sağ ile solu ayırmak gerekiyor; sola dön 7. bölümde tanıtıldı, arada yalnız bir sayma bölümü var. Ses arka arkaya iki yönerge veriyor:

- `kodlama_kutusu.l02.intro`: "Yanlış kartı bulabilir misin?"
- `kodlama_kutusu.hunt`: "Kutudan gelen kartlardan biri hatalı. Büyüteçli kartlara dokunup hatayı bulalım!"

Bu yönergelerde "izle, bul, dokun, düzelt" adımları bir arada. Minik'te bir yönergede tek iş ilkesine uymuyor (R-DIL-2). 1C seçeneği seçilirse bu bölüm olduğu gibi Kaşif'in ilk ve rehberli hata avcısı olabilir. Kaşif için de uzunluğu uygun, bir ısınma bölümü olur.

### Bulgu 16: Taslak tablosu ile JSON arasındaki farklar

- **Destek ayarları:** Taslak tablosunda rehber, hayalet yol, bellek görünürlüğü ve tahmin sütunları yok. JSON'da KK01 ve BP01 rehberli (değerlendirmeden sonra eklendi), KK05/07/10/12'de `predict` açık. Taslakta tahminden hiç söz edilmiyor.
- **Sola dön sırası:** "İncelemede sorulacaklar" 5. maddesi sola dönün 9. bölümde tanıtıldığını söylüyor. Hem tablo hem JSON'a göre 7. bölüm.
- **Kaşif başlangıcı:** Dizilim kuralındaki Kaşif başlangıcı açıklaması ADR 0018'in 2026-10-09 güncellemesini (tanıtım bölümleri önce oynanır) içermiyor.
- **KK07:** Taslak ve `meta.notes` "önce çiçeğe uğra, sonra kapıya var" diyor. JSON'da nesne elma; ses "elmayı alalım, sonra çıkışa gidelim" diyor. Ses ile tahta uyumlu, not geride. Ayrıca KK07, KK09, KK12, YA11 ve YA12'nin sesleri "çıkış" diyor. Hedef ekranda kaplumbağa olarak çiziliyorsa bu R-DIL-3 açısından bakılmalı (doğrulanmalı).
- **Yaş ve palet:** Yaş etiketleri ve paletler tabloyla uyumlu. Tablodaki "Tür" sütunu (hata avcısı, sayma) JSON'daki `meta.concepts` alanına yansımamış (Bulgu 10).

## Ekibe sorular

1. **Minik kavram kapsamı:** Minik seviyede hata ayıklama ve sayma bulunacak mı? PRD'nin yaş tablosu bu ikisini Kaşif'te sayıyor. Cevap Karar 1'deki seçeneği büyük ölçüde belirliyor.
2. **YON-01'in yorumu:** Minik'te sola dön kartı hiç olmayacak mı ("tek yönde dönüş"), yoksa iki dönüş kartı da olabilir ama tek seferde yalnız biri yeni mi gelecek? Şu an Minik'in 7., 9. ve 12. bölümleri iki dönüş kartını birlikte veriyor.
3. **Bal Peşinde'de Minik:** Bellek görünür mü olacak? BAL-02 (kalıcı bellek, arının yerinde kalması) Minik'te de geçerli mi, yoksa Minik'te bellek her çalıştırmadan sonra kendiliğinden mi temizlenecek?
4. **Atölye deneyimi:** Yüz yüze Bee-Bot atölyelerinde belleği hangi yaşta, nasıl tanıtıyorsunuz (ör. basılan tuşları kâğıda çizmek, sesli saymak)? Bu deneyim 2B ile 2C arasındaki seçimi belirleyebilir.
5. **PRD'deki Bal Peşinde cümlesi:** "Kaşif seviyede bellek görünür hale getirilebilir" cümlesi değişecekse ürün sahibine götürülsün mü?
6. **Tahmin adımı:**
   - Yayında kalsın mı, kapatılsın mı?
   - Sıralama bölümlerinde mi, hata avcısında mı olsun?
   - Yanlış tahmin deneme sayılsın mı, yıldızı etkilesin mi?
7. **Yol gölgesi:** Kaşif'te varsayılan açık kalsın mı? Bölüm verisinde kademelendirilsin mi (ör. Kaşif'in ikinci yarısında kapalı)? Tahmin anında kapansın mı?
8. **Minik'in 9. bölümden sonrası:** Minik çocuk 9. bölümden sonra ne oynamalı? Kaşif bölümlerine geçmek, Minik varyantları, Minik sonu kutlaması veya tekrar havuzu gibi seçenekler var.
9. **3,5–4 yaş:** 3,5–4 yaş çocuk, PRD'de 4+ diye işaretlenmiş Kodlama Kutusu ve Bal Peşinde'ye 4. ve 5. bölümde gelsin mi?
10. **Kaşif finalleri:** Finallerde 11–12 kartlık çözümler bilinçli bir seçim mi?

## Çocuk testinde gözlenecekler

- **Bal Peşinde'nin ilk bölümü (Minik, 5. bölüm):**
  - Çocuk kimi programladığını söylüyor mu ("arı")?
  - Sırt tuşlarına mı yoksa paletteki kartlara mı dokunuyor?
  - Temizle'ye kendiliğinden ya da yanlışlıkla basıyor mu?
- **Bal Peşinde'de ilk denemede başaramayan Minik çocuk:**
  - İkinci denemeden önce Temizle'ye basıyor mu?
  - Arının yerinde kalmasına şaşırıyor mu ("niye orada?")?
  - Kaç denemede çıkabiliyor? Bıraktığı oluyor mu?
- **Gizli ve görünür bellek karşılaştırması:** Aynı bölüm (ör. BP02) iki gruba farklı ayarla oynatılabilir. Ölçülecekler: deneme sayısı, Temizle kullanımı, keyif göstergeleri.
- **Kural değişimi:** Yön Avcısı'ndan Bal Peşinde'ye geçince çocuk arının başa dönmesini bekliyor mu?
- **Hata avcısı (KK02):**
  - Çocuk hazır kodun çalışmasını izliyor mu?
  - Büyüteçli karta mı dokunuyor, yoksa bütün kodu silip baştan mı kuruyor?
  - Sağ/sol karışıklığı yaşıyor mu? Çocuğun kendi sağını ve solunu bilip bilmediği ayrıca not edilsin.
- **Sola dön tanıtımı (7. bölüm):** İki dönüş kartı birlikteyken çocuk hangisini seçiyor? Kaç yanlış dönüş yapıyor?
- **Tahmin adımı (Kaşif):**
  - Çocuk son ayak izine mi dokunuyor, yoksa zihninde yürütüyor mu (dudak hareketi, parmakla iz sürme)?
  - Tahmin yanlış çıkınca planını mı değiştiriyor, yoksa tahmin adımını geçiştiriyor mu?
- **Oturum başına bölüm sayısı ve yenilik:**
  - Minik ve Kaşif'te 15 dakikada kaç bölüm oynanıyor?
  - Hangi bölümde ilk sıkılma ya da bırakma işareti görülüyor?
- **Görünür bellek:** BP07'de (ya da 2B seçilirse ilk görünür bellek bölümünde) çocuk şeridi kullanıyor mu? Kart sayısını sesli sayıyor mu?
