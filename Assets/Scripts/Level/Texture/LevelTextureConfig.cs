using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Level.Tiles.Interface;

namespace Level.Texture
{
    [CreateAssetMenu(menuName = "Level/TextureConfig")]
    public class LevelTextureConfig : ScriptableObject, ITileConfig
    {
        public List<TextureCollection> LevelTextureCollections = new List<TextureCollection>();
        public List<TransitionTextureData> Transitions = new List<TransitionTextureData>();

        public int AtlasDimension = 1024;
        public int HalfTileDimension = 16;

        [HideInInspector]
        public List<string> UsedTexturePixelHashes = new List<string>(8);

        [HideInInspector]
        public Material Material;

        [HideInInspector]
        public Texture2D[] Atlas = new Texture2D[(int) EAtlasType.Length];
        public bool[] ActiveMaps = new bool[(int) EAtlasType.Length];

        [HideInInspector]
        public float UVTileSize;
        [HideInInspector]
        public float UVInset;

        [HideInInspector]
        public int GeneratedWithHash = -1;

        [SerializeField] ushort m_reserveBits;

        public bool Contains(Texture2D tex)
        {
            var types = new HashSet<EAtlasType>();
            return LevelTextureCollections.Any(texCol => texCol.Has(tex, types));

            //foreach (var texCol in LevelTextureCollections)
            //{
            // todo check normal texture and return whether diffuse or normal atlas have to be regenerated
            //    if (texCol.Has(tex, types))
            //        return true;
            //}
            //return false;
        }

        public TextureCollection GetCollection(int texType)
        {
            if (texType < 0 || texType >= LevelTextureCollections.Count)
                return null;

            return LevelTextureCollections[texType];
        }

        public void GetFloorUVCoords(int texType, int variation, ref Vector2 uvTileStart, ref Vector2 uvTileEnd)
        {
            var collection = GetCollection(texType);
            if (collection == null)
                return;
            var offset = collection.GetFloorUVOffset(variation);

            uvTileStart = offset + (UVInset * Vector2.one);
            uvTileEnd = uvTileStart + (UVTileSize * Vector2.one);
        }
        public void GetWallUVCoords(ushort texType, int variation, ref Vector2 uvTileStart, ref Vector2 uvTileEnd)
        {
            var collection = GetCollection(texType);
            if (collection == null)
                return;
            var offset = collection.GetWallUVOffset(variation);

            uvTileStart = offset + (UVInset * Vector2.one);
            uvTileEnd = uvTileStart + (UVTileSize * Vector2.one);
        }

        public static readonly Vector2[] HardEdgeOffsetVector = new Vector2[]
        {
            Vector2.zero,
            new Vector2( 1.5f,  1.5f),  // 1
            new Vector2(-0.5f,  1.5f),  // 2
            new Vector2( 0.5f,  1.5f),  // 3
            new Vector2(-0.5f, -0.5f),  // 4
            Vector2.zero,               // 5
            new Vector2(-0.5f,  0.5f),  // 6
            new Vector2( 0.5f,  0.5f),  // 7
            new Vector2( 1.5f, -0.5f),  // 8
            new Vector2( 1.5f,  0.5f),  // 9
            Vector2.zero,               // 10
            new Vector2( 0.5f,  0.5f),  // 11
            new Vector2( 0.5f, -0.5f),  // 12
            new Vector2( 0.5f,  0.5f),  // 13
            new Vector2( 0.5f,  0.5f),  // 14
        };

        public static readonly Vector2[] DiagEdgeOffsetVector = new Vector2[]
        {
            Vector2.zero,
            Vector2.one,        // 1
            new Vector2(0, 1),  // 2
            new Vector2(1, 0),  // 3
            Vector2.zero,       // 4
            Vector2.zero,       // 5
            Vector2.zero,       // 6
            new Vector2(1, 0),  // 7
            new Vector2(1, 0),  // 8
            Vector2.zero,       // 9
            Vector2.zero,       // 10
            Vector2.zero,       // 11
            Vector2.zero,       // 12
            new Vector2(0, 1),  // 13
            Vector2.one,        // 14
        };

