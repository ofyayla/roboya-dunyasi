# 0029 — Oyuna göre oyuncu karakteri, hedef rozeti ve yön oku

- Durum: Kabul edildi
- Tarih: 2026-10-09
- İlgili: `docs/review/sabir-ormani-oyun-ve-pedagoji-degerlendirmesi.md` P0 1–4; ADR 0025, 0026; docs/art/style-guide.md

## Bağlam

Ses "arı" ve "çiçek" diyor, ekranda Roboya ve elma vardı; toplama bölümlerinde hedef görünmüyor, robotun yönü okunmuyordu. 3,5–7 yaş için ses ve ekran aynı şeyi göstermelidir.

## Karar

1. **Oyuncu karakteri oyuna göre seçilir:** `RegionArt.ActorFor(GameId)` Bal Peşinde için Ari'yi (ön, arka, yan, mutlu), diğer oyunlar için Roboya'yı döndürür; sprite yoksa Roboya'ya düşer. `BoardView` bunu bölüm verisinin `game` alanından alır. Roboya Bal Peşinde'de hikâye sahnelerinde yönlendirici olarak kalır.
2. **Nesne görselleri renk ve şekille:** çiçeğin her rengi ayrı yaprak biçimidir (papatya, lale, yıldız, kalp, güneş ışını, yonca); bal damlası ve petek ayrı nesnelerdir. Yeşil meyve artık armut olarak çizilir (eskiden elma).
3. **Hedef rozeti (`GoalBadge`):** hedefte toplanacak nesneler tahtanın üstünde küçük resimlerle durur, toplandıkça parlar. Yazı yoktur. Çalıştırma başında (arı hariç) sıfırlanır; arıda bellek ve toplananlar kalıcı olduğu için rozet de kalıcıdır.
4. **Yön oku (`FacingArrow`):** robotun önünde zemine yatan, tahtanın perspektifine uyan ok. Kuzeye bakarken robot gövdesi örteceği için biraz daha uzağa konur. Tahta dışına taşarsa ve kutlama anında gizlenir.

## Sonuçlar

- Görseller geçicidir (Higgsfield); kalıcı sanat aynı dosya adlarıyla gelir.
- Kodlama Kutusu'nun masa görünümü ve hata avcısı akışı ayrı PR'dır.

## Güncelleme: Kodlama Kutusu tahtası ve hata avcısı akışı

- **Tahta:** Kodlama Kutusu bölümleri çim yerine ahşap kutu kapağı üzerinde oynanır (`ObliqueGround.SetBox`: ahşap yüzey, çerçeve, ahşap damarı); PRD'deki "masa oyunu görünümü".
- **Hata avcısı = izle → bul → düzelt:** bölüm hikâyesinden sonra hazır (yanlış) kod bir kez kendiliğinden çalışır ve ayak izi bırakarak yanlış yere varır (`LevelSession.Demo`, deneme sayılmaz, yıldız etkilenmez); Roboya "kutudan gelen kartlardan biri hatalı" der (`kodlama_kutusu.hunt`). Demo bitene kadar şerit kilitlidir. Hazır kartlar, çocuk değiştirene kadar ahşap çerçeve ve büyüteç simgesiyle ("kutudan") görünür; çocuğun koyduğu kart normal görünür. İpucu merdiveni (hatalı kartı vurgula) aynen çalışır.
