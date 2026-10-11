# PK-0004 — Paylaşım Köyü: kavram etiketi ve veli raporu adı

- Durum: Kabul edildi (ürün sahibi); uzman onayı yayından önce bekleniyor (G2)
- Tarih: 2026-10-11
- Onaylayan: Omer Faruk Yayla, ürün sahibi. Pedagoji danışmanı önerisi olduğu gibi kabul edildi.
- İlgili: F2-13, VEL-03; ADR 0030 madde 10; `../paylasim-koyu-taslagi.md` soru 7 (K7); plan F1 (köy planı)

## Bağlam

Veli raporu ve beceri modeli bölümlerin kodlama kavramına göre ilerleme gösterir. Köyün toplama, çıkarma ve eşit paylaşma içeriği için yeni bir kavram eklenip eklenmeyeceği açıktı. PRD'deki beceri modeli 7 kavram diyor; şemada `counting` sonradan eklendiği için zaten 8 kavram var.

## Karar

1. Yeni kavram eklenmez; köy bölümleri `counting` ile etiketlenir. Şema v3'te kavram listesi değişmez.
2. Ayrıntılı matematik kazanımları kavram listesinde değil `meta.mebOutcomes` alanında tutulur (eşleştirmeyi ekip yapar, PK-0003 madde 7).
3. Veli raporundaki görünen ad "Sayma" yerine **"Sayma ve hesap"** olur. Tek cümlelik açıklama: "Nesneleri sayar; bal toplayıp dağıtırken ekler, eksiltir, eşit böler." Metin yerelleştirme tablosundan gelir (`report.concept.counting`).

## Gerekçe

Ölçülen şey kodda saymak ve sıralamaktır, matematik değil. Yeni kavram PRD ile şema arasındaki farkı büyütürdü. "Paylaşma" sözcüğü değer kartıyla çakışırdı ve veli değeri beceri sanabilirdi; "hesap" teknik olmayan, orman ve köyü birlikte kapsayan bir sözcük.

## Sonuçlar

- F1: görünen ad ve açıklama yerelleştirme tablosunda güncellenir; çocuk verisine yeni alan eklenmez.
- Ad bütün sayma bölümleri için değişir; açıklama çocuğu değil kategoriyi anlatır.
- Okul satışında öğretmen toplama-çıkarma ilerlemesini ayrıca görmek isterse bu ayrı bir ürün kararıdır.
- Veli görüşmesinde "bu satır size ne anlatıyor" diye sorulabilir.
