using Core.Editor.Inspector;
using Level.Room;
using Level.Room.Operations;
using UnityEditor;

namespace Editor.Level.Room
{
    [CustomEditor(typeof(RoomInstanceComponent))]
    public class RoomInstanceInspector : BaseInspector<RoomInstanceEditor>{}
    public class RoomInstanceEditor: BaseEditor<RoomInstanceComponent>
    {
        RoomEditor m_roomEditor;

        public override void Init(object parentContainer)
        {
            base.Init(parentContainer);
            m_roomEditor = new RoomEditor();
            m_roomEditor.Init(this);
        }

        public override void OnTargetChanged()
        {
            base.OnTargetChanged();
            Target.Data.UpdateVisuals();
        }

        public override void OnGUI(float width)
        {
            base.OnGUI(width);
            Target.UpdatePosition();

            m_roomEditor.SetInstance(Target);
            m_roomEditor.OnGUI(width);
        }

        public override void Terminate()
        {
            m_roomEditor.Terminate();
            base.Terminate();
        }
    }
}