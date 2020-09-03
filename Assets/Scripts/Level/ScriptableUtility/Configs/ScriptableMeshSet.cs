using Level.Data;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Level/MeshSet")]
    public class ScriptableMeshSet : ScriptableVariable<TileMeshConfig.MeshSet>
    {
        public override TileMeshConfig.MeshSet GetValue(OverrideValue value)=> new TileMeshConfig.MeshSet();
        public override OverrideValue GetValue(TileMeshConfig.MeshSet value) => new OverrideValue() { };
    }
}