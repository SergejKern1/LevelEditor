using Core.Editor.Inspector;
using Core.Events;
using Core.Unity.Extensions;
using Editor.Level.Tiles;
using Level.Data;
using Level.PlatformLayer;
using Level.PlatformLayer.Interface;
using Level.Room;
using ScriptableUtility;
using ScriptableUtility.Editor.Actions;
using UnityEditor;
using UnityEngine;

using static ScriptableUtility.Editor.CommonActionEditorGUI;

namespace Editor.Level.Room
{
    [CustomEditor(typeof(RoomBuilder))]
    public class RoomBuilderInspector : BaseInspector<RoomBuilderEditor>{}
    public class RoomBuilderEditor : BaseActionEditor<RoomBuilder>, IEventListener<TileDrawing.TilesDrawn>
    {
        public override RoomBuilder Target
        {
            get => base.Target;
            set
            {
                if (base.Target == value)
                    return;
                base.Target = value;
                InitTarget();
            }
        }

        ActionMenuData m_menu = ActionMenuData.Default;

        Vector2 m_scrollPos;

        ActionData m_actionData;

        GameObject m_previewObject;
        IContext m_previewContext;
        Mesh m_previewMesh;
        PlatformLayerConfig m_platformLayer;

        bool BuildValid => m_platformLayer != null;

        public override void Init(object parentEditor)
        {
            base.Init(parentEditor);

            InitActionDataDefault(out m_actionData, "Action", nameof(RoomBuilder.Action));
            CreateMenu(ref m_menu, (data) => OnTypeSelected(m_actionData, data));

            EventMessenger.AddListener<TileDrawing.TilesDrawn>(this);

            InitTarget();
        }

        void InitTarget()
        {
            if (Target == null) return;
            Target.MeshData.GlobalValue = MeshData.Default;
            Target.TempMeshData.GlobalValue = MeshData.Default;
        }

        #region UnityMethods

        public override void Terminate()
        {
            base.Terminate();
            ReleaseEditor(ref m_actionData);
            if (m_previewObject != null)
                m_previewObject.DestroyEx();
            if (m_previewMesh != null)
                m_previewMesh.DestroyEx();

            m_previewObject = null;
            m_previewMesh = null;

            EventMessenger.RemoveListener<TileDrawing.TilesDrawn>(this);
        }

        public override void OnGUI(float width)
        {
            if (base.Target == null)
                return;
            base.OnGUI(width);

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_scrollPos = scroll.scrollPosition;
                EditorGUILayout.HelpBox("A RoomBuilder is a set of action to generate a Room", MessageType.Info);

                DefaultActionGUI(ref m_actionData, ref m_menu);

                if (check.changed)
                    OnChanged();
            }
            m_platformLayer = (PlatformLayerConfig) EditorGUILayout.ObjectField(m_platformLayer, 
                typeof(PlatformLayerConfig), false);
            using (new EditorGUI.DisabledGroupScope(!BuildValid))
            {
                if (GUILayout.Button("Build")) 
                    OnBuild();
            }
        }

        void OnBuild()
        {
            if (!BuildValid)
                return;

            InstantiatePreviewObjects();

            Target.BuildGrid(new IPlatformLayer[]{ m_platformLayer });
            Target.Build(m_previewContext, 0, m_previewMesh);
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
                mr.sharedMaterial = Target.Material;

                var prevContext = m_previewObject.AddComponent<ContextComponent>();
                prevContext.Init();
                m_previewContext = prevContext;
            }
            else if (m_previewObject.TryGetComponent(out MeshFilter mf))
                mf.mesh = m_previewMesh;
        }
        #endregion

        public void OnEvent(TileDrawing.TilesDrawn eventType) => OnBuild();

        public void SetInputData(GameObject go, PlatformLayerConfig plc, Mesh m, IContext c)
        {
            m_previewObject = go;
            m_platformLayer = plc;
            m_previewMesh = m;
            m_previewContext = c;
        }
    }
}