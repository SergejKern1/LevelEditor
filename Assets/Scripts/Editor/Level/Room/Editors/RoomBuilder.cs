using Core.Editor.Attribute;
using Core.Editor.Interface;
using Core.Extensions;
using Editor.Level.Room.States;
using JetBrains.Annotations;
using Level.Room;

namespace Editor.Level.Room.Editors
{
    [EditorState(typeof(RoomEditorTilesState), 10)]
    [UsedImplicitly]
    public class RoomEditorBuilderEditor : IEditor
    {
        RoomBuilderEditor m_editor;

        static ref RoomInstanceData RoomInstance => ref RoomEditor.CurrentEditor.RoomInstance;
        static RoomBuilder Builder => RoomInstance.RoomConfig != null ? RoomInstance.RoomConfig.Builder : null;

        public string Name => "Builder";
        public object ParentContainer { get; private set; }

        public void Init(object parentContainer)
        {
            ParentContainer = parentContainer;
            m_editor = new RoomBuilderEditor();
            m_editor.Init(this);
        }

        public void Terminate() => m_editor?.Terminate();

        public void OnGUI(float width)
        {
            m_editor.Target = Builder;

            if (RoomPlatformsEditor.Current == null)
                return;
            var lvl = RoomPlatformsEditor.Current.CurrentLevel;
            
            ref var roomInst = ref RoomInstance;
            if (!roomInst.VisualData.IsValid)
                return;
            var layerList = roomInst.RoomConfig.PlatformLayer;
            if (!layerList.IsIndexInRange(lvl))
                return;

            var visObj = roomInst.VisualData.GetFloorVisuals(lvl);
            m_editor.SetInputData(visObj.Go, layerList[lvl], visObj.Mesh, roomInst.RoomContext);
            m_editor?.OnGUI(width);
        }
    }
}