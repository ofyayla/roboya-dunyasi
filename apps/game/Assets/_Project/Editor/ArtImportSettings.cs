using UnityEditor;

namespace Roboya.EditorTools
{
    /// <summary>
    /// Every PNG under Assets/_Project/Art imports as a sprite with settings sized for 2 GB tablets:
    /// no mipmaps, 1024 px cap (2048 for full-screen backgrounds), compressed. Keeps import settings in code,
    /// not in hand-edited .meta files (CLAUDE.md §7).
    /// </summary>
    public sealed class ArtImportSettings : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/_Project/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot) || !assetPath.EndsWith(".png"))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            bool background = System.IO.Path.GetFileName(assetPath).StartsWith("bg_");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = !background;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = background ? 2048 : 1024;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.spritePixelsPerUnit = 256;
        }
    }
}
