using UnityEngine;

namespace Level.Tiles
{
    public static class TileProcessing
    {
        // Iterating over a grid like counter-clockwise like this
        // |3|2|
        // |0|1|
        public static Vector3Int[] IterationOffset { get; } = { Vector3Int.zero, 
            new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1) };

        public struct ConfigurationResult
        {
            public int Configuration;
            public int ActiveNodes;
            public bool[] Active;
        }

        public static ConfigurationResult GetConfiguration(TilesSetFilter primary, TilesSetFilter secondary, ushort[,,] tiles, int x, int y, int z)
        {
            ConfigurationResult result;
            result.Active = new bool[4];

            result.Configuration = 0;
            result.ActiveNodes = 0;

            ushort posVal = 1;

            for (var i = 0; i <= IterationOffset.Length; ++i, posVal *= 2)
            {
                var off = IterationOffset[i];
                var tile = tiles[x + off.x, y + off.y, z + off.z];
                result.Active[i] = primary.IsTileActive(tile);

                if (secondary.Data.TileConfig != null)
                    result.Active[i] &= secondary.IsTileActive(tile);

                if (!result.Active[i])
                    continue;

                result.Configuration += posVal;
                result.ActiveNodes++;
            }

            return result;
        }
    }
}
