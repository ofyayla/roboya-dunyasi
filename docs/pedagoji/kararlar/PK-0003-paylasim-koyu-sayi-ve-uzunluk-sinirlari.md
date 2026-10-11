# PK-0003 — Paylaşım Köyü: sayı ve program uzunluğu sınırları

- Durum: Kabul edildi (ürün sahibi); uzman onayı yayından önce bekleniyor (G2)
- Tarih: 2026-10-11
- Onaylayan: Omer Faruk Yayla, ürün sahibi. Pedagoji danışmanı önerisi olduğu gibi kabul edildi.
- İlgili: F2-13; `../paylasim-koyu-taslagi.md` soru 4 (K4); bekleyen P9 (Sabır Ormanı'nın 11–12 kartlık Kaşif finalleri), G1 (MEB eşleştirmesi)

## Bağlam

Kaşif için en çok 7 bal sorunsuz, ama döngü olmadığı için her bal ayrı bir "Bırak" kartı. Taslaktaki 12, 17, 23 ve 24. bölümler Kaşif'in 5–10 kart aralığını aşıyordu; 23. bölüm tek başına 7 "Bırak" istiyordu. Çocuk köyde hem yol planlıyor hem sayıyor; kart sayısı aralığı aşarsa iki yük toplanır.

## Karar

1. Bir bölümde toplam bal ≤ 7.
2. Bir peteğin ihtiyacı ≤ 4; köşe finallerinde ≤ 5.
3. Bölüm başına "Bırak" ≤ 6.
4. Köyün Kaşif bölümlerinde en kısa çözüm **10 kartı geçmez, finaller dahil.** `maxProgramLength` en kısa çözümden 1–2 fazladır. Doğrulayıcı ikisini de zorunlu tutar. Bu kural köye özgüdür; Sabır Ormanı'nın P9 kararından bağımsızdır.
5. Bir bölüm ya yolda ya hesapta zorlaşır: "Bırak" 5–6 ise noktalar tek hat üstündedir; engel ya da dönüş varsa "Bırak" ≤ 4.
6. Taslaktaki bölümler buna göre değişir:
   - **12:** petekler ●, ●● ve ●●●, 6 bal; bal sepette başlar, petekler yan yana. Çözücü 10'u aşarsa petekler ●● ve ●●● olur (5 Bırak).
   - **17:** 4 bal, 2 petek; engeller kalır.
   - **23:** "Topla, eşit ver, artanı sofraya": sepette 3 bal, yoldaki çiçekte 2 bal; 2 petek 2'şer alır, 1 bal sofraya (5 Bırak).
   - **24:** sepette 4 bal; 3 petek birer alır, 1 bal sofraya, sonra şenlik alanına varılır (`reach`). 4 Bırak; final bir kutlamadır, zorluk zirvesi değil.
7. **MEB eşleştirmesi (G1)** yapay zekâya bırakılmaz; `meta.mebOutcomes` alanını yayından önce ekip doldurur. Bu, E1'deki yazım ve doğrulamayı durdurmaz, yayını durdurur.

## Gerekçe

Zorluk bağlamla artmalı, uzunlukla değil. Yan yana duran özdeş "Bırak" kartlarını saymak hataya açık bir iştir; peteğin ihtiyacını 4'ün altında tutmak bu yükü azaltır. 23. bölüm Köşe 2 ve 3'ü yeni bir bağlamda birleştirir; 24'te artanın 3 petekle gelmesi "ikiye böl" kalıbını çeşitlendirir.

## Sonuçlar

- B4: doğrulayıcı kuralları; E1: bölümler bu sınırlarla yazılır, taslak tablosu çözücünün gerçek kart sayılarıyla güncellenir. 23'te 5 nokta ve 5 "Bırak" 10'a sıkışık sığar; yerleşim editörde çözücüyle denenir.
- Çocuk testinde gözlenecekler: 4 ve daha fazla "Bırak"ı arka arkaya dizerken yanlış sayma; kart şeridindeki "Bırak"ları parmakla sayma; 9–10 kartlık bölümlerde sayarken yolu kaybetme.
