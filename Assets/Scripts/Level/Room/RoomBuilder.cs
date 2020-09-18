using Level.PlatformLayer.Interface;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Nodes;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode;
using XNode.Attributes;

using static Level.PlatformLayer.Operations;

namespace Level.Room
{
    [CreateAssetMenu(menuName = "Level/RoomBuilder"),
     RequireNode(typeof(EntryNode))]
    public class RoomBuilder : NodeGraph, IEntryNodeGraph
    {
        public ScriptableUnityObject MeshObject;
        public ScriptableBaseAction Action;
        public ScriptableMeshData MeshData;
        public ScriptableMeshData TempMeshData;

        public ScriptableGrid Grid;

        public Material Material;
        
        IBaseAction m_action;

        public ScriptableBaseAction EntryAction
        {
            get => Action;
            set => Action = value;
        }

        public void BuildGrid(IPlatformLayer[] platforms)
        {
            var platformA = platforms[0];
            var gridData = new ushort[platformA.TileDim.x, platforms.Length, platformA.TileDim.y];

            for (var y = 0; y < platforms.Length; ++y)
            for (var x = 0; x < platformA.TileDim.x; ++x)
            for (var z = 0; z < platformA.TileDim.y; ++z)
                gridData[x, y, z] = GetTile(platforms[y], platforms[y], x, z);

            Grid.GlobalValue = gridData;
        }

        public void Build(IContext context, int lvl, Mesh m)
        {
            MeshObject.GlobalValue = m;

            Data.MeshData.ClearData(ref MeshData.GetGlobalByRef());
            Data.MeshData.ClearData(ref TempMeshData.GetGlobalByRef());

            if (m_action == null)
                m_action = Action.CreateAction(context);
            if (m_action is IDefaultAction def)
                def.Invoke();

            m.Clear();

            m.vertices = MeshData.GlobalValue.Vertices.ToArray();
            m.uv = MeshData.GlobalValue.UVs.ToArray();
            m.triangles = MeshData.GlobalValue.Triangles.ToArray();
            m.RecalculateNormals();
            m.RecalculateTangents();
        }
    }
}
