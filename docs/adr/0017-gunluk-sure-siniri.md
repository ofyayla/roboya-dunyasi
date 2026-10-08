# 0017 — Günlük süre sınırı ve "şarj bitti" akışı

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F1-11; VEL-02; CLAUDE.md §13 (çocuk deneyimi), altın kural 1; ADR 0010, 0016

## Bağlam

Veli, çocuğunun günlük oyun süresini sınırlayabilmeli (VEL-02). Çocuk ekranında süre baskısı, sayaç, ceza ve yazı olmamalı (CLAUDE.md §13). Süre ayarı çocuk verisi değil, cihazdaki veli ayarıdır.

## Karar

1. **Seçenekler:** 10 / 15 / 20 / 30 dakika veya sınırsız. Veli seçmediyse yaşa göre öneri geçerlidir: Minik 10, Kaşif 15, Mucit 20 dakika. Seçenek listesi ve öneriler `ScreenTimeRules` içindedir (saf C#, motor).
2. **Sayım:** yalnız bir bölüm ekrandayken sayılır (harita ve veli alanı sayılmaz). Kare süresi en çok 1 sn sayılır; arka plandan dönüş süreyi şişirmez. Gün, cihazın yerel takvim günüdür; ertesi gün sıfırlanır.
3. **Saklama:** `progress/screentime.json` — profil kimliği başına seçilen sınır ve günlük saniye; 30 günden eski kayıtlar atılır, silinen profilin kaydı unutulur. Kişisel veri içermez; hesap açılsa bile sunucuya gönderilmez (cihaz ayarı).
4. **Süre dolunca:** başlamış bölüm bitirilebilir; yeni bölüm başlamaz. "Sonraki" ve "tekrar" haritaya döner, haritada patika taşı yerine **dinlenme ekranı** açılır: uykulu Roboya ve boş pil, kısa bir sesli cümle (`rest.battery_empty`). Ekranda yazı, sayaç, kırmızı uyarı yoktur; ceza yoktur (yıldız, ilerleme etkilenmez).
5. **Veli yolu:** dinlenme ekranındaki yetişkin düğmesi ebeveyn kilidini açar; veli alanında süre bölümü (seçenekler + bugünkü süre) bulunur. Sınır yükseltilince geri dönüşte ada açılır.
6. Süre sınırı **hak** değildir: ücretsiz ve premium aynı şekilde çalışır, sunucuya sorulmaz.

## Sonuçlar

- Cihaz saati değiştirilirse sınır aşılabilir; bu çocuğun yanlışlıkla aşmasını önleyen bir veli aracıdır, güvenlik sınırı değildir.
- Bildirim yok: "geri gel" mesajı gönderilmez (CLAUDE.md §13).
- Çoklu cihazda süre cihaz başınadır; hesaplar arası birleştirme gerekirse ayrı karar.
