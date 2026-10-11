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

        // Bump when the settings below change so Unity reimports art that was already imported with the old ones.
        public override uint GetVersion() => 2;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot) || !assetPath.EndsWith(".png"))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            bool background = System.IO.Path.GetFileName(assetPath).StartsWith("bg_");
            importer.textureType = TextureImporterType.Sprite;
            // A fresh import can guess "cube" for some pictures, and a cube texture makes no sprite at all (ship_engine_core had none).
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = !background;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = background ? 2048 : 1024;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.spritePixelsPerUnit = 256;
        }
    }
}
