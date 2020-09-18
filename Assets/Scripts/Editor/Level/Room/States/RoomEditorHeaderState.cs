using System;
using Core.Editor.Interface;
using UnityEngine;

namespace Editor.Level.Room.States
{
    public class RoomEditorHeaderState : EditorState
    {
        public RoomEditorHeaderState(IEditor parent) 
            => SetParent(parent);

        protected override Type ThisType => typeof(RoomEditorHeaderState);

        protected override void HeaderGUI(float width)
        {
        }

        public override string Name => "Header";
        public override Type ParentEditor => typeof(RoomEditor);
        public override bool IsApplicable => true;
    }
}
