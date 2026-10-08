# 0015 — Rıza, veriyi görme/indirme ve silme

- Durum: Kabul edildi (sunucu tarafı; uygulama içi ekranlar sonraki PR'da)
- Tarih: 2026-10-08
- İlgili: F1-13, F1-25; UYM-01, UYM-03, UYM-04; CLAUDE.md altın kural 1, §11; ADR 0007, 0011, 0012

## Bağlam

Çocuk profili oluşturulmadan önce velinin aydınlatma metnini görüp açık rıza vermesi, rızanın sürümüyle kaydedilmesi gerekir (UYM-01). Veli verisini görebilmeli, indirebilmeli ve silme talebinde bulunabilmeli; silme 30 gün içinde tamamlanmalı (UYM-03).

## Karar

1. **Aydınlatma metni dosyadır:** `content/legal/aydinlatma-metni.tr.md`; ilk satırlarındaki işaretlerden `version` ve `status` (`draft` | `final`) okunur. `GET /v1/legal/notice` herkese açıktır (veli hesaptan önce okuyabilmeli).
   - **Metin yapay zekâ taslağıdır; hukukçu incelemesi olmadan `final` yapılmaz.** `final` dışındaki her değer taslak sayılır ve **üretimde başlatma reddedilir** (staging'de çalışır).
   - Metin yurt dışı aktarımı (AB, Almanya, standart sözleşme) belirtir (UYM-04); sağlayıcı ve bildirim tarihi alanları sözleşme sonrası doldurulacaktır.
2. **Rıza:** `POST /v1/me/consents` yalnız **güncel** metin sürümü için kabul edilir (eski sürüm 409 `notice_outdated`). Kayıt `(hesap, sürüm, kabul zamanı, geri alma zamanı)` tutar. Geri alma `DELETE /v1/me/consents`.
   - **Zorunluluk:** rıza yoksa çocuk profili oluşturma, güncelleme ve ilerleme eşitleme 403 `consent_required` döner. Geri alınca çocuk verisi yazımı durur; veli hâlâ okuyabilir ve silme isteyebilir.
   - Yeni bir metin sürümü yayımlanınca eski rıza geçersiz sayılır; uygulama yeni metni gösterip yeniden rıza alır.
3. **Görme ve indirme:** `GET /v1/me/data` hesabın tuttuğu her şeyi döner (hesap, rızalar, cihazlar, çocuk profilleri ve ilerlemesi, abonelik durumu, silme talebi); `GET /v1/me/data/export` aynısını indirilebilir JSON dosyası olarak verir. Yalnız çağıranın verisidir.
4. **Silme:** `POST /v1/me/deletion-request` isteği oluşturur ve **7 günlük bekleme** koyar (yanlışlıkla dokunmayı geri alabilmek için; `ROBOYA_DELETION_COOLING_DAYS`). Bekleme boyunca `DELETE` ile iptal edilebilir. Süre dolunca zamanlanmış iş (`python -m app.maintenance`, günlük) hesabı siler.
   - Hesapla birlikte cihazlar, belirteçler, çocuk profilleri, ilerleme ve silme isteği zincirleme silinir.
   - **Mağaza abonelik kaydı** hesaptan koparılır ama silinmez: yalnız mağazanın opak işlem kimliğini ve durumunu taşır, kişi bilgisi yoktur; aynı veli sonra dönerse satın alması yeniden bağlanabilir.
   - **Rıza kayıtları** silinmez: hesap kimliği artık kimseyi belirlemeyen rastgele bir UUID'dir; rızanın alındığının kanıtı olarak (hukuki yükümlülük) saklanır. Hukukçu bunu F2-17'de teyit edecek.
   - **Silme günlüğü** (`deletion_log`) yalnız talep ve tamamlanma zamanını tutar, kimlik tutmaz; 30 gün sözünün kanıtıdır.
   - Süresi geçmiş giriş kodları aynı işte temizlenir.
5. **Analitik olaylar** anonim kimlikle tutulur ve hesapla eşlenmez (ADR 0014); bu yüzden hesap silinince silinemez, 24 ay sonra topluca düşürülür. Aydınlatma metni bunu söyler.
6. **Yedekler:** yedeklerin 30 günde silineceği altyapı kararıdır (ADR 0007); silme talebi sözü bu süreyle uyumludur.

## Sonuçlar

- Uygulama içi ekranlar (aydınlatma metnini gösterme, rıza onayı, gizlilik merkezi) veli alanında sonraki PR'da, ebeveyn kilidinin arkasında.
- Okul modunda rıza ve veri sorumlusu/işleyen rolleri farklıdır (UYM-02); bu ADR yalnız veli (aile) akışını kapsar.
- Gerçek kullanıcıyla yayına çıkmadan önce metnin hukukçu onayı ve `status: final` zorunludur.
