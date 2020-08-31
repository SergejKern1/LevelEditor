using Level.Data;
using Level.ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;

namespace Level.Room
{
    [CreateAssetMenu(menuName = "Level/RoomBuilder")]
    public class RoomBuilder : ScriptableObject
    {
        public ScriptableUnityObject MeshObject;
        public ScriptableBaseAction Action;
        public ScriptableMeshData MeshData;
        public ScriptableMeshData TempMeshData;

        public Material Material;
    }
}
