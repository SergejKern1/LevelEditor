
using UnityEngine;

namespace Level
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "Level/GridSettings")]
    public class GridSettings : ScriptableObject
    {
        public float TileSize = 1;
        public float TileHeight = 1;

        public int RoomChunkSize = 8;
        public int RoomYChunkSize = 4;

        public float HalfRoomChunkSize => 0.5f * RoomChunkSize;
        public int ExtraDim = 0;

        //public int SmallRoomSize => RoomGridSize;
        //public Vector3Int SmallFlatRoomSize => new Vector3Int(SmallRoomSize, RoomYGridSize, SmallRoomSize);
        //public int StandardRoomSize => SmallRoomSize * 2;
        //public int BigRoomSize => StandardRoomSize * 2;
        //public int HugeRoomSize => BigRoomSize * 2;
    }
}