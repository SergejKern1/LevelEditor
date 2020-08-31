using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Core.Extensions;
using Level.Tiles.Interface;

namespace Level.Texture
{
    [Serializable]
    public class TextureCollection : IEnumerable<TextureData>, IEnumerator<TextureData>, ITilesData
    {
        public string Name;

        int m_position = -1;

        public TextureData Current
        {
            get
            {
                var floorVarCount = FloorVariations?.Count ?? 0;
                var floorVarEnd = floorVarCount;

                var wallVarStart = floorVarEnd;
                var wallVarCount = WallVariations?.Count ?? 0;
                var wallVarEnd = wallVarStart + wallVarCount;

                if (m_position < 0)
                    return null;

                if (m_position < floorVarEnd)
                    return FloorVariations?[m_position];
                if (m_position < wallVarEnd)
                    return WallVariations?[m_position - wallVarStart];
                if (m_position == wallVarEnd && Edge.IsSet())
                    return Edge;
                if (m_position == wallVarEnd + 1 && DiagEdge.IsSet())
                    return DiagEdge;
                throw new InvalidOperationException();
            }
        }

        object IEnumerator.Current => Current;

        public Color DebugColor;
        public List<TextureData> FloorVariations;// = new List<TextureData>();
        public List<TextureData> WallVariations;// = new List<TextureData>();
        public TextureData Edge = new TextureData();
        public TextureData DiagEdge = new TextureData();

        public bool HasEdge => Edge.IsSet();
        public bool HasDiagEdge => DiagEdge.IsSet();

        public bool UseEdgeOnlyForTexTypeChanges;
        public bool ForceHardEdge;

        public int Count()
        {
            var textureCount = 0;
            if (FloorVariations != null)
                textureCount += FloorVariations.Count;
            if (WallVariations != null)
                textureCount += WallVariations.Count;
            if (Edge.IsSet())
                textureCount++;
            if (DiagEdge.IsSet())
                textureCount++;
            return textureCount;
        }

        public bool MoveNext()
        {
            m_position++;
            return (m_position < Count());
        }
        public void Dispose() { Reset(); }
        public void Reset() { m_position = -1; }
        IEnumerator IEnumerable.GetEnumerator() { m_position = -1; return this; }
        public IEnumerator<TextureData> GetEnumerator() { m_position = -1; return this; }

        internal Vector2 GetFloorUVOffset(int variation)
        {
            variation = Mathf.Clamp(variation, 0, FloorVariations.Count - 1);
            return variation < 0 
                ? Vector2.zero 
                : FloorVariations[variation].UVOffset;
        }

        internal Vector2 GetWallUVOffset(int variation)
        {
            variation = Mathf.Clamp(variation, 0, WallVariations.Count - 1);
            return variation < 0 
                ? Vector2.zero 
                : WallVariations[variation].UVOffset;
        }

        internal Vector2 GetEdgeUVOffset() => Edge.UVOffset;

        internal Vector2 GetDiagEdgeUVOffset() => DiagEdge.UVOffset;

        public void OnEnable()
        {
            if (Edge == null)
                Edge = new TextureData();
            if (DiagEdge == null)
                DiagEdge = new TextureData();
            if (FloorVariations == null)
                FloorVariations = new List<TextureData>();
            if (WallVariations == null)
                WallVariations = new List<TextureData>();

            foreach (var dat in this)
                dat.UpgradeToArray();
        }

        public bool Has(Texture2D tex, HashSet<EAtlasType> types)
        {
            foreach (var v in FloorVariations)
                v.Has(tex, types);
            foreach (var v in WallVariations)
                v.Has(tex, types);

            Edge.Has(tex, types);
            DiagEdge.Has(tex, types);

            return !types.IsNullOrEmpty();
        }

        public string TileName => Name;
        public Color TileColor => DebugColor;
        public Texture2D PreviewTex
        {
            get => FloorVariations.IsNullOrEmpty() ? null : FloorVariations[0].DiffuseTex;
            set { }
        }
    }

}