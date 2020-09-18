using Core.Editor.Inspector;
using Core.Editor.Utility;
using Core.Editor.Utility.GUIStyles;
using Core.Unity.Utility.GUITools;
using Level.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Tiles
{
    [CustomEditor(typeof(TileMeshConfig))]
    public class TileMeshConfigInspector: BaseInspector<TileMeshConfigEditor> { }

    public class TileMeshConfigEditor : BaseEditor<TileMeshConfig>
    {
        public override void OnGUI(float width)
        {
            base.OnGUI(width);

            for (var i = 0; i < 16; i++)
            {
                //ref var meshData = ref target.Editor_Get(i);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new BackgroundColorScope((i & 8) == 8? Color.white : Color.black))
                                GUILayout.Box(GUIContent.none, CustomStyles.Gray, GUILayout.Width(20f));
                            using (new BackgroundColorScope((i & 4) == 4? Color.white : Color.black))
                                GUILayout.Box(GUIContent.none, CustomStyles.Gray, GUILayout.Width(20f));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new BackgroundColorScope((i & 1) == 1? Color.white : Color.black))
                                GUILayout.Box(GUIContent.none, CustomStyles.Gray, GUILayout.Width(20f));
                            using (new BackgroundColorScope((i & 2) == 2? Color.white : Color.black))
                                GUILayout.Box(GUIContent.none, CustomStyles.Gray, GUILayout.Width(20f));
                        }
                    }

                    using (new EditorGUILayout.VerticalScope())
                    {
                        var prop = SerializedObject.FindProperty(TileMeshConfig.Editor_ConfigDataPropName)
                            .GetArrayElementAtIndex(i).FindPropertyRelative(nameof(TileMeshConfig.MeshSet.Meshes));
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            EditorGUILayout.PropertyField(prop);
                            if (check.changed)
                                SerializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                CustomGUI.HSplitter();
                GUILayout.Space(5f);
            }

        }
    }
}