# 0026 — Bal Peşinde: bellekli robot

- Durum: Kabul edildi
- Tarih: 2026-10-09
- İlgili: F1-02; BAL-01, BAL-02; ADR 0025

## Karar

1. **Mekanik `LevelSession`'a "durumu koru" kipi olarak eklendi** (`keepsState`, oyun `bal-pesinde` iken açılır). Arı çalışmadan sonra durduğu yerde kalır (`Robot`), toplanan nesneler toplanmış kalır, **bellek (plan şeridi) çalışmayla silinmez**. `Interpreter.Run(from)` zaten verilen durumdan başlıyordu; yeni motor kodu yok.
2. **Temizle (BAL-02):** `Temizle` düğmesi yalnız belleği siler, arı yerinde kalır. Temizlemeden yeni komut eklenip çalıştırılırsa arı eski komutları da çalıştırır; başarısızsa Roboya yargılamadan açıklar (`bal_pesinde.forgot_clear`). İpucu düğmesi de bu durumda aynı cümleyi söyler. Bu kipte kart düzeyinde ipucu (yanlış kartı vurgula) yoktur, çünkü bellekte eski komutlar bilinçli olarak durur.
3. **Bellek görünürlüğü:** Minik'te bellek görünmez: şerit boş sarı yuvarlaklar gösterir, yalnız sayı görünür. Kaşif'te `options.beeMemoryVisible: true` ile kartlar görünür.
4. **Yıldız:** diğer oyunlardaki formül; kart sayısı olarak çalıştırma anındaki bellek uzunluğu kullanılır, yani temizlemeyi unutmak yıldızı düşürebilir ama ceza yoktur.
5. **Bölümler:** renk eşleştirme, sayma ve toplama için `flower` / `honey` nesneleri (renk her zaman şekille birlikte). Şema değişmedi.

## Sonuçlar

- 4 yer tutucu bölüm (`bal-pesinde.01–04`, orders 15–18). Gerçek dizilim ve toplama/çıkarma görevleri F1-21'de pedagoji ekibiyle.
- Arı, yanlış bir köşeye sıkışırsa çıkış için geri ve dön kartları vardır; başa alma düğmesi yok (gerekirse F1-21'de eklenir).
- Çiçek/bal nesnelerinin son sanatı F1-23'te; şimdilik mevcut nesne çizimi.
