using System;
using System.Collections.Generic;
using Core.Editor.Attribute;
using Core.Editor.Interface;
using Core.Extensions;
using Editor.Level.Room.States;
using JetBrains.Annotations;
using Level.PlatformLayer;
using Level.Room;
using Level.Room.Operations;
using UnityEditor;
using UnityEngine;

using static Core.Editor.Utility.CustomGUI;
using static Level.Room.Operations.RoomOperations;

namespace Editor.Level.Room.Editors
{
    [EditorState(typeof(RoomEditorHeaderState), 10)]
    [UsedImplicitly]
    public class RoomPlatformsEditor : IEditor, IEditorExtras, IRepaintable
    {
        public static RoomPlatformsEditor Current;
        public string Name => "Platforms";

        static bool RoomInitialized => RoomEditor.CurrentEditor?.RoomInitialized == true;
        static ref RoomInstanceData RoomInstance => ref RoomEditor.CurrentEditor.RoomInstance;
        public static RoomConfig Config => RoomInstance.RoomConfig;
        static List<PlatformLayerConfig> PlatformLayerList => Config != null ? Config.PlatformLayer : null;
        static int Count => PlatformLayerList.Count;
        public int CurrentLevel { get; private set; }
        public object ParentContainer { get; private set; }

        Action<SceneView> m_func;

        public PlatformLayerConfig CurrentPlatformLayer
        {
            get
            {
                var layerList = PlatformLayerList;
                if (layerList != null && CurrentLevel.IsInRange(layerList))
                    return layerList[CurrentLevel];
                return null;
            }
        }

        public void Init(object parentContainer)
        {
            ParentContainer = parentContainer;
            Current = this;
            
            m_func = sceneView => OnSceneGUI();
            SceneView.duringSceneGui += m_func;
        }

        void OnSceneGUI()
        {
            Current = this;

            var e = Event.current;
            if (e.shift && e.type == EventType.ScrollWheel)
            {
                e.Use();
                if (e.delta.y >= 1)
                    SetCurrentLevel(CurrentLevel + 1);
                else if (e.delta.y <= -1)
                    SetCurrentLevel(CurrentLevel - 1);
                Repaint();
            }
        }

        public void Terminate()
        {
            Current = null;
            SceneView.duringSceneGui -= m_func;
        }

        public void OnGUI(float width)
        {
            if (!RoomInitialized)
                return;
            Current = this;

            // how many levels does the room have
            using (new GUILayout.HorizontalScope())
            {
                var levels = EditorGUILayout.IntField("Levels", Count);
                if (GUILayout.Button("+", GUILayout.Width(50))) { levels++; }
                if (GUILayout.Button("-", GUILayout.Width(50))) { levels--; }
                ChangeLevelCount(levels);

                GUILayout.Space(15.0f);
                if (GUILayout.Button("^", GUILayout.Width(25))) { MoveLevelUp(); }
                if (GUILayout.Button("v", GUILayout.Width(25))) { MoveLevelDown(); }
                if (GUILayout.Button("x", GUILayout.Width(25))) { DeleteLevel(); }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Current Platform {CurrentLevel}", GUILayout.Width(125.0f));
                for (var i = 0; i < Count; ++i)
                {
                    if (ActivityButton(CurrentLevel == i, i.ToString()))
                        SetCurrentLevel(i);
                }
            }
        }

        public void SetCurrentLevel(int currentLevel)
        {
            CurrentLevel = Mathf.Clamp(currentLevel, 0, Count - 1);
            ref var inst = ref RoomInstance;
            inst.SetLevel(CurrentLevel);
        }

        static void MoveLevel(int a, int b)
        {
            var pl = PlatformLayerList;
            var aLvl = pl[a];
            var bLvl = pl[b];
            pl[a] = bLvl;
            pl[b] = aLvl;
        }

        void MoveLevelUp()
        {
            if (CurrentLevel >= Count - 1)
                return;

            MoveLevel(CurrentLevel, CurrentLevel + 1);

            // todo:
            //if (CurrentLevel > 0)
            //    _Room.UpdateResulting(CurrentLevel - 1);
            //_Room.UpdateResulting(CurrentLevel);
            //_Room.UpdateResulting(CurrentLevel + 1);

            ref var inst = ref RoomInstance;
            inst.UpdateVisuals();
            SetCurrentLevel(CurrentLevel + 1);
        }

        void MoveLevelDown()
        {
            if (CurrentLevel <= 0 || Count <= 1)
                return;

            MoveLevel(CurrentLevel, CurrentLevel - 1);

            // todo:
            //if (CurrentLevel > 1)
            //    _Room.UpdateResulting(CurrentLevel - 2);
            //_Room.UpdateResulting(CurrentLevel - 1);
            //_Room.UpdateResulting(CurrentLevel);

            ref var inst = ref RoomInstance;
            inst.UpdateVisuals();
            SetCurrentLevel(CurrentLevel - 1);
        }

        void DeleteLevel()
        {
            if (Count <= 1)
                return;

            PlatformLayerList.RemoveAt(CurrentLevel);
            SetCurrentLevel(Mathf.Clamp(CurrentLevel - 1, 0, Count));

            // todo:
            //if (CurrentLevel > 0)
            //    _Room.UpdateResulting(CurrentLevel - 1);
            //_Room.UpdateResulting(CurrentLevel);
            //if (_Room.Levels.Count > CurrentLevel + 1)
            //    _Room.UpdateResulting(CurrentLevel + 1);

            ref var inst = ref RoomInstance;

            inst.UpdateVisuals();
        }

        void ChangeLevelCount(int newLevelCount)
        {
            var prevCount = Count;

            // how many levels does the room have
            newLevelCount = Mathf.Clamp(newLevelCount, 1, Config.MaxPlatformCount);
            if (newLevelCount == Count)
                return;

            // todo: 
            //var currlvl = Mathf.Clamp(CurrentLevel, 0, newLevelCount - 1);
            //if (CurrentLevel!= currlvl)
            //{
            //    Undo.RecordObjects(new Object[] { this, _Room }, "Levels resized");
            //    CurrentLevel = currlvl;
            //}
            //else Undo.RecordObject(_Room, "Levels resized");

            Config.ChangeLevelCount(newLevelCount);

            ref var inst = ref RoomInstance;

            inst.UpdateVisuals();
        }


        public void Reset() => CurrentLevel = 0;

        public bool ShowAlways => true;
        public bool ShowLabel => false;


        public void Repaint()
        {
            switch (ParentContainer)
            {
                case EditorWindow w: w.Repaint(); break;
                case UnityEditor.Editor e: e.Repaint(); break;
                case IRepaintable r: r.Repaint(); break;
            }
        }
    }
}