using Core.Editor.Inspector;
using Level.Tiles;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Tiles
{
    [CustomEditor(typeof(TilesSetListConfig))]
    public class TilesSetListConfigInspector : BaseInspector<TilesSetListConfigEditor> { }
    
    public class TilesSetListConfigEditor : BaseEditor<TilesSetListConfig>
    {
        public override void OnGUI(float width)
        {
            base.OnGUI(width);
            GUILayout.Label($"Digits {Target.Editor_Digits}");
        }
    }
}