using System;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;


namespace Level.Actions
{
    public class AddMeshData : ScriptableBaseAction
    {
        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_currentPosition;
        [SerializeField] internal ScriptableMeshData m_meshData;
        [SerializeField] internal ObjectReference m_meshVar;
        [SerializeField] internal Vector3 m_meshOffset;
        [SerializeField] internal Vector3 m_orientation;

        public override string Name => nameof(AddMeshData);
        public static Type StaticFactoryType => typeof(TileTypeToMeshAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);
            var meshData = new MeshDataReference(ctx, m_meshData);

            m_meshVar.Init(ctx);
            return new TileTypeToMeshAction(currentPos, meshData, 
                m_meshVar, m_meshOffset, Quaternion.Euler(m_orientation));
        }
    }

    public class TileTypeToMeshAction : IDefaultAction
    {
        readonly Vector3Reference m_currentPos;

        readonly MeshDataReference m_meshData;

        readonly ObjectReference m_mesh;
        readonly Vector3 m_meshOffset;
        readonly Quaternion m_orientation;

        public TileTypeToMeshAction(Vector3Reference currentPos,
            MeshDataReference mdr,
            ObjectReference mesh, 
            Vector3 meshOffset,
            Quaternion orientation)
        {
            m_currentPos = currentPos;

            m_mesh = mesh;
            m_meshData = mdr;
            m_meshOffset = meshOffset;
            m_orientation = orientation;
        }

        public void Invoke() => AddToMesh();

        void AddToMesh()
        {
            var pos = m_currentPos.Value;

            var md = m_meshData.Value;
            var verts = md.Vertices;
            var tris = md.Triangles;
            var uvs = md.UVs;

            var startCount = verts.Count;

            var mesh = m_mesh.Value as Mesh;
            foreach (var v in mesh.vertices)
            {
                var rotatedV = m_orientation * v;
                verts.Add(rotatedV + pos + m_meshOffset);
            }   
            foreach (var t in mesh.triangles) 
                tris.Add(t + startCount);

            uvs.AddRange(mesh.uv);
        }
    }
}
