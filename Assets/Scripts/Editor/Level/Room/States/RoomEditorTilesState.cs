using System;
using Core.Editor.Attribute;
using Core.Editor.Interface;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Room.States
{
    [ParentEditor(typeof(RoomEditor))]
    public class RoomEditorTilesState : EditorState
    {
        protected override Type ThisType => typeof(RoomEditorTilesState);

        protected override void HeaderGUI(float width)
        {
            EditorGUILayout.LabelField("Tiles");
        }

        public override string Name => "Tiles";
        public override Type ParentEditor => typeof(RoomEditor);
        public override bool IsApplicable => RoomEditor.CurrentEditor != null 
                                             && RoomEditor.CurrentEditor.RoomInitialized;
    }
}