        public static readonly Vector2[] TransitionOffsetVector = new Vector2[]
        {
            Vector2.zero,
            new Vector2( 1.5f,  1.5f),  // 1
            new Vector2(-0.5f,  1.5f),  // 2
            new Vector2( 0.5f,  1.5f),  // 3
            new Vector2(-0.5f, -0.5f),  // 4
            Vector2.zero,               // 5
            new Vector2(-0.5f,  0.5f),  // 6
            new Vector2( 0.5f,  0.5f),  // 7
            new Vector2( 1.5f, -0.5f),  // 8
            new Vector2( 1.5f,  0.5f),  // 9
            Vector2.zero,               // 10
            new Vector2( 0.5f,  0.5f),  // 11
            new Vector2( 0.5f, -0.5f),  // 12
            new Vector2( 0.5f,  0.5f),  // 13
            new Vector2( 0.5f,  0.5f),  // 14
        };

        internal void GetTransitionUVCoords(TransitionTextureData transitionData, 
            int config, int uvConfig, 
            ref Vector2 uvTileStart, ref Vector2 uvTileEnd)
        {
            //uvConfig = ~uvConfig & 14;
            // ignore diagonal tiles for wall-making decisions
            //if (config == 1) uvConfig &= ~4;
            //else if (config == 2) uvConfig &= ~8;
            //else if (config == 4) uvConfig &= ~1;
            //else if (config == 8) uvConfig &= ~2;

            uvTileStart = transitionData.UVOffset + (UVInset * Vector2.one);
            uvTileEnd = uvTileStart + (UVTileSize * Vector2.one);

            var offsetVector = UVTileSize * TransitionOffsetVector[uvConfig];

            uvTileStart += offsetVector;
            uvTileEnd += offsetVector;
        }

        internal void GetEdgeUVCoords(ushort texType, int variation, int config, int uvConfig, bool hard, ref Vector2 uvTileStart, ref Vector2 uvTileEnd)
        {
            var collection = GetCollection(texType);
            if (collection == null)
                return;

            if (!collection.Edge.IsSet())
            {
                GetFloorUVCoords(texType, variation, ref uvTileStart, ref uvTileEnd);
                return;
            }
            var offset = collection.GetEdgeUVOffset();
            var diagOffset = offset;

            var hasDiagEdge = collection.HasDiagEdge;
            if (hasDiagEdge)
                diagOffset = collection.GetDiagEdgeUVOffset();

            uvTileStart = offset + (UVInset * Vector2.one);
            uvTileEnd = uvTileStart + (UVTileSize * Vector2.one);

            if (collection.Edge.Type == ETexType.HardEdgeSq && hard)
            {
                // ignore diagonal tiles for wall-making decisions
                if (config != uvConfig)
                {
                    if (config == 1) uvConfig &= ~4;
                    else if (config == 2) uvConfig &= ~8;
                    else if (config == 4) uvConfig &= ~1;
                    else if (config == 8) uvConfig &= ~2;
                }

                Vector2 offsetVector = UVTileSize * HardEdgeOffsetVector[uvConfig];

                uvTileStart += offsetVector;
                uvTileEnd += offsetVector;
            }
            else if (hasDiagEdge)
            {
                // for diagonals only the diagonal tile is important
                if (config != uvConfig)
                {
                    if (config == 1 && (uvConfig & 4) == 0) uvConfig = 1;
                    else if (config == 2 && (uvConfig & 8) == 0) uvConfig = 2;
                    else if (config == 4 && (uvConfig & 1) == 0) uvConfig = 4;
                    else if (config == 8 && (uvConfig & 2) == 0) uvConfig = 8;
                }

                uvTileStart = diagOffset + (UVInset * Vector2.one);
                uvTileEnd = uvTileStart + (UVTileSize * Vector2.one);

                Vector2 offsetVector = UVTileSize * DiagEdgeOffsetVector[uvConfig];

                uvTileStart += offsetVector;
                uvTileEnd += offsetVector;
            }
        }

        internal bool IsForce(ushort texType)
        {
            var collection = GetCollection(texType);
            return collection?.ForceHardEdge ?? false;
        }

        internal bool TexTypeEdgeOnly(ushort texType)
        {
            var collection = GetCollection(texType);
            return collection?.UseEdgeOnlyForTexTypeChanges ?? false;
        }

        internal ETexType EdgeType(ushort texType)
        {
            var collection = GetCollection(texType);
            if (collection == null)
                return ETexType.Invalid;
            if (!collection.Edge.IsSet())
                return ETexType.Invalid;
            if (collection.DiagEdge.IsSet())
                return ETexType.DiagEdge;
            return collection.Edge.Type;
        }

        static bool DefaultErrorCheck(bool allError, UnityEngine.Texture texture, int tileDimension, TextureData data)
        {
            var error = allError;

            if (!error && (texture.width < tileDimension || texture.height < tileDimension)) error = true;
            if (!error && (!Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height))) error = true;
            if (!error && (texture.width % tileDimension != 0 || texture.width % tileDimension != 0)) error = true;

