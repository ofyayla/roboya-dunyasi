# CLAUDE.md — Roboya Dünyası Geliştirme Kuralları

Bu dosya, Roboya Dünyası: Minik Mucitler projesinde Claude Code'un ve geliştiricilerin uyması gereken temel kuralları içerir. Her oturumda önce bu dosyayı, sonra üzerinde çalışılan katmanın kendi `CLAUDE.md` dosyasını oku.

## 1. Proje özeti

- **Ürün:** 3.5–10 yaş çocuklara hikâye içinde kodlama ve değerler öğreten, Türkçe seslendirmeli mobil oyun (iOS + Android).
- **Kullanıcılar:** Çocuk (oynar), veli (bireysel abonelik), öğretmen ve okul (öğrenci başı lisans).
- **Bileşenler:** Unity oyun istemcisi, FastAPI sunucusu, React veli/öğretmen/kurum paneli, iç kullanım bölüm editörü.
- **Belgeler:** `docs/prd.md` (ürün gereksinimleri), `docs/plan.md` (geliştirme planı), `docs/adr/` (mimari kararlar).

Her görev bir PRD gereksinim kimliğine (ör. `YON-02`, `GLR-01`, `OKL-05`) veya plan görev kimliğine (ör. `F1-16`) bağlıdır. Kimliği olmayan iş başlatılmaz.

## 2. Altın kurallar

Bu kurallar her şeyden önce gelir. Çelişki varsa bunlar geçerlidir.

1. **Çocuk verisi en aza indirilir.** Çocuk profilinde yalnız takma ad, avatar, yaş aralığı ve oyun ilerlemesi tutulur. Gerçek ad, doğum tarihi, fotoğraf, ses, konum, reklam kimliği asla toplanmaz. Çocuk profiline yeni bir alan eklemek ADR ve insan onayı gerektirir.
2. **Üçüncü taraf SDK eklenmez.** Uygulama mağazaların çocuk kategorisindedir. Analitik, reklam, takip veya hata izleme SDK'sı önermeden ve onay almadan ekleme.
3. **Haklar (entitlement) yalnız sunucuda hesaplanır.** İstemci erişim kararını kendisi vermez; sunucunun verdiği hakkı önbelleğe alır.
4. **Sırlar repoya girmez.** API anahtarı, mağaza kimlik bilgisi, `.env` dosyası commit edilmez. Yalnız `.env.example` tutulur.
5. **Staging ve yerel ortamda gerçek kullanıcı verisi kullanılmaz.** Yalnız sentetik tohum verisi.
6. **Unity sahne ve prefab dosyaları elle düzenlenmez.** `.unity`, `.prefab`, `.asset` YAML dosyalarına yalnız açıkça istenirse dokun. Arayüz UI Toolkit ile, bölümler JSON ile, bağlantılar kodla kurulur.
7. **Kullanıcıya görünen her metin ve ses yerelleştirme tablosundan gelir.** Kodda sabit Türkçe metin yazılmaz.
8. **Test kodla birlikte yazılır.** Geçmeyen bir testi geçirmek için testi değiştirme; değiştirmen gerekiyorsa nedenini açıkça yaz.
9. **Belirsizlikte sor.** Gizlilik, ödeme, hak hesaplama ve çocuk deneyimiyle ilgili belirsizlikte varsayım yapma.
10. **Yıkıcı işlem yok.** Veritabanı silme, `git push --force`, geçmişi yeniden yazma, toplu dosya silme gibi işlemleri açık onay olmadan yapma.

## 3. Repo haritası

```
apps/game/               Unity 6 projesi (oyun istemcisi)
apps/api/                FastAPI sunucusu
apps/web/                React + TypeScript paneller (veli, öğretmen, kurum, iç yönetim)
apps/level-editor/       Bölüm editörü ve doğrulayıcı (iç araç)
packages/level-schema/   Bölüm JSON şeması — tek doğruluk kaynağı
packages/api-contract/   OpenAPI şeması ve üretilmiş istemciler
content/                 Bölüm JSON'ları, seslendirme senaryosu (CSV), yerelleştirme tabloları
tools/                   TTS üretimi, içerik doğrulama, derleme betikleri
infra/                   Docker Compose, dağıtım tanımları, yedekleme
docs/                    PRD, plan, ADR'ler, API notları
```

