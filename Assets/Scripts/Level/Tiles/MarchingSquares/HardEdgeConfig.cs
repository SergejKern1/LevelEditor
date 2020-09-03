using System;
using Level.Tiles.Interface;
using UnityEngine;

namespace Level.Tiles.MarchingSquares
{
    [CreateAssetMenu(menuName = "Level/Tiles/HardEdgeConfig")]
    public class HardEdgeConfig : ScriptableObject, ITileConfig
    {
        public string Name => "HardEdge";
        public ushort ReserveBits => 2;
        public int Count => 3;

        // todo: don't make me implement editor shit
        public ITilesData this[int idx] => new HardEdgeTilesData(){ Name = ((HardEdgeState)idx).ToString() };

        public struct HardEdgeTilesData : ITilesData
        {
            public string Name;

            public string TileName => Name;
            public Color TileColor => Color.white;
            public Texture2D PreviewTex { get => null; set{} }
        }
    }

    public enum HardEdgeState
    {
        Default = 0,
        Hard = 1,
        Break = 2,
        BreakAndHard = 3
    }
}