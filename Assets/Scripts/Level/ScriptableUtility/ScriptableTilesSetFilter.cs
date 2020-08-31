using Level.Tiles;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Level/TilesSetFilter")]
    public class ScriptableTilesSetFilter : ScriptableVariable<TilesSetFilter>
    {
        public override TilesSetFilter GetValue(OverrideValue value)=> new TilesSetFilter();
        public override OverrideValue GetValue(TilesSetFilter value) => new OverrideValue() { };
    }
}