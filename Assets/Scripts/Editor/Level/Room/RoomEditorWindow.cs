using UnityEditor;

namespace Editor.Level.Room
{
    public class RoomEditorWindow : EditorWindow
    {
        RoomEditor m_roomEditor;

        [MenuItem("Tools/RoomEditor")]
        public static void OpenWindow()
        {
            var roomEditor = GetWindow(typeof(RoomEditorWindow), false, RoomEditor.StaticName, true);
            roomEditor.Show();
        }

        void OnEnable()
        {
            m_roomEditor = new RoomEditor();
            m_roomEditor.Init(this);
        }

        void OnDisable() 
            => m_roomEditor.Terminate();

        void OnGUI() => m_roomEditor.OnGUI(position.width);

        void Update() 
            => m_roomEditor.Update();
    }
}
