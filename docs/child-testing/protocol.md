# Çocuk testi protokolü (F0-21)

> **Taslak.** Pedagoji ekibi tarafından gözden geçirilmeli. Veli bilgilendirme ve onay metni lansman öncesi hukukçu kontrolüne girecek KVKK metinleriyle (F1-25, F2-17) uyumlu hale getirilmelidir.

## Amaç

Kapı 1 ölçütü: **test edilen 4 yaş çocukların en az %75'i ilk bölümü yetişkin yardımı olmadan bitirmeli.** Ek olarak sesli yönergelerin anlaşılması, kart sürükleme ve dokunma becerisi, ipucu kademelerinin işe yarayıp yaramadığı ve çocuğun keyfi gözlenir.

## İlkeler

- **Video, fotoğraf veya ses kaydı alınmaz** (plan "Kapı 1 öncesi" listesi). Yalnız gözlemci notu tutulur.
- Çocuğun adı yazılmaz. Her çocuğa oturum kodu verilir (`T1-01`, `T1-02` …). Kod ile ad eşleşmesi hiçbir yerde tutulmaz.
- Toplanan bilgiler: yaş (yıl ve ay), okuma durumu (evet/hayır), daha önce tablet kullanımı (az/orta/çok), cinsiyet toplanmaz.
- Çocuk istediği an bırakabilir. Oturum en çok 15 dakikadır (Minik seviye önerisi).
- Gözlemci yardım etmez. Çocuk takılırsa 60 saniye beklenir; sonra yalnız "Dinle düğmesine basmak ister misin?" denebilir ve bu not edilir.
- Uygulamada hesap açılmaz, internet gerekmez; cihazda yalnız prototip yüklüdür.
- Formlar 6 ay sonra imha edilir; yalnız toplu sonuçlar (yüzdeler, gözlem özetleri) saklanır.

## Hazırlık

- [ ] Test cihazı: en az bir düşük seviye Android tablet (2 GB RAM, OpenGL ES 3.1+) — bkz. ADR 0005.
- [ ] `make android-apk` ile güncel derleme; ses açık, ekran kilidi ve bildirimler kapalı.
- [ ] Sessiz bir oda, çocuğun yanında güvendiği bir yetişkin (veli veya öğretmen); veli müdahale etmez.
- [ ] İmzalı veli onay formu (aşağıda).

## Akış (her çocuk için ~15 dk)

1. Tanışma, 1 dk: "Seninle bir robot oyunu deneyeceğiz. Doğru ya da yanlış yok; sen oynarken ben not alacağım."
2. Serbest oyun, en çok 12 dk: Uygulama 1. bölümde açılır. Gözlemci yalnız not alır.
3. Kapanış, 1 dk: "En çok neyi sevdin? Zor gelen bir şey oldu mu?" Cevap aynen yazılır.

## Gözlem formu

| Alan | Kayıt |
| --- | --- |
| Oturum kodu / tarih / gözlemci | |
| Yaş (yıl, ay) / okuma / tablet deneyimi | |
| Cihaz modeli | |
| **1. bölüm yardımsız bitti mi?** (evet / yardımla / hayır) | |
| 1. bölüm süresi (sn) ve deneme sayısı | |
| Ulaşılan son bölüm ve toplam yıldız | |
| Giriş sesini dinledi mi, anladı mı? (gözlenen davranış) | |
| İlk kartı nasıl koydu? (dokunma / sürükleme / denemedi) | |
| Oynat düğmesini kendiliğinden buldu mu? | |
| Çarpma veya hata anındaki tepki (güldü / üzüldü / bıraktı) | |
| İpucu düğmesi görüldü mü, kullanıldı mı, hangi kademe yardımcı oldu? | |
| Hayalet yolu takip etti mi? (1–3. bölümler) | |
| Takıldığı yerler (bölüm, adım, ne oldu) | |
| Keyif göstergeleri (gülme, "bir daha", heyecan) / sıkılma göstergeleri | |
| Çocuğun kendi sözleri | |
| Gözlemci notu ve öneri | |

## Değerlendirme

- Kapı 1: 4 yaş grubunda ilk bölümü **yardımsız** bitirenlerin oranı ≥ %75. Hedef: tur başına 8–10 çocuk (PRD bölge testi önerisi), iki tur.
- Bulgular sprint notuna yazılır ve bölüm, ses veya arayüz görevlerine (PRD kimliğiyle) dönüştürülür.

## Veli bilgilendirme ve onay formu (taslak)

**Roboya Dünyası prototip oyun testi**

Roboya Kids olarak geliştirdiğimiz, okul öncesi çocuklara kodlamayı oyunla tanıtan tablet oyununun ilk sürümünü deniyoruz. Çocuğunuzun oyunu nasıl anladığını gözlemlemek istiyoruz.

- Test yaklaşık 15 dakika sürer ve sizin ya da güvendiği bir yetişkinin yanında yapılır.
- **Video, fotoğraf veya ses kaydı alınmaz.** Gözlemci yalnız not alır.
- Çocuğunuzun adı forma yazılmaz; yalnız bir oturum kodu kullanılır. Kaydedilen bilgiler: yaş (yıl/ay), okuma durumu, tablet kullanma deneyimi ve oyun sırasındaki gözlem notları.
- Notlar yalnız oyunu geliştirmek için kullanılır, üçüncü kişilerle paylaşılmaz ve en geç 6 ay içinde imha edilir; yalnız toplu, kimliksiz sonuçlar saklanır.
- Çocuğunuz istediği an oyunu bırakabilir. Onayınızı istediğiniz zaman geri alabilirsiniz; bu durumda notlar hemen imha edilir.
- Sorularınız için: *(iletişim e-postası)*

☐ Yukarıdaki bilgileri okudum; çocuğumun bu teste katılmasına onay veriyorum.

Veli adı soyadı: ____________ İmza: ____________ Tarih: ____________

*(Bu sayfa çocuğun oturum koduyla birlikte saklanmaz; onay formları ayrı ve kilitli bir dosyada tutulur.)*
