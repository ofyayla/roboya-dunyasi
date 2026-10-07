# Bölüm yazım rehberi

Bölümler kod yazmadan, JSON dosyasıyla eklenir (OYN-07). Şema: [`packages/level-schema/level.schema.json`](../../packages/level-schema/level.schema.json). İleride bölüm editörü bu dosyaları sizin yerinize üretecek.

## Dosya yeri ve kimlik

`content/levels/<bölge>/<oyun>/<nn>.json` — kimlik `<bölge>.<oyun>.<nn>` olmalı (ör. `sabir-ormani.yon-avcisi.03`). Kimlik ilerleme kayıtlarında kullanılır; yayınlanmış bir kimlik değiştirilmez ve silinen kimlik yeniden kullanılmaz.

## Izgara

`grid.rows` satırlarından oluşur; ilk satır kuzeydir (ekranın üstü).

| Karakter | Anlam |
| --- | --- |
| `.` | Yürünebilir zemin |
| `#` | Engel (kütük, kaya, ağaç) |

Koordinatlar: `x` soldan sağa, `y` yukarıdan aşağıya, 0'dan başlar. Robotun yönü `north` (yukarı), `east` (sağ), `south` (aşağı), `west` (sol).

## Seviye kuralları

| Seviye | Yaş | En kısa çözüm | Notlar |
| --- | --- | --- | --- |
| Minik | 3.5–5 | 2–4 kart | Ekranda yazı yok; yeni kart `cards.introduces` ile tanıtılır; ilk bölümde `options.guided` |
| Kaşif | 5–7 | 5–10 kart | Engeller, nesne toplama |

`cards.maxProgramLength`, en kısa çözümden 1–2 fazla olmalı. Fazlası 3. yıldızı (en kısa kod) anlamsızlaştırır.

## Sesler

`voice.intro`, `voice.success` ve `voice.hints` anahtarları [`content/voice/script.csv`](../voice/script.csv) dosyasında bulunmalıdır. Metinler yalnız bu dosyaya yazılır; bölüm dosyasına Türkçe yönerge yazılmaz (`meta.notes` iç notlar içindir ve çocuğa gösterilmez).

## Doğrulama

```bash
make validate-content   # şema + çözülebilirlik + ses anahtarları
make fix-content        # en kısa çözüm uzunluğunu (solution.shortestLength) dosyalara yazar
```

Doğrulayıcı şunları kontrol eder: şemaya uygunluk, bölümün tanımlı kartlarla ve plan şeridi uzunluğunda çözülebilir olması, kayıtlı en kısa çözümün doğru olması, ses anahtarlarının senaryoda bulunması, dosya yeri, kimlik ve sıra çakışmaları, alternatif bölüm (YZ-03) bağlantıları.

## Prototip bölümleri (F0-16)

`sabir-ormani/yon-avcisi/01–10` taslak bölümlerdir; pedagoji ekibinin incelemesi ve çocuk testleri (F0-21) sonrası güncellenecektir.

| # | Kavram | Yeni şey | En kısa |
| --- | --- | --- | --- |
| 01 | Yön | İleri kartı (destekli, hayalet yol) | 2 |
| 02 | Yön, sıralama | Daha uzun düz yol | 3 |
| 03 | Yön | Sağa dön kartı (destekli) | 3 |
| 04 | Sıralama | Desteksiz tek dönüş | 4 |
| 05 | Yön | Sola dön kartı | 3 |
| 06 | Sıralama | Robot başka yöne bakarak başlar | 4 |
| 07 | Sıralama | İlk engel (kütük) | 7 |
| 08 | Sıralama | İlk nesne toplama (elma) | 7 |
| 09 | Sıralama | İki nesne + engeller | 8 |
| 10 | Sıralama | Bölge finali: meyve + gemi parçası | 9 |
