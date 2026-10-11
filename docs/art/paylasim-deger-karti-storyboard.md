# Paylaşma değer kartı — hikâye taslağı (plan E4-a)

Amaç: "Herkese eşit pay vermek ve artanı birlikte kullanmak paylaşmaktır" fikrini ~30 sn'de, **yazısız** anlatmak. Sabır filmiyle ([`sabir-deger-karti-storyboard.md`](sabir-deger-karti-storyboard.md)) aynı üretim yolu ve aynı kurallar; her köşe sonunda bu film oynar (köy 4 köşe, bölge sonu dahil). Karakterler yalnız mevcut setten gelir: **Ari**, yaşlı köy arısı, köylü arılar (`Characters/KoyArilari`), sahne 5'te Roboya. Stil: `style-guide.md`; arka plan `PaylasimKoyu/bg_paylasim_koyu`.

Dil kuralı (PK-0001): "kimse fazla almasın" gibi yasaklayıcı ifade yok; artan bal sofraya gider, "atılmış" değil "birlikte yenecek" gösterilir; üzgün arı kızgın değil, bekleyen ve yumuşak.

| # | Süre | Sahne | Hareket | Anlatım (`value.sharing.N`) |
| --- | --- | --- | --- | --- |
| 1 | 6 sn | Köyün üç peteği; biri dolu, ikisinin yuvaları boş. Ari çiçekten topladığı balla sepetiyle geliyor. | Ari sakin adımlarla yaklaşır, sepetinde bal damlaları parlar. | "Köyün peteklerinde bal azmış. Ari çiçeklerden bal topladı." |
| 2 | 6 sn | Yaşlı köy arısı boş peteklere eliyle işaret eder; küçük bir arı yavrusu boş yuvaya bakar. | Hafif yakınlaşma; yavru başını yana eğer (üzgün değil, meraklı). | "Bazı petekler bomboş, küçük arılar bekliyor." |
| 3 | 6 sn | Ari balı iki peteğe sırayla bırakır: bir ona, bir ona. Yuvalar tek tek dolar; iki petek yan yana aynı doluluğa gelir. | Damlalar birer birer uçar; yuvalar dolarken ışıldar. | "Ari balı bir ona, bir ona dağıttı. Herkesin payı aynı oldu." |
| 4 | 6 sn | Sepette tek damla kalmıştır; Ari onu köy sofrasına koyar, arılar toplanır. | Damla sofraya iner, arılar gülümseyerek yaklaşır. | "Artan balı ortak sofraya koydular." |
| 5 | 6 sn | Şenlik bayrakları altında herkes sofrada; Roboya ve Ari el sallar; yıldız parıltısı. | Sabit kamera, hafif geri çekilme; arılar bal yer, gülüşür. | "Birlikte yediler, birlikte güldüler. Paylaşmak herkesi mutlu eder!" |

Toplam ≈ 30 sn. Her klip 16:9, sabit kamera ya da çok hafif yaklaşma, düşük hareket yoğunluğu (3.5 yaş için sakin). Kart özeti (film yokken ya da bittikten sonra): `value.sharing`.

## Üretim hattı (Sabır filmiyle aynı)

1. **Anahtar kareler:** Higgsfield (görsel); referanslar `Characters/KoyArilari`, `Characters/Ari`, `Characters/Roboya`, `PaylasimKoyu/bg_paylasim_koyu`, `item_hive_*`, `prop_village_table`.
2. **Hareket:** Higgsfield görselden videoya, kare başına 5–6 sn.
3. **Birleştirme:** `ffmpeg`, 720p H.264 (~5–8 MB) → `content/video/value_sharing.mp4`. `ValueMoments` bu yolu ve `value.sharing.1…5` satırlarını arar; **satırlar film hazır olunca `script.csv`'ye eklenir** (film yokken sahne satırları eklenmez) ve `make tts` ile seslendirilir.
4. **Ses:** anlatım + kısa müzik (köy döngüsü, plan E4-c).
5. **Unity:** `VideoPlayer` ile `ValueCard` içinde (kod hazır, C-c); film yoksa sakin kart ve `value.sharing` satırı.

## Denetim ve maliyet

- Her klibin her karesi bir insan tarafından izlenir (çocuk uygunluğu, karakter tutarlılığı, arıların "sevimli doğal arı" kalması). Karede yazı ya da rakam çıkarsa klip atılır. Üzgün arı yalnız ima edilir (sahne 2), korkutucu ya da kavgacı kare yok.
- Yapay zekâ üretimi mağaza gizlilik/sanat notlarında belirtilir; ticari hak için kullanılan planın koşulları doğrulanır (`style-guide.md`).
- Sabır filmi referansıyla 5 klip ≈ 135 kredi (6 sn, 720p, sessiz, Seedance 2.0), yeniden denemeler hariç. **Harcama onayı ürün sahibindedir.**

## Köşe sonu anları

`ValueMoments` değere bağlıdır; dört köy köşesi de aynı filmi oynar (ormandaki gibi). Köşeye özgü varyant (ör. köşe 3'te yalnız eşit bölme, köşe 4'te çıkarma vurgusu) ayrı bir mimari iş; pedagoji incelemesi (Köşe 1, soru 4) bu konuyu ekibe sormuştu. Şimdilik tek film.
