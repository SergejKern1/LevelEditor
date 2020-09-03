using Level.Tiles.Actions;
using ScriptableUtility.Editor;
using ScriptableUtility.Editor.Actions;
using UnityEditor;


namespace Editor.Level.Action
{
    [CustomEditor(typeof(TileTypeIterator))]
    public class TileTypeIteratorInspector : ScriptableBaseActionInspector
    {
        // ReSharper disable once InconsistentNaming
        new TileTypeIterator target => base.target as TileTypeIterator;

        CommonActionEditorGUI.ActionMenuData m_menuData;
        CommonActionEditorGUI.ActionData m_actionData;

        void OnDisable() => ReleaseEditor(ref m_actionData);

        public override void Init()
        {
            base.Init();
            InitActionDataDefault(out m_actionData, "Continue with", TileTypeIterator.Editor_ContinueWithPropName);
            CreateMenu(ref m_menuData, (data) => OnTypeSelected(m_actionData, data));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DefaultActionGUI(ref m_actionData, ref m_menuData);
        }
    }
}
