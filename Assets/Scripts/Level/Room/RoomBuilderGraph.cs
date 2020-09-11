using Level.ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Nodes;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode;
using XNode.Attributes;

namespace Level.Room
{
    [CreateAssetMenu(menuName = "Level/RoomBuilderGraph"),
    RequireNode(typeof(EntryNode))]
    public class RoomBuilderGraph : NodeGraph, IEntryNodeGraph
    {
        public ScriptableUnityObject MeshObject;
        public ScriptableBaseAction Action;
        public ScriptableMeshData MeshData;
        public ScriptableMeshData TempMeshData;

        public Material Material;
        public ScriptableBaseAction EntryAction
        {
            get => Action;
            set => Action = value;
        }
    }
}
