# 0007 — Barındırma: staging ve üretim

- Durum: **Önerildi** (üretim sağlayıcısı yazılı tekliflerden sonra seçilecek)
- Tarih: 2026-10-08
- İlgili: F0-19; UYM-04; ADR 0001; plan "Ortamlar, altyapı ve CI/CD"

## Bağlam

- Kişisel veri (veli e-postası, çocuk takma adı ve ilerlemesi, okul kayıtları) yalnız üretimde bulunur ve Türkiye'de tutulmalıdır (UYM-04). Staging'de yalnız sentetik veri olur (ADR 0001).
- Başlangıç yükü küçük: kapalı beta 50–100 aile, ardından birkaç bin aktif profil. Ekip tek geliştirici; operasyon yükü düşük tutulmalı.
- 2026-10-08'deki araştırmada Türkiye bölgesinde **yönetilen PostgreSQL** sunan bir hiper ölçekli bulut doğrulanamadı. AWS İstanbul Local Zone'da yönetilen veritabanı yok; Google Cloud'un Türkiye bölgesi 2028–2029'da bekleniyor. Yerli sağlayıcıların (Türk Telekom Bulut, Turkcell, Bulutistan, Kuzey DC vb.) yönetilen veritabanı ve fiyat bilgisi herkese açık değil.

## Karar

1. **Staging:** Türkiye şartı yok (sentetik veri). Uygun maliyetli herhangi bir bulutta 1 sanal sunucu + Docker Compose (API, PostgreSQL 16, Redis). Ana dala birleşmede otomatik dağıtım.
2. **Üretim:** Türkiye'de veri merkezi olan yerli bir sağlayıcı. Seçim aşağıdaki tekliflerle yapılır. Teklifler gelene kadar mimari sağlayıcıdan bağımsız kalır: konteyner + standart PostgreSQL + S3 uyumlu nesne depolama.
3. **Başlangıç kurulumu:** 2 sanal sunucu (uygulama + iş kuyruğu), yönetilen PostgreSQL varsa o; yoksa ayrı sanal sunucuda PostgreSQL + günlük şifreli yedek + ikinci lokasyona kopya + aylık geri yükleme testi. Kubernetes'e yalnız yük gerektirirse geçilir.
4. **İçerik paketleri** (bölümler, sesler, çizimler; kişisel veri yok): global CDN.

## Teklif kontrol listesi (sağlayıcılardan yazılı istenecek)

| Kalem | Neden |
| --- | --- |
| Veri merkezi şehri ve ülkesi sözleşmede yazılı | KVKK; "bulut" ifadesi yetmez |
| Yönetilen PostgreSQL 16: vCPU/RAM/disk fiyatı, yüksek erişilebilirlik, otomatik yedek, saklama süresi, PITR | Tek geliştiricinin operasyon yükü |
| Sanal sunucu (2 vCPU / 4 GB) aylık TL fiyatı ve kur maddesi | Bütçe, enflasyon |
| Ağ çıkış (egress) ücreti | API trafiği |
| S3 uyumlu nesne depolama | Yedekler, öğretmen PDF raporları |
| SLA (≥ %99,5) ve destek kanalı | PRD fonksiyonel olmayan gereksinim |
| KVKK veri işleyen sözleşmesi, ISO 27001 belgesi | Uyum, okul sözleşmeleri |
| Disk şifreleme ve anahtar yönetimi | CLAUDE.md §11 |
| Yurt dışına veri aktarımı yapılıp yapılmadığı (yedek, izleme, destek) | UYM-04 |

Aday listesi (doğrulanacak): Türk Telekom Bulut, Turkcell bulut hizmetleri, Bulutistan, Kuzey DC; ayrıca Huawei Cloud ve Oracle Cloud'un Türkiye bölgesi olup olmadığı.

## Sonuçlar

- Üretim seçimi F1-14'ten (veli hesabı, ilk kişisel veri) **önce** yapılmalıdır. Kapalı beta (Ay 5) üretim ortamında çalışacak.
- Seçilen sağlayıcı bu ADR'ye eklenip durum "Kabul edildi" yapılır.
- Google Cloud Türkiye bölgesi açılınca taşıma seçeneği açık kalır (konteyner + standart PostgreSQL).
