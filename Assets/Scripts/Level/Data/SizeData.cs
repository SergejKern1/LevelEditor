using Level.Data.Interface;
using UnityEngine;

namespace Level.Data
{
    public struct SizeData : ISizeData
    {
        public static implicit operator Vector3Int(SizeData e) { return e.Size; }
        public static implicit operator SizeData(Vector3Int e) { return new SizeData { Size = e }; }

        public Vector3Int Size { get; set; }
    }
}
