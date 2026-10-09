# Görsel dil — Sabır Ormanı (F0-17)

Karar (2026-10-08, veli/ürün sahibi): **düz vektör** stil. Oyun içi tüm varlıklar bu belgedeki kurallara uyar.

## İlkeler

- **Okunurluk önce:** 3.5 yaş için net siluetler, yüksek kontrast, az detay.
- **Kalın, sıcak kontur:** koyu kahverengi `#3A2A1D`, tüm karakter ve nesnelerde aynı kalınlık hissi.
- **Düz dolgu + tek yumuşak gölge**, üst-solda tek küçük beyaz parlama.
- **Yuvarlak köşeler**, sivri ya da tehditkâr şekil yok.
- **Renk tek başına anlam taşımaz:** her nesnenin kendine özgü bir şekli vardır (elma yuvarlak, armut damla, çark dişli).
- **Arka planlar sakin:** oyun tahtasının oturduğu orta-alt bölge az detaylı kalır.

## Palet

| Kullanım | Renk |
| --- | --- |
| Roboya gövde / gölge | `#F28C28` / `#D9701A` |
| Roboya yüz ekranı | `#FFF4E2` |
| Roboya kol ve bacak | `#8A5A3B` |
| Yanak | `#F4A9A0` |
| Kontur | `#3A2A1D` |
| Kaplumbağa ten / gölge | `#7DB55A` / `#5E9443` |
| Kaplumbağa kabuk / kenar | `#6E8F3A` / `#D8B77A` |
| Çim karo | `#8BC26B` |
| Toprak yol | `#E7C98F` |
| Gökyüzü | `#CDEBF5` |

## Karakterler

**Roboya:** tek parça yuvarlak köşeli turuncu küp (ayrı kafa ve boyun yok); büyük krem yüz ekranı, nokta gözler, küçük gülümseme, pembe yanaklar; küpün **sol üst köşesinden** çıkan ince anten ve turuncu top; iki yanda kahverengi kulak çıkıntıları; eldiven eller; kısa bacaklar ve yuvarlak ayaklar; sırtında dört vidalı krem kapak.

**Bilge Kaplumbağa:** yuvarlak yeşil baş, küçük yuvarlak gözlük, beyaz kaş tutamı, sakin yarı kapalı gözler; altıgen desenli zeytin yeşili kabuk ve ten rengi kenar.

## Varlıklar

`apps/game/Assets/_Project/Art/` altında; PNG, saydam arka plan. İçe aktarma ayarları `Editor/ArtImportSettings.cs` ile yapılır: sprite, mipmap yok, en fazla 1024 px (arka plan 2048), sıkıştırılmış.

