using Level.Actions;
using Level.Tiles.Interface;
using ScriptableUtility.Editor;
using ScriptableUtility.Editor.Actions;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Action
{
    [CustomEditor(typeof(TileTypeSwitch))]
    public class TileTypeSwitchInspector : ScriptableBaseActionInspector
    {
        // ReSharper disable once InconsistentNaming
        new TileTypeSwitch target => base.target as TileTypeSwitch;

        CommonActionEditorGUI.ActionMenuData m_menuData = CommonActionEditorGUI.ActionMenuData.Default;
        Vector2 m_scrollPos;

        CommonActionEditorGUI.ActionListData m_actionsList;

        ITileConfig m_cacheTileConfig;

        public override void Init()
        {
            base.Init();

            InitActionListDataDefault(out m_actionsList, TileTypeSwitch.Editor_ActionProperty, "{0}");
            CreateMenu(ref m_menuData, (data) => OnTypeSelected(m_actionsList, data));

            SetLabels();
        }

        void SetLabels()
        {
            m_actionsList.CustomLabels = null;
            m_cacheTileConfig = target.Editor_TileConfig;
            if (m_cacheTileConfig == null)
                return;
            m_actionsList.CustomLabels = new GUIContent[m_cacheTileConfig.Count];
            for (var i = 0; i < m_cacheTileConfig.Count; i++)
                m_actionsList.CustomLabels[i] = new GUIContent(m_cacheTileConfig[i].TileName);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ReleaseEditor(ref m_actionsList);
        }

        public override void OnInspectorGUI()
        {
            NameGUI(TileTypeSwitch.Editor_NameProperty);
            DrawDefaultInspector();

            if (m_cacheTileConfig != target.Editor_TileConfig)
                SetLabels();

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_scrollPos = scroll.scrollPosition;

                UpdateActionListData(ref m_actionsList);
                ListGUI(ref m_actionsList, ref m_menuData);

                if (check.changed)
                    OnChanged();
            }
        }
    }
}
