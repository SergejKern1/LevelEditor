using JetBrains.Annotations;

namespace Level.Tiles
{
    /// <summary>
    /// Used for finding specific ITilesData from a set on a combined tile
    /// </summary>
    public struct TilesSetFilter
    {
        public TilesSetData Data;
        public ushort FilterIdx;
        [Pure]
        public bool IsTileActive(uint combinedTile) => Data.GetTileIdx(combinedTile) == FilterIdx;
    }
}
