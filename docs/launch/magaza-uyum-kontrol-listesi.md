# Mağaza çocuk kategorisi uyum kontrol listesi (F1-26, UYM-06/07)

Durum: **yapay zekâ taslağı**. Mağaza yönergeleri sık değişir; her madde başvurudan önce App Store ve Google Play'in güncel metinleriyle ve hukuk danışmanıyla doğrulanır. Aşağıdaki "kanıt" sütunu, ürünün bugün ne yaptığını gösterir.

## Ortak ilkeler

| İlke | Ürün durumu | Kanıt |
| --- | --- | --- |
| Reklam yok | Karşılandı | CLAUDE.md §11; reklam SDK'sı yok |
| Üçüncü taraf analitik/takip SDK'sı yok | Karşılandı | Birinci taraf olay kuyruğu (ADR 0014, 0022); hata izleme yalnız sunucuda (ADR 0028); satın alma için Unity IAP yerine yerel köprü (ADR 0008) |
| Reklam kimliği / konum / mikrofon / kamera izni istenmez | Karşılandı | Çocuk verisi kuralları (CLAUDE.md §1, §11); `AndroidManifest`/`Info.plist` izin listesi sürümden önce denetlenecek |
| Satın alma, ayarlar, dış bağlantılar ebeveyn kilidi arkasında | Karşılandı | `ParentGate` (ADR 0010), veli alanı |
| Çocuğa satış dili yok | Karşılandı | GLR-01; abonelik yalnız veli alanında (ADR 0024) |
| Uygulama içinde mağaza dışı ödeme yönlendirmesi yok | Karşılandı | CLAUDE.md §12 |
| Gizlilik politikası bağlantısı | **Eksik** | Aydınlatma metni taslak (hukukçu onayı bekliyor); herkese açık bir adres gerekir |
| Hesap silme (uygulama içi ve web) | Kısmen | Uygulama içi gizlilik merkezi var (ADR 0023); mağazalar çoğunlukla web üzerinden de silme yolu ister: **eksik** |

## Apple (App Privacy ve Kids Category) — doldurulacak taslak

- Hedef kitle: Kids Category, yaş bandı seçimi (5 Altı / 6–8 / 9–11) hukuk ve pedagoji ile belirlenir; uygulama 3,5–10 yaş olduğu için **"5 Altı" bandını** hedefleyen en katı kurallar varsayılır.
- Toplanan veri (App Privacy): *Contact Info → E-posta* (veli hesabı, kullanıcıya bağlı, izleme için kullanılmaz); *Identifiers → Device ID* (rastgele cihaz kimliği, uygulama ürettiği); *Usage Data → Product Interaction* (anonim olaylar, kullanıcıya bağlı değil); *Purchases* (mağaza). İzleme (tracking): **yok**.
- Satın alma: StoreKit 2; fiş doğrulaması sunucuda; ebeveyn kilidi.
- Yapılacaklar: App Store Connect'te ürünler (aylık, yıllık, 7 günlük deneme), App Review notları (ebeveyn kilidi nasıl geçilir, test hesabı).

## Google Play (Data safety ve Families) — doldurulacak taslak

- Hedef kitle ve içerik: Families Policy; "çocuklara yönelik" beyanı; yalnız izinli SDK'lar (Google'ın sertifikalı SDK listesi dışında SDK yok).
- Data safety: *Personal info → E-posta* (hesap yönetimi); *App activity → App interactions* (anonim); *Device or other IDs* (rastgele kimlik). Üçüncü taraflarla **paylaşım yok**; veri aktarımda şifreli (TLS); silme talebi mekanizması var (uygulama içi), **web silme adresi eksik**.
- Satın alma: Play Billing 8.x; sunucuda Developer API ile doğrulama ve onay.
- Yapılacaklar: Play Console'da abonelik ürünleri, kapalı test kanalı, hedef kitle bildirimi.

## Başvuru öncesi yapılacaklar (sahip: ürün/hukuk)

1. Aydınlatma metni ve gizlilik politikası hukukçu onayı; herkese açık URL.
2. Web üzerinden hesap/veri silme sayfası.
3. İzin listesi denetimi (manifest/plist) ve üçüncü taraf kütüphane denetimi (Unity paketleri, `com.unity.services.*` yok).
4. Mağaza varlıkları: ekran görüntüleri, açıklama (çocuğa değil veliye yönelik), yaş derecelendirmesi anketleri.
5. Sandbox satın alma testleri (gerçek cihaz) ve App Review test notları.
