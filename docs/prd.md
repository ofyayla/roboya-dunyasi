# Roboya Dünyası: Minik Mucitler — Ürün Gereksinim Dokümanı (PRD)

Oct 7, 2026 · @Ömer

## Yönetici özeti

Roboya Dünyası, 3.5–10 yaş çocuklara hikâye içinde kodlama ve değerler öğreten, Türkçe seslendirmeli bir mobil oyun uygulamasıdır. Ailelere bireysel abonelikle, okullara ve kurumlara lisansla satılır.

- **Ürün:** Robot Roboya'nın kaza ile düştüğü Değerler Adası'nda 5 bölge, 7 oyun modülü ve 3 yaş seviyesi.
- **Kullanıcılar:** Çocuk oynar; veli bireysel abonelikte, öğretmen ve okul yönetimi kurumsal lisansta karar verir.
- **Gelir:** Ücretsiz başlangıç + aile aboneliği (aylık/yıllık), sınıf ve kampus lisansı, kurum lisansı, okul-ev köprüsü.
- **Farklılaşma:** Okuma gerektirmeyen Türkçe deneyim, oyun mekaniğine gömülü değerler, tek üründe veli ve okul modu, mühendislerin tasarladığı müfredat.
- **MVP:** Sabır Ormanı bölgesi, 3 oyun, yaklaşık 36 bölüm, Minik ve Kaşif seviyeleri, temel veli alanı ve aile aboneliği.

| Alan | Değer |
| --- | --- |
| Ürün adı | Roboya Dünyası: Minik Mucitler (çalışma adı) |
| Marka | Roboya Kids |
| Doküman sürümü | v0.2 (açık soru kararları işlendi) |
| Kapsam | MVP'den v2.0'a kadar |
| Platformlar | iOS ve Android (telefon + tablet), web tabanlı öğretmen ve veli paneli |
| İlk pazar | Türkiye, Türkçe |

## Problem ve fırsat

Veliler ve okullar erken yaşta kodlama istiyor, ancak Türkçe, okuma gerektirmeyen, pedagojik olarak güvenilir ve ekran süresini ciddiye alan bir uygulama bulmakta zorlanıyor.

**Problemler**

1. **Dil ve okuma engeli:** Bu alandaki uygulamaların çoğu İngilizce. 3.5–6 yaş çocuk okuyamadığı için yazılı yönergeli oyunları tek başına oynayamıyor.
2. **Ekran kaygısı:** Veliler çocuğun öğrenip öğrenmediğini göremiyor; bağımlılık yapan mekaniklerden çekiniyor.
3. **Değer boşluğu:** Kodlama uygulamaları teknik beceriye odaklanıyor; sabır, paylaşma, yardımlaşma gibi değerleri işlemiyor.
4. **Okulun ölçeklenme sorunu:** Okul öncesi öğretmenlerinin çoğu kodlama eğitimi almış değil. Okullar uzman eğitmen getirmek zorunda kalıyor, bu da pahalı ve sınırlı.
5. **Roboya Kids'in kendi darboğazı:** Yüz yüze model eğitmen saatine bağlı; gelir eğitmen sayısıyla doğrusal büyüyor.

**Fırsat**

- Roboya Kids'in mevcut 4 aşamalı müfredatı (kodlama matı, kutu içi kodlama, Bee-Bot, Lego) ve değerler yaklaşımı dijital oyunlara doğrudan aktarılabilir.
- Tek bir dijital ürün hem velilere hem okullara satılabilir; eğitmen saati darboğazı ortadan kalkar.
- Okul lisansı, veliyle doğal bir temas noktası yaratır ve bireysel aboneliğe düşük maliyetli bir edinim kanalı açar.
- Dijital içerik yerelleştirilerek değer odaklı eğitime ilgi duyan yurt dışı pazarlara düşük maliyetle taşınabilir.

## Hedefler, başarı metrikleri ve kapsam dışı

MVP'nin tek amacı, çocukların uygulamaya kendiliğinden geri döndüğünü ve velilerin bunun için ödeme yapmaya istekli olduğunu kanıtlamaktır. Aşağıdaki hedef değerler ilk varsayımlardır; pilot verisiyle güncellenecektir.

**Hedefler**

1. **Ürün:** 3.5 yaşındaki bir çocuk, okuma bilmeden ve yetişkin yardımı olmadan ilk bölümü tamamlayabilmeli.
2. **Pedagoji:** Her bölüm bir kodlama kavramını ve bir değeri ölçülebilir şekilde işlemeli.
3. **İş (B2C):** Ücretsiz kullanıcıları aile aboneliğine dönüştürmek.
4. **İş (B2B):** Pilot okullarda kullanımı kanıtlayıp ücretli lisansa geçirmek.
5. **Güven:** Velinin çocuk verisi ve ekran süresi konusunda tam kontrolü olduğunu hissetmesi.

**Başarı metrikleri**

| Metrik | Tanım | MVP hedefi (varsayım) |
| --- | --- | --- |
| İlk bölüm tamamlama | Profil oluşturan çocuklardan ilk bölümü bitirenlerin oranı | ≥ %85 |
| D1 / D7 / D30 elde tutma | Kurulumdan 1, 7 ve 30 gün sonra oturum açan çocuk profili oranı | %40 / %20 / %10 |
| Haftalık aktif gün | Aktif çocuk başına haftada oynanan gün sayısı | ≥ 3 |
| Deneme başlatma | Ücretsiz bölümü bitirip abonelik ekranını gören velilerden deneme başlatanlar | ≥ %15 |
| Denemeden ücretliye geçiş | Deneme başlatanlardan ücretli aboneliğe geçenler | ≥ %40 |
| Kurulumdan ücretliye geçiş | Tüm kurulumlardan ücretli aboneliğe geçenler | ≥ %3 |
| Pilot okul dönüşümü | Pilot okullardan ücretli lisansa geçenler | ≥ %50 |
| Öğretmen haftalık kullanımı | Lisanslı sınıflarda uygulamanın kullanıldığı hafta oranı | ≥ %70 |
| Veli memnuniyeti | Veli alanındaki kısa ankette 5 üzerinden puan | ≥ 4.3 |
| Çökmesiz oturum | Hata vermeden tamamlanan oturum oranı | ≥ %99.5 |

**Kapsam dışı (bilinçli olarak yapılmayacaklar)**

- Fiziksel kit, basılı materyal veya ekransız ev etkinlikleri.
- Canlı veya kayıtlı video dersler.
- Çocuklar arası sohbet, mesajlaşma veya halka açık paylaşım.
- Çocuğun serbestçe konuştuğu bir yapay zekâ sohbet asistanı.
- Reklam, sürpriz kutu, oyun içi para birimi ile satın alma.
- Liderlik tabloları ve çocukları birbiriyle kıyaslayan sıralamalar.

## Hedef kullanıcılar ve personalar

Ürünü kullanan (çocuk) ile satın alan (veli, okul) farklıdır; bu yüzden her ekranın kime hitap ettiği net olmalıdır.

| Persona | Profil | İhtiyaç | Kaygı | Üründe teması |
| --- | --- | --- | --- | --- |
| Ali, Minik oyuncu | 4.5 yaş, okuma bilmiyor, tablette çizgi film izliyor | Kolay, sesli, anında geri bildirim veren oyun | Zor gelirse bırakır | Harita, oyunlar, ödüller |
| Zeynep, Mucit oyuncu | 8 yaş, okuyor, ScratchJr denemiş | Meydan okuma, kendi şeyini üretme | Çocuksu bulursa sıkılır | Zor bölümler, Serbest Mucit Modu |
| Ayşe, veli (B2C alıcı) | 34 yaş, çalışan anne, iki çocuk | Ekran süresinin faydalı geçtiğini görmek | Bağımlılık, reklam, veri güvenliği, gizli ücretler | Veli alanı, raporlar, abonelik |
| Merve, okul öncesi öğretmeni | 28 yaş, kodlama eğitimi almamış, sınıfında 18 çocuk | Hazır, kolay yönetilen etkinlik | Teknik sorun, sınıf kontrolünü kaybetmek | Öğretmen paneli, okul modu |
| Hakan, özel okul kurucusu (B2B alıcı) | Zincir anaokulu, 4 kampus | Kayıt döneminde gösterilebilir bir teknoloji programı | Maliyet, veli şikayeti, KVKK | Kampus lisansı, kurum raporları |
| Kurumsal sponsor | Belediye, vakıf veya şirket sosyal sorumluluk birimi | Toplu erişim ve etki raporu | Bütçe kontrolü, görünürlük | Kurum lisansı, etki raporu |

