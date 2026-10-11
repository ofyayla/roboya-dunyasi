# PK-0001 — Paylaşım Köyü: bırakma, paylaşma ve artan bal

- Durum: Kabul edildi (ürün sahibi); uzman onayı yayından önce bekleniyor (G2)
- Tarih: 2026-10-11
- Onaylayan: Omer Faruk Yayla, ürün sahibi. Pedagoji danışmanı önerisi olduğu gibi kabul edildi. PRD'nin istediği eğitmen ve okul öncesi danışmanı onayı köy bölümleri yayına çıkmadan önce alınır.
- İlgili: F2-13, BAL-01, BAL-03; ADR 0030; `../paylasim-koyu-taslagi.md` soru 2, 5, 6 (K2, K5, K6)

## Bağlam

ADR 0030 bal taşıma ve bırakma motorunu önerdi ama üç noktayı pedagojiye bıraktı: Minik'te otomatik bırakmanın kuralı, sayı göstermeden eşit paylaşmanın tanımı ve artan balın nereye gideceği. Ön inceleme, "eşit ve hepsi dağıtılmış" kuralının tek başına yetmediğini gösterdi: 5 bal ve 2 petekte hem 1-1 ve sofraya 3, hem 0-0 ve sofraya 5 bu kurala uyuyor.

## Karar

1. **Minik'te otomatik bırakma (K2).** Arı bir peteğin hücresine girince petek ihtiyacı (`need`) dolana kadar sepetten alır; fazlasını almaz. Minik bölümlerinde toplam bal toplam ihtiyaca eşittir (doğrulayıcı kuralı). Böylece çocuk hangi sırayla dolaşırsa dolaşsın düzeltemeyeceği bir dağıtım hatası oluşmaz.
2. **Eşit paylaşma (K5).**
   - `share` hedefi: listedeki petekler eşit, bütün bal dağıtılmış ve sofradaki artan petek sayısından az. Yani en büyük eşit pay tek doğru dağıtımdır. Kural bölüm tasarımında da oyun sırasındaki hedef denetiminde de uygulanır.
   - Peteklerde sayı gösterilmez. Petekler yan yana durur ve yuvalar çalıştırma sırasında canlı dolar; karşılaştırma yalnız bitişe bırakılmaz.
   - 13. bölüm rehberlidir ve 2 petek × 1 balla başlar. 14. bölümün ipucu "bir ona, bir ona" diye tek tek dağıtmayı önerir.
   - 16. bölümdeki "kaç kişi paylaşırsa o kadar az düşer" Roboya'nın kısa bir gözlemi olarak kalır; çocuğa sorulmaz.
3. **Artan bal (K6).**
   - Artan ortak köy sofrasına gider. `leftoverTo` yalnız `table` türündeki noktayı gösterebilir.
   - "En küçük arıya ver" kullanılmaz (ihtiyaca göre paylaşma ayrı bir değer tartışması ve eşitlik kuralıyla çelişiyor).
   - "Kimse fazla almasın" gibi yasaklayıcı ifadeler sese girmez. Ses dili: "Artan bal sofraya, herkes birlikte yesin."
   - Sofra bölüm sonunda herkesin birlikte oturduğu yer olarak görünür, "atılmış bal" gibi değil.
4. **Ari'nin kovanı (`home`) kullanılmaz** ve şema v3'e girmez. `deliver` hedefinde sahnede sofra yoksa kalan bal sepette kalabilir (9. ve 20. bölüm). Kalanı kovana bırakmak "Kaç kaldı?" sorusunun cevabını 0 yapar ve Ari'yi fazlayı evine götürüyor gibi gösterir.
5. **Taşma ve boş sepet.** Kaşif'te "Bırak" petek ihtiyacını aşarsa ya da sepet boşken veya bırakma noktası dışında çalışırsa yürütme o kartta nazikçe durur, kart işaretlenir (OYN-03) ve taşan bal görünür. Ceza yok. Dünya her çalıştırmada başa döndüğü için (PK-0005) taşan balı geri alma mekaniği gerekmez.

## Gerekçe

Düzeltilemeyen bir hata Minik için öğrenme değil hayal kırıklığıdır; eksik yuvanın boş görünmesi birebir eşlemeyi öğretmeye yeter. Bu yaşta eşit paylaşma önce tek tek dağıtarak yapılır; canlı dolum ve görünen bellek çocuğa bu stratejiyi geri verir. Sayı göstermek hedefi "eşit"ten "şu kadar teslim et"e çevirirdi. Sofra artanı ayrı ve görünür bir yere koyar ve az önce öğretilen eşitlik kuralıyla çelişmez.

## Sonuçlar

- ADR 0030 madde 1, 4, 5 ve 8 buna göre güncellendi (B1, B2, B4).
- Taslakta 20. bölüm değişti (kalan bal sepette).
- Çocuk testinde gözlenecekler: peteğin önünden geçerken bal verilmesine tepki; eksik yuvayı fark edip geri dönmeyi önermesi; Kaşif'in 7. bölümde "Bırak" olmadan peteğe gidip beklemesi; kod yazmadan önce parmakla "bir sana, bir sana" dağıtması; bitişte eşitsizliği kendisinin söylemesi; "Artan bal nereye gitti, kim yiyecek?" sorusuna cevabı ve sofrayı kayıp gibi görüp görmemesi.
