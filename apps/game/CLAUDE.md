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
- Oyun kuralları (plan şeridi, oturum, ipucu kademesi, yıldız) `CodingEngine/Play` ve `CodingEngine/Scoring` altında saf C#'tır; oyun assembly'leri yalnız görünüm ve akış içerir.
- Bileşim kökü `Roboya.Core.Bootstrap` (Boot sahnesi). Sahneler `ISceneEntry` ile servisleri alır; singleton yazılmaz.
- Çocuk ekranlarında yazı yok. Görseller bölgenin `RegionArt` sprite setinden gelir; eksik sprite için `Roboya.UI.Icon` kodla çizilmiş yedeği kullanılır. Oyun tahtası `ObliqueProjection` ile eğik çizilir. Bölüm başı ve sonu hikâye sahneleri `StoryStage` ile kodla kurulur; veri bölüm JSON'undaki `story` alanındadır (bkz. `docs/art/style-guide.md`).
- Komutlar: `make unity-test` (EditMode, batch), `make unity-playmode` (kritik akışlar + ekran görüntüleri), `make test-engine` (motor, .NET + kapsam ≥ %90).