**Birincil segmentler (öncelik sırasıyla)**

1. 3.5–7 yaş çocuğu olan, kentli, akıllı cihaz sahibi aileler (B2C).
2. Özel anaokulları ve özel ilkokulların okul öncesi ve 1.–2. sınıfları (B2B).
3. Zincir okullar, belediye çocuk kulüpleri ve vakıflar (kurum lisansı).

## Ürün konsepti: hikâye, dünya, bölgeler ve değerler

Çocuk, Değerler Adası'na düşen robot Roboya'nın evine dönmesine kod yazarak yardım eder; her bölge bir değer ve bir kodlama kavramı öğretir.

**Ana hikâye**

Roboya'nın uzay gemisi fırtınada bozulur ve adaya düşer. Gemi parçaları beş bölgeye dağılmıştır. Her bölgede yaşayan karakterlerin bir sorunu vardır. Çocuk, Roboya'yı programlayarak bu sorunları çözer, karşılığında bir gemi parçası ve o bölgenin değerini kazanır. Beş parça tamamlandığında gemi onarılır; sonraki sezonlarda yeni adalar açılır.

| Bölge | Değer | Kodlama kavramı | Ana oyun(lar) | Bölge karakteri ve sorunu | Sürüm |
| --- | --- | --- | --- | --- | --- |
| Sabır Ormanı | Sabır | Sıralama; önce planla, sonra çalıştır | Yön Avcısı, Kodlama Kutusu, Bal Peşinde | Bilge Kaplumbağa; orman yolları karışmış | MVP |
| Paylaşım Köyü | Paylaşma | Sayma, toplama ve eşit dağıtma | Bal Peşinde (genişletilmiş) | Köylü arılar; hasat herkese yetmiyor | v1.0 |
| Yardımlaşma Limanı | Yardımlaşma | Paralel görevler, iş bölümü | Birlikte Başaralım | Balıkçı fok; tekne tek başına yürümüyor | v1.5 |
| Nezaket Bahçesi | Nezaket | Döngüler ve tekrar eden desenler | Döngü Dansı | Utangaç çiçekler; bahçe şenliği için dans lazım | v1.5 |
| Sorumluluk Kulesi | Sorumluluk | Hata ayıklama, koşullar (eğer-o zaman) | Robot Atölyesi | Kule bekçisi baykuş; makineler bozulmuş | v2.0 |
| Mucit Atölyesi (serbest alan) | Üretkenlik | Serbest blok kodlama | Serbest Mucit Modu | Roboya'nın kendi atölyesi | v2.0 |

**Değerlerin oyuna gömülme ilkeleri**

1. **Değer mekaniktir, ders değildir.** Sabır Ormanı'nda kod adım adım değil, tüm plan bitince çalışır; sabır oyunun kuralıdır.
2. **Her bölümün başında bir ihtiyaç vardır.** Çocuk kod yazar çünkü bir karaktere yardım etmek ister; amaç puan değil, iyiliktir.
3. **Hata cezalandırılmaz.** Yanlış kodda Roboya güler, "Bir daha deneyelim!" der; can veya süre kaybı yoktur.
4. **Değer anı kısadır.** Bölge sonunda 30–60 saniyelik animasyonlu hikâye kartı değeri pekiştirir; vaaz tonu kullanılmaz.
5. **Değerler evrenseldir.** Sabır, paylaşma, yardımlaşma, nezaket ve sorumluluk her aile profiline hitap edecek dille işlenir.

**Karakterler**

- **Roboya:** Meraklı, biraz sakar, hatalarına gülen dost robot. Çocuğun rehberi ve sesi.
- **Bölge karakterleri:** Her bölgede bir hayvan karakteri; sorunu getirir, çözümde teşekkür eder.
- Karakterler ileride kitap, animasyon ve lisanslı ürünlere taşınabilecek şekilde özgün tasarlanır ve marka olarak tescil edilir.

## Yaş seviyeleri ve pedagojik çerçeve

Aynı oyunlar üç zorluk seviyesinde sunulur; seviye yaşla başlar, sonra çocuğun performansına göre otomatik ayarlanır.

| Seviye | Yaş | Okuma | Görev uzunluğu | Kavramlar | Etkileşim | Oturum önerisi |
| --- | --- | --- | --- | --- | --- | --- |
| Minik | 3.5–5 | Yok; tamamen sesli ve görsel | 2–4 komut | Yön, neden-sonuç, basit sıralama | Büyük kartları sürükle-bırak, tek dokunuş | 10–15 dk |
| Kaşif | 5–7 | İsteğe bağlı tek kelimeler | 5–10 komut | Sıralama, döngü, hata ayıklama, sayma | Kart dizme, adım adım çalıştırma | 15–20 dk |
| Mucit | 7–10 | Kısa yönergeler okunur | 10+ komut | Koşullar, fonksiyonlar, olaylar, serbest üretim | Blok kodlama, çok adımlı problemler | 20–30 dk |

**Kodlama kavramlarının ilerleyişi**

1. Yön ve hareket (ileri, dön)
2. Sıralama (adımları doğru dizme)
3. Hata ayıklama (yanlış adımı bulup düzeltme)
4. Döngü ("3 kere tekrarla")
5. Koşul ("eğer ışık kırmızıysa bekle")
6. Fonksiyon (bir hareket dizisini tek kartta toplama)
7. Olaylar ve serbest üretim (dokununca başla, kendi animasyonunu yap)

**Pedagojik ilkeler**

- **Oyun temelli öğrenme:** Her kavram önce oyun içinde keşfedilir; açıklama en sonda ve kısadır.
- **Destekli ilerleme:** Yeni kavram tanıtılırken ilk bölümde Roboya kodu birlikte kurar, sonrakilerde destek azalır.
- **Hatadan öğrenme:** Kod çalışırken hangi adımda yanlış gidildiği görsel olarak işaretlenir.
- **Kısa ve ölçülü oturum:** Seviyeye göre önerilen süreyi aşan oturumlar Roboya'nın "şarjı bitti" anıyla nazikçe sonlandırılır (veli ayarlarına tabi).
- **Müfredat eşleştirmesi:** Her bölüm, güncel MEB okul öncesi ve ilkokul programlarındaki ilgili kazanımlarla eşleştirilir. Bu eşleştirme okul satışında kullanılır.
- **Uzman onayı:** Her bölüm Roboya Kids mühendis eğitmenleri ve bir okul öncesi eğitim danışmanı tarafından onaylanmadan yayınlanmaz.

## Oyun modülleri ve detaylı gereksinimler

Yedi oyun modülü, Roboya Kids'in yüz yüze atölyelerinin dijital karşılıklarıdır; MVP ilk üçüyle çıkar. Öncelikler: P0 MVP için zorunlu, P1 v1.x, P2 v2.0 ve sonrası.

| Oyun | Kaynak atölye | Yaş | Kavram | Değer | Bölüm (ilk sürüm) | Öncelik |
| --- | --- | --- | --- | --- | --- | --- |
| Yön Avcısı | Canlı Robot Oyunu | 3.5+ | Yön, sıralama | Sabır | 12 | P0 |
| Kodlama Kutusu | Kutu İçi Kodlama | 4+ | Sıralama, hata ayıklama | Sabır | 12 | P0 |
| Bal Peşinde | Bee-Bot | 4+ | Programlama belleği, sayma | Sabır, paylaşma | 12 | P0 |
| Döngü Dansı | Yeni | 5+ | Döngü, desen | Nezaket | 15 | P1 |
| Birlikte Başaralım | Yeni | 5+ | Paralel görev, iş bölümü | Yardımlaşma | 12 | P1 |
| Robot Atölyesi | Lego | 6+ | Tasarım, koşul, hata ayıklama | Sorumluluk | 15 | P2 |
| Serbest Mucit Modu | Yeni | 7+ | Blok kodlama, olaylar | Üretkenlik | Serbest alan | P2 |

### Ortak oyun gereksinimleri

- **OYN-01 (P0):** Tüm yönergeler profesyonel Türkçe seslendirmeyle verilir; Minik seviyede ekranda yönerge metni yoktur.
- **OYN-02 (P0):** Her bölümde "Dinle" butonu yönergeyi tekrar eder.
- **OYN-03 (P0):** Kod çalışırken aktif komut kartı vurgulanır; hata olan adımda Roboya durur ve kart titrer.
- **OYN-04 (P0):** Bölüm sonunda 1–3 yıldız verilir (çözüldü / az denemeyle / en kısa kodla). Yıldız kaybetmek mümkün değildir.
- **OYN-05 (P0):** Üç başarısız denemeden sonra ipucu önerilir (bkz. Uyarlanabilir öğrenme).
- **OYN-06 (P0):** Dokunma hedefleri en az 64 dp; sürükle-bırak, bırakılan konuma yakın en uygun yuvaya oturur.
- **OYN-07 (P0):** Bölümler veri olarak tanımlanır (JSON); pedagoji ekibi kod değişikliği olmadan bölüm ekleyebilir.
- **OYN-08 (P1):** Her oyunda sol el kullanımına uygun ayna yerleşim seçeneği.

