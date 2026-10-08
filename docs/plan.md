# Roboya Dünyası — Geliştirme Planı

Oct 7, 2026 · @Ömer

## Özet ve kararlar

Bu plan, Roboya Dünyası'nı Faz 0'dan v2.0'a kadar yaklaşık 21 ayda (Ay 0–20) hayata geçirir. Kapalı beta 5. ayda, mağaza lansmanı 9. ayın sonunda hedeflenir.

**Varsayımlar**

- Ana geliştirme tek bir geliştirici ve Claude Code ile yapılır; çizim, animasyon ve ses dış kaynak veya yapay zekâ destekli üretilir.
- Pedagojik içerik ve çocuk testleri Roboya Kids eğitmen ekibi tarafından yürütülür.
- Sprintler 2 haftalıktır; süreler ekip büyürse kısalır, büyümezse Faz 3 ve 4 uzayabilir.
- Görev büyüklükleri: **S** ≤ 2 iş günü, **M** 3–5 gün, **L** 6–10 gün. L görevler başlamadan önce parçalanır.

**PRD açık sorularına verilen kararlar ve plana etkisi**

| Konu | Karar | Plana etkisi |
| --- | --- | --- |
| Ürün adı | Roboya Dünyası: Minik Mucitler | Mağaza adı kontrolü ve marka tescili başvurusu Faz 0'da |
| Ücretsiz katman | İlk 3 bölüm | Abonelik ekranı 4. bölümde açılır; ilk 3 bölüm tüm ilk izlenimi taşıdığı için en çok test edilen içerik olur |
| Yıllık plan | Aylık fiyatın 12 katından %15 ucuz | Mağazada aylık ve yıllık iki ürün; fiyat tutarı sonra girilir |
| Okul lisansı | Öğrenci başı yıllık | Lisansta koltuk = öğrenci profili; koltuk sayımı ve aşım uyarısı Faz 2'de |
| Okul ailesi indirimi, ev erişimi | Fiyatlar sonra | Teklif kodu ve ev erişimi hakkı Faz 3'te fiyattan bağımsız, parametrik kurulur |
| Oyun motoru | Unity | Unity 6 LTS, 2D, C#; kurallar CLAUDE.md'de |
| Barındırma | Önerimiz izlenir | Kişisel veri Türkiye'de yerel bir bulut sağlayıcıda; içerik paketleri global CDN'de (bkz. Ortamlar) |
| Seslendirme | Önce ElevenLabs benzeri TTS, sonra profesyonel ses | TTS'e yalnız senaryo metni gider; ses dosyaları tek bir ses kütüphanesinden yönetilir ki sonradan kolayca değişsin |
| KVKK metinleri | Taslaklar yapay zekâ ile hazırlanır | Taslaklar Faz 1'de; çocuk verisi içerdiği için lansman öncesi tek seferlik hukukçu kontrolü önerilir |
| Mucit seviyesi (7–10 yaş) | Cevaplanmadı | Varsayım: MVP dışında, Faz 4'te |
| Pilot okullar, okul öncesi danışmanı, yüz yüze atölyeler | Belli değil | Pilot okulların en geç Ay 4'te belirlenmesi gerekir; Riskler bölümünde izlenir |
| Hesapsız satın alma | Satın alma anında veli hesabı zorunlu | Ücretsiz oyun hesapsız; F1-16 akışı hesap oluşturmayı içerir |
| Deneme süresi | 7 gün, yalnız yıllık planda | Mağaza ürün tanımı buna göre |
| Çözücünün tek kaynağı | Kodlama motoru Unity dışında da derlenen saf .NET kodu | Doğrulayıcı ve editör aynı C# çözücüyü kullanır (ADR 0002) |
| Repo görünürlüğü | Public GitHub reposu | Sırlar yalnız GitHub Secrets ve yerel `.env`'de |

## Çalışma modeli

Her görev aynı döngüden geçer: küçük bir görev kartı, Claude Code ile plan, onay, uygulama, test ve insan incelemesi. Ayrıntılı kurallar repo kökündeki CLAUDE.md dosyasındadır.

**Görev döngüsü**

1. **Görev kartı:** Amaç, kabul kriterleri ve ilgili PRD gereksinim kimliği (ör. YON-02, GLR-01) yazılır. M boyutunu aşan görev bölünür.
2. **Plan:** Claude Code plan modunda dosya listesi ve yaklaşım önerir; geliştirici onaylar veya düzeltir.
3. **Uygulama:** Küçük, tek amaca hizmet eden commitler. Test, kodla aynı görevde yazılır.
4. **Yerel doğrulama:** Test, lint ve biçimlendirme çalıştırılır; Unity tarafında EditMode testleri geçer.
5. **PR ve inceleme:** CI yeşil olmadan birleştirme yapılmaz. Kişisel veri, ödeme, hak hesaplama ve kimlik doğrulama değişiklikleri satır satır insan tarafından incelenir.
6. **Dağıtım:** Ana dala birleşen kod otomatik olarak staging'e gider; mobil derleme iç test kanalına yüklenir.

**Sprint ritmi (2 hafta)**