Her `apps/*` klasöründe o katmana özgü kısa bir `CLAUDE.md` bulunur. Katman kuralları bu dosyayla çelişemez, yalnız ayrıntı ekler.

## 4. Komutlar

Komutlar kök dizindeki `Makefile` üzerinden çalışır. Bir komut değişirse bu bölümü de güncelle.

| Komut | Ne yapar |
| --- | --- |
| `make setup` | Bağımlılıkları kurar, Docker servislerini başlatır, tohum verisini yükler |
| `make api-dev` | API'yi yerelde yeniden yüklemeli çalıştırır |
| `make web-dev` | Web panelini yerelde çalıştırır |
| `make editor-dev` | Bölüm editörünü yerelde çalıştırır |
| `make test` | Motor, API, web ve içerik testlerinin tamamını çalıştırır |
| `make test-engine` | Kodlama motoru testleri (.NET) ve ≥ %90 kapsam eşiği |
| `make lint` | ruff, mypy, eslint, tip kontrolü |
| `make gen` | Bölüm şemasından ve OpenAPI'den C# / TypeScript kodu üretir |
| `make validate-content` | Tüm bölümleri şema ve çözücüyle doğrular, ses manifestini kontrol eder |
| `make unity-test` | Unity EditMode testlerini batch modda çalıştırır |
| `make unity-playmode` | Unity PlayMode kritik akış testleri; ekran görüntüleri `apps/game/TestResults/screens` |

Bir görevi bitirdiğini söylemeden önce ilgili testleri ve lint'i çalıştır ve neyi çalıştırdığını raporla.

## 5. Claude Code çalışma biçimi

1. **Bağlamı oku:** İlgili `CLAUDE.md`, PRD bölümü, plan görevi ve dokunulacak kod.
2. **Plan yap:** Birden fazla dosyaya dokunan, ya da gizlilik, ödeme, kimlik doğrulama, hak hesaplama ile ilgili her işte önce plan modunda dosya listesi ve yaklaşım sun; onay bekle.
3. **Küçük adımlarla uygula:** Bir PR tek bir amaca hizmet eder. İlgisiz yeniden düzenleme yapma.
4. **Doğrula:** Test, lint ve gerekiyorsa `make gen`, `make validate-content` çalıştır.
5. **Raporla:** Ne değişti, hangi testler çalıştı, bilinen eksikler neler, hangi PRD kimliği karşılandı.

Ek kurallar:

- Yeni bağımlılık eklemeden önce neden gerektiğini ve alternatifini yaz; mevcut bağımlılıklarla çözülebiliyorsa ekleme.
- Mimari bir karar verdiysen `docs/adr/NNNN-kisa-baslik.md` olarak kısa bir ADR yaz (bağlam, karar, sonuçlar).
- Davranış değiştiyse ilgili belgeyi aynı PR'da güncelle.
- Kod tabanında bulamadığın bir şeyi uydurma; dosya, fonksiyon veya uç nokta adlarını önce ara.

## 6. Genel kod kuralları

- **Dil:** Kod, değişken, fonksiyon, dosya ve commit mesajları İngilizce. Kullanıcıya görünen metinler Türkçe ve yerelleştirme tablolarında. Yorumlar İngilizce veya Türkçe olabilir, bir dosya içinde tutarlı olmalı.
- **Yorumlar:** Neyi değil, neden yapıldığını açıkla. Açık kodu tekrar eden yorum yazma.
- **Hata yönetimi:** Hataları yutma. Beklenen hatalar tipli olarak ele alınır; beklenmeyenler loglanır ve yukarı iletilir.
- **Loglama:** Yapılandırılmış log. Loglara e-posta, takma ad, IP adresi gibi kişisel veri yazılmaz; yalnız anonim kimlikler.
- **Yapılandırma:** Ücretsiz bölüm sayısı, süre sınırı seçenekleri, özellik bayrakları gibi değerler koda gömülmez; yapılandırmadan veya sunucudan gelir.
- **Fiyatlar koda yazılmaz.** Mağaza fiyatları mağazadan, kurum lisans fiyatları sunucudaki lisans kaydından okunur.

