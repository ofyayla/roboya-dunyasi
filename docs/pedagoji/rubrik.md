# Pedagoji değerlendirme rubriği

Bölüm, bölüm dizisi, ses metni veya oyun akışı incelenirken kullanılır. Her madde bir kimlik taşır; inceleme raporlarında "Dayanak" sütununda bu kimlik anılır. Gerekçeler [`cerceve.md`](cerceve.md) içindedir.

## Önem dereceleri

| Önem | Anlamı | Ne zaman |
| --- | --- | --- |
| **Engel** | Yayından önce düzeltilmeli | Çocuk ilerleyemez; yaşa açıkça uygunsuz; ses–ekran uyumsuzluğu bölümü çözülemez kılar; CLAUDE.md §13 ihlali |
| **Önemli** | Öğrenmeyi belirgin zayıflatır | Birden fazla yenilik aynı anda; iskele eksik; değer yalnız sözde |
| **Öneri** | İyileştirme | Çeşitlilik, ton, ince ayar |

## 1. Gelişimsel uygunluk (GEL)

- **R-GEL-1** En kısa çözüm seviyenin aralığında (Minik 2–4, Kaşif 5–10, Mucit 10+); `maxProgramLength` en kısa çözümden 1–2 fazla.
- **R-GEL-2** Bilişsel yük: aynı anda akılda tutulacak öğe sayısı yaşa uygun (hedef sayısı, renk kuralı, sıra kuralı, engeller birlikte).
- **R-GEL-3** Minik'te ekranda yönerge metni yok (OYN-01); gereken her bilgi ses ve görselle veriliyor.
- **R-GEL-4** Görünmeyen bilgi (gizli bellek, sesle söylenip ekranda olmayan hedef) yaşa uygun mu.
- **R-GEL-5** Yön ve bakış: robotun baktığı yön okunuyor; "robotun sağı" ile "çocuğun sağı" çatıştığında seviyeye uygun destek var.
- **R-GEL-6** Oturum süresi: seviyenin önerilen oturumunda kaç bölüm ve kaç yenilik düşüyor.

## 2. Kavram ilerleyişi ve iskele (ILR)

- **R-ILR-1** Bir bölümde tek yenilik (kart, mekanik, hedef türü, ilk engel).
- **R-ILR-2** Yeni kart `cards.introduces` ile tanıtılıyor (YON-01); tanıtılmamış kart palette yok.
- **R-ILR-3** Yeni mekaniğin ilk bölümü rehberli (`options.guided`); destek kademeli azalıyor (hayalet yol → rehber → ipucu).
- **R-ILR-4** Zorluk eğrisi: ani sıçrama yok; zorluk yalnız kart sayısıyla değil bağlamla da artıyor (engel, sıra, toplama).
- **R-ILR-5** Yaş başlangıcı: her seviyenin ilk bölümünden itibaren çocuk her mekanikle tanıtım bölümünde karşılaşıyor (ADR 0018).
- **R-ILR-6** Tekrar: yeni beceri, bir sonraki yenilikten önce en az birkaç bölümde farklı bağlamda pekişiyor.
- **R-ILR-7** Alternatif bölüm (YZ-03) gerçekten daha kolay ve aynı beceriyi çalıştırıyor.

## 3. Değer gömülmesi (DEG)

- **R-DEG-1** Değer mekaniğin içinde (ör. sabır: plan bütün olarak çalışır), yalnız seste değil.
- **R-DEG-2** Bölümün başında bir karakterin ihtiyacı var; amaç puan değil yardım.
- **R-DEG-3** Değer anı kısa, vaaz tonu yok; çocuğa "şöyle olmalısın" denmiyor.
- **R-DEG-4** Değer dili evrensel; belirli bir aile profilini dışlamıyor.

## 4. Ses ve dil (DIL)

- **R-DIL-1** Kelimeler yaşa uygun; soyut ve yetişkin kelimeleri yok.
- **R-DIL-2** Cümleler kısa, tek mesajlı; Minik'te bir yönergede tek iş.
- **R-DIL-3** Seste adı geçen nesne, karakter ve renk ekranda görünüyor.
- **R-DIL-4** Yön dili tutarlı ("sağa dön" her yerde aynı); kart adlarıyla birebir.
- **R-DIL-5** Ton sıcak, cesaretlendirici; başarı abartısız, hata suçlamasız.
- **R-DIL-6** Satış, "geri gel" veya kıyaslama dili yok.

## 5. Hata ve motivasyon (MOT)

- **R-MOT-1** Hata cezalandırılmıyor (can, süre, yıldız kaybı yok); hata sonrası yön gösteriliyor.
- **R-MOT-2** İpucu kademeleri (YZ-01) bu bölümde anlamlı: ses ipucu bölüme uygun, hatalı kart vurgulanabilir.
- **R-MOT-3** Yıldız ölçütleri (OYN-04) bu bölümde ulaşılabilir ve adil; üçüncü yıldız anlamlı.
- **R-MOT-4** Ödül içsel motivasyonu desteklemekle kalıyor; §13'teki yasak mekanikler yok.

## 6. Erişilebilirlik ve görsel dil (ERS)

- **R-ERS-1** Renk tek başına anlam taşımıyor; biçim veya simgeyle destekleniyor.
- **R-ERS-2** Dokunma hedefleri çocuk ekranında ≥ 64 dp.
- **R-ERS-3** Hedef, toplanacaklar ve ilerleme ekranda görünüyor (hedef rozeti, sayma).
- **R-ERS-4** Oyunun görsel kimliği mekaniği anlatıyor (Kodlama Kutusu masası, arının tuşları).
