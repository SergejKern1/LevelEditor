using UnityEngine;
using System;
using System.Collections.Generic;
using Core.Extensions;
using UnityEngine.Serialization;

namespace Level.Texture
{
    public enum ETexType
    {
        SimpleFloorTile,
        SimpleWallTile,
        SimpleEdgeTile,
        HardEdgeSq,
        DiagEdge,
        WallTriplet,
        WallTripletEdged,
        Transition,
        Invalid
    }
    public enum EAtlasType
    {
        Default = 0,
        Normal,
        MetallicSmoothness,
        Occlusion,
        Emission,
        Length
    }

    public static class Utility
    {
        public static int SortBiggestDimensionFirst(TextureData dat1, TextureData dat2)
        {
            var dim1 = dat1.TileDimension();
            var dim2 = dat2.TileDimension();
            if (dim1.y > dim2.y) return -1;
            if (dim1.y < dim2.y) return 1;
            if (dim1.x > dim2.x) return -1;
            if (dim1.x < dim2.x) return 1;
            return 0;
        }

        public static readonly string[] KTexNames = {
            "_MainTex",
            "_BumpMap",
            "_MetallicGlossMap",
            "_OcclusionMap",
            "_EmissionMap"
        };

        public static Vector2Int TileDimension(ETexType type)
        {
            switch (type)
            {
                case ETexType.SimpleFloorTile:
                case ETexType.SimpleWallTile:
                case ETexType.SimpleEdgeTile: return Vector2Int.one;
                case ETexType.HardEdgeSq:
                case ETexType.DiagEdge:
                case ETexType.Transition:
                    return Vector2Int.one * 2;
                case ETexType.WallTriplet: return new Vector2Int(1, 3);
                case ETexType.WallTripletEdged: return new Vector2Int(2, 3);
                default: return Vector2Int.zero;
            }
        }
    }

    [Serializable]
    public class TextureData
    {
        [FormerlySerializedAs("Textures")]
        [SerializeField]
        Texture2D[] m_textures = new Texture2D[(int)EAtlasType.Length];

        public Vector2 UVOffset;
        public ETexType Type;
        public Texture2D DiffuseTex => m_textures[(int) EAtlasType.Default];

        public bool IsSet()
        {
            return m_textures != null
                   && m_textures.IsIndexInRange((int)EAtlasType.Default)
                   && m_textures[(int)EAtlasType.Default] != null;
        }
        public Vector2Int TileDimension() { return Utility.TileDimension(Type); }
        public Texture2D GetTexture(EAtlasType atlasType = EAtlasType.Default) => 
            m_textures[(int)atlasType];

        public void SetTexture(EAtlasType atlasType, Texture2D tex)
        {
            m_textures[(int)atlasType] = tex;
            if (atlasType != EAtlasType.Default || tex != null)
                return;
            Type = ETexType.Invalid;
        }

        public void Has(Texture2D tex, HashSet<EAtlasType> types)
        {
            for (var i = 0; i < m_textures.Length; i++)
            {
                if (m_textures[i] == tex && !types.Contains((EAtlasType)i))
                    types.Add((EAtlasType)i);
            }
        }
    }

    [Serializable]
    public class TransitionTextureData : TextureData
    {
        public string TexAName;
        public string TexBName;

        public int TexAIdx;
        public int TexBIdx;
    }
}