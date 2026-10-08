# 0007 — Barındırma: staging ve üretim

- Durum: **Kabul edildi** — üretim AB bölgesinde (Frankfurt). Sağlayıcı, KVKK standart sözleşmesini imzalayacağının yazılı teyidine bağlı.
- Tarih: 2026-10-08 (aynı gün yeniden yazıldı; ilk sürüm Türkiye'de yerli sağlayıcı öneriyordu)
- İlgili: F0-19; UYM-04 (yeniden yazıldı), UYM-02; ADR 0001; [KVKK yurt dışı aktarım özeti](../kvkk/yurt-disi-aktarim.md)

## Bağlam

- Kişisel veri (veli e-postası, çocuk takma adı ve ilerlemesi, okul kayıtları) yalnız üretimde bulunur. Staging'de yalnız sentetik veri olur (ADR 0001).
- İlk sürümdeki "kişisel veri Türkiye'de" şartı kaldırıldı:
  - Türkiye'de yönetilen PostgreSQL sunan bir hiper ölçekli bulut doğrulanamadı. AWS İstanbul Local Zone'da yönetilen veritabanı yok; Google Cloud'un Türkiye bölgesi 2028–2029'da bekleniyor.
  - Yerli sağlayıcıların yönetilen veritabanı ve fiyat bilgisi herkese açık değil.
  - Ekip tek geliştirici; işletim yükü düşük olmalı.
- KVKK m.9 (7499 sayılı Kanun, 2024) yurt dışı aktarımı standart sözleşme ile mümkün kılıyor. Sürekli barındırma için doğru mekanizma budur; açık rızaya dayalı arızi aktarım değil.

## Seçenekler (Frankfurt, yönetilen PostgreSQL 16)

Fiyatlar 2026 yazı-sonbaharı ikincil kaynaklarından, yalnız örnek amaçlıdır; disk, yedek, trafik ve KDV hariç. "Doğrulanacak" alanlar sağlayıcı belgelerinden veya fiyat hesaplayıcısından kontrol edilmeden karar girdisi sayılmaz. AWS Multi-AZ SLA'sı da AWS'nin güncel SLA sayfasından teyit edilmeli.

| Sağlayıcı | Başlangıç maliyeti | Yüksek erişilebilirlik ve yedek | SLA | İşletim yükü | Not |
| --- | --- | --- | --- | --- | --- |
| **AWS RDS** (eu-central-1) | db.t4g.small ≈ 27 $/ay (1 yıl rezerv ≈ 17 $); Multi-AZ yaklaşık iki katı | Multi-AZ, otomatik yedek, PITR, KMS şifreleme | Multi-AZ %99,95 | Düşük | En olgun ekosistem; standart sözleşme teyidi gerekli |
| Google Cloud SQL (europe-west3) | Fiyat hesaplayıcıyla doğrulanacak | Bölgesel HA, PITR | Doğrulanacak | Düşük | Türkiye bölgesi açılınca taşıma kolay |
| Azure Database for PostgreSQL (Germany West Central) | Fiyat hesaplayıcıyla doğrulanacak | Bölge içi HA, PITR | Doğrulanacak | Düşük | Standart sözleşme talebi kullanıcılarca gündemde |
| DigitalOcean Managed PostgreSQL (FRA1) | ≈ 15 $/ay'dan; yedek düğüm ek ücretli | Günlük yedek, PITR, yedek düğüm | Doğrulanacak | Çok düşük | Basit; kurumsal sözleşme süreçleri sınırlı |
| Hetzner + kendi PostgreSQL'imiz (veya Ubicloud) | ≈ 12 $/ay'dan | Yedek ve HA bizde | Yok veya sınırlı | **Yüksek** | En ucuz; tek geliştirici için riskli |
| Türkiye'de yerli sağlayıcı (eski seçenek) | Yazılı teklif gerekli | Teklife bağlı | Teklife bağlı | Orta | Yurt dışı aktarım yok; okul satışında artı |

Tüm AB seçenekleri için ortak noktalar:

- **KVKK mekanizması:** standart sözleşme Modül 2 (veri sorumlusundan veri işleyene) ve 5 iş günü içinde Kurum'a bildirim.
- **Gecikme:** İstanbul–Frankfurt yaklaşık 40–50 ms. Oyun çevrimdışı öncelikli ve eşitleme arka planda çalıştığı için yeterli.

## Karar

1. **Üretim:** AB bölgesi, Frankfurt.
   - **Önerilen sağlayıcı: AWS** (RDS PostgreSQL, Multi-AZ; uygulama için ECS Fargate veya küçük EC2; S3 ve KMS). Olgun yönetilen veritabanı, PITR, şifreleme ve SLA tek geliştiricinin işletim yükünü en aza indiriyor.
   - **Önkoşul:** AWS'nin KVKK standart sözleşmesini (Modül 2, Kurul metniyle) imzalayacağının yazılı teyidi.
   - Teyit alınamazsa veya maliyet sorun olursa sıradaki seçenek **DigitalOcean FRA1**; aynı teyit onun için de aranır.
2. **Staging:** yalnız sentetik veri; en ucuz seçenek (ör. Hetzner, tek sunucu + Docker Compose). Kişisel veri olmadığı için aktarım mekanizması gerekmez.
3. **Yedekleme:** günlük otomatik yedek + PITR, 30 gün saklama, aynı sağlayıcıda ikinci bir AB bölgesine veya ayrı hesaba şifreli kopya, ayda bir geri yükleme testi. Silme talepleri (UYM-03) yedek saklama süresiyle uyumlu yürür.
4. **İçerik paketleri** (bölümler, sesler, çizimler; kişisel veri yok): global CDN.
5. **Mimari** sağlayıcıdan bağımsız kalır: konteyner + standart PostgreSQL + S3 uyumlu nesne depolama.

## Yurt dışı aktarım envanteri

Kişisel veri taşıyan her yeni servis bu tabloya eklenmeden üretimde kullanılmaz (CLAUDE.md §11).

| Servis | Kişisel veri | Ülke | Mekanizma | Bildirim tarihi | Aydınlatma metninde |
| --- | --- | --- | --- | --- | --- |
| Barındırma ve yedek (AWS önerisi) | Evet: veli, çocuk profili, okul | Almanya (AB) | Standart sözleşme Modül 2 | — | Eklenecek |
| E-posta ile tek kullanımlık kod (sağlayıcı seçilecek) | Evet: veli/öğretmen e-postası | Seçime bağlı | Standart sözleşme | — | Eklenecek |
| App Store / Google Play bildirimleri | Abonelik kimlikleri | ABD / AB | Mağaza rolü hukukçuyla değerlendirilecek | — | — |
| CDN | IP adresi loglanıyorsa evet | Global | Log kapatılır veya kısaltılır; değilse standart sözleşme | — | — |
| ElevenLabs (TTS) | **Hayır**: yalnız senaryo metni | ABD | Aktarım değil | — | — |
| GitHub (kod, CI) | **Hayır**: sentetik veri (ADR 0001) | ABD | Aktarım değil | — | — |

## Süreç

1. Sağlayıcıdan standart sözleşme teyidi alınır ve imzalanır (yetki belgeleriyle).
2. İmzadan sonra **5 iş günü içinde** Kurum'a bildirilir; tarih envantere yazılır.
3. Aydınlatma metnine (F1-25) ve okul veri işleme sözleşmesine (F2-17) aktarım cümlesi eklenir: alıcı, ülke, amaç, veri kategorileri, mekanizma.
4. Seçim F1-14'ten (ilk gerçek kişisel veri) önce tamamlanır; kapalı beta (Ay 5) bu ortamda çalışır.

## Sonuçlar

- PRD'deki "Türkiye'de barındırılan veri" pazarlama vaadi kaldırıldı. Yerine en az veri, şifreleme ve KVKK uyumu vurgulanır.
- Okullar (B2B) Türkiye'de barındırma isteyebilir. Mimari taşınabilir kaldığı için ileride Türkiye'de ikinci bir dağıtım veya Google Cloud Türkiye bölgesi seçeneği açıktır. Bu bir satış riski olarak izlenir.
- Yeni bir yurt dışı veri işleyen eklemek artık bir süreçtir: sözleşme, bildirim, aydınlatma ve envanter.
