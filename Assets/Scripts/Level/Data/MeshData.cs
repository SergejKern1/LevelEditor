using System.Collections.Generic;
using UnityEngine;

namespace Level.Data
{
    /// <summary>
    /// Data for Mesh-Building, will be set to the MeshFilter
    /// </summary>
    public struct MeshData
    {
        public static MeshData Default => new MeshData()
        {
            Vertices = new List<Vector3>(1000),
            UVs = new List<Vector2>(1000),
            Triangles = new List<int>(1000),
        };

        public List<Vector3> Vertices;
        public List<Vector2> UVs;
        public List<int> Triangles;
    }
}
