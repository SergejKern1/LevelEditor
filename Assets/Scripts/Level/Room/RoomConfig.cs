using System.Collections.Generic;
using Core.Unity.Extensions;
using Level.PlatformLayer;
using Level.Tiles;
using UnityEngine;

namespace Level.Room
{
    public class RoomConfig : ScriptableObject
    {
        public RoomData RoomData;
        public List<PlatformLayerConfig> PlatformLayer = new List<PlatformLayerConfig>();
        public List<ExitInfo> ExitRestrictions = new List<ExitInfo>();

        public GridSettings Grid => RoomData.Settings.Grid;
        public TilesSetListConfig Setting => RoomData.Settings.TileSet;
        public RoomBuilder Builder => RoomData.Settings.Builder;

        // todo: remove this here, should be applied from builder
        public Material FloorAndWallMaterial => RoomData.Settings.FloorAndWallMaterial;
        public Material CeilingMaterial => RoomData.Settings.CeilingMaterial;
        public Vector3Int Size => RoomData.Size;
        public Vector2Int TileDim => RoomData.TileDim;
        public int MaxPlatformCount => RoomData.MaxPlatformCount;
    }
}

