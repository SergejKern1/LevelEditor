using System.Collections.Generic;
using System.IO;
using Core.Types;
using Editor.AssetProcessors;
using Level.Texture;
using UnityEditor;
using UnityEngine;
using static Level.Texture.Utility;

namespace Editor.Level.Texture
{
    public static class AtlasGeneration
    {
        static string TextureAtlasPath(Object config, string textureDirectory, EAtlasType type) => 
            $"{textureDirectory}{config.name}{type}Atlas.png";

        public static void GenerateAtlas(LevelTextureConfig config, string materialPath, string textureDirectory)
        {
            EditorUtility.SetDirty(config);

            //AddMaskToAtlas(atlas, GroundTextureAtlasPath, target.MaskTextureCollections, out target.MaskUVInset, out target.MaskUVFactor, out target.MaskWrapValue);
            //var newAtlas =
            config.Material = LoadOrCreateMaterial(materialPath);

            for (var i = 0; i < (int)EAtlasType.Length; i++)
            {
                if (!config.ActiveMaps[i])
                    continue;
                var atType = (EAtlasType)i;
                var atlas = GenerateAtlas(config, atType);
                EditorUtility.DisplayProgressBar("Generating Atlas", "Saving Atlas File", 1f);

                var atlasPath = TextureAtlasPath(config, textureDirectory, atType);
                ApplyAndWriteTextureToPath(atlas, atlasPath);
                Object.DestroyImmediate(atlas);
                config.Atlas[i] = AssetDatabase.LoadMainAssetAtPath(atlasPath) as Texture2D;
                UpdateMaterial(config.Material, config.Atlas[i], KTexNames[i]);
            }

            EditorUtility.ClearProgressBar();

            // todo validation stuff
            //int newHash = setting.GetHash();
            //if (setting.GeneratedWithHash!= newHash)
            //    _HashChanged = true;
            //setting.GeneratedWithHash = newHash;

            config.UsedTexturePixelHashes.Clear();
            GetTextureHashes(config.LevelTextureCollections, config.UsedTexturePixelHashes);

            //GetTextureHashes(target.MaskTextureCollections, target.UsedTexturePixelHashes);

            EditorGUIUtility.PingObject(config.Material);
        }

        static void GetTextureHashes(IEnumerable<TextureCollection> collections, ICollection<string> result)
        {
            foreach (var collection in collections)
            {
                foreach (var dat in collection)
                {
                    var tex = dat.GetTexture();
                    var hash = LevelTexturePreprocessor.GetHash(tex);
                    result.Add(hash);
                }
            }
        }

        static Material LoadOrCreateMaterial(string path)
        {
            var mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
            if (mat != null)
                return mat;

            var stdShader = Shader.Find("Standard");
            var shader = Shader.Find("Editor/DungeonEditor");

            mat = new Material( shader == null? stdShader : shader);
            AssetDatabase.CreateAsset(mat, path);

            mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
            return mat;
        }

