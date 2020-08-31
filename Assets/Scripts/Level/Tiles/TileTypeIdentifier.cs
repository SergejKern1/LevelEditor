using System;
using Level.Tiles.Interface;

//todo: Draw better inspector for this
namespace Level.Tiles
{
    /// <summary>
    /// To identify a specific Tile in a TileConfig within a Set (TilesSetListConfig)
    /// </summary>
    [Serializable]
    public struct TileTypeIdentifier
    {
        public RefITileConfig Config;
        public ushort TileIdx;
    }
}