### Yön Avcısı (P0)

**Amaç:** Roboya'yı ızgara üzerinde yön kartlarıyla hedefe götürmek.

**Mekanik:** 4×4 ile 6×6 arası ızgara. Alt şeritte ileri, sola dön, sağa dön kartları bulunur. Çocuk kartları plan şeridine dizer, "Oynat"a basar.

**Bölüm ilerleyişi:** Düz yol → tek dönüş → engelden kaçınma → birden fazla nesne toplama.

- **YON-01 (P0):** Minik seviyede yalnız "ileri" ve tek yönde dönüş kartı açık gelir; yeni kart bir tanıtım anıyla eklenir.
- **YON-02 (P0):** Sabır mekaniği: kod ancak plan şeridi tamamlandığında çalışır; tek tek adım oynatma yoktur.
- **YON-03 (P1):** "Hayalet yol" desteği: ilk bölümlerde doğru rota soluk olarak gösterilir.

### Kodlama Kutusu (P0)

**Amaç:** Karışık verilen komut kartlarını doğru sıraya koymak ve hatalı bir kodu düzeltmek.

**Mekanik:** Masa oyunu görünümü. Bölümün yarısında kod hazır verilir ama bir veya iki kart yanlıştır; çocuk hatayı bulup değiştirir.

- **KUT-01 (P0):** "Hata avcısı" bölümleri: hazır kodda yanlış kartı bulma görevi.
- **KUT-02 (P0):** Kod çalışırken Roboya'nın yolu iz bırakır; çocuk planlanan ve gerçekleşen yolu karşılaştırır.
- **KUT-03 (P1):** Kaşif seviyede "en kısa kod" meydan okumaları.

### Bal Peşinde (P0)

**Amaç:** Sanal arı robotu, sırtındaki tuşlarla programlayıp çiçekleri dolaştırmak.

**Mekanik:** Arının sırtında ileri, geri, sol, sağ, git ve temizle tuşları bulunur. Bee-Bot'taki gibi kod arının belleğinde saklanır ve görünmez; Kaşif seviyede bellek görünür hale getirilebilir.

- **BAL-01 (P0):** Renk eşleştirme, sayma (3 çiçek topla) ve basit toplama-çıkarma görevleri.
- **BAL-02 (P0):** "Temizle" unutulduğunda eski komutların da çalışması; gerçek robot deneyimini yansıtan, öğretici bir hata olarak.
- **BAL-03 (P1):** Paylaşım Köyü'nde toplanan balı köylülere eşit dağıtma bölümleri.

### Döngü Dansı (P1)

**Amaç:** Roboya'ya dans hareketleri öğreterek tekrar eden desenleri döngü kartıyla kısaltmak.

- **DON-01 (P1):** "Tekrarla" kartı içine hareket kartları yerleştirilir; tekrar sayısı noktalarla seçilir.
- **DON-02 (P1):** Dans müzik ritmine senkron oynatılır; tamamlanan koreografi kaydedilip veliye gösterilebilir (uygulama içinde).

### Birlikte Başaralım (P1)

**Amaç:** Aynı cihazda iki oyuncunun bir görevi birlikte çözmesi.

- **BIR-01 (P1):** Ekran ikiye bölünür; bir oyuncu köprü ve kapıları açar, diğeri Roboya'yı yürütür. Görev ancak iki plan uyumlu olduğunda başarılır.
- **BIR-02 (P1):** Oyuncular sırası gelince planlarını gösterir; çakışma olursa Roboya "Konuşalım mı?" der.
- **BIR-03 (P1):** Ebeveyn-çocuk modu: ebeveyn tarafı daha zor görev alır.

### Robot Atölyesi (P2)

**Amaç:** Parçalardan robot tasarlayıp onu bir görev için programlamak.

- **ROB-01 (P2):** Gövde, teker, palet, kol, ışık ve sensör parçaları sürüklenerek birleştirilir; her parça robota bir yetenek kazandırır.
- **ROB-02 (P2):** Basit 2D fizik: yanlış yerleştirilen teker robotu eğik yürütür; çocuk tasarımı düzeltir.
- **ROB-03 (P2):** Sensör parçasıyla koşul kartları açılır ("engel görürsen dön").

### Serbest Mucit Modu (P2)

**Amaç:** 7–10 yaş için kendi animasyonunu veya mini oyununu blok kodlama ile yapmak.

- **SER-01 (P2):** Karakter, arka plan ve ses kütüphanesi; hareket, görünüm, ses, olay, kontrol blokları.
- **SER-02 (P2):** Projeler cihazda ve hesapta saklanır; yalnız veli alanında görüntülenir, halka açık paylaşım yoktur.
- **SER-03 (P2):** Hazır proje şablonları ("Roboya'yı zıplat", "Bahçede kelebek").

## İlerleme, ödül ve kişiselleştirme

Ödül sistemi çocuğu öğrenmeye geri getirmek için tasarlanır, uygulamada tutmak için değil; tüm ödüller oyun oynanarak kazanılır, satın alınamaz.

**İlerleme yapısı**

1. **Ada haritası:** Ana ekran. Bölgeler sırayla açılır; açılan bölge renklenir.
2. **Bölge:** Her bölgede 3–4 oyunun bölümleri tek bir patika üzerinde dizilir.
3. **Bölüm:** 1–3 yıldız. Sonraki bölümü açmak için tek yıldız yeterlidir.
4. **Bölge tamamlama:** Gemi parçası + Değer Albümü'ne yeni hikâye kartı.

**Ödüller**

| Ödül | Nasıl kazanılır | Ne işe yarar |
| --- | --- | --- |
| Yıldız | Bölüm tamamlama kalitesi | Bölüm ve bölge ilerlemesi |
| Robot parçası (anten, renk, kanat, şapka) | Her 5 bölümde bir ve bölge sonlarında | Roboya'yı kişiselleştirme |
| Gemi parçası | Bölge tamamlama | Ana hikâyede ilerleme |
| Değer kartı | Bölge tamamlama | 30–60 sn animasyonlu değer hikâyesi; albümde tekrar izlenebilir |
| Mucit rozeti | Bir kodlama kavramında ustalık (ör. 5 döngü bölümünü 3 yıldızla bitirmek) | Veli raporunda beceri göstergesi |

**Gereksinimler**

- **ILR-01 (P0):** Çocuk profili başına ilerleme cihazda saklanır ve hesap varsa sunucuyla eşitlenir.
- **ILR-02 (P0):** Tamamlanan bölümler istendiği kadar tekrar oynanabilir; yıldız sayısı yalnız artabilir.
- **ILR-03 (P0):** Roboya garajı: kazanılan parçaları takma ve çıkarma ekranı.
- **ILR-04 (P1):** Değer Albümü: kazanılan hikâye kartlarının tekrar izlenmesi.
- **ILR-05 (P1):** "Günün görevi": günde bir kısa özel bölüm. Kaçırıldığında ceza yoktur.

**Bilinçli olarak kullanılmayan mekanikler**

- Seri (streak) kaybetme cezası, çocuğu suçlu hissettiren bildirimler.
- Sürpriz kutu, şans çarkı, oyun içi para birimi.
- Sonsuz içerik akışı, otomatik sonraki bölüme geçiş.
- Diğer çocuklarla kıyaslayan sıralama tabloları.

## Uyarlanabilir öğrenme ve yapay zekâ

Yapay zekâ çocuğun karşısına sohbet aracı olarak çıkmaz; arka planda zorluğu ayarlar, ipucunu kişiselleştirir ve yetişkinlere rapor üretir.

