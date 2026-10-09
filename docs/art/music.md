# Bölge müziği — kaynak ve lisans

| Dosya | Kaynak | Not |
| --- | --- | --- |
| `apps/game/Assets/_Project/Resources/Music/sabir_ormani.ogg` | Suno (suno.com) ile üretilen "Sabir Ormani Loop" (sözsüz, marimba/glockenspiel/gitar/flüt) | WAV'dan dönüştürüldü: ilk 6 sn sonuna çapraz geçişle eklenip kesintisiz döngü yapıldı, −23 LUFS'a normalize edildi, Vorbis q4 (≈2 MB). |

**Durum (2026-10-09):** Ürün sahibi parçanın ücretli Suno planında üretilip indirildiğini bildirdi. Mağaza sürümünden önce plan kanıtı (fatura/ekran) hâlâ saklanmalı.

**Lisans notu:** Suno yalnız ücretli (Pro/Premier) planda üretilen parçalara ticari kullanım hakkı verir; ücretsiz plan çıktıları ticari kullanıma uygun değildir. Parçanın üretildiği ve indirildiği hesabın planı mağaza sürümünden önce doğrulanmalı, plan kanıtı (fatura/ekran) saklanmalı. Doğrulanamazsa dosya silinir: kod, kodla üretilen müziğe (`ProceduralMusic.Render`) kendiliğinden döner.

Kaynak WAV/M4A depoya eklenmedi (28 MB); `Downloads` dışında saklanmalıdır.