| Klasör | Dosyalar |
| --- | --- |
| `Characters/Roboya` | `roboya_master`, `front`, `threequarter`, `side`, `back`, `happy`, `curious`, `surprised`, `laughing` (çarpma anı, OYN-03), `proud`, `walk` |
| `Characters/BilgeKaplumbaga` | `turtle_front`, `threequarter`, `side_walk`, `happy`, `explaining`, `thanks` |
| `Characters/Ari` | `ari_front`, `ari_back`, `ari_side` (batıya bakar), `ari_happy` (Bal Peşinde'nin oyuncu karakteri; Roboya orada yönlendirici) |
| `SabirOrmani` (çiçek ve bal) | `item_flower_{yellow,red,blue,purple,orange,green}` (her renk ayrı yaprak biçimi: papatya, lale, yıldız, kalp, güneş ışını, yonca), `item_honey_drop`, `item_honeycomb` |
| `SabirOrmani` | `tile_grass`, `tile_path`, `tile_log`, `prop_rock`, `prop_tree`, `prop_bush`, `item_apple`, `item_pear`, `item_gear`, `bg_sabir_ormani` |

Kaynak sayfalar (yeniden kesim için): `docs/art/sources/`.

## Oyunda kullanım

- Bölge sprite seti `Art/SabirOrmani/SabirOrmaniArt.asset` (`Roboya.UI.RegionArt`); sahneye `SceneBuilder` bağlar, elle düzenlenmez.
- **Robotun yönü görünümle gösterilir:** kuzey = arka görünüm, güney = ön, batı = yan, doğu = aynalanmış yan. Dönüşte kısa bir "sıkış-çevir" animasyonu oynar.
- Çarpmada `laughing`, başarıda `happy` ifadesi; kaplumbağa başarıda `turtle_happy` olur ve yana kayar.
- **Eğik tahta:** tahta hafif perspektifle, önden ve yukarıdan görülür (`Roboya.UI.ObliqueProjection`). Arka sıra ön sıranın %80'i kadardır. Izgara çizgileri düz kalır ve ekranda yukarı her zaman kuzeydir.
- Zemin kodla çizilir (`ObliqueGround`): çim yüzey, sayılabilsin diye hafif dama deseni, toprak ön yüz ve kontur. Karo sprite'ları (`tile_grass`, `tile_path`) perspektife uymadığı için tahtada kullanılmaz.
- Karakter, nesne ve engeller hücrenin üzerinde ayakta durur. Uzaktan yakına doğru sıralanır, derinliğe göre küçülür ve altlarında yumuşak gölge olur.
- Hayalet yol (YON-03), başlangıçtan hedefe kesintisiz bir toprak patika olarak çizilir. Engeller hücreye göre sabit seçilen ağaç, kaya veya çalıdır. Tahtanın kenarında birkaç çalı ve ağaç (`decor`) tahtayı ormana oturtur.
- Sprite atanmamış bir alan varsa kodla çizilen yedek simge kullanılır.
- **Çocuk testinde gözlenecek:** başlangıçta robot yukarı bakıyorsa arkası görünür (yüzü görünmez). Çocukların bunu "yukarı gidiyor" diye anlayıp anlamadığı F0-21'de not edilmeli.

## Hikâye sahneleri

- Her bölüm geniş açılı kısa bir sahneyle açılır ve kapanır (`Roboya.UI.StoryStage`). Sahne yeni çizim gerektirmez; bölge arka planı, karakter ifadeleri ve nesnelerle kodla kurulur.
- **Yerleşim:**
  - Roboya sol üçte birde, ekran yüksekliğinin ~%52'si boyunda.
  - Bölge karakteri sağ üçte birde, ~%44 boyunda.
  - Nesneler ikisinin arasında yerde durur.
  - Alt köşelerdeki büyük çalılar ön plan çerçevesi olur.
  - Arka plan yavaşça yakınlaşır; bu kamera hareketi hissi verir.
- **Hareket:** karakterler kenarlardan zıplayarak girer. Ses çalarken Roboya küçük sekmelerle konuşur, kaplumbağa başını sallar. Bitişte ikisi birlikte zıplar.
- **Giriş sahnesi:** ses bitince 1,2 sn sonra kendiliğinden tahtaya geçer; büyük yeşil "devam" düğmesiyle de geçilebilir. "Tekrar oyna" sahneyi atlar.
- **Bitiş sahnesi:** yıldızlar gökyüzünde, "tekrar" ve "sonraki" düğmeleri sağ altta görünür; karakterler görünür kalır.
- **Veri:** ifadeler ve nesneler bölüm JSON'undaki `story` alanından gelir (şema v2). Alan yoksa varsayılan ifadeler kullanılır. Ses `voice.intro` ve `voice.success` anahtarlarıdır.
- **Kütük** (`log`) kodla çizilir (`LogShape`); elimizdeki kütük görseli çimli bir karo olduğu için sahnede kullanılamıyor.
- **Yeni kart (YON-01):** kartı tanıtan bölümde, bölüm sesi bitince kart iki karakterin arasında, beyaz bir halkanın içinde büyük olarak belirir ve kendi tanıtım satırı çalar. Karta dokununca satır tekrar çalar.
- **Tahta sesle uyumlu olmalı:** seste adı geçen nesne tahtada da görünür. Engelin görünüşü `grid.looks`, tahtanın kenarındaki süsler `scenery` ile seçilir.
- Bölge açılış sahnesi ve değer kartı (F1-24) özel çizimle ayrıca yapılacak.

## Ada haritası ve robot parçaları

- **Ada haritası:**
  - Görsel: `Art/Island/bg_island.png`; kaynaklar `sources/island-a.png` ve seçilen `sources/island-b.png`.
  - Bölgelerin konumu `content/map/island.json` dosyasında, görselin oranı olarak. Görsel değişirse yalnız bu dosya güncellenir.
  - Kapalı bölgeler bulut ve asma kilitle örtülür; açık bölgenin etrafında beyaz, nabız atan bir halka olur.
- **Bulutlar:** `Art/Island/map_cloud_*.png`.
- **Roboya'nın gemisi (ödüller, ILR-03):**
  - Roboya'nın görünüşü hiçbir yerde değişmez; ödüller gemiyi onarır.
  - Bozuk gövde (`Art/Ship/ship_base.png`) ve 7 parça (pervane, ışıklar, kanatçıklar, çanak anten, bayrak, roket ağzı, iniş ayakları) tek bir sayfadan kesildi; kaynak `sources/ship-sheet.png`.
  - Parçaların gövde üzerindeki yeri ve sırası `content/rewards/ship-parts.json` dosyasında (gövdenin oranı olarak). Kanatçık, ayak, roket ağzı ve anten gövdenin arkasında çizilir.
  - Ada haritasında `ShipView`, görseldeki boyalı geminin tam üstüne oturur ve onu örter. Yeni parça haritada bir kez yukarıdan inip yerine oturur.
  - Atölyede henüz kazanılmamış parçalar gövdenin arkasında soluk gölge olarak görünür.
- **Üretim:** ada için iki deneme, gemi için bir sayfa (iki deneme), toplam yaklaşık 11 kredi. Gemi sayfasında referans, ada görselinden kesilen gemiydi.

## Üretim yöntemi

Higgsfield, GPT Image 2.5 Sunburst; High kalite, 2K, saydam arka plan. Ana Roboya (`sources/master-2.png`) **her yeni karakter ve varlık üretiminde referans** olarak verilir. Sayfalar ızgara halinde üretilir ve `tools/art` dışı tek seferlik bir betikle bağlı bileşen analizine göre tek tek PNG'lere kesilir. Bu yöntem tutarlılığı korur ve maliyeti düşürür. Bu turda yaklaşık 30 kredi harcandı.

## Lisans ve marka

- Görseller yapay zekâyla üretildi. Higgsfield planının ticari kullanım koşulları satın alma/abonelik sırasında doğrulanmalı.
- Yapay zekâ çıktısının telif koruması belirsiz. **Marka olarak tescil edilecek ana Roboya**, lansmandan önce bu belgeyi referans alan bir illüstratör tarafından temize çekilmeli veya rötuşlanmalı.
- Prototip ve çocuk testlerinde (F0-21) bu varlıklar kullanılabilir.

## Arı, çiçek ve bal (2026-10-09)

Higgsfield ile üretilmiş geçici setler (kaynak sayfalar `docs/art/sources/bee-sheet.png`, `flower-sheet.png`; `tools/art/slice_sheet.py` ile kesilir). Her çiçek rengi ayrı yaprak biçimine sahiptir; renk tek başına anlam taşımaz. Kalıcı sanat (F1-23) aynı dosya adlarıyla değişir.
