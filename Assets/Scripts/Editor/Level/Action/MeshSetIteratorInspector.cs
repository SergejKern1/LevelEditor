using Level.Actions;
using ScriptableUtility.Editor;
using ScriptableUtility.Editor.Actions;
using UnityEditor;


namespace Editor.Level.Action
{
    [CustomEditor(typeof(MeshSetIterator))]
    public class MeshSetIteratorInspector : ScriptableBaseActionInspector
    {
        // ReSharper disable once InconsistentNaming
        new MeshSetIterator target => base.target as MeshSetIterator;

        CommonActionEditorGUI.ActionMenuData m_menuData;
        CommonActionEditorGUI.ActionData m_actionData;

        public override void OnDisable()
        {
            base.OnDisable();
            ReleaseEditor(ref m_actionData);
        }

        public override void Init()
        {
            base.Init();
            InitActionDataDefault(out m_actionData, "Continue with", MeshSetIterator.Editor_ContinueWithPropName);
            CreateMenu(ref m_menuData, (data) => OnTypeSelected(m_actionData, data));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DefaultActionGUI(ref m_actionData, ref m_menuData);
        }
    }
}
