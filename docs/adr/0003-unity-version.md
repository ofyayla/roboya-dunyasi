# 0003 — Unity sürümü

- Durum: Kabul edildi
- Tarih: 2026-10-07
- İlgili: F0-02, F0-20

## Bağlam

Kurallar "Unity 6 LTS" diyor. Geliştirici makinesinde Unity 6.6 (6000.6.4f1) kurulu. Unity 6 için LTS sürümleri 6.0 (desteği Ekim 2026'da bitiyor) ve 6.3'tür; sıradaki LTS'nin 6.7 olması bekleniyor. Lansman yaklaşık 9 ay sonra.

## Karar

- Faz 0 prototipi Unity **6000.6.4f1** ile başlar. 6.6'dan 6.7 LTS'ye geçiş küçük bir sürüm atlamasıdır; 6.3 LTS'ye geri inmek ise proje dosyalarını eski sürüme çevirmeyi gerektirir.
- 6.7 LTS yayınlandığında (en geç Faz 1 başında) projeye geçilir ve bu ADR güncellenir. Lansman bir LTS sürümüyle yapılır.
- CI'daki GameCI imajı `ProjectVersion.txt` ile aynı sürüme sabitlenir.

## Sonuçlar

- Faz 0 boyunca LTS olmayan sürümde olası editör hataları kabul edilir.
- iOS ve Android derleme modülleri Unity Hub'dan eklenmelidir (F0-02 öncesi).
