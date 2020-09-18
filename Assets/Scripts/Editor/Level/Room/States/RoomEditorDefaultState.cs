using System;
using Core.Editor.Attribute;
using Core.Editor.Interface;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Room.States
{
    [ParentEditor(typeof(RoomEditor))]
    public class RoomEditorDefaultState : EditorState
    {
        protected override Type ThisType => typeof(RoomEditorDefaultState);

        protected override void HeaderGUI(float width)
        {
            EditorGUILayout.LabelField("Default");
        }

        public override string Name => "Default";
        public override Type ParentEditor => typeof(RoomEditor);
        public override bool IsApplicable => RoomEditor.CurrentEditor != null 
                                             && RoomEditor.CurrentEditor.RoomInitialized;
    }
}
