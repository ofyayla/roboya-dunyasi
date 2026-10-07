# 0006 — Prototipte bölümlerin cihaza taşınması

- Durum: Kabul edildi (Faz 1'de Addressables ile yenilenecek)
- Tarih: 2026-10-08
- İlgili: F0-13, F0-21; OYN-07; plan "İçerik dağıtımı: Addressables + CDN"

## Bağlam

Bölümlerin tek kaynağı `content/levels` (CLAUDE.md §10). Unity projesi bu klasörün dışında. Çocuk testleri (F0-21) için cihaz derlemeleri bölümleri içermeli. Addressables + CDN, bölge paketleri mağaza güncellemesi olmadan yayınlanabilsin diye planlandı, ama prototipte tek bölge ve 10 bölüm var.

## Karar

- `ILevelSource` arayüzü bölüm JSON'larını sağlar; oyun kodu kaynağı bilmez.
- **Editörde** `FileLevelSource` doğrudan `content/levels` klasörünü okur: içerik değişikliği anında oyunda görünür, kopya yoktur.
- **Cihaz derlemesinde** `LevelContentBuildStep` (her derleme öncesi) bölümleri `StreamingAssets/levels` altına kopyalar ve bir `index.json` yazar. `StreamingAssetsLevelSource` bunları okur. Kopyalar Git'e girmez.
- Bozuk bir bölüm dosyası oyunu düşürmez; loglanır ve atlanır. Asıl koruma CI'daki `make validate-content`.

## Sonuçlar

- Faz 1'de (bölge paketleri, F1-05 ve sonrası) `AddressablesLevelSource` eklenir. Arayüz aynı kalır, oyun kodu değişmez.
- Prototip derlemesindeki bölümler ancak yeni derlemeyle güncellenir. Çocuk testleri için bu yeterli.