## 7. Unity / C# kuralları

**Sürüm ve proje**

- Unity 6 LTS. Proje ayarlarında metin serileştirme (Force Text) açık; ikili varlıklar (`.png`, `.psd`, `.wav`, `.ogg`, `.mp3`, `.ttf`) Git LFS ile tutulur.
- `ProjectSettings/` ve `Packages/manifest.json` yalnız görev gerektiriyorsa değiştirilir ve PR'da belirtilir.

**Assembly yapısı**

| Assembly | İçerik | Kural |
| --- | --- | --- |
| `Roboya.CodingEngine` | Komutlar, program, yorumlayıcı, ızgara, çözücü | `noEngineReferences: true` — `UnityEngine` referansı yasak |
| `Roboya.Core` | Açılış, bileşim kökü, olay yolu, yapılandırma | |
| `Roboya.Games.*` | Her oyun ayrı assembly (`YonAvcisi`, `KodlamaKutusu`, `BalPesinde`, ...) | Oyunlar birbirine referans vermez; ortak şeyler Core veya CodingEngine'e |
| `Roboya.Services` | API istemcisi, eşitleme, satın alma, olay gönderimi, yerel depolama | |
| `Roboya.UI` | UI Toolkit denetleyicileri | |
| `Roboya.Tests.EditMode` / `Roboya.Tests.PlayMode` | Testler | |

**Mimari**

- Oyun mantığı saf C# sınıflarında; `MonoBehaviour` yalnız görünüm ve giriş köprüsüdür ve ince tutulur.
- Bağımlılıklar yapıcı (constructor) ile verilir; tek bir bileşim kökü (`Bootstrap`) servisleri kurar. Bileşim kökü dışında singleton yazma.
- Oyunlar ortak kodlama motorunu kullanır. Bir oyuna özel davranış gerekiyorsa motoru çatallamak yerine genişletme noktası ekle ve ADR yaz.
- Bölümler JSON'dan (Addressables üzerinden) yüklenir. Bölüm verisini koda gömme.
- Ses ve metinler anahtarla çağrılır (`"yon_avcisi.intro.01"`); dosya adıyla doğrudan çağrı yapılmaz.

**Kod stili**

- Alanlar `[SerializeField] private`; public alan yok, gerekirse salt okunur özellik.
- Yeni asenkron kodda Unity 6 `Awaitable` veya `async/await` kullan; yeni coroutine yazma.
- `Update` içinde bellek ayırma (allocation) yapma; tekrar eden efektlerde nesne havuzu kullan.
- Hedef: 2 GB RAM'li Android tablette 30 fps. Performansı etkileyebilecek değişikliklerde bunu PR'da belirt.

**Arayüz**

- Menü, veli alanı, okul girişi ve abonelik ekranları UI Toolkit (UXML + USS) ile yapılır: `Assets/_Project/UI/`.
- Çocuk ekranlarında dokunma hedefi en az 64 dp, yetişkin ekranlarında 48 dp.
- Renk tek başına anlam taşımaz; şekil veya simgeyle desteklenir.

**Sahneler**

- Sahne sayısı az ve ince: `Boot`, `Map`, `Game`. Sahne kurulumu mümkünse editör betikleriyle yapılır.
- Bir görev sahne değişikliği gerektiriyorsa ne değişmesi gerektiğini adım adım yaz; geliştirici editörde uygular.

**Testler**

