using Core.Editor.Inspector;
using Level.Tiles.Actions;
using ScriptableUtility.Editor;
using ScriptableUtility.Editor.Actions;
using UnityEditor;


namespace Editor.Level.Action
{
    [CustomEditor(typeof(TileTypeIterator))]
    public class TileTypeIteratorInspector : BaseInspector<TileTypeIteratorEditor>{}
    public class TileTypeIteratorEditor : ScriptableBaseActionEditor<TileTypeIterator>
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

            InitActionDataDefault(out m_actionData, "Continue with", TileTypeIterator.Editor_ContinueWithPropName);
            CreateMenu(ref m_menuData, (data) => OnTypeSelected(m_actionData, data));
        }

        public override void OnGUI(float width)
        {
            base.OnGUI(width);
            DefaultActionGUI(ref m_actionData, ref m_menuData);
        }
    }
}
