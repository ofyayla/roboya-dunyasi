# 0023 — Gizlilik merkezi (istemci)

- Durum: Kabul edildi (insan incelemesi gerekli: veri silme ve rıza)
- Tarih: 2026-10-09
- İlgili: F1-13 (istemci), UYM-01, UYM-03; ADR 0015, 0016, 0019, 0020; CLAUDE.md §11

## Karar

Veli alanında (ebeveyn kilidinin arkasında) "Gizlilik merkezi" bölümü:

1. **Aydınlatma metnini oku:** yalnız okunur görünüm (`NoticeReader`), rıza düğmesi yok.
2. **Rızayı geri çek:** iki dokunuş. Rıza hemen yerelde kaldırılır; bundan sonra cihazdan çocuğa dair hiçbir şey gönderilmez (olay kuyruğu da silinir, ADR 0022) ve oyun karşılama ekranına döner; yeniden onay verilene kadar açılmaz. Giriş yapılmışsa sunucuya `DELETE /v1/me/consents` gider; ulaşılamazsa `privacy.json` işareti kalır ve sonraki açılışta tekrar denenir.
3. **Bu cihazdaki tüm verileri sil:** iki dokunuş. Tüm çocuk profilleri ve ilerleme dosyaları silinir, ardından eşitleme sunucudaki profil kopyalarını siler, sonra rıza geri çekilir. Silme eşitlemesi rıza gerektirmez (ADR 0020 güncellendi: silme, rıza kontrolünden önce çalışır).
4. **Sunucu işlemleri** (yalnız giriş yapılmışsa): hesapta tutulan verinin özeti (`GET /v1/me/data`), tam dışa aktarım dosyası (uygulama klasörüne `roboya-verilerim.json`), hesap silme talebi (iki dokunuş; sunucu bekleme süresi tanır) ve talepten vazgeçme.
5. Hata iletileri `tr.json` tablosundan; kısa cümleler.

## Sonuçlar

- Dışa aktarılan dosya uygulamanın özel klasörüne yazılır; paylaşım menüsü (iOS/Android yerel) ayrı iş olarak kaldı. Veli dosyayı şimdilik cihaz bağlantısıyla alabilir.
- Hesap silme talebi sunucuda 7 gün bekler (ADR 0015); istemci kendi yerel verisini bu talep sırasında silmez, veli isterse "cihazdaki verileri sil" ile ayrıca siler.
- Çevrimdışıyken rıza geri çekme ve cihaz silme sorunsuz çalışır; sunucu kopyası bağlanınca silinir.