- Kodlama motoru ve çözücü için EditMode test kapsamı ≥ %90.
- Test adı biçimi: `Method_Condition_ExpectedResult`.
- Kritik akışlar (bölüm aç–oyna–bitir, süre sınırı, okul girişi) PlayMode testiyle korunur.

## 8. API / Python kuralları

- Python 3.12, FastAPI, Pydantic v2, SQLAlchemy 2 (async), Alembic, PostgreSQL 16.
- **Katmanlar:** `routers/` → `services/` → `repositories/`. Router içinde veritabanı erişimi yok. Pydantic şemaları (`schemas/`) ORM modellerinden (`models/`) ayrıdır.
- **Uç noktalar:** Her uç noktada kimlik doğrulama bağımlılığı, açık `response_model`, tanımlı hata kodları. API `/v1` altında sürümlenir.
- **OpenAPI sözleşmedir.** Uç nokta değişince `make gen` çalıştır ve üretilen istemcileri aynı PR'a ekle.
- **Göçler:** Yalnız geriye uyumlu. Uygulanmış bir göç dosyası düzenlenmez. Sütun silme iki adımda yapılır (önce kullanımdan kaldır, sonra sil).
- **Hak hesaplama** tek modülde (`services/entitlements.py`) yaşar. Bu modüle dokunan her değişiklik ≥ %95 test kapsamı ve insan incelemesi gerektirir.
- **Mağaza bildirimleri** imza doğrulamasıyla ve olay kimliği üzerinden idempotent işlenir. Fiş doğrulaması her zaman sunucuda yapılır.
- **Arka plan işleri** (bildirim işleme, rapor, silme talebi) kuyrukta çalışır, tekrar denenebilir ve idempotenttir.
- **Araçlar:** `ruff` (lint + format), `mypy` (servis ve repository katmanında strict), `pytest` gerçek PostgreSQL ile (test konteyneri).
- **Rol tabanlı erişim:** Öğretmen yalnız kendi sınıfını, yönetici yalnız kendi kurumunu görür. Her sorguda kapsam filtresi zorunludur; bunun için testi olmayan uç nokta birleştirilmez.

## 9. Web / TypeScript kuralları

- React, TypeScript (`strict: true`), Vite, TanStack Query.
- API çağrıları yalnız OpenAPI'den üretilmiş istemciyle yapılır; elle `fetch` yazılmaz.
- Klasör yapısı özellik bazlıdır: `src/features/teacher`, `src/features/parent`, `src/features/admin`, `src/shared`.
- `any` kullanılmaz. Bileşenler küçük ve tek amaçlıdır.
- Tüm metinler i18n dosyalarından gelir.
- Erişilebilirlik: anlamsal HTML, klavyeyle gezinme, form alanlarında etiket.
- Testler: Vitest + Testing Library; kritik akışlar için Playwright.

## 10. İçerik ve bölüm formatı

- `packages/level-schema` bölüm formatının tek kaynağıdır. Şema değişirse: sürüm numarasını artır, mevcut bölümler için dönüştürme betiği yaz, `make gen` çalıştır.
- Her bölüm `make validate-content` doğrulamasından geçmelidir: şemaya uygun, çözülebilir, en kısa çözüm uzunluğu hesaplanmış.
- Her bölümün meta verisinde kodlama kavramı, değer, zorluk ve yaş seviyesi bulunur.
- Seslendirme senaryosu `content/voice/script.csv` dosyasındadır (`key`, `text`, `context`, `level_ids`). Ses dosyaları anahtarla adlandırılır. TTS ile üretilen dosyalar sonradan profesyonel kayıtla aynı anahtarlarla değiştirilir; kodda değişiklik gerekmez.
- TTS servisine yalnız senaryo metni gönderilir; hiçbir kullanıcı verisi gönderilmez.

## 11. Gizlilik ve güvenlik

