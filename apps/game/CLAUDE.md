# apps/game — Unity istemcisi

Kök `CLAUDE.md` §7 geçerlidir; burada yalnız ayrıntı vardır.

- Kod: `Assets/_Project/Scripts/<Assembly>/`. Her klasörde bir `.asmdef` bulunur.
- `Roboya.CodingEngine` saf C#'tır (`noEngineReferences: true`). Aynı kaynak `tools/engine-dotnet` altında .NET ile de derlenir ve test edilir (ADR 0002). Bu yüzden:
  - C# 9 ve .NET Standard 2.1 dışında dil özelliği/API kullanma.
  - `UnityEngine`, `System.Text.Json`, LINQ'ya bağımlı sıcak yol (hot path) yazma.
- `Scripts/CodingEngine/Levels/Generated/` elle düzenlenmez; `make gen` şemadan üretir.
- Sahneler: `Boot`, `Map`, `Game`. Kurulum `Assets/_Project/Editor/` altındaki editör betikleriyle yapılır; `.unity`/`.prefab` YAML'ına elle dokunma.
- UI: `Assets/_Project/UI/` altında UXML + USS. Metinler Unity Localization tablosundan anahtarla gelir.
- Testler: `Assets/_Project/Tests/EditMode` (motor testleri `dotnet test` ile de koşar) ve `PlayMode`.
- Proje ayarları ve sahneler `Assets/_Project/Editor/ProjectSetup.cs` ile uygulanır (menü: Roboya → Project Setup). Ayar değişikliği bu betikte yapılır.
- Paket ekleme/çıkarma ADR 0005'in güncellenmesini gerektirir.
- Komutlar: `make unity-test` (EditMode, batch), `make test-engine` (motor, .NET + kapsam ≥ %90).
