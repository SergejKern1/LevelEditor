using Core.Editor.Attribute;
using Core.Editor.Interface;
using Editor.Level.PlatformLayer;
using Editor.Level.Room.States;
using JetBrains.Annotations;
using Level.PlatformLayer;
using UnityEngine;

namespace Editor.Level.Room.Editors
{
    [EditorState(typeof(RoomEditorTilesState), 10)]
    [UsedImplicitly]
    public class TileDrawing : IEditor, IEditorExtras
    {
        //public static TileDrawing Current;
        //static RoomInstanceData RoomInstance => RoomEditor.CurrentEditor.RoomInstance;
        static PlatformLayerConfig LayerConfig => RoomPlatformsEditor.Current.CurrentPlatformLayer;

        PlatformEditor m_editor;

        public string Name => "Tile Drawing";
        public object ParentContainer { get; private set; }
        public bool ShowAlways => true;
        public bool ShowLabel => false;

        public void Init(object parentContainer)
        {
            ParentContainer = parentContainer;
            m_editor = new PlatformEditor();
            m_editor.Init(this);
        }

        public void Terminate() => m_editor?.Terminate();

        public void OnGUI(float width)
        {
            Debug.Log(RoomPlatformsEditor.Current.CurrentLevel);
            m_editor.Target = LayerConfig;
            m_editor?.OnGUI(width);
        }
    }
}