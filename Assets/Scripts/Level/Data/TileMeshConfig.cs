using System;
using UnityEngine;

namespace Level.Data
{
    [CreateAssetMenu(menuName = "Level/TileMeshConfig")]
    public class TileMeshConfig : ScriptableObject
    {
        [Serializable] 
        public struct MeshSet
        {
            public Mesh[] Meshes;
        }

        [SerializeField] MeshSet[] m_configData = new MeshSet[16];
        public MeshSet Get(int i) => m_configData[i];

#if UNITY_EDITOR
        public const string Editor_ConfigDataPropName = nameof(m_configData);
#endif
    }
}
