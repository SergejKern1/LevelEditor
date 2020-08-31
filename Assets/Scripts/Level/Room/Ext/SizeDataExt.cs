using Core.Unity.Extensions;
using UnityEngine;

using Level.Data.Interface;
namespace Level.Room.Ext
{
    /// <summary>
    /// Extension for SizeData in regards to a room
    /// </summary>
    public static class SizeDataExt
    {
        public static Vector2Int RoomTileDim(this ISizeData size) { return size.Size.Vector2Int() + new Vector2Int(1, 1); }
        public static int RoomMaxLevel(this ISizeData size) { return size.Size.y - 1; }
    }
}