| Bileşen | Ne yapar | Teknik yaklaşım | Nerede çalışır | Öncelik |
| --- | --- | --- | --- | --- |
| Beceri modeli | Her çocuğun 7 kavramdaki ustalık düzeyini tahmin eder | Bayes tabanlı bilgi takibi veya Elo benzeri puanlama | Cihazda, sunucuyla eşitlenir | P0 (basit), P1 (gelişmiş) |
| Zorluk ayarlayıcı | Sonraki bölümü, seviyeyi ve destek miktarını seçer | Kural tabanlı + beceri modeli çıktısı | Cihazda | P0 |
| Akıllı ipucu | Çocuğun takıldığı adımı tespit edip yalnız o adıma işaret eder | Çözüm uzayı araması (en yakın doğru koda mesafe) | Cihazda | P0 |
| Veli raporu metni | Beceri verisini sıcak, anlaşılır haftalık özete çevirir | Şablon + LLM ile metin üretimi, yetişkine yönelik | Sunucuda | P1 |
| Öğretmen asistanı | "Sınıfım döngülerde zorlanıyor, ne yapayım?" sorusuna müfredattan öneri verir | Müfredat içeriğiyle sınırlı RAG | Sunucuda, öğretmen panelinde | P2 |
| Kişisel hikâye varyasyonları | Çocuğun takma adını ve robotunu içeren hikâye kartları (gerçek ad toplanmaz) | Önceden üretilip insan onayından geçmiş metin + TTS | İçerik olarak paketlenir | P2 |

**Gereksinimler**

- **YZ-01 (P0):** Üç ardışık başarısız denemede ipucu önerilir. İpucu üç kademelidir: Roboya'nın sesli ipucu → hatalı kartın vurgulanması → doğru kartın gösterilmesi.
- **YZ-02 (P0):** Bir kavramda üç bölüm üst üste 3 yıldızla geçilirse sonraki bölümde destek azaltılır veya bir üst zorluk önerilir.
- **YZ-03 (P0):** Bir bölümde beş denemeden sonra hala çözüm yoksa daha kolay bir alternatif bölüm sunulur; çocuk asla kilitli kalmaz. Ücretsiz bölümlerin alternatifleri de ücretsiz havuzdadır.
- **YZ-04 (P1):** Veli raporlarındaki üretilmiş metin, yalnız ölçülmüş veriye dayanır; tanı niteliğinde veya psikolojik yorum içeren ifade üretmez.
- **YZ-05 (P0):** Çocuğun ses, görüntü veya serbest metin verisi toplanmaz ve hiçbir modele gönderilmez.
- **YZ-06 (P1):** LLM kullanılan tüm özellikler yalnız yetişkin arayüzlerinde bulunur ve çıktılar loglanıp örneklemle denetlenir.

## Veli alanı

Veli alanı, bireysel aboneliğin satıldığı ve korunduğu yerdir: veli ne öğrenildiğini görmeli, süreyi kontrol etmeli ve hesabını kolayca yönetmelidir.

**Erişim:** Uygulama içinde ebeveyn kilidinin arkasında (rastgele üretilen basit bir işlem veya "şu sayıları sırayla yaz"). Ayrıca web üzerinden veli paneli (P1).

