Yeni bir bölüm JSON'u oluştur: $ARGUMENTS

1. `packages/level-schema/level.schema.json` ve `content/levels/README.md` dosyalarını oku; aynı oyundaki mevcut bölümlere bak.
2. Bölümü `content/levels/<bölge>/<oyun>/<id>.json` olarak yaz. Meta veride kavram, değer, zorluk ve yaş seviyesi zorunlu.
3. Yönerge ses anahtarlarını `content/voice/script.csv` dosyasına ekle (`key,text,context,level_ids`). Kodda sabit metin yok.
4. `make validate-content` çalıştır; en kısa çözüm uzunluğunu ve çözülebilirliği raporla.
