using System;
using JetBrains.Annotations;
using Level.Tiles.Interface;

namespace Level.Tiles
{
    /// <summary>
    /// Adds Mask and Shift to TileConfig depending on the position in the Set (TilesSetListConfig)
    /// </summary>
    [Serializable]
    public struct TilesSetData
    {
        public RefITileConfig TileConfig;
        public ushort Mask;
        public ushort Shift;
        //public float DebugTileSize;

        public int Count => TileConfig.Result?.Count ?? 0;
        public ITilesData this[ushort tile] => TileConfig.Result[tile];

        /// <summary>
        /// extracts only relevant tile data
        /// </summary>
        [Pure]
        public ushort GetTileIdx(uint combinedTile) => (ushort)((combinedTile & Mask) >> Shift);

        /// <summary>
        /// combines the data
        /// </summary>
        [Pure]
        public ushort GetCombinedTile(uint combinedTile, uint tileIdx) => (ushort) ((combinedTile & (~Mask)) | (Mask & (tileIdx << Shift)));
    }
}