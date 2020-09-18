using System;
using Core.Unity.Extensions;
using Level.Tiles;
using UnityEngine;

namespace Level.Room
{
    [Serializable]
    public struct RoomData
    {
        public string Name;
        public Vector3Int Size;
        public RoomSettings Settings;

        public Vector2Int TileDim => Size.Vector2Int() + Vector2Int.one;
        public int MaxPlatformCount => Size.y - 1;
    }

    [Serializable]
    public struct RoomSettings
    {
        public GridSettings Grid;
        public TilesSetListConfig TileSet;
        public RoomBuilder Builder;
        // todo: remove this here, should be applied from builder
        public Material FloorAndWallMaterial;
        public Material CeilingMaterial;
    }
}
