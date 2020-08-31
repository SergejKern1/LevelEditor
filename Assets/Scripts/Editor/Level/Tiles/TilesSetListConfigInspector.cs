using Core.Editor.Inspector;
using Level.Tiles;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Tiles
{
    [CustomEditor(typeof(TilesSetListConfig))]
    public class TilesSetListConfigInspector : BaseInspector
    {
        // ReSharper disable once InconsistentNaming
        new TilesSetListConfig target => base.target as TilesSetListConfig;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label($"Digits {target.Editor_Digits}");
        }
    }
}