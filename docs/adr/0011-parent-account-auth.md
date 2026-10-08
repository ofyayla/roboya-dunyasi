# 0011 — Veli hesabı: e-posta kodu, kısa JWT, dönen yenileme belirteci

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F1-14; GLR-03, UYM-01; CLAUDE.md altın kural 1 ve 4, §8, §11

## Bağlam

Satın alma anında veli hesabı zorunlu (GLR-03); ücretsiz oyun hesapsız sürer. Hesapta yalnız e-posta tutulur, ödeme bilgisi mağazada kalır. Parola yok: veliler için en az sürtünme ve unutulan parola desteği yükü yok.

## Karar

1. **Giriş:** e-posta adresine 6 haneli tek kullanımlık kod gönderilir.
   - Kod 10 dakika geçerli, en çok 5 yanlış denemeye dayanır, tek kullanımlıktır; yenisi istenince eskisi geçersiz olur.
   - Aynı e-posta için saatte en çok 5 kod istenebilir.
   - `POST /v1/auth/code` bilinen ve bilinmeyen adreslere aynı yanıtı verir; hangi adreslerin kayıtlı olduğu sorgulanamaz.
2. **Saklama:** `login_codes` tablosunda e-posta ve kod düz tutulmaz; anahtarlı özetleri (HMAC-SHA256) tutulur. Hesap tablosunda e-posta düz tutulur (gönderim için gerekli). Yenileme belirteçleri özet olarak saklanır.
3. **Oturum:** 30 dakikalık erişim JWT'si (HS256, `PyJWT`) ve 60 günlük, **her kullanımda dönen** yenileme belirteci.
   - Dönmüş bir belirtecin yeniden sunulması çalınma işareti sayılır: o cihazın tüm belirteçleri iptal edilir.
   - Çıkış idempotenttir.
4. **Cihaz kaydı:** uygulamanın ürettiği rastgele bir kimlik ve platform (ios/android). Reklam kimliği veya donanım kimliği alınmaz (altın kural 1). Hesap başına en çok 10 cihaz; bilinen cihaz sınırda da girebilir.
5. **Kapsam:** her hesap uç noktası kimlik doğrulama bağımlılığından geçer; cihaz sorguları hesaba göre filtrelenir. Başka velinin cihazı "yok" gibi 404 döner.
6. **E-posta gönderimi:** `EmailSender` arayüzü. Şimdilik tek uygulama yerel dosyaya yazan `OutboxEmailSender` (dosya izni 600). Staging ve üretimde bu uygulamayla başlamak reddedilir; gerçek sağlayıcı seçimi, sözleşmesi, Kurum bildirimi, aydınlatma metni ve ADR 0007 envanter satırı olmadan eklenmez (CLAUDE.md §11).
7. **Başlangıç denetimi:** staging ve üretimde varsayılan veya 32 karakterden kısa JWT gizli anahtarı ile başlamak reddedilir.
8. **Günlük:** e-posta, kod ve IP loglanmaz; uvicorn erişim günlüğü kapalı çalışır.

## Yeni bağımlılık

`PyJWT`: imzalı belirteç üretip doğrulamak için. Alternatif kendi HS256 kodumuzu yazmak olurdu; güvenlik açısından standart, küçük ve yaygın bir kütüphane daha doğru.

## Sonuçlar

- Rıza (UYM-01) kaydı F1-25'te eklenir; o zamana kadar `verify` hesabı rızasız oluşturur ve gerçek kullanıcıyla bu hâliyle yayına çıkılmaz.
- Silme talebi (F1-13) hesabı ve bağlı cihazları, belirteçleri `ON DELETE CASCADE` ile siler.
- Yenileme belirteci süresi ve cihaz sınırı yapılandırmadandır (`ROBOYA_*`).