- Çocuk profilinde izinli alanlar: `nickname`, `avatar_id`, `age_band`, `level` ve ilerleme kayıtları. Başka alan ekleme.
- Analitik olayları rastgele üretilmiş anonim profil kimliğiyle gönderilir; takma ad, e-posta veya cihaz reklam kimliği olaya eklenmez.
- Uygulama mikrofon, kamera, konum veya reklam kimliği izni istemez.
- Satın alma, ayarlar, dış bağlantılar ve veli alanı ebeveyn kilidinin arkasındadır.
- Reklam yok, üçüncü taraf takip yok.
- Yapay zekâ (LLM) kullanan özellikler yalnız yetişkin arayüzlerinde bulunur; çocuk verisi modele gönderilmez; çıktılar kaydedilir.
- Veri silme talebi uçtan uca çalışır ve 30 gün içinde tamamlanır; silme işlevine dokunan değişiklik testsiz birleştirilmez.
- Aktarımda TLS, sunucuda ve cihazda şifreli depolama.

## 12. Ödeme, abonelik ve lisans

- **Ücretsiz katman:** İlk 3 bölüm. Sayı yapılandırmadan gelir.
- **Aile Premium:** Aylık ve yıllık; yıllık fiyat, aylık fiyatın 12 katından %15 düşük olacak şekilde mağazada tanımlanır. Fiyatlar kodda yer almaz.
- **Okul lisansı:** Öğrenci başı yıllık; koltuk = öğrenci profili. Lisanslar mağaza dışında, sözleşmeyle satılır ve uygulamada yalnız sınıf koduyla etkinleşir.
- Uygulama içinde mağaza dışı ödemeye yönlendiren bağlantı veya metin bulunmaz.
- İstemci, sunucudan aldığı hakkı süreli olarak önbelleğe alır; çevrimdışıyken önbellek süresi içinde içerik açık kalır.
- Satın alma, yenileme, iptal, geri yükleme ve deneme bitişi akışlarının her biri için test bulunur.

## 13. Çocuk deneyimi kuralları

- Minik seviyede (3.5–5 yaş) ekranda yönerge metni yoktur; her şey sesli ve görseldir.
- Hata cezalandırılmaz: can kaybı, süre baskısı, yıldız kaybı yok.
- Seri (streak) cezası, sürpriz kutu, şans çarkı, oyun içi para birimi, sonsuz otomatik oynatma, çocukları kıyaslayan sıralama yok.
- Çocuğa "geri gel" bildirimi gönderilmez; bildirimler yalnız veliye gider.
- Satış dili çocuğa yönelmez; abonelik ekranına yalnız ebeveyn kilidinden sonra ulaşılır.

## 14. Git, commit ve PR

- **Dal stratejisi:** Ana dal (`main`) her zaman dağıtılabilir. Kısa ömürlü dallar: `feat/`, `fix/`, `chore/`, `content/` + kısa açıklama (ör. `feat/yon-avcisi-sabir-mekanigi`).
- **Commit mesajları:** Conventional Commits, İngilizce (ör. `feat(game): add patience mechanic to direction hunter`).
- **PR açıklaması şu başlıkları içerir:**
  - Özet (ne ve neden)
  - PRD / plan kimliği
  - Çalıştırılan testler
  - Ekran görüntüsü veya video (arayüz değişikliklerinde)
  - Gizlilik, ödeme veya hak etkisi (var / yok; varsa açıklama)
- CI kırmızıyken birleştirme yapılmaz.

## 15. Tamamlanma tanımı

Bir görev ancak şu koşulların hepsi sağlandığında bitmiş sayılır:

- [ ] Kabul kriterleri karşılandı; PRD veya plan kimliği PR açıklamasında anıldı.
- [ ] Birim ve gerekiyorsa entegrasyon testleri yazıldı; CI yeşil.
- [ ] Yeni metin ve sesler yerelleştirme tablolarından geliyor.
- [ ] Oyun ekranı değişikliklerinde düşük seviye Android tablette elle denendi.
- [ ] Kişisel veri veya ödeme etkisi varsa güvenlik kontrol listesi işaretlendi ve insan incelemesi yapıldı.
- [ ] Gerekiyorsa belgeler güncellendi (ADR, API şeması, bu dosya).
