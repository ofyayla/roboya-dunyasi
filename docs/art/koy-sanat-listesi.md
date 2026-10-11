# Paylaşım Köyü — sanat teslim listesi (plan E3-a)

Bu liste, dış sanatçıya ya da yeni bir Higgsfield turuna verilecek işi tek yerde toplar. Stil: [`style-guide.md`](style-guide.md) (düz vektör, kalın sıcak kontur `#3A2A1D`, yuvarlak köşe, **renk tek başına anlam taşımaz**, çocuk ekranında yazı yok). Şu an oyunda **geçici set** (Higgsfield, ücretsiz plan; lisans notu `style-guide.md` "Paylaşım Köyü — geçici set") çalışıyor. Kalıcı sanat **aynı dosya adlarıyla** gelir; kod değişmez, yalnız `make refresh-art` çalıştırılır.

## Bugün var ve oyunda kullanılıyor (kalıcı sürümü istenen)

| Dosya (`apps/game/Assets/_Project/Art/…`) | `RegionArt` alanı | Gerekli durumlar / not |
| --- | --- | --- |
| `PaylasimKoyu/bg_paylasim_koyu` | `background` | orta-alt bölge boş çayır (tahta oturur) |
| `PaylasimKoyu/item_hive_empty`, `item_hive_full`, `item_hive_overflow` | `hiveEmpty`, `hiveFull`, `hiveOverflow` | üç durum **şekille** ayrışır (boş altıgen / dolu bal / taşan damla); yuvalar kodla çizilir, resimde yuva olmamalı ya da silik olmalı |
| `PaylasimKoyu/item_honey_basket` | `honeyBasket` | Ari'nin sepeti (HUD'da ve tahtada) |
| `PaylasimKoyu/prop_village_table` | `propVillageTable` | ortak köy sofrası; artan bal burada durur ("atılmış bal" gibi değil) |
| `PaylasimKoyu/prop_festival_flags` | `festivalFlags` | şenlik bayrakları; 24. bölümde hedefin yanında çizilecek (bkz. aşağıda "Eksik") |
| `PaylasimKoyu/prop_ari_home` | — | şimdilik kullanılmıyor (Ari'nin kovanı kararla kaldırıldı, PK-0001 madde 4) |
| `Characters/KoyArilari/elder_bee_{front,happy,explaining,thanks}` | `goalIdle/Happy/Explaining/Thanks` | bölge karakteri: yaşlı köy arısı (hasırşapka, çiçekli baston); Bilge Kaplumbağa ile aynı 4 poz |
| `Characters/KoyArilari/villager_bee_{happy,sad,thanks,child}` | `villagerHappy/Sad/Thanks/Child` | doğal arı (ekran yok → robot Ari'den ayrışır); `sad` yumuşak, ağlamıyor; **cezalandırıcı değil açıklayıcı** |
| `Ship/ship_engine_core` | gemi parçası (`content/rewards/ship-parts.json`, `engine`) | bölge sonunun büyük parçası |

## Eksik (bugün ormandan ödünç)

| İş | Neden | Bağlanacağı yer |
| --- | --- | --- |
| **Köy engelleri** (çit, saman balyası/kasa, ayçiçeği; en az 3 farklı silüet) | köy bölümleri engelleri orman çalısı/kayası olarak çiziyor; ses/anlatımla ("çalıların arasından") uyuşuyor ama bölgeye yabancı | `obstacles[]`, `propBush`, `propRock`, `propTree`, `propBox` (`SceneBuilder.EnsurePaylasimKoyuArt`) |
| **Köy patika taşları** (altıgen petek taşı: geçilen / sıradaki / gelinmemiş / köşe) | patika orman taşlarıyla çiziliyor | `stoneDone/Current/Ahead/Corner` |
| **Şenlik bayrakları hedef işareti** | `festivalFlags` sprite'ı hiçbir yerde çizilmiyor; 24. bölüm "şenlik alanına var" diyor ama hedef yaşlı arı | `BoardView` hedef çizimi (küçük kod işi) + sanat |
| **Ada haritasında köyün "açılmış" hâli** (bayraklı küçük işaret) | açık/kapalı bölge şimdi nabız ve bulutla gösteriliyor; köyün kendine özgü bir "açıldı" anı yok | `IslandView` / `PartArt` (en küçük çözüm PR'da seçilir) |
| **Bal miktarı resmi** (bir noktada 2–5 bal: yığılı damlalar) | `amount` > 1 olan bal tek damla çiziliyor; ses "üç bal" diyor (pedagoji incelemesi, bulgu 3) | ayrı iş açıldı; sanat 1/2/3/5 damlalı varyant verebilir |
| **Şenlik alanı** (24. bölüm arka planı için ayrı varyant, isteğe bağlı) | final kutlama | `background` varyantı |

## Teslim biçimi

PNG, saydam arka plan, `style-guide.md`'deki içe aktarma ayarları (en çok 1024 px, arka plan 2048). Dosya adları yukarıdaki gibi; yeni dosya adı gerekirse listeye eklenir. Her varlık için kaynak sayfa `docs/art/sources/` altına konur.

## Lisans

Geçici setin Higgsfield ücretsiz planında üretildiği `style-guide.md`'de not edildi. Mağaza sürümünden önce ya ücretli planın ticari koşulları doğrulanır ya da bu liste kalıcı sanatla karşılanır (F1-23 gibi dış kaynak). Yapay zekâ çıktısının telif belirsizliği ve ana karakter notları `style-guide.md` "Lisans ve telif" bölümünde.
