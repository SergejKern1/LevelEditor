using System.Collections.Generic;
using Core.Unity.Extensions;
using Level.Enums;
using Level.PlatformLayer;
using UnityEngine;

namespace Level.Room.Operations
{
    public static class RoomOperations
    {
        #region RoomConfig
        public static RoomConfig CreateRoomAsset(this RoomData data)
        {
            var room = ScriptableObject.CreateInstance<RoomConfig>();
            room.RoomData = data;

            var platform = ScriptableObject.CreateInstance<PlatformLayerConfig>();
            platform.name = $"{room.name} lvl {0}";

            // todo: RoomSettings to config?
            platform.SetTilesSet(data.Settings.TileSet);
            platform.SetGrid(data.Settings.Grid);

            // AssetDatabase.AddObjectToAsset(platform, room);
            room.PlatformLayer.Add(platform);

            platform.Position = Vector2Int.zero;
            platform.InitSize(data.Size.Vector2Int());
            return room;
        }

        public static void ChangeLevelCount(this RoomConfig config, int newLevelCount)
        {
            var data = config.RoomData;
            var prevCount = config.PlatformLayer.Count;

            // how many levels does the room have
            newLevelCount = Mathf.Clamp(newLevelCount, 1, config.MaxPlatformCount);
            if (newLevelCount == prevCount)
                return;

            for (var i = prevCount; i < newLevelCount; i++)
            {
                var platform = ScriptableObject.CreateInstance<PlatformLayerConfig>();
                platform.name = $"{config.name} lvl {i}";
                platform.SetTilesSet(data.Settings.TileSet);
                platform.SetGrid(data.Settings.Grid);
                platform.Position = Vector2Int.zero;
                platform.InitSize(data.Size.Vector2Int());

                config.PlatformLayer.Add(platform);
            }

            for (var i = prevCount; i > newLevelCount; i--) 
                config.PlatformLayer.RemoveAt(i-1);
        }
        #endregion

        #region RoomInstanceData

        public static Bounds PlatformLayerBounds(this ref RoomInstanceData data, int idx)
        {
            var config = data.RoomConfig;
            if (config.PlatformLayer.Count <= idx || idx < 0)
                return new Bounds();
            var lvl = config.PlatformLayer[idx];
            return new Bounds(data.Position.Vector3() + lvl.Position.Vector3(idx) + lvl.Size.Vector3(1.0f) / 2, lvl.Size.Vector3(1.0f));
        }

        public static void UpdateRoomPosition(this ref RoomInstanceData data, Vector3 position)
        {
            data.Position = position.Vector3Int();
            data.Bounds = new Bounds(data.Position.Vector3() + (data.Size.Vector3() / 2.0f), data.Size.Vector3());
        }
        public static void ShowRoom(this ref RoomInstanceData data)
        {
            var config = data.RoomConfig;
            var builder = config.Builder;
            data.UpdateVisuals();

            for (var lvl = 0; lvl < config.PlatformLayer.Count; lvl++)
            {
                //var platformLayer = config.PlatformLayer[lvl];
                var m = data.VisualData.GetFloorVisuals(lvl).Mesh;

                builder.Build(data.RoomContext, lvl, m);
            }

            //OperationData dat
            //if (RoomComponent!= null)
            //{
            //    RoomComponent.gameObject.SetActive(true);
            //    return;
            //}
            //RoomComponent = CreateObjectForRoom(this, Name);
            //dat.Room = this;
            //GenerateMesh(dat);
        }

        internal static void HideRoom(this ref RoomInstanceData data)
        {
            //if (RoomComponent == null)
            //    return;
            //RoomComponent.Room = null;
            //RoomGameObjectPool.I.Return(RoomComponent.gameObject);
            //// TODO: remove this but reset it properly, or there is this bug that collision object is missing
            //RoomComponent.DestroyEx();
            //RoomComponent = null;
        }


        /// <summary>
        /// Sets the Level the player is currently at, eg. what should be visible
        /// </summary>
        /// <param name="data">the room instance</param>
        /// <param name="lvl">the level</param>
        public static void SetLevel(this ref RoomInstanceData data, int lvl) => data.VisualData.SetVisibleHeight(lvl);
        public static void UpdateVisuals(this ref RoomInstanceData data)
        {
            var config = data.RoomConfig;
            if (config == null)
                return;

            if (!data.VisualData.IsValid)
            {
                data.Transform.CreateChildObject("FloorVisuals", out Transform floorVisual);
                data.Transform.CreateChildObject("CeilingVisuals", out Transform ceilingVisual);
                ceilingVisual.localPosition = 2 * Vector3.up;
                data.VisualData = new RoomVisualData(floorVisual, ceilingVisual);
            }

            data.VisualData.UpdateVisuals(config.MaxPlatformCount, config.FloorAndWallMaterial, config.CeilingMaterial);
        }

        public static void UpdateData(this ref RoomInstanceData iData, RoomData data)
        {
            iData.GameObject.name = data.Name;
            iData.RoomConfig.RoomData = data;
            iData.UpdateRoomPosition(iData.Transform.position);
        }

        public static void Destroy(this ref RoomInstanceData data)
        {
            data.VisualData.Destroy();
            if (data.GameObject != null)
                data.GameObject.DestroyEx();
            data = default;
        }

        public static ref RoomData GetRoomData(this ref RoomInstanceData data) => ref data.RoomConfig.RoomData;
        #endregion

    }
}
