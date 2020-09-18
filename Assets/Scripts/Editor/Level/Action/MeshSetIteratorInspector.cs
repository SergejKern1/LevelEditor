using Core.Editor.Inspector;
using Level.Actions;
using ScriptableUtility.Editor;
using ScriptableUtility.Editor.Actions;
using UnityEditor;


namespace Editor.Level.Action
{
    [CustomEditor(typeof(MeshSetIterator))]
    public class MeshSetIteratorInspector : BaseInspector<MeshSetIteratorEditor>{}
    public class MeshSetIteratorEditor : ScriptableBaseActionEditor<MeshSetIterator>
    {
        CommonActionEditorGUI.ActionMenuData m_menuData;
        CommonActionEditorGUI.ActionData m_actionData;

        public override void Terminate()
        {
            base.Terminate();
            ReleaseEditor(ref m_actionData);
        }

        public override void Init(object parentContainer)
        {
            base.Init(parentContainer);

            InitActionDataDefault(out m_actionData, "Continue with", MeshSetIterator.Editor_ContinueWithPropName);
            CreateMenu(ref m_menuData, (data) => OnTypeSelected(m_actionData, data));
        }

        public override void OnGUI(float width)
        {
            base.OnGUI(width);
            DefaultActionGUI(ref m_actionData, ref m_menuData);
        }
    }
}
