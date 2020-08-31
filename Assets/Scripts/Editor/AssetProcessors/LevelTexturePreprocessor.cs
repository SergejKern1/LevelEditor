using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.AssetProcessors
{
    public class LevelTexturePreprocessor : AssetPostprocessor
    {
        static readonly List<string> k_recentlyImportedTextures = new List<string>();

        public static IEnumerable<Texture2D> PullRecentlyImportedTextures()
        {
            var texList = k_recentlyImportedTextures.Select(AssetDatabase.LoadAssetAtPath<Texture2D>).ToList();

            k_recentlyImportedTextures.Clear();
            return texList;
        }

        public static string GetHash(Texture2D texture)
        {
            var bytes = texture.GetRawTextureData();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(bytes);
                var hashStr = System.BitConverter.ToString(hash);
                return hashStr;
            }
        }

        bool IsInLevelTextureFolder()
        {
            var dirName = System.IO.Path.GetDirectoryName(assetPath);
            return string.Equals(dirName, "Assets/Art/LevelTextures", System.StringComparison.Ordinal);
        }

        void OnPreprocessTexture()
        {
            if (!IsInLevelTextureFolder())
                return;

            var textureImporter = (TextureImporter)assetImporter;
            textureImporter.isReadable = true;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        }

        void OnPostprocessTexture(Texture2D _)
        {
            if (!IsInLevelTextureFolder())
                return;

            if (!k_recentlyImportedTextures.Contains(assetPath))
                k_recentlyImportedTextures.Add(assetPath);
        }
    }
}