using System;
using System.Collections.Generic;
using Core.Unity.Extensions;
using Level.Enums;
using Level.PlatformLayer.Interface;
using UnityEngine;

namespace Level.PlatformLayer
{
    public static class Operations
    {
        public static ushort GetTile(IPlatformLayerSpatialData platform,
            IPlatformLayerTiles tiles,
            int x, int z,
            OutOfLevelBoundsAction action = OutOfLevelBoundsAction.Error)
        {
            //Debug.Log($"GetTile {z} * {platform.TileDim.x} + {x} = {z * platform.TileDim.x + x}");
            if (platform.IsInside(x, z))
                return tiles[z * platform.TileDim.x + x];
            switch (action)
            {
                case OutOfLevelBoundsAction.Error:
                    Debug.LogError("tile is out of level bounds");
                    break;
                case OutOfLevelBoundsAction.ExpandBounds:
                    Debug.LogError("ExpandBounds not supported in this method");
                    break;
                case OutOfLevelBoundsAction.IgnoreTile:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
            return ushort.MaxValue;
        }
        public static void SetTile(IPlatformLayerSpatialData lvlDat, IPlatformLayerTiles tiles, 
            int lvlXPos, int lvlZPos, ushort tile,
            ushort mask, ushort shift = 0, 
            OutOfLevelBoundsAction action = OutOfLevelBoundsAction.Error)
        {
            if (!lvlDat.IsInside(lvlXPos, lvlZPos))
            {
                switch (action)
                {
                    case OutOfLevelBoundsAction.Error:
                        Debug.LogError("tile is out of level bounds");
                        break;
                    case OutOfLevelBoundsAction.ExpandBounds:
                        Debug.LogError("ExpandBounds not supported in this method");
                        break;
                    case OutOfLevelBoundsAction.IgnoreTile:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }

                return;
            }
            var invertedMask = (ushort)~mask;
            var origTile = tiles[lvlZPos * lvlDat.TileDim.x + lvlXPos];
            tiles[lvlZPos * lvlDat.TileDim.x + lvlXPos] = (ushort)((origTile & invertedMask) | (mask & (tile << shift)));
        }

        public static bool IsInside(this IPlatformLayerSpatialData lvl, int x, int z) =>
            x >= 0 && x < lvl.TileDim.x && z >= 0 && z < lvl.TileDim.y;

        public static bool IsInsideWithoutEdge(this IPlatformLayerSpatialData lvl, int x, int z)
            => lvl.IsInsideWithInset(x, z, 1);
        public static bool IsInsideWithInset(this IPlatformLayerSpatialData lvl, int x, int z, int inset)
            => x >=inset && x < lvl.TileDim.x - inset && z >= inset && z < lvl.TileDim.y - inset;

        public static IEnumerable<Vector2Int> GetRegionTiles(IPlatformLayerSpatialData lvlDat, IPlatformLayerTiles tiles, 
            int startX, int startZ,
            ushort compareMask)
        {
            var v2Tiles = new List<Vector2Int>();
            // mark tiles that have already been processed
            var mapFlags = new int[lvlDat.TileDim.x, lvlDat.TileDim.y];

            var tileType = (GetTile(lvlDat, tiles, startX, startZ) & compareMask);

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startZ));
            mapFlags[startX, startZ] = 1;

            // spread-algorithm, select all adjacent tiles with same tileType
            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();
                v2Tiles.Add(tile);

                for (var x = tile.x - 1; x <= tile.x + 1; ++x)
                for (var z = tile.y - 1; z <= tile.y + 1; ++z)
                {
                    var adjacent = (z == tile.y || x == tile.x);
                    if (!adjacent)
                        continue;
                    if (z == tile.y && x == tile.x)
                        continue;
                    if (!lvlDat.IsInside(x, z))
                        continue;
                    if (mapFlags[x, z] != 0 || (GetTile(lvlDat, tiles, x, z) & compareMask) != tileType)
                        continue;

                    mapFlags[x, z] = 1;
                    queue.Enqueue(new Vector2Int(x, z));
                }
            }

            return v2Tiles;
        }


        public static Bounds GetBounds(IPlatformLayer layer) => GetBounds(layer, layer.PositionOffset, layer.GridSize, layer.GridYSize);

        public static Bounds GetBounds(IPlatformLayerSpatialData platform, 
            Vector3Int posOffset, float gridSize, float gridYSize)
        {
            var size = gridSize * platform.Size.Vector3() + new Vector3(0, gridYSize, 0);
            var bounds = new Bounds(posOffset.Vector3() + platform.Position.Vector3() + (size / 2), size);
            return bounds;
        }
    }
}