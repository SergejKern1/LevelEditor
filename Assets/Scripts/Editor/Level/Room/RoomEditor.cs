using System;
using Core.Editor.Interface;
using Core.Extensions;
using Editor.Level.Room.Editors;
using Editor.Level.Room.States;
using Level.Room;
using ScriptableUtility;
using UnityEditor;
using UnityEngine;

using static Core.Editor.Utility.CustomGUI;
using static Level.Room.Operations.RoomOperations;

namespace Editor.Level.Room
{
    public class RoomEditor : StateEditor
    {
        protected override Type ThisType => typeof(RoomEditor);
        public override string Name => StaticName;
        public static string StaticName => "Room Editor";

        public static RoomEditor CurrentEditor { get; private set; }
        public ref RoomInstanceData RoomInstance => ref m_instanceComponent.Data;

        public RoomEditorHeaderState Header;
        public bool RoomInitialized => m_instanceComponent != null 
                                       && RoomInstance.GameObject != null;

        #region Editor Prefs
        static readonly string k_editorPref_editorState = $"{nameof(RoomEditor)}.{nameof(m_state)}";
        #endregion

        RoomInstanceComponent m_instanceComponent;

        public void SetInstance(RoomInstanceComponent @value) => m_instanceComponent = @value;

        public override void Init(object parentContainer)
        {
            CurrentEditor = this;
            Header = new RoomEditorHeaderState(this);
            Header.Init(this);

            base.Init(parentContainer);

            if (RoomInitialized)
                RoomInstance.UpdateVisuals();
        }

        public override void Terminate()
        {
            base.Terminate();
            CurrentEditor = null;
        }

        public override void MainEditorGUI(float width)
        {
            base.MainEditorGUI(width);

            CurrentEditor = this;
            Header.OnGUI(width);

            if (!RoomInitialized)
            {
                NoRoomGUI();

                if (!RoomInitialized)
                    return;
            }

            RoomGUI(width);
        }

        void NoRoomGUI()
        {
            RoomConfig r = null;
            if (GUILayout.Button("Create Room"))
            {
                var rd = RoomPropertiesEditor.NewRoomData;
                r = rd.CreateRoomAsset();
                // r.UpdateResulting(0);
                r.RoomData = rd; // Global.Resources.Instance.StandardLevelSetting;
            }
            if (GUILayout.Button("Load Room"))
            {
                var path = EditorUtility.OpenFilePanel("Load Room", "Rooms", "asset");
                path = path.Replace(Application.dataPath, "Assets");
                r = AssetDatabase.LoadAssetAtPath(path, typeof(RoomConfig)) as RoomConfig;
            }

            if (r == null)
                return;

            // todo: check if needed
            RoomPlatformsEditor.Current.Reset();

            var go = new GameObject(r.RoomData.Name, typeof(ContextComponent), typeof(RoomInstanceComponent)) { hideFlags = HideFlags.DontSave };
            if (!go.TryGetComponent(out RoomInstanceComponent rc))
                return;

            m_instanceComponent = rc;
            rc.SetData(new RoomInstanceData(r, rc));

            RoomPlatformsEditor.Current?.SetCurrentLevel(0);
        }

        void RoomGUI(float width)
        {
            if (GUILayout.Button("Discard Room", GUILayout.Width(100)))
                RoomInstance.Destroy();
            if (GUILayout.Button("Save Room", GUILayout.Width(100)))
                SaveRoom();
            // LevelGUI();
            using (new GUILayout.VerticalScope("Editing Mode:", EditorStyles.helpBox, GUILayout.Width(width - 22)))
            {
                GUILayout.Space(15.0f);

                EditorStateButtons();
            }
        }

        void EditorStateButtons()
        {
            using (new GUILayout.HorizontalScope())
            {
                for (var i = 0; i < m_editorStates.Length; ++i)
                {
                    if (!m_editorStates[i].IsApplicable)
                        continue;

                    if (!ActivityButton(m_state == m_editorStates[i], m_editorStates[i].Name, GUILayout.Width(100)))
                        continue;

                    m_state = m_editorStates[i];
                    EditorPrefs.SetInt(k_editorPref_editorState, i);
                }
            }
        }

        void SaveRoom()
        {
            var r = RoomInstance.RoomConfig;
            var path = AssetDatabase.GetAssetPath(r);
            if (path.IsNullOrEmpty())
            {
                path = EditorUtility.SaveFilePanel("Save Room", "Rooms", r.name + ".asset", "asset");
                if (!path.IsNullOrEmpty())
                {
                    path = path.Replace(Application.dataPath, "Assets");
                    AssetDatabase.CreateAsset(r, path);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }

            EditorUtility.SetDirty(r);
            AssetDatabase.SaveAssets();
        }
    }
}
