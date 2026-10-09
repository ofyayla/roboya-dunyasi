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

## Engellerin görünüşü ve kenar süsleri (şema v2)

Ses metninde adı geçen nesne tahtada da görünmelidir.
- `grid.looks`: bir `#` karesinin neye benzeyeceğini seçer: `tree`, `rock`, `bush`, `log`. Listelenmeyen engeller bölgenin karışık engelleriyle çizilir.
- `scenery`: oynanan karelerin hemen dışına, yani tahtanın kenarına süs koyar. `x` veya `y` -1 ya da ızgara boyutu olmalıdır. Yürünebilir kareye konamaz, bu yüzden yol gibi görünmez.

```json
"grid": { "rows": [".....", ".#..."], "looks": [{ "x": 1, "y": 1, "look": "log" }] },
"scenery": [{ "x": 1, "y": -1, "look": "tree" }]
```

Örnek: 4. bölümde "Kaplumbağa ağacın arkasında bekliyor" dendiği için kaplumbağanın arkasında bir ağaç (`scenery`) var. 7. bölümde "yola bir kütük devrilmiş" dendiği için engeller kütük (`grid.looks`).

## Rehberli bölüm (`options.guided`)

Yeni bir kavramın ilk bölümünde (ör. 1, 3, 5) Roboya kodu çocukla birlikte kurar: sıradaki doğru kartı paletteki kart nabız atarak gösterir, plan doğru tamamlanınca oynat düğmesi nabız atar. Kartları yine çocuk yerleştirir; Roboya onun yerine bir şey koymaz. Yanlış bir kart konursa nabız, doğru olan karta gider. İşaret, ipucuyla aynı en kısa rota hesabını kullanır; ek veri gerekmez.

## Yeni kart tanıtımı

`cards.introduces` olan bölümde giriş sahnesi, bölüm sesi bittikten sonra kartı büyük gösterir ve `card.<kart>.intro` satırını çalar (ör. `card.turn_right.intro`). Çocuk karta dokunursa bu satır tekrar çalar. Doğrulayıcı bu satırın `script.csv` dosyasında bulunduğunu kontrol eder.

## Hikâye sahneleri (şema v2)

Bölüm, tahtadan önce bir giriş sahnesiyle açılır ve başarıdan sonra bir bitiş sahnesiyle kapanır. Sahnede ses olarak `voice.intro` ve `voice.success` çalar. `story` alanı yalnız ifadeleri ve nesneleri seçer:

```json
"story": {
  "intro": { "roboya": "surprised", "friend": "explaining", "props": ["log"] },
  "outro": { "roboya": "happy", "friend": "thanks", "props": ["log"] }
}
```

- `roboya`: `front`, `happy`, `curious`, `surprised`, `proud`, `laughing`
- `friend` (bölge karakteri): `front`, `happy`, `explaining`, `thanks`
- `props` (en çok 3): `apple`, `pear`, `gear`, `log`, `tree`, `bush`, `rock`

`story` yazılmazsa giriş için `curious` + `explaining`, bitiş için `happy` + `happy` kullanılır. İfadeyi ses metniyle uyumlu seçin. Örneğin "Eyvah, yola bir kütük devrilmiş!" satırı için `surprised` ve `log`.

Şema sürümü 2'dir. Eski (v1) dosyalar `node packages/level-schema/scripts/migrate-v2.mjs content/levels` ile dönüştürülür.

## Daha kolay alternatif (`alternativeLevelId`, YZ-03)

Bir bölümde beş başarısız denemeden sonra Roboya daha kolay bir bölüm önerir; çocuk kabul etmezse aynı bölümde devam eder. Doğrulayıcı şunları zorunlu tutar: alternatif aynı oyunda ve bölgede olmalı, bölümden daha zor olmamalı ve ücretsiz bir bölümün alternatifi de ücretsiz olmalı (ücretsiz bölüm sayısı 3). Alternatif, çocuk için oynanabilir (açık) değilse teklif gösterilmez.

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

## Kodlama Kutusu: hata avcısı ve sıralama

`game: "kodlama-kutusu"`. Bölge patikasında `order` değerleri bölge içinde benzersizdir; oyunlar iç içe dizilir.

- **Sıralama bölümü:** diğer oyunlarla aynı. Hayalet yol (`options.ghostPath`) planlanan rotayı, koşarken çizilen ayak izi gerçekleşeni gösterir (KUT-02).
- **Hata avcısı bölümü:** `starterProgram` yanlış kartlı hazır kodu şeride koyar. Palette hem yanlış hem doğru kart bulunmalıdır. `solution.shortestLength` en kısa doğru çözümdür; hazır kodun yanlış kart sayısı Minik'te 1, Kaşif'te 1–2 olmalıdır. İpucu en kısa çözümle karşılaştırıp yanlış kartı gösterir.
- Ses anahtarları `kodlama_kutusu.lNN.intro` / `.success`.