        static void UpdateMaterial(Material mat, UnityEngine.Texture groundTex, string texName)
        {
            var currentGroundTex = mat.GetTexture(texName);

            if (currentGroundTex == groundTex)
                return;

            EditorUtility.SetDirty(mat);
            mat.SetTexture(texName, groundTex);
            AssetDatabase.SaveAssets();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        static Texture2D ApplyAndWriteTextureToPath(Texture2D texture, string path)
        {
            texture.Apply();
            var bytes = texture.EncodeToPNG();

            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (IOException e)
            {
                Debug.LogError($"IO exception while writing atlas texture: {e}");
            }

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path);
            return (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        }

        const int k_repeatedPixels = 8;
        struct CopyTextureToAtlasData
        {
            public Texture2D Texture;
            public Texture2D Atlas;
            public int X;
            public int Y;
        }
        struct CopyQuadrantData
        {
            public int X;
            public int Y;
            public int CopyDirX;
            public int CopyDirY;
        }
        struct RepeatedPositions
        {
            public int RepeatedXRead;
            public int RepeatedYRead;
            public int RepeatedXWrite;
            public int RepeatedYWrite;
        }

        static void CopyQuadrantToAtlas(CopyTextureToAtlasData data, int quadX, int quadY, int copyDirectionX, int copyDirectionY)
            => CopyQuadrantToAtlas(data, new CopyQuadrantData()
            {
                CopyDirY = copyDirectionY,
                CopyDirX = copyDirectionX,
                X = quadX,
                Y = quadY
            });

        static void CopyQuadrantToAtlas(CopyTextureToAtlasData data, CopyQuadrantData quadrant)
        {
            var offset = new Vector2Int(data.X + k_repeatedPixels, data.Y + k_repeatedPixels);
            var halfSize = new Vector2Int(data.Texture.width / 2, data.Texture.height / 2);
            var readPos = new Vector2Int(halfSize.x * quadrant.X, halfSize.y * quadrant.Y);

            var pixels = data.Texture.GetPixels(readPos.x, readPos.y, halfSize.x, halfSize.y);

            GetRepeatedReadWritePositions(data, quadrant, readPos, offset, halfSize, out var rp);

            var pixelsRepeatedX = data.Texture.GetPixels(rp.RepeatedXRead, readPos.y, k_repeatedPixels, halfSize.y);
            var pixelsRepeatedY = data.Texture.GetPixels(readPos.x, rp.RepeatedYRead, halfSize.x, k_repeatedPixels);
            var pixelsRepeatedXY = data.Texture.GetPixels(rp.RepeatedXRead, rp.RepeatedYRead, k_repeatedPixels, k_repeatedPixels);

            data.Atlas.SetPixels(offset.x + readPos.x, offset.y + readPos.y, halfSize.x, halfSize.y, pixels);
            data.Atlas.SetPixels(rp.RepeatedXWrite, offset.y + readPos.y, k_repeatedPixels, halfSize.y, pixelsRepeatedX);
            data.Atlas.SetPixels(offset.x + readPos.x, rp.RepeatedYWrite, halfSize.x, k_repeatedPixels, pixelsRepeatedY);
            data.Atlas.SetPixels(rp.RepeatedXWrite, rp.RepeatedYWrite, k_repeatedPixels, k_repeatedPixels, pixelsRepeatedXY);
        }

        static void GetRepeatedReadWritePositions(CopyTextureToAtlasData data, CopyQuadrantData quad,
            Vector2Int readPos, Vector2Int offset, Vector2Int halfSize, out RepeatedPositions repeatPos)
        {
            repeatPos.RepeatedXRead = readPos.x;
            repeatPos.RepeatedYRead = readPos.y;

            repeatPos.RepeatedXWrite = offset.x + readPos.x + (data.Texture.width * quad.CopyDirX);
            repeatPos.RepeatedYWrite = offset.y + readPos.y + (data.Texture.height * quad.CopyDirY);

            if (quad.CopyDirX == -1)
            {
                repeatPos.RepeatedXWrite += (halfSize.x - k_repeatedPixels);
                repeatPos.RepeatedXRead += (halfSize.x - k_repeatedPixels);
            }

            // ReSharper disable once InvertIf
            if (quad.CopyDirY == -1)
            {
                repeatPos.RepeatedYWrite += (halfSize.y - k_repeatedPixels);
                repeatPos.RepeatedYRead += (halfSize.y - k_repeatedPixels);
            }
        }

        static void CopyTextureToAtlas(Texture2D tex, Texture2D outputTexture, int x, int y)
        {
            var data = new CopyTextureToAtlasData()
            {
                Texture = tex,
                Atlas = outputTexture,
                X = x,
                Y = y
            };
            CopyTextureToAtlas(data);
        }
        static void CopyTextureToAtlas(CopyTextureToAtlasData data)
        {
            CopyQuadrantToAtlas(data, 0, 0, 1, 1);
            CopyQuadrantToAtlas(data, 1, 0, -1, 1);
            CopyQuadrantToAtlas(data, 0, 1, 1, -1);
            CopyQuadrantToAtlas(data, 1, 1, -1, -1);
        }

        struct GenerateAtlasData
        {
            public EAtlasType AtlasType;
            public Texture2D OutputTexture;
            public Vector2Int AtlasSize;
            public Vector2Int CurrentDim;
            public int TileWidthHeight;
            public int X;
            public int Y;
            public int CurrentRowYEnd;
        }

        // ReSharper disable once FlagArgument
        static Texture2D GenerateAtlas(LevelTextureConfig config, EAtlasType atlasType)
        {
            var textureCollections = config.LevelTextureCollections;
            var tileWidthAndHeight = 2 * config.HalfTileDimension;
            var atlasSize = new Vector2Int(config.AtlasDimension, config.AtlasDimension);

            //int widthAndHeight = texWidthAndHeight + (repeatedPixels * 2);
            var outputTexture = new Texture2D(config.AtlasDimension, config.AtlasDimension, TextureFormat.ARGB32, false);

            config.UVInset = (float) k_repeatedPixels / config.AtlasDimension;
            config.UVTileSize = (float) tileWidthAndHeight / atlasSize.x;

            var allTexData = GatherAllTextures(textureCollections, config);

            var data = new GenerateAtlasData()
            {
                AtlasType = atlasType,
                OutputTexture = outputTexture,
                CurrentDim = Vector2Int.zero,
                AtlasSize = atlasSize,
                TileWidthHeight = tileWidthAndHeight,
                CurrentRowYEnd = -1,
                X = 0,
                Y = 0
            };
            //currentRowYStart = 0,
            //blockXStart = 0,
            //blockXEnd = -1,
            //blockX = 0,
            //blockY = 0;

            var progressIndex = 0;
            var initialCount = allTexData.Count;

            while (allTexData.Count > 0)
            {
                EditorUtility.DisplayProgressBar("Generating Atlas", $"{progressIndex}/{initialCount}", (float)(progressIndex++) / initialCount);
                var current = allTexData[0];

                if (GenerateAtlasProcessTexture(current, ref data) == OperationResult.Error)
                    break;
                allTexData.RemoveAt(0);
            }

            EditorUtility.ClearProgressBar();
            return outputTexture;
        }

        static OperationResult GenerateAtlasProcessTexture(TextureData current, ref GenerateAtlasData data)
        {
            if (data.CurrentRowYEnd != -1 && data.CurrentDim.y != current.TileDimension().y)
            {
                data.X = 0;
                data.Y += (data.CurrentDim.y * (2 * k_repeatedPixels + data.TileWidthHeight));
                data.CurrentRowYEnd = -1;
            }

            if (data.CurrentRowYEnd == -1)
            {
                data.CurrentDim = current.TileDimension();
                data.CurrentRowYEnd = data.Y + (data.CurrentDim.y * (2 * k_repeatedPixels + data.TileWidthHeight));

                if (data.CurrentRowYEnd > data.AtlasSize.y)
                {
                    Debug.LogError("Atlas cannot be created because you run out of space!");
                    return OperationResult.Error;
                }
            }

            data.CurrentDim.x = current.TileDimension().x;

            // copy texture
            var tex = current.GetTexture(data.AtlasType);
            if (tex != null)
                CopyTextureToAtlas(tex, data.OutputTexture, data.X, data.Y);

            var uVOffset = new Vector2((float)data.X / data.AtlasSize.x, (float)data.Y / data.AtlasSize.y);
            if (data.AtlasType == EAtlasType.Default)
                current.UVOffset = uVOffset;

            data.X += data.CurrentDim.x * (2 * k_repeatedPixels + data.TileWidthHeight);

            var nextXEnd = data.X + (data.CurrentDim.x * (2 * k_repeatedPixels + data.TileWidthHeight));
            if (nextXEnd <= data.AtlasSize.x)
                return OperationResult.OK;

            data.X = 0;
            data.Y = data.CurrentRowYEnd;
            data.CurrentRowYEnd = -1;

            return OperationResult.OK;
        }

        static List<TextureData> GatherAllTextures(IEnumerable<TextureCollection> textureCollections, LevelTextureConfig setting)
        {
            var allTexData = new List<TextureData>();
            foreach (var texCollection in textureCollections)
            {
                allTexData.AddRange(texCollection.FloorVariations);
                allTexData.AddRange(texCollection.WallVariations);
                if (texCollection.Edge.IsSet())
                    allTexData.Add(texCollection.Edge);
                if (texCollection.DiagEdge.IsSet())
                    allTexData.Add(texCollection.DiagEdge);
            }

            foreach (var trans in setting.Transitions)
                allTexData.Add(trans);

            allTexData.Sort(SortBiggestDimensionFirst);
            return allTexData;
        }

        //static void CopyQuadrantToAtlas(Texture2D texture, Texture2D atlas, int halfTexWidthAndHeight,
        //int quadX, int quadY, int toX, int toY)
        //{
        //    Color[] pixels = texture.GetPixels(quadX, quadY, halfTexWidthAndHeight + repeatedPixels, halfTexWidthAndHeight + repeatedPixels);
        //    atlas.SetPixels(toX, toY, halfTexWidthAndHeight + repeatedPixels, halfTexWidthAndHeight + repeatedPixels, pixels);
        //}
        //static void CopyTextureToAtlas(Texture2D texture, Texture2D atlas, int halfTexWidthAndHeight, int xOff, int yOff)
        //{
        //    CopyQuadrantToAtlas(texture, atlas, halfTexWidthAndHeight, 0, 0, xOff + halfTexWidthAndHeight, yOff + halfTexWidthAndHeight);
        //    CopyQuadrantToAtlas(texture, atlas, halfTexWidthAndHeight, halfTexWidthAndHeight - repeatedPixels, 0, xOff - repeatedPixels, yOff + halfTexWidthAndHeight);
        //    CopyQuadrantToAtlas(texture, atlas, halfTexWidthAndHeight, 0, halfTexWidthAndHeight - repeatedPixels, xOff + halfTexWidthAndHeight, yOff - repeatedPixels);
        //    CopyQuadrantToAtlas(texture, atlas, halfTexWidthAndHeight, halfTexWidthAndHeight - repeatedPixels, halfTexWidthAndHeight - repeatedPixels, xOff - repeatedPixels, yOff - repeatedPixels);
        //}
        //static Texture2D GenerateAtlas(List<TextureCollection> textureCollections, out Vector2 uvInset, out Vector2 uvFactor, out float wrapValue)
        //{
        //    int texWidthAndHeight = -1;
        //    int maxVariations = 0;
        //    int texCount = 0;
        //    foreach (var texCollection in textureCollections)
        //    {
        //        maxVariations = Mathf.Max(maxVariations, texCollection.Count());
        //        foreach (TextureCollection.Data dat in texCollection)
        //        {
        //            if (texWidthAndHeight == -1)
        //                texWidthAndHeight = dat.DiffuseTex.width;
        //            texCount++;
        //        }
        //    }
        //    int halfTexWidthAndHeight = texWidthAndHeight / 2;
        //    int widthAndHeight = texWidthAndHeight + (repeatedPixels * 2);
        //    int horTexCount = Mathf.CeilToInt(Mathf.Sqrt(texCount));
        //    int atlasWidth = 2;
        //    while (atlasWidth < horTexCount * widthAndHeight)
        //    {
        //        atlasWidth *= 2;
        //        while ((horTexCount + 1) * widthAndHeight <= atlasWidth)
        //        {
        //            horTexCount++;
        //        }
        //    }
        //    wrapValue = (float)(horTexCount * widthAndHeight) / atlasWidth;
        //    if (wrapValue < 0.1f)
        //    {
        //        atlasWidth = horTexCount * widthAndHeight;
        //        wrapValue = 1.0f;
        //        Debug.LogWarning("Failed to make TextureAtlas power of 2!");
        //    }
        //    int atlasHeight = atlasWidth;
        //    Texture2D outputTexture = new Texture2D(atlasWidth, atlasHeight, TextureFormat.ARGB32, false);
        //    uvInset = new Vector2((float)repeatedPixels / atlasWidth, (float)repeatedPixels / atlasHeight);
        //    uvFactor = new Vector2((float)widthAndHeight / atlasWidth, (float)widthAndHeight / atlasHeight);
        //    int x = repeatedPixels;
        //    int y = repeatedPixels;
        //    int progressIndex = 0;
        //    foreach (var texCollection in textureCollections)
        //    {
        //        EditorUtility.DisplayProgressBar("Generating Atlas", texCollection.Name, (float)(progressIndex++) / textureCollections.Count);
        //        texCollection.UVOffset = new Vector2((float)(x - repeatedPixels) / atlasWidth, (float)(y - repeatedPixels) / atlasHeight);
        //        foreach (TextureCollection.Data dat in texCollection)
        //        {
        //            var tex = dat.DiffuseTex;
        //            CopyTextureToAtlas(tex, outputTexture, halfTexWidthAndHeight, x, y);
        //            x += widthAndHeight;
        //            if (x + widthAndHeight > atlasWidth)
        //            {
        //                x = repeatedPixels;
        //                y += widthAndHeight;
        //            }
        //        }
        //    }
        //    EditorUtility.ClearProgressBar();
        //    return outputTexture;
        //}

        //static Texture2D AddMaskToAtlas(Texture2D atlas, string outputPath, List<TextureCollection> textureCollections, out Vector2 uvInset, out Vector2 uvFactor, out float wrapValue)
        //{
        //    int texWidthAndHeight = -1;
        //    int maxVariations = 0;
        //    int texCount = 0;
        //    foreach (var texCollection in textureCollections)
        //    {
        //        maxVariations = Mathf.Max(maxVariations, texCollection.Textures.Count);
        //        for (int i = 0; i < texCollection.Textures.Count; i++)
        //        {
        //            if (texWidthAndHeight == -1)
        //                texWidthAndHeight = texCollection.Textures[i].width;
        //            texCount++;
        //        }
        //    }
        //    //int halfTexWidthAndHeight = texWidthAndHeight / 2;
        //    int widthAndHeight = texWidthAndHeight;
        //    int horTexCount = Mathf.FloorToInt((float)atlas.width / texWidthAndHeight);
        //    int atlasWidth = atlas.width;
        //    wrapValue = (float)(horTexCount * widthAndHeight) / atlasWidth;
        //    int atlasHeight = atlasWidth;
        //    uvInset = Vector3.zero;
        //    uvFactor = new Vector2((float)widthAndHeight / (float)atlasWidth, (float)widthAndHeight / (float)atlasHeight);
        //    if (texCount > (horTexCount * horTexCount))
        //    {
        //        Debug.LogError("GroundTextureSettingsEditor.AddMaskToAtlas(): More Mask then Ground Space needed!");
        //        return null;
        //    }
        //    int x = 0;
        //    int y = 0;
        //    int progressIndex = 0;
        //    foreach (var texCollection in textureCollections)
        //    {
        //        EditorUtility.DisplayProgressBar("Adding Mask to Atlas", texCollection.Name, (float)(progressIndex++) / textureCollections.Count);
        //        texCollection.UVOffset = new Vector2((float)x / (float)atlasWidth, (float)y / (float)atlasHeight);
        //        for (int j = 0; j < texCollection.Textures.Count; j++)
        //        {
        //            Color[] maskPixels = texCollection.Textures[j].GetPixels();
        //            Color[] originalPixels = atlas.GetPixels(x, y, widthAndHeight, widthAndHeight);
        //            if (maskPixels.Length!= originalPixels.Length)
        //            {
        //                Debug.LogError("GroundTextureSettingsEditor.AddMaskToAtlas(): Unexpected array length!");
        //                return null;
        //            }
        //            for (int i = 0; i < originalPixels.Length; ++i)
        //            {
        //                originalPixels[i].a = maskPixels[i].g;
        //            }
        //            atlas.SetPixels(x, y, widthAndHeight, widthAndHeight, originalPixels);
        //            x += widthAndHeight;
        //            if (x + widthAndHeight > atlasWidth)
        //            {
        //                x = 0;
        //                y += widthAndHeight;
        //            }
        //        }
        //    }
        //    EditorUtility.DisplayProgressBar("Adding Mask to Atlas", "Saving Atlas File", 1f);
        //    var newAtlas = ApplyAndWriteTextureToPath(atlas, outputPath);
        //    DestroyImmediate(atlas);
        //    return newAtlas;
        //}
    }
}
