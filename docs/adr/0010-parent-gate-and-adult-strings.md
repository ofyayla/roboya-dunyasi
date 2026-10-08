# 0010 — Ebeveyn kilidi ve yetişkin ekranı metinleri

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F1-09; VEL-01, UYM-07, GLR-01; CLAUDE.md kural 7 ve §11; ADR 0005

## Bağlam

- Satın alma, ayarlar, dış bağlantılar ve veli alanı ebeveyn kilidinin arkasında olmalı (VEL-01, UYM-07). PRD yöntemi: "rastgele üretilen basit bir işlem veya şu sayıları sırayla yaz".
- Yetişkin ekranlarında ilk kez yazı gösteriyoruz. Kural 7: kullanıcıya görünen metin yerelleştirme tablosundan gelir, kodda sabit Türkçe metin olmaz. Çocuk ekranlarında yazı yok.
- ADR 0005 `com.unity.localization` paketini listelemişti, ancak şimdiye kadar kullanılmadı.

## Karar

1. **Ebeveyn kilidi:** dört rastgele rakam gösterilir, yetişkin bunları **tersten** yazar. Rakamlar komşu tekrar içermez. Üç yanlış cevapta tuş takımı 30 saniye kilitlenir. Her denemede yeni rakamlar üretilir.
   - Küçük çocuklar bunu yapamaz; yetişkin birkaç saniyede yapar. Amaç caydırıcılık, güvenlik duvarı değil.
   - Kurallar saf C#'ta (`CodingEngine/Parents/ParentGate`), zaman ve rastgelelik dışarıdan verilir, testlidir.
   - Dokunma hedefleri 72 birim (yetişkin ekranında en az 48 dp).
2. **Giriş noktası:** ada ekranının köşesindeki "yetişkin" simgesi. Doğrudan veli alanını açmaz, kilidi açar. Veli alanı (`ParentView`) bir kabuktur; süre sınırı, ilerleme raporu, gizlilik merkezi ve abonelik bölümleri sonraki işlerde buraya eklenir ve hepsi aynı kilidin arkasında kalır.
3. **Yetişkin metinleri `content/localization/tr.json` dosyasından anahtarla okunur** (`LocalizedStrings`). Dosya uygulamayla birlikte StreamingAssets'e kopyalanır.
   - Eksik anahtar `[anahtar]` olarak görünür ve bir kez uyarı verir; bir test kullanılan tüm anahtarların tabloda bulunduğunu doğrular.
   - Unity Localization paketi şimdilik kullanılmıyor. Neden: anahtar tabloları editör varlığı olarak üretilmesi ve Addressables bağlantısı gerekiyor; ihtiyacımız tek dilli, küçük bir tablo. Anahtarlar aynı kaldığı için ikinci dil geldiğinde pakete geçmek yalnız `LocalizedStrings`'i değiştirir.
   - Çocuk ekranlarının sesleri aynı ilkeyle `content/voice/script.csv` anahtarlarından gelir.

## Sonuçlar

- `apps/game/CLAUDE.md` "Unity Localization tablosu" ifadesi, bu kararın geçerliliğiyle güncellendi.
- İkinci dil (İngilizce, F-sonrası) geldiğinde `tr.json` yanına dil dosyaları eklenir.
- Paket listesinde kullanılmayan `com.unity.localization` ileride kaldırılabilir (ADR 0005 güncellemesi gerekir).
