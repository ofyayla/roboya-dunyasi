# apps/level-editor — Bölüm editörü (iç araç)

- React + TypeScript + Vite; npm workspace adı `@roboya/level-editor`. Yalnız iç kullanım, çocuk verisi yok.
- Bölüm tipleri `@roboya/level-schema` paketinden gelir; elle tip yazma.
- Çözülebilirlik ve en kısa çözüm C# çözücüsünden gelir (ADR 0002); editör kendi çözücüsünü yazmaz.
- Kaydedilen dosyalar `content/levels/<bölge>/<oyun>/<id>.json`.
- Çalıştırma: `make editor-dev` → http://127.0.0.1:5174 (çözücü sunucusu 127.0.0.1:5199'da birlikte başlar).
- Dosya erişimi yalnız Vite geliştirme sunucusu eklentisinde (`src/server/contentApi.ts`) ve `content/levels` ile sınırlı; derlenmiş sürüme girmez.
- Bu araç ürün API'sini kullanmadığı için CLAUDE.md §9'daki "yalnız üretilmiş istemci" kuralı burada `fetch` ile yazılan yerel uçlara uygulanmaz.
- Ham ızgara düzenleme işlemleri `src/model/levelOps.ts` içinde saf fonksiyonlardır ve testlidir.
