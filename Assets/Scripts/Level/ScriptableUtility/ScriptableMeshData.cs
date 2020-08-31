using Core.Extensions;
using Level.Data;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Level/MeshData")]
    public class ScriptableMeshData : ScriptableVariable<MeshData> {
        public override MeshData GetValue(OverrideValue value)
        {
            var vars = value.StringValue.Split(',');
            if (vars.IsNullOrEmpty())
                return MeshData.Default;

            return MeshData.Default;
        }

        public override OverrideValue GetValue(MeshData value) 
            => new OverrideValue() {StringValue = "" };


        // todo: make it easier to use/ extend scriptable variables?
        public void __GLOBAL_Clear()
        {
            GlobalValue.Vertices.Clear();
            GlobalValue.UVs.Clear();
            GlobalValue.Triangles.Clear();
        }
    }
}