| Özellik | Açıklama | Ücretsiz | Aile Premium | Öncelik |
| --- | --- | --- | --- | --- |
| Çocuk profilleri | Takma ad, avatar, yaş seviyesi | 1 profil | 4 profile kadar | P0 |
| Günlük süre sınırı | 10 / 15 / 20 / 30 dk veya sınırsız; süre bitince Roboya şarjı biter | Var | Var | P0 |
| Oyun saatleri | Belirli saat aralıklarında kapalı (ör. 20:30 sonrası) | Yok | Var | P1 |
| İlerleme özeti | Tamamlanan bölümler ve kazanılan rozetler | Var | Var | P0 |
| Beceri raporu | 7 kodlama kavramı ve 5 değer bölgesindeki ilerleme | Yok | Var | P0 |
| Haftalık özet | Bildirim ve e-posta ile kısa özet ("Bu hafta Elif döngüleri öğrendi"; takma ad metne şablonla eklenir, LLM'e gönderilmez) | Yok | Var | P1 |
| Mucit projeleri | Serbest modda yapılan projeleri izleme | Yok | Var | P2 |
| Abonelik yönetimi | Plan, yenileme tarihi, iptal bağlantısı, hediye kodu kullanımı | Var | Var | P0 |
| Gizlilik merkezi | Toplanan veriyi görme, indirme ve silme | Var | Var | P0 |
| Okul bağlantısı | Okul lisansındaki profili aile hesabına bağlama | Var | Var | P1 |

**Gereksinimler**

- **VEL-01 (P0):** Ebeveyn kilidi, satın alma, ayarlar, dış bağlantılar ve veli alanının tamamını korur.
- **VEL-02 (P0):** Süre sınırı dolduğunda devam eden bölüm tamamlanabilir, yenisi başlatılamaz.
- **VEL-03 (P0):** Veli alanının dili sade ve teknik terimden uzaktır; her beceri bir cümleyle açıklanır.
- **VEL-04 (P1):** Bildirimler yalnız veliye gider; çocuğa yönelik "geri gel" bildirimi gönderilmez.
- **VEL-05 (P1):** Veli alanında iki soruluk memnuniyet anketi, en fazla ayda bir kez.

## Okul ve kurum modu, öğretmen paneli

Okul modu, aynı uygulamanın sınıf için yapılandırılmış halidir: öğretmen web panelinden sınıfı yönetir, çocuklar kişisel veri vermeden tabletlerden girer.

**Çalışma biçimi**

1. Okul yöneticisi lisansı etkinleştirir, öğretmenleri davet eder.
2. Öğretmen web panelinde sınıf açar; çocukları takma ad ve avatarla ekler (gerçek ad zorunlu değildir).
3. Okul tabletinde uygulama "Okul modu"nda açılır ve 6 haneli sınıf koduyla sınıfa bağlanır.
4. Çocuk kendi avatarını seçer ve üç resimden oluşan resim parolasını girer.
5. Öğretmen hangi bölge ve bölümlerin açık olacağını belirler; ilerlemeyi panelden izler.

**Öğretmen paneli özellikleri**

| Özellik | Açıklama | Öncelik |
| --- | --- | --- |
| Sınıf yönetimi | Sınıf açma, çocuk ekleme ve çıkarma, sınıf kodu yenileme | P0 (v1.0) |
| İçerik atama | Haftalık bölüm setleri açma, kilitleme, sıralama | P0 (v1.0) |
| Sınıf ilerleme görünümü | Çocuk × kavram ısı haritası, zorlanan çocukların işaretlenmesi | P0 (v1.0) |
| Hazır ders planları | 30–32 haftalık yıllık plan; her hafta için uygulamadaki bölüm seti ve 5 dakikalık sınıf sohbeti önerisi | P1 |
| Akıllı tahta modu | Ana hikâye ve bölümlerin sınıfa yansıtılması, birlikte çözme | P1 |
| Dönem raporu | Veliye gönderilebilir PDF gelişim raporu (çocuk bazlı) | P1 |
| Kazanım eşleştirmesi | Her bölümün ilgili MEB kazanımlarıyla gösterimi | P1 |
| Öğretmen eğitimi | Panel içinde kısa video eğitimler ve tamamlama sertifikası | P1 |
| Öğretmen asistanı | Müfredatla sınırlı YZ önerileri (bkz. Uyarlanabilir öğrenme) | P2 |

**Kurum paneli (kampus ve kurum lisansı)**

- **OKL-01 (P1):** Birden çok kampus veya birimi tek hesaptan yönetme, lisans koltuklarını dağıtma.
- **OKL-02 (P1):** Kampus bazında kullanım ve ilerleme raporu; sponsorlu lisanslar için etki raporu.
- **OKL-03 (P2):** Tek oturum açma (SSO) ve toplu kullanıcı aktarımı (CSV).

**Gereksinimler**

- **OKL-04 (v1.0, P0):** Okul modunda satın alma ekranı, dış bağlantı ve veli alanı kapalıdır.
- **OKL-05 (v1.0, P0):** Bir tablet birden fazla sınıf ve çocuk tarafından paylaşılabilir; oturum bitince profil otomatik kapanır.
- **OKL-06 (v1.0, P0):** Okul ağı yavaş veya kapalıyken uygulama çalışmaya devam eder; ilerleme bağlantı gelince eşitlenir.
- **OKL-07 (v1.0, P0):** Okul profili verisi lisans sahibi okulun kontrolündedir; veli bağlantısı için okulun ve velinin onayı gerekir.

## Gelir modeli ve fiyatlandırma: bireysel abonelik ve B2B

Gelir iki ana kanaldan gelir: ailelere uygulama mağazası üzerinden bireysel abonelik (B2C) ve okullara, kurumlara sözleşmeyle lisans (B2B). İki kanalı okul-ev köprüsü birbirine bağlar.

### Bireysel abonelik (B2C)

| Plan | İçerik | Fiyat yapısı | Kanal |
| --- | --- | --- | --- |
| Ücretsiz | Sabır Ormanı'nın ilk 3 bölümü (sayı yapılandırmadan gelir), 1 çocuk profili, temel ilerleme özeti, süre sınırı | 0 | Mağaza |
| Aile Premium Aylık | Tüm bölgeler ve oyunlar, 4 çocuğa kadar, beceri raporu, haftalık özet, yeni içerikler | Baz fiyat (A) | Mağaza içi satın alma |
| Aile Premium Yıllık | Aylıkla aynı; 7 günlük ücretsiz deneme | 12 × A × 0,85 ≈ 10,2 × A (aylığın 12 katından %15 düşük) | Mağaza içi satın alma |
| Hediye aboneliği | 3, 6 veya 12 aylık kod; doğum günü ve bayram hediyesi | Aylık fiyatın katı | Mağaza teklif kodları, web sitesi |
| Okul ailesi indirimi | Okul lisansı olan çocuğun ailesine Aile Premium | Liste fiyatından indirimli (ör. %30–40) | Mağaza teklif kodu |

- **GLR-01 (P0):** Abonelik ekranı ebeveyn kilidinin arkasındadır. Çocuk ücretsiz içeriğin sonuna geldiğinde Roboya "Devamı için büyüklerine sor" der; satış dili çocuğa yönelmez.
- **GLR-02 (P0):** Fiyat, deneme süresi, yenileme ve iptal koşulları abonelik ekranında açıkça yazılır; deneme bitmeden 24 saat önce veliye hatırlatma gönderilir.
- **GLR-03 (P0):** Aile Premium tek hesaba bağlıdır; satın alma anında veli hesabı (e-posta ile tek kullanımlık kod) zorunludur, ücretsiz oyun hesapsız sürer; aynı hesapla açılan tüm cihazlarda geçerlidir (mağaza aile paylaşımı desteklenir).
- **GLR-04 (P1):** Aylık planda iptal akışında yıllığa geçiş veya bir ay duraklatma seçeneği.

### Kurumsal lisans (B2B)

| Lisans | Kime | İçerik | Fiyat yapısı | Sözleşme |
| --- | --- | --- | --- | --- |
| Sınıf lisansı | Tek sınıf, tek öğretmen | 1 öğretmen hesabı, 25 çocuk profiline kadar, okul modu, öğretmen paneli, ders planları | Sınıf başına yıllık sabit ücret | Eğitim yılı (eylül–haziran) |
| Okul lisansı | Tek kampus | Sınırsız sınıf, yönetici paneli, dönem raporları, öğretmen eğitimi | Öğrenci başına yıllık, kademeli indirim | Yıllık |
| Kurum lisansı | Zincir okul, belediye, vakıf, şirket | Çok kampus, kurum paneli, etki raporu, öncelikli destek, isteğe bağlı marka görünürlüğü | Hacim bazında teklif | 1–3 yıl |
| Sponsorlu lisans | Şirket veya vakıf, köy okulları ve dezavantajlı bölgeler için | Okul lisansı + sponsor etki raporu | Sponsorun ödediği okul lisansı | Yıllık |
| Pilot | Yeni okullar | 4–8 hafta tam erişim | Ücretsiz veya sembolik | Pilot protokolü |

- **GLR-05 (v1.0, P0):** B2B lisanslar mağaza dışında, fatura ve sözleşmeyle satılır; uygulamada yalnız sınıf koduyla etkinleştirilir. Uygulama içinde mağaza dışı ödeme bağlantısı yer almaz.
- **GLR-06 (v1.0, P0):** Lisans koltuk sayısı, geçerlilik tarihi ve kampuslar yönetici panelinden görülür; süre bitmeden 30 gün önce yenileme hatırlatması gönderilir.
- **GLR-07 (P1):** Lisans süresi dolan okulda ilerleme verisi 90 gün saklanır, sonra okulun talimatına göre silinir veya aktarılır.

### Okul-ev köprüsü (B2B2C)

Okul lisansı, bireysel abonelik için en ucuz edinim kanalıdır: çocuk uygulamayı okulda tanır, veli raporu görür, evde devam etmek için aile aboneliği alır.

1. Dönem başında okul, velilere uygulamanın tanıtım mektubunu ve QR kodu iletir.
2. Veli aile hesabı açar, öğretmenin verdiği davet koduyla çocuğunun okul profilini bağlar.
3. Veli, okul profilindeki ilerlemeyi ücretsiz izler; evde oynatmak için indirimli Aile Premium teklifi görür.
4. Evde kazanılan ilerleme okul profiline de yansır; öğretmen bunu ayrı renkte görür.

- **GLR-08 (P1):** Okul lisansına opsiyonel "ev erişimi" eklentisi: okul, tüm öğrencileri için ev erişimini toplu satın alabilir.
- **GLR-09 (P1):** Okul başına aile dönüşüm oranı izlenir; yüksek dönüşüm sağlayan okullara yenilemede indirim verilebilir.

### Hak (entitlement) mantığı

Bir çocuk profili birden fazla kaynaktan erişim alabilir; uygulama her zaman en geniş hakkı uygular.

| Kaynak | Nerede geçerli | Kapsam |
| --- | --- | --- |
| Ücretsiz katman | Her yerde | Sabır Ormanı ilk 3 bölüm (yapılandırmadan) |
| Aile Premium | Aile hesabının cihazları | Tüm içerik |
| Sınıf / okul lisansı | Yalnız okul modunda | Öğretmenin açtığı içerik |
| Ev erişimi eklentisi | Bağlanmış aile hesabı | Tüm içerik, lisans süresince |

### Fiyatlandırma yaklaşımı

- Fiyatlar TL olarak, mağaza fiyat basamaklarına göre belirlenir; enflasyona karşı yılda en az bir kez gözden geçirilir.
- Baz fiyat (A), pilot döneminde veli anketi (fiyat duyarlılığı ölçümü) ve abonelik ekranında A/B testiyle belirlenir.
- Rakip uygulamaların ve yüz yüze kodlama kurslarının aylık fiyatları referans alınır; Aile Premium bir yüz yüze dersten belirgin biçimde ucuz konumlanır.
- Okul fiyatı, okulun veliden aldığı ek etkinlik ücretinin içinde kolayca eriyebilecek düzeyde tutulur.

### Birim ekonomisi

| Kalem | B2C | B2B |
| --- | --- | --- |
| Tahsilat kanalı | App Store / Google Play | Fatura, havale, kurumsal kart |
| Kanal kesintisi | Mağaza komisyonu; küçük geliştirici ve abonelik programlarında genellikle %15 (güncel oranlar kontrol edilmeli) | Yok |
| Vergi | KDV ve mağaza vergi uygulaması | KDV, fatura |
| Edinim maliyeti | Performans reklamı, içerik pazarlaması, okul-ev köprüsü | Satış ziyareti, pilot, fuarlar |
| Ana metrik | LTV / CAC, aylık kayıp oranı, yıllık plan payı | Yenileme oranı, okul başı aile dönüşümü |
| Hedef | LTV / CAC ≥ 3 | Yıllık yenileme ≥ %80 |

**Gelir karışımı varsayımı:** İlk yıl B2B ağırlıklı (güven ve referans), ikinci yıldan itibaren okul-ev köprüsü ve performans pazarlamasıyla B2C payının artırılması. Oranlar pilot sonrası belirlenecek.

## Temel kullanıcı akışları

Beş akış MVP ve v1.0'ın iskeletini oluşturur; hepsinde çocuk ekranları ile yetişkin ekranları ebeveyn kilidiyle ayrılır.

**A1. İlk kurulum (veli, B2C)**

1. Uygulama açılır; Roboya 10 saniyelik tanıtımla karşılar.
2. "Bir büyük yardımı gerekiyor" ekranı: ebeveyn kilidi.
3. Veli, KVKK aydınlatma metnini görür ve açık rızayı verir.
4. Çocuk profili: takma ad, avatar, yaş aralığı (seviyeyi belirler).
5. Günlük süre sınırı seçimi (önerilen değer yaşa göre önceden seçili).
6. Hesap oluşturma isteğe bağlıdır; hesapsız devam edilebilir, ilerleme cihazda kalır.
7. Çocuk ada haritasına geçer; ilk bölüm destekli olarak başlar.

**A2. Çocuk oturumu**

1. Profil seçimi (avatar) → ada haritası.
2. Bölgeye dokunma → bölüm patikası → bölüm.
3. Bölüm: sorunu getiren karakter → kod kurma → oynatma → sonuç → yıldız ve ödül.
4. Süre sınırı dolunca Roboya'nın şarjı biter; uygulama veda animasyonuyla kapanır.

**A3. Abonelik (B2C)**

1. Çocuk ücretsiz içeriğin sonuna gelir; Roboya "Devamı için büyüklerine sor" der.
2. Ebeveyn kilidi → abonelik ekranı: planlar, fiyat, deneme ve iptal koşulları.
3. Mağaza satın alma → sunucu doğrulaması → içerik açılır.
4. Hesap yoksa bu noktada hesap oluşturulması önerilir (cihaz değişikliğinde ilerleme kaybolmasın diye).

**A4. Okul kurulumu (B2B)**

1. Satış ekibi okulu yönetici olarak tanımlar; yönetici davet e-postası alır.
2. Yönetici öğretmenleri davet eder; öğretmen web panelinde sınıf açar ve çocukları ekler.
3. Panel her çocuk için avatar ve resim parolası kartı üretir (yazdırılabilir).
4. Okul tabletinde "Okul modu" seçilir, sınıf kodu girilir.
5. Ders sırasında çocuk avatarını seçer, resim parolasını girer, öğretmenin açtığı bölümleri oynar.

**A5. Okul-ev bağlantısı (B2B2C)**

1. Veli, okulun ilettiği QR kodla uygulamayı indirir.
2. Aile hesabı açar, öğretmenin verdiği çocuğa özel davet kodunu girer.
3. Okul profili aile hesabına bağlanır; veli ilerlemeyi görür.
4. İndirimli Aile Premium teklifi veya okulun ev erişimi eklentisi ile evde oyun açılır.

## Fonksiyonel olmayan gereksinimler

Uygulama, okullardaki eski tabletlerde ve internetsiz ortamda da sorunsuz çalışmalıdır; performans ve çevrimdışı çalışma pazarlık konusu değildir.

| Alan | Gereksinim | Hedef |
| --- | --- | --- |
| Cihaz desteği | Android 9+ (2 GB RAM tablet dahil), iOS / iPadOS son 3 ana sürüm | Desteklenen cihazların ≥ %95'i |
| Performans | Oyun ekranlarında kare hızı | 2 GB RAM cihazda ≥ 30 fps, modern cihazda 60 fps |
| Açılış süresi | Soğuk başlangıçtan ada haritasına | ≤ 5 sn (orta seviye cihaz) |
| Uygulama boyutu | İlk indirme; bölgeler sonradan indirilir | ≤ 150 MB |
| Çevrimdışı | İndirilmiş tüm bölgeler internetsiz oynanır; ilerleme bağlantı gelince eşitlenir | %100 oyun içeriği |
| Kararlılık | Çökmesiz oturum oranı | ≥ %99.5 |
| Pil ve ısı | 20 dakikalık oturumda pil tüketimi | ≤ %8 (orta seviye cihaz) |
| Erişilebilirlik | Renk körlüğüne uygun palet (renk tek başına anlam taşımaz), ses seviyesi ayrı ayarlanır, alt yazı seçeneği | WCAG 2.2 AA ilkeleri, çocuk arayüzüne uyarlanmış |
| Dokunma | Minimum dokunma hedefi | 64 dp (çocuk ekranları), 48 dp (yetişkin ekranları) |
| Yerelleştirme | Tüm metin ve sesler dosya bazında dışsal; sağdan sola dil desteğine hazır mimari | TR (MVP), EN, AR, DE (v2.0) |
| Sunucu | API erişilebilirliği | ≥ %99.5 aylık |
| Senkronizasyon | Aynı profilin iki cihazda çakışan ilerlemesi | Kayıp olmadan birleştirme (en yüksek yıldız, en ileri bölüm) |
| Güvenlik | Aktarımda TLS 1.2+, sunucuda şifreli depolama, cihazdaki veride şifreleme | Yıllık sızma testi |

## Teknik mimari ve veri modeli

Mobil uygulama oyunları ve ilerlemeyi cihazda çalıştırır; sunucu hesap, hak, eşitleme ve raporlamayı üstlenir. Böylece okulda internet kesilse de ders aksamaz.

&#91;embedded content: sistem mimarisi · 3 katman, 10 bileşen\]

Kesikli çizgi, bölge içerik paketlerinin sunucuyu yormadan doğrudan CDN'den uygulamaya indirildiğini gösterir; mağaza bildirimleri yalnız hak servisine gelir.

**Teknoloji önerisi**

| Katman | Öneri | Gerekçe |
| --- | --- | --- |
| Mobil oyun istemcisi | Unity (2D), C# | Tek kod tabanıyla iOS ve Android; 2D animasyon, ses ve sürükle-bırak için olgun araçlar; geniş geliştirici havuzu. Alternatif: Godot. |
| Yerel depolama | SQLite + şifreli dosya | Çevrimdışı ilerleme ve kuyruk |
| İçerik paketleri | Bölge bazında indirilebilir paketler (Addressables), CDN | İlk indirme boyutunu küçük tutmak, yeni bölgeyi mağaza güncellemesi olmadan yayınlamak |
| Veli ve öğretmen web paneli | React + TypeScript | Tek tasarım sistemi, hızlı geliştirme |
| API | Python FastAPI | Ekibin mevcut uzmanlığı, hızlı geliştirme |
| Veritabanı | PostgreSQL | İlişkisel model, raporlama sorguları |
| Abonelik doğrulama | StoreKit 2 ve Google Play Billing sunucu bildirimleri, sunucu tarafında hak hesaplama | Tek doğruluk kaynağı sunucudaki hak tablosu |
| Bölüm editörü | İç kullanım web aracı; bölümleri JSON olarak üretir ve doğrular | Pedagoji ekibi geliştiriciye bağlı kalmadan bölüm üretir |
| Barındırma | AB bölgesinde yönetilen bulut (ör. Frankfurt); KVKK m.9 standart sözleşmesiyle aktarım | Yönetilen veritabanı, maliyet, düşük işletim yükü, KVKK uyumu |
| Analitik | Birinci taraf olay toplama (kendi sunucumuz) | Çocuk kategorisi mağaza kuralları ve gizlilik |

**Veri modeli (ana varlıklar)**

| Varlık | Ana alanlar | Not |
| --- | --- | --- |
| Hesap (veli) | id, e-posta, dil, oluşturma tarihi, rıza kayıtları | Hesapsız kullanımda yalnız cihaz kimliği |
| Çocuk profili | id, hesap\_id veya sınıf\_id, takma ad, avatar, yaş aralığı, seviye | Gerçek ad, doğum tarihi tutulmaz |
| Okul / Kurum | id, ad, tür, kampuslar, fatura bilgisi | Çoklu kampus için üst kurum ilişkisi |
| Sınıf | id, okul\_id, öğretmen\_id, sınıf kodu, yaş seviyesi | Kod yenilenebilir |
| Öğretmen / Yönetici | id, okul\_id, rol, e-posta | Rol tabanlı yetki |
| Lisans | id, kurum\_id, tür, koltuk sayısı, başlangıç, bitiş, ev erişimi | B2B haklarının kaynağı |
| Abonelik | id, hesap\_id, mağaza, ürün, durum, yenileme tarihi | Mağaza bildirimleriyle güncellenir |
| Hak | profil veya hesap, kaynak (ücretsiz / abonelik / lisans), kapsam, geçerlilik | Hesaplanmış görünüm; uygulama bunu okur |
| Bölge / Oyun / Bölüm | id, sürüm, kavram etiketleri, değer etiketi, zorluk, MEB kazanım kodları | İçerik; editörden yayınlanır |
| Bölüm denemesi | profil\_id, bölüm\_id, süre, deneme sayısı, ipucu sayısı, yıldız, kod uzunluğu | Uyarlanabilir öğrenmenin ham verisi |
| Beceri durumu | profil\_id, kavram, ustalık puanı, son güncelleme | Beceri modeli çıktısı |
| Envanter | profil\_id, kazanılan parçalar, değer kartları, rozetler |  |
| Rıza kaydı | hesap veya okul, metin sürümü, zaman, kapsam | KVKK ispatı için |

## Gizlilik, güvenlik ve uyum

Çocuk verisi en hassas veri türüdür; ürün "en az veri" ilkesiyle tasarlanır ve uyum gereksinimleri lansmandan önce hukukçu tarafından doğrulanır.

**Veri minimizasyonu**

- Çocuğun gerçek adı, doğum tarihi, fotoğrafı, sesi, konumu ve cihaz reklam kimliği toplanmaz.
- Çocuk profili yalnız takma ad, avatar, yaş aralığı ve oyun ilerlemesinden oluşur.
- Veli hesabında yalnız e-posta ve abonelik kaydı tutulur; ödeme bilgisi mağazada kalır.

**KVKK**

- **UYM-01 (P0):** Aydınlatma metni ve veliden açık rıza, profil oluşturulmadan önce alınır ve sürümüyle kaydedilir.
- **UYM-02 (P0):** Okul modunda veri sorumlusu ve veri işleyen rolleri okul sözleşmesinde tanımlanır; okul velilerden gerekli onayı alır, şablon metin Roboya Kids tarafından sağlanır.
- **UYM-03 (P0):** Veli, gizlilik merkezinden veriyi görüntüleyebilir, indirebilir ve silme talebinde bulunabilir; silme 30 gün içinde tamamlanır.
- **UYM-04 (P0):** Kişisel veriler yurt dışında (AB bölgesi) barındırılabilir. Yurt dışı aktarım KVKK m.9'a uygun bir mekanizmayla yapılır: yeterlilik kararı yoksa Kurum'un ilan ettiği standart sözleşme imzalanır ve imzadan sonra 5 iş günü içinde Kurum'a bildirilir. Açık rızaya dayalı arızi aktarım sürekli barındırma için kullanılmaz. Aktarım (alıcı, ülke, amaç, veri kategorileri) aydınlatma metninde ve okul veri işleme sözleşmesinde belirtilir. Kişisel veri taşıyan her yeni yurt dışı servis bu süreçten geçer ve aktarım envanterine eklenir (ADR 0007).
- **UYM-05 (P0):** VERBİS kayıt yükümlülüğü ve veri envanteri hukuk danışmanıyla netleştirilir.

**Uygulama mağazası kuralları**

- **UYM-06 (P0):** App Store çocuk kategorisi ve Google Play aile programı gereksinimlerine uyum: reklam yok, üçüncü taraf analitik ve takip SDK'ları yok veya yalnız izin verilen türde.
- **UYM-07 (P0):** Satın alma, dış bağlantı ve ayarlar ebeveyn kilidi arkasındadır.
- **UYM-08 (P0):** B2B lisansların mağaza dışında satılması, kurumlara doğrudan satılan hizmetler için mağaza yönergelerinin izin verdiği çerçevede yapılır; uygulama içinde mağaza dışı ödemeye yönlendirme yapılmaz. Güncel yönergeler lansman öncesi kontrol edilir.

**Uluslararası genişleme (v2.0)**

- ABD için COPPA, AB için GDPR ve çocuklara yönelik yaşa uygun tasarım kuralları, ilgili pazara açılmadan önce değerlendirilir.

**Güvenlik**

- Rol tabanlı erişim: öğretmen yalnız kendi sınıfını, yönetici yalnız kendi kurumunu görür.
- Yönetici ve öğretmen hesaplarında iki adımlı doğrulama (P1).
- Lansman öncesi ve yılda bir bağımsız sızma testi; olay müdahale ve veri ihlali bildirim prosedürü.

## Analitik ve ölçümleme

Analitik birinci taraf olarak toplanır ve iki soruya cevap verir: çocuk öğreniyor mu, aile ve okul ödemeye devam ediyor mu?

**Temel olaylar**

| Olay | Özellikler | Kullanım |
| --- | --- | --- |
| `app_open` | profil türü (aile/okul), seviye, cihaz sınıfı | Aktif kullanıcı, elde tutma |
| `level_start` / `level_complete` | bölüm, deneme, süre, yıldız, ipucu sayısı, kod uzunluğu | Zorluk ayarı, bölüm kalitesi |
| `level_abandon` | bölüm, süre, son adım | Fazla zor bölümlerin tespiti |
| `hint_used` | bölüm, ipucu kademesi | İpucu etkinliği |
| `session_limit_reached` | ayarlanan süre | Veli kontrollerinin kullanımı |
| `paywall_view` / `trial_start` / `purchase` | plan, kaynak ekran, teklif kodu | B2C dönüşüm hunisi |
| `subscription_renew` / `subscription_cancel` | plan, süre, iptal nedeni (isteğe bağlı) | Kayıp analizi |
| `class_session` | sınıf, aktif çocuk sayısı, süre | Okul kullanımı, yenileme riski |
| `school_family_link` | okul, bağlanan aile sayısı | Okul-ev köprüsü dönüşümü |

**Panolar**

1. **Öğrenme panosu:** Kavram bazında ustalık eğrisi, bölüm başına ortalama deneme ve terk oranı.
2. **B2C panosu:** Kurulum → ilk bölüm → ücretsiz içerik sonu → abonelik ekranı → deneme → ücretli hunisi; aylık kayıp; yıllık plan payı.
3. **B2B panosu:** Okul başına haftalık aktif sınıf, lisans kullanım oranı, yenilemesine 60 günden az kalan düşük kullanımlı okullar.

**Kurallar**

- Olaylar takma adla değil, rastgele üretilmiş anonim profil kimliğiyle ilişkilendirilir; reklam kimliği kullanılmaz.
- Ham olay verisi 24 ay sonra toplulaştırılıp bireysel düzeyde silinir.

## İçerik üretimi, sanat ve seslendirme

Yazılımdan çok içerik zaman alır; bölüm tasarımı, çizim, animasyon ve seslendirme için tekrarlanabilir bir üretim hattı kurulmalıdır.

**Sanat yönü**

- Sıcak, yuvarlak hatlı, yüksek kontrastlı 2D çizim; Roboya Kids'in mevcut turuncu-sıcak marka diliyle uyumlu.
- Her bölgenin kendi renk paleti ve müzik teması vardır; renk tek başına anlam taşımaz (şekil ve simgeyle desteklenir).
- Karakterler özgün tasarlanır; tüm haklar Roboya Kids'e devredilecek şekilde sözleşme yapılır.

**Seslendirme ve ses**

- Roboya ve anlatıcı için profesyonel Türkçe seslendirme; çocuklara yönelik, neşeli ama abartısız ton.
- MVP tahmini: yaklaşık 400–600 ses satırı (yönergeler, geri bildirimler, ipuçları, hikâye kartları).
- Prototip aşamasında sentetik ses (TTS) kullanılır; lansmandan önce insan seslendirmesiyle değiştirilir.
- Her bölge için arka plan müziği ve ses efekti seti; tüm sesler lisansı temiz kaynaklardan.

**Bölüm üretim hattı**

1. Pedagoji ekibi kavram, değer ve zorluk hedefini belirler.
2. Bölüm editöründe bölüm kurulur; editör çözülebilirliği ve en kısa çözümü otomatik doğrular.
3. Seslendirme metni yazılır, kayıt listesine eklenir.
4. İç test ve çocuk testi (her yeni bölge için en az 8–10 çocukla gözlemli oturum).
5. Okul öncesi danışmanının onayı → içerik paketi olarak yayın.

**İçerik takvimi**

- Lansmandan sonra her çeyrekte bir yeni bölge veya mevsimsel bölüm paketi (ör. kış, bayram, okulun ilk haftası).
- Düzenli yeni içerik, abonelik değerinin korunması ve kayıp oranının düşürülmesi için zorunludur.

## Rekabet ve konumlandırma

Roboya Dünyası, "okul öncesinin Türkçe, değer odaklı kodlama uygulaması" olarak konumlanır; rakiplerin çoğu İngilizce, okuma gerektiren ve yalnız teknik beceriye odaklanan ürünlerdir.

Aşağıdaki tablo genel bilgiye dayanır; lansman öncesi güncel fiyat ve özelliklerle ayrıntılı rakip analizi yapılacaktır.

| Ürün türü / örnek | Güçlü yönü | Zayıf yönü (bizim açımızdan) |
| --- | --- | --- |
| ScratchJr | Ücretsiz, serbest üretim, güçlü akademik geçmiş | Hikâye ve yönlendirme yok; 3.5–5 yaş için fazla açık uçlu; veli raporu yok |
| Bee-Bot ve benzeri robotların uygulamaları | Fiziksel robotla tanıdık deneyim | Tek mekanik, sınırlı içerik, ilerleme ve rapor zayıf |
| Uluslararası kodlama oyunları (ör. Lightbot, Kodable) | Cilalı oyun tasarımı, okul paneli olanlar var | İngilizce, okuma gerektiren yönergeler, değer boyutu yok, Türkiye'ye özel fiyat ve destek yok |
| Genel eğitim uygulamaları | Geniş içerik, güçlü marka | Kodlama yüzeysel; çoğunda reklam veya yoğun bağlayıcı mekanik |
| Yüz yüze kodlama kursları | Bire bir ilgi, velide güven | Pahalı, lokasyona ve eğitmene bağlı, ölçeklenemez |

**Farklılaşma ekseni**

1. **Okuma gerektirmeyen Türkçe deneyim:** 3.5 yaşındaki çocuk tek başına oynayabilir.
2. **Değerler mekaniğe gömülü:** Rakiplerin kolayca kopyalayamayacağı hikâye evreni ve karakterler.
3. **Tek üründe aile ve okul:** Okul-ev köprüsü hem güven hem düşük maliyetli edinim sağlar.
4. **Veliye güven:** Reklamsız, ölçülü süre, şeffaf rapor; en az veriyle, şifreli ve KVKK'ya uygun saklanan bilgiler.
5. **Mühendis güvencesi:** Müfredatı sahada çocuklarla çalışan mühendis eğitmenler tasarlar.

**Konumlandırma cümlesi:** "Roboya Dünyası, 3.5–10 yaş çocukların okumayı bilmeden kodlamayı ve iyi değerleri birlikte öğrendiği, reklamsız ve ölçülü bir Türkçe oyun dünyasıdır."

## Yol haritası ve sürüm planı

MVP 3.–6. aylarda kapalı betayla tamamlanır; her sonraki faz, önceki fazın ölçüt kapısı geçildiğinde başlar.

&#91;embedded content: yol haritası · 5 faz, 4 ölçüt kapısı\]

Bir kapı geçilemezse sonraki faza geçilmez, o fazda iyileştirmeye devam edilir. Süreler ekip büyüklüğü netleşince güncellenecektir.

## Ekip ve kaynaklar

MVP küçük bir çekirdek ekiple yapılabilir; en kritik roller oyun geliştirici, illustratör-animatör ve pedagojik tasarımdır.

| Rol | Sorumluluk | MVP | v1.0 sonrası |
| --- | --- | --- | --- |
| Ürün sahibi | Önceliklendirme, yol haritası, metrikler | 1 (yarı zamanlı olabilir) | 1 |
| Pedagojik tasarım | Kavram ve değer kurgusu, bölüm tasarımı, çocuk testleri | Roboya Kids eğitmen ekibi | + 1 bölüm tasarımcısı |
| Okul öncesi danışmanı | Yaşa uygunluk onayı, MEB kazanım eşleştirmesi | Dış danışman | Dış danışman |
| Oyun geliştirici (Unity) | Oyun mekanikleri, bölüm motoru, performans | 1–2 | 2 |
| Backend ve web geliştirici | API, hak ve abonelik, paneller, bölüm editörü | 1 | 2 |
| İllustratör / animatör | Karakterler, bölgeler, arayüz, animasyon | 1 | 1–2 |
| UX / arayüz tasarımcısı | Çocuk ve yetişkin arayüzleri, kullanılabilirlik testleri | 1 (yarı zamanlı) | 1 |
| Seslendirme ve ses | Seslendirme sanatçısı, müzik ve efekt | Dış kaynak | Dış kaynak |
| Test / kalite | Cihaz matrisi, regresyon | Geliştiriciler + dış test | 1 |
| Okul satışı ve başarı | Pilot okullar, sözleşme, öğretmen eğitimi, yenileme | Kurucu ekip | 1–2 |
| Hukuk | KVKK, sözleşmeler, marka tescili | Dış danışman | Dış danışman |

**Kaynak ihtiyacı (MVP)**

- Test cihazı matrisi: en az 2 düşük seviye Android tablet, 1 orta seviye Android telefon, 1 iPad, 1 iPhone.
- Unity lisansı, tasarım araçları, bulut barındırma, Apple ve Google geliştirici hesapları.
- Marka tescili (Roboya adı ve karakterleri) ve seslendirme stüdyosu bütçesi.
- Finansman için KOSGEB, kalkınma ajansları ve TÜBİTAK girişimcilik destekleri değerlendirilebilir.

## Riskler ve önlemler

En büyük risk, çocukların uygulamaya geri dönmemesi ve içerik üretiminin abonelik değerini koruyacak hızda yapılamamasıdır.

| Risk | Etki | Olasılık | Önlem |
| --- | --- | --- | --- |
| Çocukların geri dönmemesi (düşük D7) | Yüksek | Orta | Kapalı betada çocuk gözlemi; ilk 10 dakikanın ayrıca test edilmesi; MVP çıkış kriteri olarak D7 hedefi |
| İçerik üretiminin yavaşlığı | Yüksek | Yüksek | Veri tabanlı bölüm editörü; bölüm şablonları; çeyreklik içerik takvimi |
| "Ekran" itirazı (marka vaadiyle çelişki algısı) | Orta | Orta | Varsayılan süre sınırı, reklamsız model ve şeffaf raporun pazarlamada öne çıkarılması |
| Düşük abonelik dönüşümü | Yüksek | Orta | Ücretsiz içerik miktarı ve abonelik ekranı A/B testleri; yıllık plan ve deneme süresi optimizasyonu; okul-ev köprüsü |
| Okul satış döngüsünün uzunluğu | Orta | Yüksek | Bütçe dönemine göre satış takvimi (bahar–yaz); ücretsiz pilot; referans okul vakaları |
| Öğretmenin uygulamayı sınıfta kullanmaması | Yüksek | Orta | Hazır haftalık planlar, kısa öğretmen eğitimi, düşük kullanımı erken yakalayan B2B panosu |
| Mağaza politikası değişikliği veya ret | Orta | Düşük | Lansman öncesi yönerge kontrolü; üçüncü taraf SDK kullanımının en aza indirilmesi |
| Çocuk verisi ihlali | Çok yüksek | Düşük | En az veri ilkesi, şifreleme, sızma testi, olay müdahale planı |
| Eski okul tabletlerinde performans sorunu | Orta | Orta | Düşük seviye cihaz matrisi; düşük kalite grafik modu |
| Büyük oyuncuların Türkçe içerikle pazara girmesi | Orta | Orta | Karakter ve hikâye evreni, okul ilişkileri ve değer konumlandırmasıyla savunulabilir fark |
| Enflasyon ve kur dalgalanması | Orta | Yüksek | TL fiyatların düzenli gözden geçirilmesi; yıllık plan ve kurumsal yıllık sözleşmelerle nakit öngörülebilirliği |

## Açık sorular ve kararlar

Aşağıdaki kararlar MVP geliştirmesi başlamadan önce netleşmelidir.

- [x] Nihai ürün adı: "Roboya Dünyası: Minik Mucitler" mi, kısa bir ad mı? Mağaza ve marka tescili uygunluk kontrolü. (Bu şekilde devam edelim)
- [ ] MVP kapsamında Mucit seviyesi (7–10 yaş) tamamen dışarıda mı kalacak, yoksa bir tanıtım bölümü mi olacak?
- [x] Ücretsiz katmanın sınırı: 12 bölüm mü, yoksa süreye dayalı (ör. 7 gün) bir sınır mı? (12 bölüm bile çok 3 bölüm yeterli)
- [ ] Aile Premium baz fiyatı (A) ve yıllık indirim oranı; pilot dönemi fiyat anketi ne zaman yapılacak? (yıllık olursa %15 daha az olsun)
- [x] Okul lisansı için sınıf başı mı, öğrenci başı mı fiyatlama ana model olacak?(okul lisansı da öğrenci başı)
- [ ] Okul ailesi indirim oranı ve "ev erişimi" eklentisinin fiyatı.(fiyatları daha sonra belirleriz)
- [x] Oyun motoru kararı: Unity mi, Godot mu? Ekip yetkinliği ve lisans maliyetine göre. (Unity ile yapalım)
- [x] Barındırma sağlayıcısı ve veri merkezi seçimi.(önerilerine göre ilerleyelim; 2026-10-08: Türkiye şartı kaldırıldı, AB bölgesi + KVKK standart sözleşmesi — ADR 0007)
- [x] Seslendirme sanatçısı seçimi ve Roboya'nın ses karakteri.(başlangıçta TTS kullanırız elevenlab gibi bir kaynaktan, sonrasında gerçek seslendiririz)
- [ ] Pilot okullar: hangi 3–5 okul, hangi yaş grupları, hangi tarihlerde?(belli değil)
- [ ] Okul öncesi eğitim danışmanı ve olası üniversite iş birliği (etki çalışması için). (belli değil)
- [x] KVKK hukuk danışmanı ile rıza metinleri ve okul veri işleme sözleşmesi şablonunun hazırlanması.(taslak yapay zeka ile hazırlarız)
- [ ] Mevcut yüz yüze atölyelerin uygulamaya geçişte nasıl konumlanacağı (sürdürülecek mi, tanıtım kanalı mı olacak?).(belli değil)