- 1. gün: sprint planı, görev kartlarının kabul kriterleri.
- Her gün: ana dala en az bir birleştirme; uzun yaşayan dal yok.
- Son gün: demo derlemesi TestFlight ve Google Play iç test kanalına; kısa sprint notu.
- Her iki sprintte bir: en az 5 çocukla gözlemli oyun testi (Faz 0'dan itibaren).

**Tamamlanma tanımı**

- [ ] Kabul kriterlerinin hepsi karşılandı ve PRD kimliği PR açıklamasında anıldı.
- [ ] Birim ve gerekiyorsa entegrasyon testleri yazıldı, CI yeşil.
- [ ] Yeni metin ve sesler yerelleştirme tablosundan geliyor; kodda sabit Türkçe metin yok.
- [ ] Düşük seviye Android tablette elle denendi (oyun ekranı değişikliklerinde).
- [ ] Kişisel veri veya ödeme etkisi varsa güvenlik kontrol listesi işaretlendi.
- [ ] Gerekiyorsa belge güncellendi (ADR, API şeması, CLAUDE.md).

**Claude Code ile verimli çalışma**

- Repo kökünde genel CLAUDE.md, her uygulama klasöründe (oyun, API, web) o katmana özgü kısa bir CLAUDE.md bulunur.
- PRD ve bu plan Markdown olarak `docs/` altında tutulur; mimari kararlar kısa ADR dosyalarıyla kaydedilir.
- Sık işler için proje komutları (ör. yeni bölüm şeması, yeni API uç noktası, PR inceleme) `.claude/commands/` altında tanımlanır.
- Unity sahne ve prefab dosyaları YAML olduğu için yapay zekâyla düzenlenmesi kırılgandır. Bu yüzden arayüz metin tabanlı UI Toolkit ile, bölümler JSON verisiyle, sahne bağlantıları kodla kurulur; sahneler ince tutulur.

## Repo yapısı ve teknoloji yığını

Tüm kod tek bir monorepoda durur; bölüm formatı ve API sözleşmesi tek kaynaktan üretilir, böylece oyun, sunucu ve web paneli birbirinden kopmaz.

```
roboya-dunyasi/
├── CLAUDE.md                 # Genel geliştirme kuralları
├── docs/                     # prd.md, plan.md, adr/, api/, icerik-rehberi.md
├── .claude/commands/         # Proje komutları (yeni bölüm, yeni uç nokta, PR inceleme)
├── apps/
│   ├── game/                 # Unity projesi (CLAUDE.md içerir)
│   │   └── Assets/_Project/
│   │       ├── Scripts/
│   │       │   ├── Core/          # Açılış, servis kaydı, olay yolu
│   │       │   ├── CodingEngine/  # Saf C#: komutlar, program, yorumlayıcı, ızgara (Unity bağımsız)
│   │       │   ├── Games/         # YonAvcisi, KodlamaKutusu, BalPesinde, ...
│   │       │   ├── Progression/   # Yıldız, ödül, beceri modeli, zorluk ayarı
│   │       │   ├── Parent/        # Ebeveyn kilidi, veli alanı
│   │       │   ├── School/        # Okul modu, resim parolası
│   │       │   ├── Services/      # API istemcisi, eşitleme, satın alma, olay gönderimi
│   │       │   └── UI/            # UI Toolkit denetleyicileri
│   │       ├── UI/                # UXML ve USS dosyaları
│   │       ├── Art/  Audio/
│   │       └── Tests/             # EditMode ve PlayMode testleri
│   ├── api/                  # FastAPI (app/, migrations/, tests/)
│   ├── web/                  # React + TypeScript: veli, öğretmen, kurum panelleri
│   └── level-editor/         # İç araç: bölüm editörü ve doğrulayıcı
├── packages/
│   ├── level-schema/         # Bölüm JSON şeması (tek kaynak) → C# ve TS tipleri
│   └── api-contract/         # OpenAPI → C# ve TS istemcileri
├── content/                  # Bölüm JSON'ları, seslendirme senaryosu, yerelleştirme tabloları
├── tools/                    # TTS üretimi, içerik doğrulama, derleme betikleri
├── infra/                    # Docker Compose, dağıtım tanımları, yedekleme
└── .github/workflows/        # CI/CD
```

**Teknoloji yığını**

| Katman | Teknoloji | Not |
| --- | --- | --- |
| Oyun motoru | Unity 6 LTS, C#, 2D (URP) | Lisans koşulları başlangıçta kontrol edilir |
| Oyun arayüzü | UI Toolkit (UXML/USS) | Metin tabanlı olduğu için Claude Code ile güvenle düzenlenir |
| Kodlama motoru | Saf C# kütüphanesi (ayrı assembly) | Tüm oyunların komut ve yorumlayıcı çekirdeği; Unity olmadan birim testlenir |
| İçerik dağıtımı | Addressables + CDN | Bölge paketleri mağaza güncellemesi olmadan yayınlanır |
| Yerelleştirme | Unity Localization | Metin ve ses tabloları; TR ile başlar |
| Yerel veri | SQLite + şifreleme | Çevrimdışı ilerleme ve olay kuyruğu |
| Mağaza satın alma | Unity IAP (StoreKit ve Play Billing) | Fiş her zaman sunucuda doğrulanır |
| API | Python 3.12, FastAPI, Pydantic v2, SQLAlchemy 2, Alembic | OpenAPI şeması sözleşmenin kaynağıdır |
| Arka plan işleri | Redis + iş kuyruğu (ör. arq) | Mağaza bildirimleri, haftalık raporlar, silme talepleri |
| Veritabanı | PostgreSQL 16 | Olay tablosu zamana göre bölümlenir |
| Web panelleri | React, TypeScript, Vite, TanStack Query | Veli, öğretmen ve kurum panelleri tek uygulamada, role göre |
| Kimlik | Veli ve öğretmen için e-posta ile tek kullanımlık kod; JWT erişim + yenileme | Çocuk hesabı yok; okulda resim parolası |
| Hata izleme | Kendi sunucumuzda barındırılan Sentry uyumlu servis (ör. GlitchTip) | Çocuk kategorisinde üçüncü taraf SDK riskini önler |
| TTS | ElevenLabs benzeri API, `tools/tts` betikleri | Senaryo CSV'sinden toplu ses üretimi |
| CI/CD | GitHub Actions, GameCI (Unity derleme), Fastlane | iOS derlemesi macOS runner gerektirir |

## Ortamlar, altyapı ve CI/CD

Kişisel veri yalnız üretim ortamında ve Türkiye'de bulunur; kalan her şey sentetik veriyle çalışır ve konteyner tabanlı olduğu için sağlayıcı değiştirmek kolaydır.

**Ortamlar**

| Ortam | Amaç | Veri | Nerede |
| --- | --- | --- | --- |
| Yerel | Geliştirme | Sentetik tohum verisi | Docker Compose (API, PostgreSQL, Redis, web) |
| Staging | Entegrasyon, iç test, demo | Yalnız sentetik; gerçek çocuk verisi yasak | Uygun maliyetli herhangi bir bulut |
| Üretim | Gerçek kullanıcılar | Kişisel veri | Türkiye'de veri merkezi olan yerel bulut sağlayıcı |

**Barındırma önerisi**

- **API ve veritabanı:** Türkiye'de veri merkezi olan yerel bir bulut sağlayıcı (ör. Turkcell veya Türk Telekom bulut hizmetleri). Faz 0'da yönetilen PostgreSQL, konteyner desteği, yedekleme, SLA, KVKK sözleşmesi ve fiyat üzerinden kısa bir karşılaştırma yapılır.
- **Başlangıç kurulumu:** 2 sanal sunucu üzerinde Docker Compose + yönetilen PostgreSQL. Kubernetes'e ancak yük gerektirirse geçilir.
- **Taşınabilirlik:** Google Cloud'un Türkiye bölgesinin 2028–2029'da açılması planlanıyor ([kaynak](https://www.qnbinvest.com.tr/investodak/qnbarastirma/turkcell-google-cloud-stratejik-is-birligi)). Mimari konteyner ve standart PostgreSQL ile kurulduğu için o gün taşıma seçeneği açık kalır.
- **İçerik paketleri:** Kişisel veri içermeyen çizim, ses ve bölüm paketleri global bir CDN üzerinden dağıtılır; düşük gecikme ve düşük maliyet sağlar.
- **Dikkat:** E-posta gönderimi ve Google Play abonelik bildirimleri (Pub/Sub) gibi yurt dışı servislere giden veri en aza indirilir; e-posta sağlayıcısı seçiminde yurt dışı aktarım kuralları değerlendirilir.
- **Yedekleme:** Günlük veritabanı yedeği, 30 gün saklama, Türkiye'de ikinci bir lokasyon; ayda bir geri yükleme testi.
- **Sırlar:** API anahtarları ve mağaza kimlik bilgileri sır yöneticisinde tutulur; repoya asla girmez.

**CI/CD hatları**

| Tetik | İşler | Çıktı |
| --- | --- | --- |
| Her PR | API: ruff, mypy, pytest · Web: lint, tip kontrolü, vitest · İçerik: bölüm şeması ve çözülebilirlik doğrulaması · Unity: EditMode testleri · OpenAPI sözleşme fark kontrolü | Yeşil/kırmızı durum; kırmızıda birleştirme yok |
| Ana dala birleşme | API ve web imajları → staging · Unity Android derlemesi → Google Play iç test | Staging güncel; test cihazlarında yeni sürüm |
| Gece | iOS derlemesi (macOS runner) → TestFlight · PlayMode testleri | TestFlight derlemesi |
| Sürüm etiketi | Üretim dağıtımı (elle onay) · Fastlane ile mağaza gönderimi | Mağaza incelemesine giden sürüm |
| İçerik değişikliği | Doğrulama → Addressables paketleri → staging CDN → elle onaylı üretim CDN'e aktarım | Mağaza güncellemesi olmadan yeni bölümler |

## Zaman çizelgesi

Beş faz 21 aya yayılır; kapalı beta 5. ayda başlar, mağaza lansmanı 9. ayın sonunda yapılır.

&#91;embedded content: geliştirme takvimi · 5 faz, 5 paralel iş akışı\]

Üstteki beş satır sıralı fazlardır, alttakiler fazlar boyunca paralel yürüyen iş akışlarıdır. Bir kapı geçilemezse sonraki faz ve ona bağlı paralel işler birlikte kayar.

## Faz 0 — Temeller ve prototip (Ay 0–2, sprint 1–6)

Faz 0'ın çıktısı, çalışan bir kodlama motoru, bölüm formatı ve çocuklarla test edilmiş oynanabilir bir Yön Avcısı prototipidir. Bu fazda yazılan çekirdek, sonraki tüm oyunların temelidir.

| ID | Görev | Alan | Boyut | PRD / not |
| --- | --- | --- | --- | --- |
| F0-01 | Monorepo, kök ve katman CLAUDE.md dosyaları, proje komutları, PR şablonu | Altyapı | S |  |
| F0-02 | Unity 6 projesi; assembly tanımları (Core, CodingEngine, Games, Tests), Git LFS, metin serileştirme | Oyun | M |  |
| F0-03 | FastAPI iskeleti, Alembic, Docker Compose, sağlık ucu, yapılandırma | API | S |  |
| F0-04 | React + Vite iskeleti, tasarım belirteçleri (renk, tipografi, boşluk) | Web | S |  |
| F0-05 | CI: PR hattı (API, web, şema) ve GameCI ile Unity EditMode testleri | Altyapı | M |  |
| F0-06 | Kodlama motoru: komut modeli (ileri, geri, dön, tekrarla, eğer) ve program yapısı | Oyun | M | Saf C#, Unity bağımsız |
| F0-07 | Izgara dünyası: hücre türleri, engel, toplanabilir nesne, hedef | Oyun | M |  |
| F0-08 | Yorumlayıcı: adım adım yürütme, olaylar (adım, çarpma, hedef), hata izi | Oyun | L | OYN-03 temeli |
| F0-09 | Çözücü: çözülebilirlik ve en kısa çözüm uzunluğu (genişlik öncelikli arama) | Oyun | M | Yıldız hesabı ve editör için |
| F0-10 | Bölüm JSON şeması v1 ve C# / TypeScript tip üretimi | Ortak | M | OYN-07 |
| F0-11 | İçerik doğrulayıcı komut satırı aracı (şema + çözücü), CI'a bağlanır | Araç | S |  |
| F0-12 | Bölüm editörü v0: ızgara çizme, kart seti seçme, kaydetme, anında çözülebilirlik | Araç | L |  |
| F0-13 | Yön Avcısı ekranı: ızgara, geçici Roboya animasyonu, sürükle-bırak kart şeridi | Oyun | L | OYN-06 |
| F0-14 | Oynat → görsel yürütme, hatalı adımda durma ve kart titreşimi | Oyun | M | OYN-03 |
| F0-15 | Ses manifesti, "Dinle" butonu, TTS toplu üretim betiği | Oyun / Araç | M | OYN-01, OYN-02 |
| F0-16 | 10 prototip bölüm | İçerik | M | Pedagoji ekibi |
| F0-17 | Sanat yönü: Roboya tasarımı, Sabır Ormanı görsel dili, arayüz stili | Sanat | L | Dış kaynak |
| F0-18 | Marka tescili başvurusu, mağaza adı kontrolü, geliştirici hesapları | İş | S |  |
| F0-19 | Barındırma sağlayıcı karşılaştırması ve seçimi (ADR) | Altyapı | S |  |
| F0-20 | Unity paket kararları (IAP, Localization, Addressables) ve lisans kontrolü (ADR) | Oyun | S |  |
| F0-21 | Çocuk testi protokolü (gözlem formu, veli onayı) ve 2 test turu | Pedagoji | M |  |

**Çıkış kriteri (Kapı 1)**

- [ ] Test edilen 4 yaş çocukların en az %75'i ilk bölümü yetişkin yardımı olmadan bitiriyor.
- [ ] Kodlama motoru ve çözücü için birim test kapsamı ≥ %90.
- [ ] Yeni bir bölüm, kod yazılmadan yalnız editör ve JSON ile eklenebiliyor.
- [ ] Barındırma ve Unity paket kararları ADR olarak kayıtlı.

## Faz 1 — MVP ve kapalı beta (Ay 3–6, sprint 7–14)

Faz 1 sonunda Sabır Ormanı'nın 36 bölümü, veli alanı ve aile aboneliği çalışır; kapalı beta 5. ayda başlar. Geliştirme yükü yaklaşık 90–100 iş günüdür ve Claude Code hızlandırmasıyla 8 sprinte sığması beklenir. Sığmazsa F1-13 ve F1-20 Faz 2'ye kayar.

| ID | Görev | Alan | Boyut | PRD / not |
| --- | --- | --- | --- | --- |
| F1-01 | Kodlama Kutusu: sıralama ve "hata avcısı" bölümleri, iz bırakan yürütme | Oyun | L | KUT-01, KUT-02 |
| F1-02 | Bal Peşinde: bellekli robot, temizle tuşu, sayma ve renk görevleri | Oyun | L | BAL-01, BAL-02 |
| F1-03 | Yön Avcısı tamamlama: kart tanıtım anları, sabır mekaniği | Oyun | M | YON-01, YON-02 |
| F1-04 | Ortak oyun özellikleri: yıldız hesabı, ipucu tetikleme, Dinle, dokunma hedefleri | Oyun | M | OYN-02, 04, 05, 06 |
| F1-05 | Ada haritası, Sabır Ormanı patikası, bölüm kilidi; ilk 3 bölüm ücretsiz | Oyun | M | Karar: ücretsiz 3 bölüm |
| F1-06 | Yerel ilerleme (SQLite), yıldız ve ödüller, Roboya garajı | Oyun | M | ILR-01, 02, 03 |
| F1-07 | Beceri modeli v1, zorluk ayarlayıcı, 3 kademeli akıllı ipucu | Oyun | L | YZ-01, 02, 03 |
| F1-08 | Seviye seçimi (Minik / Kaşif) ve yaşa göre başlangıç | Oyun | S |  |
| F1-09 | Ebeveyn kilidi | Oyun | S | VEL-01, UYM-07 |
| F1-10 | Çocuk profilleri (ücretsizde 1, premiumda 4), avatar seçimi | Oyun | M |  |
| F1-11 | Günlük süre sınırı ve "şarj bitti" akışı | Oyun | M | VEL-02 |
| F1-12 | İlerleme özeti ve beceri raporu ekranı | Oyun | M |  |
| F1-13 | Gizlilik merkezi v1: veriyi görme ve silme talebi | Oyun / API | M | UYM-03 |
| F1-14 | API: veli hesabı (e-posta ile tek kullanımlık kod), JWT, cihaz kaydı | API | M |  |
| F1-15 | API: ilerleme eşitleme uç noktaları ve çakışma birleştirme | API / Oyun | L | En yüksek yıldız, en ileri bölüm |
| F1-16 | Unity IAP: aylık ve yıllık ürünler, abonelik ekranı, deneme hatırlatması | Oyun | L | GLR-01, GLR-02; yıllık = 12 × aylık − %15 |
| F1-17 | API: fiş doğrulama, App Store ve Google Play sunucu bildirimleri, hak servisi | API | L | GLR-03 |
| F1-18 | Birinci taraf olay toplama: istemci kuyruğu, API ucu, bölümlenmiş tablo | API / Oyun | M | Analitik bölümü |
| F1-19 | Kendi sunucumuzda hata izleme | Altyapı | S |  |
| F1-20 | Öğrenme ve B2C panoları (kendi sunucumuzda bir BI aracı) | Altyapı | M |  |
| F1-21 | Sabır Ormanı: 36 bölüm (3 oyun × 12), Minik ve Kaşif varyantları | İçerik | L | Pedagoji ekibi |
| F1-22 | Seslendirme senaryosu ve TTS üretimi | İçerik | M | Karar: önce TTS |
| F1-23 | Sanat: Sabır Ormanı arka planları, Roboya animasyonları, Bilge Kaplumbağa, arayüz | Sanat | L | Dış kaynak |
| F1-24 | Sabır değer kartı animasyonu | Sanat | M |  |
| F1-25 | KVKK aydınlatma ve rıza metinleri (yapay zekâ taslağı), rıza kaydı akışı | Uyum / API | M | UYM-01 |
| F1-26 | Mağaza çocuk kategorisi kontrolü, gizlilik etiketleri ve veri güvenliği formları | Uyum | S | UYM-06 |
| F1-27 | Kapalı beta: TestFlight ve Play kapalı test, 50–100 aile, geri bildirim formu, fiyat anketi | Ürün | M |  |
| F1-28 | 3–5 pilot okulla protokol | İş | M | En geç Ay 4 |

**Çıkış kriteri (Kapı 2)**

- [ ] Beta grubunda D7 elde tutma ≥ %20 ve ilk 3 bölümü tamamlama ≥ %85.
- [ ] Abonelik ekranını gören velilerin ≥ %15'i denemeyi başlatıyor (beta'da test satın alımı + fiyat anketiyle ölçülür).
- [ ] Çökmesiz oturum ≥ %99.5; düşük seviye tablette ≥ 30 fps.
- [ ] Satın alma, yenileme, iptal ve geri yükleme akışları iki mağazanın test ortamında uçtan uca doğrulandı.

## Faz 2 — v1.0 mağaza lansmanı ve okul modu (Ay 7–9, sprint 15–20)

Faz 2 iki işi birlikte bitirir: okul modu ile kurumsal lisansı satılabilir hale getirmek ve uygulamayı mağazalarda herkese açmak. Lansman 9. ayın sonunda hedeflenir.

| ID | Görev | Alan | Boyut | PRD / not |
| --- | --- | --- | --- | --- |
| F2-01 | Okul modu girişi: sınıf kodu, avatar seçimi, resim parolası | Oyun | M | OKL-05 |
| F2-02 | Paylaşılan tablet oturumu, otomatik çıkış; okul modunda satın alma ve veli alanı kapalı | Oyun | M | OKL-04 |
| F2-03 | Öğretmenin açtığı içeriğe göre bölüm kilidi | Oyun | S |  |
| F2-04 | Okul ağında çevrimdışı çalışma ve toplu eşitleme testi | Oyun / API | M | OKL-06 |
| F2-05 | Kurum, yönetici ve öğretmen rolleri; davet akışı | API / Web | M |  |
| F2-06 | Sınıf yönetimi, öğrenci profilleri, yazdırılabilir resim parolası kartları | Web | M |  |
| F2-07 | İçerik atama: haftalık bölüm setleri | Web / API | M |  |
| F2-08 | Sınıf ilerleme ısı haritası ve zorlanan çocuk işareti | Web | M |  |
| F2-09 | Lisans modeli: öğrenci başı koltuk, geçerlilik, koltuk aşımı uyarısı | API | M | GLR-05, GLR-06; karar: öğrenci başı |
| F2-10 | İç yönetim aracı: kurum ve lisans tanımlama, sözleşme kaydı | Web | M | Satış ekibi için |
| F2-11 | Lisans yenileme hatırlatma e-postaları | API | S |  |
| F2-12 | Hediye ve teklif kodu desteği (mağaza teklif kodları) | Oyun / API | S |  |
| F2-13 | Paylaşım Köyü: Bal Peşinde genişletmesi, yaklaşık 24 bölüm | İçerik / Oyun | L | BAL-03 |
| F2-14 | Profesyonel seslendirme: MVP ve Paylaşım Köyü satırları, TTS dosyalarının değişimi | İçerik | M | Karar: sonra gerçek ses |
| F2-15 | Performans: 2 GB RAM tablette 30 fps, açılış ≤ 5 sn, ilk indirme ≤ 150 MB | Oyun | L |  |
| F2-16 | Erişilebilirlik ve renk körlüğü kontrolü | Oyun / Web | S |  |
| F2-17 | KVKK metinlerinin hukukçu kontrolü ve okul veri işleme sözleşmesi şablonu | Uyum | M | UYM-02 |
| F2-18 | Bağımsız sızma testi ve bulguların kapatılması | Güvenlik | M |  |
| F2-19 | Mağaza sayfaları: ekran görüntüleri, tanıtım videosu, Türkçe açıklamalar | Pazarlama | M |  |
| F2-20 | Destek kanalı, SSS ve durum sayfası | Operasyon | S |  |
| F2-21 | Mağaza incelemesi ve aşamalı lansman | Ürün | M |  |

**Çıkış kriteri (Kapı 3)**

- [ ] Uygulama her iki mağazada yayında; aşamalı dağıtım %100'e ulaştı.
- [ ] Pilot okulların en az yarısı ücretli öğrenci başı lisansa geçti.
- [ ] Sızma testinin yüksek ve kritik bulguları kapatıldı.
- [ ] Lisanslı sınıflarda uygulama haftaların ≥ %70'inde kullanılıyor.

## Faz 3 — v1.5 büyüme (Ay 10–14, sprint 21–30)

Faz 3, aboneliğin değerini yeni bölgelerle artırır ve okul-ev köprüsünü kurarak bireysel aboneliğe ucuz bir edinim kanalı açar. İndirim ve eklenti fiyatları sonra belirleneceği için altyapı parametrik kurulur.

| ID | Görev | Alan | Boyut | PRD / not |
| --- | --- | --- | --- | --- |
| F3-01 | Döngü Dansı: tekrarla kartı, ritme senkron dans, koreografi kaydı | Oyun | L | DON-01, DON-02 |
| F3-02 | Nezaket Bahçesi bölgesi ve bölümleri | İçerik / Oyun | L |  |
| F3-03 | Birlikte Başaralım: bölünmüş ekran, uyum kontrolü, ebeveyn-çocuk modu | Oyun | L | BIR-01, 02, 03 |
| F3-04 | Yardımlaşma Limanı bölgesi ve bölümleri | İçerik / Oyun | L |  |
| F3-05 | Okul profili ile aile hesabını bağlama: davet kodu, okul ve veli onayı | API / Oyun | M | OKL-07 |
| F3-06 | Okul ailesi teklifi: parametrik indirim, mağaza teklif kodları | API / Oyun | M | Fiyat sonra |
| F3-07 | Ev erişimi eklentisi hakkı | API | M | GLR-08 |
| F3-08 | Okul başına aile dönüşüm metriği ve pano | API / Altyapı | S | GLR-09 |
| F3-09 | Veli web paneli: ilerleme, abonelik bilgisi, gizlilik merkezi | Web | M |  |
| F3-10 | Haftalık özet bildirimi ve e-postası; LLM metni yalnız ölçülmüş veriye dayanır | API | M | YZ-04, YZ-06, VEL-04 |
| F3-11 | Öğretmen: 30–32 haftalık ders planları, akıllı tahta modu, dönem PDF raporu | Web / Oyun | L |  |
| F3-12 | MEB kazanım eşleştirmesi (bölüm meta verisi + panelde gösterim) | İçerik / Web | M |  |
| F3-13 | Öğretmen eğitim videoları ve tamamlama sertifikası | İçerik / Web | M |  |
| F3-14 | Abonelik ekranı A/B test altyapısı (sunucudan yapılandırma) | API / Oyun | M |  |
| F3-15 | İptal akışı: yıllığa geçiş ve duraklatma seçeneği | Oyun | S | GLR-04 |
| F3-16 | Oyun saatleri, günün görevi, Değer Albümü | Oyun | M | ILR-04, ILR-05 |
| F3-17 | Lisansı dolan okulun verisini 90 gün sonra silen veya aktaran zamanlanmış iş | API | S | GLR-07 |
| F3-18 | İki adımlı doğrulama: yönetici ve öğretmen hesapları | API / Web | S |  |

**Çıkış kriteri (Kapı 4)**

- [ ] B2C'de LTV / CAC ≥ 3.
- [ ] Okul lisanslarında yıllık yenileme ≥ %80.
- [ ] Okul-ev köprüsünden gelen aile abonelikleri ayrı ölçülüyor ve raporlanıyor.

## Faz 4 — v2.0 genişleme (Ay 15–20, sprint 31–42)

Faz 4, 7–10 yaşı Mucit seviyesiyle ürüne katar, kurumsal satışı ölçekler ve uygulamayı yurt dışına hazırlar. Bu fazdaki en büyük iki iş, Robot Atölyesi ve Serbest Mucit Modu, başlamadan önce kendi içinde sprintlere bölünür.

| ID | Görev | Alan | Boyut | PRD / not |
| --- | --- | --- | --- | --- |
| F4-01 | Robot Atölyesi: parça birleştirme, 2D fizik, sensör ve koşul kartları | Oyun | L × 3 | ROB-01, 02, 03 |
| F4-02 | Sorumluluk Kulesi bölgesi ve bölümleri | İçerik / Oyun | L |  |
| F4-03 | Mucit seviyesi: koşul ve fonksiyon kartları, kısa okunur yönergeler | Oyun | L |  |
| F4-04 | Serbest Mucit Modu: blok kodlama, karakter ve ses kütüphanesi, proje kaydı, şablonlar | Oyun / API | L × 3 | SER-01, 02, 03 |
| F4-05 | Kurum paneli: çok kampus, koltuk dağıtımı, sponsorlu lisans etki raporu | Web / API | L | OKL-01, OKL-02 |
| F4-06 | Tek oturum açma (SSO) ve CSV ile toplu kullanıcı aktarımı | API / Web | M | OKL-03 |
| F4-07 | Öğretmen asistanı: müfredatla sınırlı RAG, çıktı kaydı ve örneklemli denetim | API / Web | L | YZ-06 |
| F4-08 | Kişiselleştirilmiş hikâye varyasyonları (önceden üretilip onaylanmış metin + ses) | İçerik | M |  |
| F4-09 | İngilizce ve Arapça yerelleştirme, sağdan sola düzen | Oyun / Web | L |  |
| F4-10 | Hedef pazarlar için COPPA ve GDPR değerlendirmesi, gereken değişiklikler | Uyum | M |  |
| F4-11 | Mucit projelerinin veli alanında görüntülenmesi | Oyun | S |  |

**Çıkış kriteri**

- [ ] Beş bölge ve yedi oyun yayında; 7–10 yaş grubunda D7 elde tutma, 3.5–7 yaş grubuyla karşılaştırılıyor.
- [ ] En az bir kurum lisansı (çok kampus) aktif.
- [ ] İlk yurt dışı pazar için yerelleştirilmiş sürüm mağazada.

## Test ve kalite stratejisi

Test yükünün ağırlığı Unity'den bağımsız kodlama motorunda ve API'de durur; ekran testleri az ama kritik akışlara odaklıdır. Çocuk testi, otomatik testlerin yerini tutmayan ayrı bir kalite kapısıdır.

| Katman | Araç | Kapsam hedefi | Ne zaman çalışır |
| --- | --- | --- | --- |
| Kodlama motoru ve çözücü | Unity Test Framework (EditMode), saf C# | ≥ %90 satır | Her PR |
| Oyun mantığı (ilerleme, haklar, zorluk) | EditMode | ≥ %70 | Her PR |
| Oyun akışları (bölüm aç, oyna, bitir; süre sınırı; okul girişi) | PlayMode | Kritik akışların tamamı | Gece |
| API | pytest, gerçek PostgreSQL ile test konteyneri | ≥ %80; hak ve ödeme modülleri ≥ %95 | Her PR |
| Web panelleri | Vitest + Testing Library; Playwright uçtan uca | Kritik akışlar | Her PR / gece |
| İçerik | Şema + çözücü doğrulayıcı; ses manifesti eksik dosya kontrolü | Tüm bölümler | Her PR |
| Sözleşme | OpenAPI fark kontrolü; üretilmiş istemcilerin derlenmesi | Tüm uç noktalar | Her PR |
| Performans | Unity Profiler, düşük seviye cihazda fps ve bellek ölçümü | 30 fps, bellek ≤ 600 MB | Her sprint sonu |
| Çocuk testi | Gözlem formu, video kaydı yok, not alınır | Yeni oyun ve bölgelerin tamamı | İki sprintte bir |

**Cihaz matrisi**

- Düşük seviye: 2 GB RAM Android tablet (okul senaryosu), Android 9.
- Orta seviye: güncel Android telefon.
- iOS: en az 3 sürüm eski bir iPad ve güncel bir iPhone.

**Kritik senaryolar (her sürüm öncesi elle)**

- [ ] İlk kurulum, rıza, profil oluşturma, ilk 3 bölüm, abonelik ekranı.
- [ ] Satın alma, geri yükleme, yenileme, iptal, deneme bitişi (iki mağaza test ortamı).
- [ ] İnternet kesikken oynama ve bağlantı gelince eşitleme.
- [ ] Süre sınırı dolarken bölüm ortasında olma.
- [ ] Okul modu: paylaşılan tablette iki çocuğun ard arda oturumu.
- [ ] Veri silme talebi ve sonrasında hesabın durumu.

## Yayın, mağaza ve operasyon süreçleri

Uygulama sürümleri ayda bir, içerik paketleri mağaza güncellemesi beklemeden yayınlanır; üretimde her değişiklik geri alınabilir olmalıdır.

**Sürüm ritmi**

| Ne | Sıklık | Nasıl |
| --- | --- | --- |
| Uygulama sürümü | Ayda bir (acil düzeltme gerektiğinde) | Sürüm dalı → elle test listesi → Fastlane → aşamalı dağıtım (%10 → %50 → %100) |
| İçerik paketi | İhtiyaç halinde, en az çeyrekte bir | Doğrulama → staging CDN'de test → üretim CDN; eski paket 30 gün saklanır |
| API ve web | Haftalık veya sürekli | Staging'de duman testi → elle onaylı üretim dağıtımı → geri alma tek komut |
| Veritabanı göçleri | API sürümüyle | Yalnız geriye uyumlu göçler; sütun silme iki adımda |

**Geriye uyumluluk**

- API sürümlenir (`/v1`); eski uygulama sürümleri en az 6 ay desteklenir.
- Uygulama açılışta sunucudan minimum sürüm bilgisini alır; zorunlu güncelleme yalnız güvenlik veya ödeme sorunlarında kullanılır.
- Özellik bayrakları sunucudan yönetilir; yeni oyunlar ve abonelik ekranı testleri bayrakla açılır.

**İzleme ve uyarılar**

| Sinyal | Eşik | Aksiyon |
| --- | --- | --- |
| API hata oranı | %1'i 5 dk aşarsa | Bildirim; son dağıtımı geri al |
| Uygulama çökme oranı | Oturumların %0.5'ini aşarsa | Aşamalı dağıtımı durdur |
| Mağaza bildirim işleme gecikmesi | 15 dk'yı aşarsa | Kuyruğu kontrol et; hakları yeniden hesapla |
| Veritabanı yedeği | Günlük yedek eksikse | Aynı gün elle yedek ve neden analizi |

**Destek ve olay yönetimi**

- Destek e-postası ve uygulama içi "bize yaz" (ebeveyn kilidi arkasında); okullar için ayrı öncelikli kanal.
- Olay seviyeleri: S1 (ödeme veya veri), S2 (oyun oynanamıyor), S3 (küçük hata). S1'de 4 saat içinde müdahale.
- Veri ihlali şüphesinde KVKK bildirim süreçleri için hazır bir prosedür ve iletişim şablonu Faz 2'de yazılır.

## Güvenlik ve uyum kontrol listesi

Her madde bir faz kapısına bağlıdır; işaretlenmeden o kapı geçilmez.

**Kapı 1 öncesi (Faz 0)**

- [ ] Repoya sır girmesini engelleyen tarama (pre-commit ve CI).
- [ ] Staging'de gerçek çocuk verisi kullanılmayacağı ADR ile kayıtlı.
- [ ] Çocuk testleri için yazılı veli onayı; video veya fotoğraf kaydı alınmıyor.

**Kapı 2 öncesi (Faz 1)**

- [ ] Çocuk profilinde yalnız takma ad, avatar, yaş aralığı ve ilerleme tutuluyor (kod incelemesiyle doğrulandı).
- [ ] Uygulamada reklam kimliği, konum, mikrofon, kamera izni istenmiyor.
- [ ] Üçüncü taraf SDK listesi çıkarıldı; her biri mağaza çocuk kategorisi kurallarına göre onaylı.
- [ ] Rıza kaydı metin sürümüyle tutuluyor; silme talebi uçtan uca çalışıyor.
- [ ] Aktarımda TLS, sunucuda ve cihazda şifreli depolama.
- [ ] Ebeveyn kilidi satın alma, ayarlar ve dış bağlantıların tamamını koruyor.

**Kapı 3 öncesi (Faz 2)**

- [ ] KVKK aydınlatma ve rıza metinleri hukukçu kontrolünden geçti; okul veri işleme sözleşmesi şablonu hazır.
- [ ] VERBİS yükümlülüğü netleşti; veri envanteri ve saklama süreleri yazılı.
- [ ] Rol tabanlı erişim testleri: öğretmen yalnız kendi sınıfını, yönetici yalnız kendi kurumunu görüyor.
- [ ] Bağımsız sızma testi yapıldı; yüksek ve kritik bulgular kapandı.
- [ ] Okul lisanslarının mağaza dışı satışı güncel mağaza yönergelerine göre kontrol edildi; uygulamada mağaza dışı ödeme bağlantısı yok.
- [ ] Veri ihlali müdahale prosedürü yazılı.

**Kapı 4 ve sonrası (Faz 3–4)**

- [ ] LLM kullanan özellikler yalnız yetişkin arayüzlerinde; çocuk verisi modele gitmiyor; çıktılar kayıtlı ve örneklemle denetleniyor.
- [ ] Okul-ev bağlantısı için okulun ve velinin iki taraflı onayı doğrulanıyor.
- [ ] Yönetici ve öğretmen hesaplarında iki adımlı doğrulama.
- [ ] Yurt dışı pazar öncesi COPPA ve GDPR değerlendirmesi tamamlandı.
- [ ] Yıllık sızma testi tekrarlandı.

## Geliştirme riskleri ve tamponlar

Planın en kırılgan noktası tek geliştiricili kapasite ve pilot okulların henüz belirlenmemiş olmasıdır; her faz sonunda bir sprintlik tampon bırakılması önerilir.

| Risk | Belirti | Önlem |
| --- | --- | --- |
| Tek geliştirici darboğazı | Sprint hedeflerinin üst üste iki kez kaçması | Faz kapsamını PRD önceliğine göre kıs (P1 işler sonraki faza); Faz 2'den itibaren ikinci geliştirici değerlendir |
| Unity dosyalarının yapay zekâyla düzenlenmesinin bozulması | Sahne ve prefab birleştirme çakışmaları, kayıp referanslar | Arayüz UI Toolkit'te, bağlantılar kodda, sahneler ince; sahne değişiklikleri elle ve ayrı commitlerde |
| Kodlama motorunun sonradan yetmemesi | Yeni oyunlar için motorda özel durumların artması | Faz 0'da Döngü Dansı ve Robot Atölyesi ihtiyaçlarını da kapsayan komut modeli tasarımı (ADR) |
| Mağaza abonelik entegrasyonunun beklenenden uzun sürmesi | Test ortamında yenileme ve iptal akışlarının tutarsızlığı | F1-16 ve F1-17'yi Faz 1'in ilk yarısına al; hak servisini erken kur |
| Sanat ve animasyon gecikmesi | Oyunların geçici görsellerle kalması | Kod geçici varlıklarla ilerler; sanat teslimleri sprint takvimine bağlanır |
| TTS sesinin çocuklara doğal gelmemesi | Çocuk testinde yönergelerin anlaşılmaması | Ses kütüphanesi soyut; profesyonel ses kaydı Faz 2'den öne çekilebilir |
| Pilot okul bulunamaması | Ay 4 itibarıyla protokol yok | Roboya Kids'in mevcut okul ilişkileri üzerinden erken görüşmeler; Kapı 3'ün okul koşulu yalnız B2B lansmanını etkiler |
| Düşük cihaz performansı | 2 GB RAM tablette fps düşüşü | Her sprint sonu performans ölçümü; düşük kalite grafik modu |
| Kapsam kayması | Yeni fikirlerin faz ortasında eklenmesi | Yeni istekler yalnız faz sınırlarında değerlendirilir; PRD kimliği olmayan iş sprint'e girmez |

**Kaynaklar**

- [Turkcell – Google Cloud stratejik iş birliği (QNB Invest)](https://www.qnbinvest.com.tr/investodak/qnbarastirma/turkcell-google-cloud-stratejik-is-birligi)
## Notlar (Faz 0 sırasında öğrenilenler)

- **Unity 6.6 ve OpenGL ES 3.1 (2026-10-08):** Unity 6.6 Android'de en az OpenGL ES 3.1 istiyor. "2 GB RAM, Android 9" hedef tabletlerin çoğu bunu karşılar, ama pilot okulların cihaz listesi F1-28'de kontrol edilmeli (ADR 0005). Mac'teki Android emülatörü ES 3.0 ile sınırlı olduğu için cihaz testleri gerçek tablette yapılır.
- **Barındırma (2026-10-08):** Türkiye'de yönetilen PostgreSQL sunan sağlayıcıların fiyatları herkese açık değil; ADR 0007'deki kontrol listesiyle yazılı teklif istenmeli. Üretim seçimi F1-14'ten önce tamamlanmalı.
