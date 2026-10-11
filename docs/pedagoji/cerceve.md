# Pedagojik çerçeve

Bu belge, bölüm tasarımı ve pedagoji incelemelerinde kullanılan ortak çerçevedir. Kaynağı PRD'dir ("Yaş seviyeleri ve pedagojik çerçeve", "Değerlerin oyuna gömülme ilkeleri", "İlerleme, ödül"); burada tek yerde ve uygulanabilir biçimde toplanır. PRD ile çelişirse PRD geçerlidir. Ekibin sonradan verdiği kararlar `kararlar/` altındadır ve bu belgeden önce gelir.

## Yaş seviyeleri

| Seviye | Yaş | Okuma | En kısa çözüm | Kavramlar | Etkileşim | Oturum |
| --- | --- | --- | --- | --- | --- | --- |
| Minik | 3.5–5 | Yok; tamamen sesli ve görsel | 2–4 kart | Yön, neden-sonuç, basit sıralama | Büyük kartları sürükle-bırak, tek dokunuş | 10–15 dk |
| Kaşif | 5–7 | İsteğe bağlı tek kelimeler | 5–10 kart | Sıralama, döngü, hata ayıklama, sayma | Kart dizme, adım adım çalıştırma | 15–20 dk |
| Mucit | 7–10 | Kısa yönergeler okunur | 10+ kart | Koşullar, fonksiyonlar, olaylar, serbest üretim | Blok kodlama, çok adımlı problemler | 20–30 dk |

Seviye yaşla başlar, sonra performansa göre ayarlanır (YZ-02, YZ-03). Çocuk asla kilitli kalmaz.

## Seviyelere göre gelişimsel dikkat noktaları

Aşağıdakiler genel gelişim ilkeleridir; her biri çocuk testinde (`docs/child-testing/protocol.md`) doğrulanmalıdır.

**Minik (3.5–5)**

- Çalışma belleği küçüktür: bir bölümde tek yenilik; plan 2–4 kartı geçmez.
- Sağ/sol ayrımı kendi bedeninde bile henüz oturmamış olabilir; başka bir varlığın (robotun) sağını düşünmek daha da zordur. Yön kartları tek tek, bol tekrarla ve robotun baktığı yön görünür biçimde tanıtılır.
- Görünmeyeni akılda tutmak zordur: görünmez bellek, görünmez hedef, yalnız seste kalan bilgi Minik için yüktür. Bilgi ekranda olmalıdır.
- Somut, anlık neden-sonuç: kart → hareket ilişkisi hemen görülmelidir.
- Hata ayıklama (başkasının kodundaki yanlışı bulmak) bu yaş için ileri bir beceridir; ancak çok kısa kodda ve tek yanlışla, iyi tanıtılmışsa denenebilir.
- Yönerge metni yoktur (OYN-01); her şey ses, görsel ve hareketle anlatılır.

**Kaşif (5–7)**

- Daha uzun planlar ve "önce şu, sonra bu" sıraları kurulabilir.
- Hata ayıklama, planlanan ile gerçekleşen yolu karşılaştırma ve tahmin etme bu seviyenin çekirdeğidir.
- Sayma ve renk/biçim eşleştirme oyunun içinde anlamlıdır.
- Destek kademeli azaltılır; ama yeni mekanik geldiğinde yeniden destek verilir.

**Mucit (7–10)**

- Soyutlama (döngü, fonksiyon), koşul ve olaylar; kısa yönergeleri okuyabilir.
- "Çocuksu" bulma riski: meydan okuma ve kendi ürününü yapma isteği öne çıkar.

## Kavram ilerleyişi

1. Yön ve hareket (ileri, dön)
2. Sıralama (adımları doğru dizme)
3. Hata ayıklama (yanlış adımı bulup düzeltme)
4. Döngü
5. Koşul
6. Fonksiyon
7. Olaylar ve serbest üretim

## Öğretim ilkeleri

- **Önce keşif, sonra kısa açıklama.** Kavram oyunda keşfedilir; açıklama en sonda ve kısadır.
- **Destekli ilerleme (iskele).** Yeni kavramın ilk bölümünde Roboya kodu çocukla birlikte kurar (`options.guided`); sonraki bölümlerde destek azalır: hayalet yol → rehber → yalnız ipucu. Destek azaltma takvimi bölüm verisinde açık olmalıdır.
- **Bir bölümde tek yenilik.** Yeni kart, yeni mekanik, yeni hedef türü veya ilk engel aynı bölümde birleşmez.
- **Yeni kart tanıtım anıyla gelir** (YON-01, `cards.introduces`).
- **Hatadan öğrenme.** Hangi adımda yanlış gidildiği görsel olarak işaretlenir (OYN-03); hata ceza değil yöndür.
- **Ekran–ses uyumu.** Seste adı geçen her şey ekranda görünür (`content/levels/README.md`).
- **Tekrar ve çeşitlilik dengesi.** Aynı beceri farklı bağlamlarda tekrar edilir; aynı bölümün kopyası değil.

## Değerler

1. Değer mekaniktir, ders değildir (Sabır Ormanı'nda plan bütün olarak çalışır).
2. Her bölümün başında bir ihtiyaç vardır; çocuk bir karaktere yardım etmek için kod yazar.
3. Hata cezalandırılmaz; Roboya güler, "bir daha deneyelim" der.
4. Değer anı kısadır (30–60 sn), vaaz tonu yoktur.
5. Değerler evrenseldir; her aile profiline hitap eden dille işlenir.

## Çocuk deneyimi sınırları (CLAUDE.md §13)

Can, süre baskısı ve yıldız kaybı yoktur. Seri cezası, sürpriz kutu, şans çarkı, oyun içi para, sonsuz otomatik oynatma ve çocukları kıyaslayan sıralama yoktur. Çocuğa "geri gel" bildirimi gönderilmez. Satış dili çocuğa yönelmez.
