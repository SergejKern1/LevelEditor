using System;
using System.Collections.Generic;
using Core.Editor.Attribute;
using Core.Editor.Extensions;
using Core.Editor.Inspector;
using Core.Events;
using Core.Types;
using Core.Unity.Extensions;
using Editor.Level.Tiles;
using Level.Data;
using Level.Room;
using Level.Tiles.Actions;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Editor.Actions;
using UnityEditor;
using UnityEngine;
using StrPair = System.Tuple<string, string>;
using static Core.Editor.Extensions.EditorExtensions;
using static ScriptableUtility.Editor.CommonActionEditorGUI;

namespace Editor.Level.Room
{
    [CustomEditor(typeof(RoomBuilder))]
    public class RoomBuilderInspector : ScriptableBaseActionInspector, IEventListener<TileDrawing.TilesDrawn>
    {
        public static RoomBuilderInspector CurrentInspector;

        // ReSharper disable once InconsistentNaming
        new RoomBuilder target => base.target as RoomBuilder;

        ActionMenuData m_menu = ActionMenuData.Default;
        List<Type> m_actionTypes = new List<Type>();
        Vector2 m_scrollPos;

        ActionData m_actionData;

        GameObject m_previewObject;
        ContextComponent m_previewContext;
        Mesh m_previewMesh;

        #region UnityMethods
        void OnDisable()
        {
            ReleaseEditor(ref m_actionData);
            if (m_previewObject != null)
                m_previewObject.DestroyEx();
            if (m_previewMesh != null)
                m_previewMesh.DestroyEx();

            m_previewObject = null;
            m_previewMesh = null;

            EventMessenger.RemoveListener(this);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CurrentInspector = this;

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_scrollPos = scroll.scrollPosition;
                EditorGUILayout.HelpBox("A RoomBuilder is a set of action to generate a Room", MessageType.Info);

                DefaultActionGUI(ref m_actionData, ref m_menu);

                if (check.changed)
                    OnChanged();
            }

            if (GUILayout.Button("Build")) 
                OnBuild();
        }

        void OnBuild()
        {
            InstantiatePreviewObjects();
            target.MeshObject.GlobalValue = m_previewMesh;
            target.MeshData.GlobalValue = MeshData.Default;
            target.TempMeshData.GlobalValue = MeshData.Default;
            
            var prevAction = target.Action.CreateAction(m_previewContext);
            if (prevAction is IDefaultAction def)
                def.Invoke();

            m_previewMesh.Clear();

            m_previewMesh.vertices = target.MeshData.GlobalValue.Vertices.ToArray();
            m_previewMesh.uv = target.MeshData.GlobalValue.UVs.ToArray();
            m_previewMesh.triangles = target.MeshData.GlobalValue.Triangles.ToArray();
        }

        void InstantiatePreviewObjects()
        {
            if (m_previewMesh == null)
                m_previewMesh = new Mesh();
            if (m_previewObject == null)
            {
                m_previewObject = new GameObject("Preview") {hideFlags = HideFlags.DontSave | HideFlags.NotEditable};
                var mf = m_previewObject.AddComponent<MeshFilter>();
                mf.mesh = m_previewMesh;

                var mr = m_previewObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = target.Material;

                var cc = m_previewObject.AddComponent<ContextComponent>();
                cc.Init();
            }
            else if (m_previewObject.TryGetComponent(out MeshFilter mf))
                mf.mesh = m_previewMesh;
        }
        #endregion

        public override void Init()
        {
            base.Init();
            InitActionDataDefault(out m_actionData, "Action", nameof(RoomBuilder.Action));
            CreateMenu(ref m_menu, (data) => OnTypeSelected(m_actionData, data));

            EventMessenger.AddListener(this);
        }

        public void OnEvent(TileDrawing.TilesDrawn eventType) => OnBuild();
    }
}