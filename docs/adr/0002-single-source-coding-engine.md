# 0002 — Kodlama motoru ve çözücü için tek kaynak

- Durum: Kabul edildi
- Tarih: 2026-10-07
- İlgili: F0-06…F0-12, OYN-07

## Bağlam

Çözücü (çözülebilirlik, en kısa çözüm) üç yerde gerekiyor: oyun (yıldız hesabı, akıllı ipucu), içerik doğrulayıcı (CI) ve bölüm editörü (web). İki dilde ayrı çözücü yazmak zamanla farklı sonuç üretir; bir bölüm editörde "çözülebilir" görünüp oyunda çözülemeyebilir.

## Karar

- Kodlama motoru (`Roboya.CodingEngine`) Unity projesinde `apps/game/Assets/_Project/Scripts/CodingEngine/` altında yaşar, `noEngineReferences: true`.
- Aynı kaynak dosyalar `tools/engine-dotnet/` altındaki SDK tarzı projelerle .NET'te de derlenir:
  - `Roboya.CodingEngine` (netstandard2.1, C# 9 — Unity ile aynı dil düzeyi),
  - `Roboya.CodingEngine.Tests` (NUnit; Unity EditMode testleriyle aynı dosyalar),
  - `Roboya.LevelValidator` (komut satırı + editör için yerel HTTP `serve` modu).
- Bölüm editörü çözülebilirlik sorgusunu Vite geliştirme sunucusunun vekil (proxy) ayarı üzerinden `LevelValidator serve`'e yapar.
- Kapsam ölçümü .NET tarafında (coverlet) yapılır; ≥ %90 eşiği CI'da zorunludur.

## Sonuçlar

- Motor kodunda Unity'ye özgü API ve C# 9 üstü dil özelliği kullanılamaz; derleme bunu yakalar.
- Testler hem `dotnet test` (hızlı, her PR) hem Unity EditMode (gerçek ortam) ile koşar.
- Bölüm JSON'u okumak için motor `Newtonsoft.Json` kullanır (Unity'de `com.unity.nuget.newtonsoft-json`, .NET'te NuGet paketi).
