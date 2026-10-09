# Sabır değer kartı — hikâye taslağı (F1-24)

Amaç: "Acele etmeden önce planlamak sabırdır" değerini ~30 sn'de, yazısız anlatmak. Karakterler yalnız mevcut setten (Roboya, Bilge Kaplumbağa) gelir; yeni karakter yok. Stil: `docs/art/style-guide.md` (düz vektör, kalın sıcak kontur `#3A2A1D`, sakin Sabır Ormanı). Ekranda yazı yok. Anlatım: `value.patience` satırı (ElevenLabs).

| # | Süre | Sahne | Hareket |
| --- | --- | --- | --- |
| 1 | 6 sn | Roboya, elmalarla dolu bir sepete koşarak gidiyor; acelesi var. | Roboya yan görünüşten hızla zıplayarak ilerler. |
| 2 | 6 sn | Yolda bir kütüğe takılıp düşer, elmalar etrafa saçılır. | Roboya sendeler, düşer; elmalar yuvarlanır. Üzgün değil, şaşkın. |
| 3 | 6 sn | Bilge Kaplumbağa yavaşça yanına gelir, sakince gülümser. | Kaplumbağa ağır adımlarla yürür, Roboya'ya elini uzatır. |
| 4 | 6 sn | İkisi birlikte yere yön kartlarını sırayla dizer. | Kartlar tek tek yerine oturur; Roboya sabırla bakar. |
| 5 | 6 sn | Roboya planı yürür, elmaları toplar, kütüğün etrafından dolanır; mutlu. | Roboya ağır ve emin adımlarla yürür; elmalar sepete dolar; Kaplumbağa başını sallar; yıldız parıltısı. |

Toplam ≈ 30 sn. Her klip 16:9, sabit kamera ya da çok hafif yaklaşma, düşük hareket yoğunluğu (3.5 yaş için sakin).

## Üretim hattı

1. **Anahtar kareler:** Higgsfield (görsel) — referans: `Art/Characters/Roboya`, `Art/Characters/BilgeKaplumbaga`, `Art/SabirOrmani/bg_sabir_ormani.png`.
2. **Hareket:** Higgsfield görselden videoya, kare başına 5–6 sn.
3. **Birleştirme:** `ffmpeg` ile klipler birleştirilir, 720p H.264 (~5–8 MB).
4. **Ses:** `value.patience` anlatımı + kısa müzik (Suno döngüsü).
5. **Unity:** `VideoPlayer` ile `ValueCard` içinde oynatılır; video yoksa bugünkü durağan kart kalır.

## Denetim

- Her klibin her karesi bir insan tarafından izlenir (çocuk uygunluğu, karakter tutarlılığı).
- Karede yazı ya da rakam çıkarsa klip atılır.
- Yapay zekâ üretimi olduğu mağaza gizlilik/sanat notlarında belirtilir; ticari hak için kullanılan planın koşulları doğrulanır.

## Maliyet (2026-10-09, Higgsfield, Seedance 2.0)

6 sn, 16:9, 720p, sessiz klip ≈ **27 kredi** (indirimli; liste 36). 5 klip ≈ 135 kredi, yeniden denemeler hariç. Başlangıç kare yüklemesi sayfa geçişinde kayboldu; üretimde görsel, video sayfasında (`/ai/video`) yüklenmeli.