            if (error)
                data.Type = ETexType.Invalid;
            return error;
        }

        public void UpdateCollectionDataTypes()
        {
            var allError = false;
            var tileDimension = 2 * HalfTileDimension;
            if (tileDimension <= 0 || !Mathf.IsPowerOfTwo(HalfTileDimension))
                allError = true;

            foreach (var collection in LevelTextureCollections)
            {
                Texture2D texture;
                foreach (var floorTex in collection.FloorVariations)
                {
                    texture = floorTex.GetTexture();
                    if (DefaultErrorCheck(allError, texture, tileDimension, floorTex))
                        continue;
                    var dim = new Vector2Int(texture.width / tileDimension, texture.height / tileDimension);

                    floorTex.Type = 
                        dim == Utility.TileDimension(ETexType.SimpleFloorTile) 
                            ? ETexType.SimpleFloorTile 
                            : ETexType.Invalid;
                }
                for (var j = 0; j < collection.WallVariations.Count; ++j)
                {
                    texture = collection.WallVariations[j].GetTexture();
                    if (DefaultErrorCheck(allError, texture, tileDimension, collection.WallVariations[j]))
                        continue;
                    var dim = new Vector2Int(texture.width / tileDimension, texture.height / tileDimension);
                    if (dim == Utility.TileDimension(ETexType.SimpleWallTile))
                        collection.WallVariations[j].Type = ETexType.SimpleWallTile;
                    else if (dim == Utility.TileDimension(ETexType.WallTriplet))
                        collection.WallVariations[j].Type = ETexType.WallTriplet;
                    else if (dim == Utility.TileDimension(ETexType.WallTripletEdged))
                        collection.WallVariations[j].Type = ETexType.WallTripletEdged;
                    else
                        collection.FloorVariations[j].Type = ETexType.Invalid;
                }

                texture = collection.Edge.GetTexture();

                if (texture != null)
                {
                    if (DefaultErrorCheck(allError, texture, tileDimension, collection.Edge))
                        continue;
                    var dim = new Vector2Int(texture.width / tileDimension, texture.height / tileDimension);
                    if (dim == Utility.TileDimension(ETexType.SimpleEdgeTile))
                        collection.Edge.Type = ETexType.SimpleEdgeTile;
                    else if (dim == Utility.TileDimension(ETexType.HardEdgeSq))
                        collection.Edge.Type = ETexType.HardEdgeSq;
                    else
                        collection.Edge.Type = ETexType.Invalid;
                }

                texture = collection.DiagEdge.GetTexture();
                if (texture == null)
                    continue;

                {
                    if (DefaultErrorCheck(allError, texture, tileDimension, collection.DiagEdge))
                        continue;
                    var dim = new Vector2Int(texture.width / tileDimension, texture.height / tileDimension);
                    if (dim == Utility.TileDimension(ETexType.DiagEdge))
                        collection.DiagEdge.Type = ETexType.DiagEdge;
                    else
                        collection.Edge.Type = ETexType.Invalid;
                }
            }

            foreach (var transition in Transitions)
            {
                var texture = transition.GetTexture();
                if (DefaultErrorCheck(allError, texture, tileDimension, transition))
                    continue;
                var dim = new Vector2Int(texture.width / tileDimension, texture.height / tileDimension);
                transition.Type = dim == Utility.TileDimension(ETexType.Transition) 
                    ? ETexType.Transition 
                    : ETexType.Invalid;
            }
        }

        public void UpdateTransitionNames()
        {
            foreach (var transition in Transitions)
            {
                transition.TexAName = LevelTextureCollections[transition.TexAIdx].Name;
                transition.TexBName = LevelTextureCollections[transition.TexBIdx].Name;
            }
        }

        public void UpdateTransitionIndices()
        {
            foreach (var transition in Transitions)
            {
                transition.TexAIdx = LevelTextureCollections.FindIndex(a => string.Equals(a.Name, transition.TexAName, StringComparison.Ordinal));
                transition.TexBIdx = LevelTextureCollections.FindIndex(b => string.Equals(b.Name, transition.TexBName, StringComparison.Ordinal));
            }
        }

        public void OnEnable()
        {
            foreach (var dat in LevelTextureCollections)
                dat.OnEnable();
        }


        public string Name => name;
        public ushort ReserveBits => m_reserveBits;
        public int Count => LevelTextureCollections.Count;
        public ITilesData this[int idx] => LevelTextureCollections[idx];
    }
}