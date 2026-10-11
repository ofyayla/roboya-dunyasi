# PK-0005 — Paylaşım Köyü: erişim, başlangıç, Minik payı ve çalıştırma durumu

- Durum: Kabul edildi (ürün sahibi); uzman onayı yayından önce bekleniyor (G2)
- Tarih: 2026-10-11
- Onaylayan: Omer Faruk Yayla, ürün sahibi. Pedagoji danışmanı önerisi olduğu gibi kabul edildi.
- İlgili: F2-13; ADR 0018, 0026, 0030; plan A2, A3; `../paylasim-koyu-taslagi.md` soru 1 (K1); bekleyen P1, P2, P5, P6

## Bağlam

Köy, Sabır Ormanı'nın 36. bölümü bitince açılıyor (A2) ve Kaşif çocuk ormanı bitirmeden geçemiyor (A3). Köydeki Minik payı (K1), Minik ve Kaşif çocuğun köyde nereden başlayacağı, köyde belleğin ve dünyanın çalıştırmalar arasında korunup korunmayacağı ve Minik'in 4. bölümdeki dönüş kartı Sabır Ormanı'nın açık kararlarına (P1, P2, P5) bağlıydı. Bu kayıt yalnız köyü bağlar; Sabır Ormanı bölümlerini değiştirmez.

## Karar

1. **Minik payı (K1).** Köşe 1 altı bölüm kalır ve yalnız birebir eşleme, 3'e kadar sayma ve 1 + 1 içerir. Çıkarma ve eşit paylaşma Minik'e inmez. Köşe 1 yalnız ormanın 1–9. bölümlerinde öğrenilen kartları kullanır (ileri, sağa dön), böylece Minik'e erken açma seçeneği açık kalır.
2. **Erişim ve başlangıç.** A2 ve A3 aynen kalır.
   - Minik profil köye 1. bölümden girer.
   - Kaşif profil önce 1. bölümü tanıtım olarak oynar, sonra 7. bölümden başlar. 2–6. bölümler ona açıktır ama "sıradaki" olarak gelmez. Atlanan köşenin gemi parçası mevcut kuralla kazanılır (geçilen köşe sayılır, `RewardRules`).
   - Köşe başları (1, 7, 13, 19) bölüm verisinde **açıkça tanıtım olarak işaretlenir**; bugünkü örtük kural (oyunun ilk bölümü ya da kart tanıtan bölüm) köy 1'i yakalamıyor. ADR 0018'deki "tanıtım bölümleri atlanmaz" kuralı bu işaretle çalışır.
3. **Çalıştırma durumu (P2'nin köy uygulaması).** Köyün tamamında (1–24) dünya her çalıştırmada başa döner: arı başlangıç yerinde, sepet ve petekler ilk hâlinde. Fark yalnız bellektedir:
   - Minik (1–6): bellek gizli ve her çalıştırmadan sonra kendiliğinden temizlenir.
   - Kaşif (7–24): bellek görünür ve Temizle'ye kadar korunur. Temizle unutulursa eski komutlar baştan yeniden çalışır (BAL-02 korunur).
   - Davranış bölüm verisinde açık bir seçenekle tanımlanır, `autoDrop`'tan örtük olarak çıkarılmaz.
   - Arı başa dönerken uçarak gider ve yuvalar görünür biçimde boşalır; kural sözle değil görüntüyle öğretilir. Kaşif bunu ilk kez köy 1'de görür.
   - Kart düzeyindeki ipucu köyde yeniden açılır (dünya korunmadığı için ADR 0026'daki gerekçe köyde geçerli değil).
4. **Destek.** Köy 1 ve 4'te `guided` ya da `ghostPath` açıktır (gizli bellekle yol gölgesinin de kapalı olduğu Engel bulgusu köyde tekrarlanmasın diye).
5. **4. bölümdeki dönüş (P5'ten bağımsız).** Yalnız "sağa dön" kullanılır. Arı yukarı bakarak başlar, petek sağda durur, en kısa çözümde tek dönüş vardır.

## Gerekçe

- Kaşif çocuk 7. bölümde sepet, petek yuvası, köy ve "Bırak" kartıyla aynı anda karşılaşırsa bir bölümde dört yenilik olur; 2–3 kartlık rehberli 1. bölüm bunu dağıtır ve "şimdi kaç bal vereceğine sen karar veriyorsun" geçişini kolaylaştırır. Köşe 1'in tamamı ise 6 yaş için tekrar ve sıkılma olurdu.
- Dünya korunursa çocuk "kime kaç bal daha" sorusunu bölümün problemi yerine yarım kalmış bir ara durumdan hesaplamak zorunda kalır; saymadaki hata ile Temizle'yi unutma hatası birbirine karışır. "Kaç kaldı?" (PK-0002) de tek bir çalıştırmanın hikâyesini gerektirir.
- Minik ormanda ilk tanıdığı dönüşü 3. bölümde öğreniyor (sağa dön); yukarı bakan arının sağı çocuğun sağıyla aynıdır.
- Dürüst not: bugünkü kuralla 3,5–4,5 yaş çocuk köye büyük olasılıkla ulaşmaz; Köşe 1 ormanı bitirmiş, çoğu 5 yaşa yakın Minik profillere hizmet eder.

## Sonuçlar

- ADR 0030 madde 7 değişti; ADR 0026'ya köy notu eklendi. B1: tanıtım işareti ve çalıştırma durumu seçeneği şemaya girer; B2: dünya sıfırlama ve Minik'te bellek temizleme; C1/C2: tanıtım işaretinin başlangıç kuralında kullanılması.
- **Açık kalan (ürün sahibi, P6 ile birlikte):** Köşe 1'in Minik profile ormanın Minik koridoru (9. bölüm) bitince erken açılması. Bu A2'yi değiştirir; Sabır Ormanı'nın P1 ve P6 kararıyla birlikte ele alınır.
- Çocuk testinde gözlenecekler: Kaşif 7. bölümde sepeti ve yuvaları sormadan kullanıyor mu; 1. bölümde sıkılma belirtisi; başarısız çalıştırmadan sonra Temizle'ye mi basıyor, eksik kartı sona mı ekliyor; arının başa dönmesine şaşırıyor mu; dönüş tuşuna basmadan önce arının yüzüne bakıyor mu.
