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
        public ITilesData this[int idx] => null;
    }

    [Flags]
    public enum HardEdgeState
    {
        Default = 0,
        Hard = 1,
        Break = 2
    }
}