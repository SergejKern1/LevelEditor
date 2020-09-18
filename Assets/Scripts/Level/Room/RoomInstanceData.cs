using System;
using System.Collections.Generic;
using ScriptableUtility;
using UnityEngine;

namespace Level.Room
{
    [Serializable]
    public struct RoomInstanceData
    {
        public RoomInstanceData(RoomConfig roomConfig, RoomInstanceComponent comp)
        {
            RoomConfig = roomConfig;
            Component = comp;
            OpenExits = new List<ExitInfo>();
            ClosedExits = new List<ExitInfo>();

            Position = default;
            Bounds = default;
            VisualData = default;
        }
        
        public RoomInstanceComponent Component;
        public RoomConfig RoomConfig;

        public List<ExitInfo> OpenExits;
        public List<ExitInfo> ClosedExits;

        public Vector3Int Position;
        public Bounds Bounds;
        public RoomVisualData VisualData;

        public Vector3Int Size => RoomConfig != null ? RoomConfig.Size : Vector3Int.zero; 
        public Vector2Int TileDim => RoomConfig != null ? RoomConfig.TileDim : Vector2Int.zero;
        public int MaxPlatformCount => RoomConfig != null ? RoomConfig.MaxPlatformCount : 0;

        public Transform Transform => Component != null ? Component.Transform : null;
        public GameObject GameObject => Component != null ? Component.GameObject : null;

        public IContext RoomContext => Component != null ? Component.Context : null;
    }
}