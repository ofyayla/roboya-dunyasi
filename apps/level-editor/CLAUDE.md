# apps/level-editor — Bölüm editörü (iç araç)

- React + TypeScript + Vite; npm workspace adı `@roboya/level-editor`. Yalnız iç kullanım, çocuk verisi yok.
- Bölüm tipleri `@roboya/level-schema` paketinden gelir; elle tip yazma.
- Çözülebilirlik ve en kısa çözüm C# çözücüsünden gelir (ADR 0002); editör kendi çözücüsünü yazmaz.
- Kaydedilen dosyalar `content/levels/<bölge>/<oyun>/<id>.json`.
