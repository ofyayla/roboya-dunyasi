# PK-0002 — Paylaşım Köyü: "Kaç kaldı?" adımı

- Durum: Kabul edildi (ürün sahibi); uzman onayı yayından önce bekleniyor (G2)
- Tarih: 2026-10-11
- Onaylayan: Omer Faruk Yayla, ürün sahibi. Pedagoji danışmanı önerisi olduğu gibi kabul edildi.
- İlgili: F2-13, BAL-01; ADR 0030 madde 6; `../paylasim-koyu-taslagi.md` soru 3 (K3); bekleyen P3 (Kodlama Kutusu "tahmin et")

## Bağlam

Taslak, Köşe 4'te çıkarmayı başarı sonrası bir "Sepette kaç bal kaldı?" sorusuyla ölçmeyi öneriyordu. Soru başarıdan sonra soruluyor ve sepet ekranda noktalarla görünüyor, yani çocuk kalanı zihinden çıkararak değil sayarak bulur. Sabır Ormanı'ndaki "tahmin et" adımı (P3) için de ön inceleme "çocuk testine kadar kapalı" yolunu önermişti.

## Karar

1. Çıkarmayı mekanik taşır: sepetin görünür azalması ve `carryAtEnd` hedefi. Ölçülen çıkarma becerisi bu hedeftir.
2. `options.askRemaining` başarı sonrası kısa bir yansıtma adımıdır: yıldızı etkilemez, deneme sayılmaz, motor hedefi değildir. Veli raporunda "çıkarma ölçümü" olarak gösterilmez.
3. Adım bölüm verisinde yazılır ama **çocuk testine kadar bir özellik bayrağıyla kapalı** kalır (yapılandırmadan gelir, koda gömülmez).
4. Üç seçenek: "başta vardı" (başlangıçta sepetteki bal + yolda toplanan bal), "verdiğim", "kalan". Seçenekler motordan türetilir; doğrulayıcı üçünün birbirinden farklı olmasını zorunlu tutar (ör. 4 bal, 2 verildi, 2 kaldı olan bölümde adım açılamaz).
5. Seçenekler önce noktayla gösterilir (zar düzeninde); rakam altta küçük ve ikincildir; sayıyı ses okur.
6. Yanlış seçimde Ari sepeti açar ve birlikte sayarlar; ceza yok.

## Gerekçe

Başarı sonrası soru "kaldı" sözcüğünü ve "verince azalır" fikrini somut olarak adlandırır; bu değerli ama ölçüm değildir. Zihinden çıkarma istenseydi soru çalıştırmadan önce sorulmalıydı; bu da P3'teki tartışmayı köye taşırdı. Rastgele çeldiriciler soruyu tahmine çevirirdi. Kaşif'te okuma isteğe bağlı olduğu için rakam ikincil kalır.

## Sonuçlar

- B1: `askRemaining` şema seçeneği; B4: farklı seçenek kuralı; D4: adımın görünümü; E2: soru ve cevap satırları.
- Bayrak, F4 çocuk testinden sonra açılıp açılmayacağına karar verilir. Sabır Ormanı'nda P3 karara bağlanınca bu kayıtla tutarlılığı yeniden gözden geçirilir.
- Çocuk testinde gözlenecekler: noktaları parmakla sayıyor mu, ezberden mi dokunuyor; yanlıştan sonra birlikte saymada ilgisini koruyor mu; rakamı mı okuyor, noktaları mı sayıyor; soruyu atlamak istiyor mu.
