# 0025 — Ortak oyun ekranı ve Kodlama Kutusu

- Durum: Kabul edildi
- Tarih: 2026-10-09
- İlgili: F1-01; KUT-01, KUT-02; PRD "Sabır Ormanı"; ADR 0009; CLAUDE.md §7

## Bağlam

Sabır Ormanı üç oyundan oluşur (Yön Avcısı, Kodlama Kutusu, Bal Peşinde) ve bölgenin tek patikasında iç içe dizilir (doğrulayıcı kuralı: bölge içinde `order` benzersiz). Üç oyun aynı kart şeridi, tahta, hikâye sahnesi, ipucu ve yıldız akışını kullanır; oyunlar arasındaki fark bölüm verisinde ve küçük kurallardadır.

## Karar

1. `Roboya.Games.YonAvcisi` assembly'si **`Roboya.Games.Common`** oldu (ekran `GameScreen`, denetleyici `GameController`). Tek Game sahnesi seçilen bölümün bölgesinin patikasındaki **tüm bölümleri** yükler; "sonraki" bölüm oyun değişse bile patikayı izler. Oyuna özgü davranış bölüm verisiyle (`starterProgram`, `options`, `goal`) ve gerektiğinde küçük genişletme noktalarıyla gelir; motor çatallanmaz.
2. **Kodlama Kutusu** bu temelin üzerinde:
   - **Hata avcısı (KUT-01):** `starterProgram` yanlış kartlı hazır kodu şeride yükler; çocuk yanlış kartı bulup çıkarır/değiştirir. İpucu merdiveni (sesli → hatalı kartı vurgula → doğru kartı göster) en kısa çözüme göre yanlış kartı zaten bulur.
   - **İz bırakan yürütme (KUT-02):** koşarken ayak izi çizilir (`BoardView` izi); hayalet yol planlanan rotayı gösterir. Kodlama Kutusu'nda çalıştırma bitince izler 2,5 sn (diğer oyunlarda 0,8 sn) kalır ki çocuk karşılaştırabilsin.
3. Patika ilerleme noktaları en çok 9 nokta pencere olarak çizilir (patika 36 bölüme çıkacak).
4. **Şema değişmedi** (v2): oyun ayrımı için yeni alan eklenmedi.

## Sonuçlar

- 4 örnek Kodlama Kutusu bölümü (`sabir-ormani.kodlama-kutusu.01–04`, orders 11–14) ve sesleri eklendi: 2 sıralama, 2 hata avcısı (Minik ve Kaşif). **Bunlar yer tutucu dizilimdir:** 36 bölümün gerçek sırası, kavram ilerlemesi ve Minik/Kaşif ayrımı F1-21'de pedagoji ekibiyle belirlenir; Yön Avcısı 10. bölüm metni ("bölge finali") bölge sonu yeniden belirlenince değişecek.
- "Masa oyunu" görünümü (PRD) sanat işidir (F1-23); şimdilik Yön Avcısı tahtası.
- Bal Peşinde (bellekli robot) motor değişikliği gerektirir; ayrı PR (F1-02).
