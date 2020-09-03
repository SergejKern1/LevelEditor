using System;
using Editor.Level.Tiles;
using Level.Data;
using Level.PlatformLayer;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using static Editor.Level.Tools.SizePositionTools;
using static Editor.Level.Tiles.TileDrawing;
using static Core.Editor.Utility.GUIStyles.CustomStyles;
using static Core.Editor.Extensions.EditorExtensions;

namespace Editor.Level.PlatformLayer
{
    [CustomEditor(typeof(PlatformLayerScriptable))]
    public class PlatformLayerScriptableInspector : UnityEditor.Editor
    {
        Color m_drawBoundsColor = Color.green;

        // ReSharper disable once InconsistentNaming
        new PlatformLayerScriptable target => base.target as PlatformLayerScriptable;
        static PlatformLayerScriptableInspector m_currentInspector;

        static readonly SizeData k_roomSizeDat = new SizeData() { Size = new Vector3Int(int.MaxValue-1, int.MaxValue-1, int.MaxValue-1) };

        TileDrawingOpData m_tileDrawingData;
        readonly AnimBool m_showSizeTools = new AnimBool();

        Action<SceneView> m_func;

        TilesSetDebugData[] m_debugData = new TilesSetDebugData[1];
        SizePositionCategory m_sizePositionCategory;

        bool m_2dMode;
        //int m_currentlySelected;

        void OnEnable()
        {
            if (m_currentInspector != null)
                return;
            Init_SizePositionTools(m_showSizeTools, this);

            InitDebugData();

            m_tileDrawingData.Platform = target;
            m_tileDrawingData.Tiles = target;
            Init_TilesMode(ref m_tileDrawingData, this);

            m_currentInspector = this;
            m_func = sceneView => OnSceneGUI();
            SceneView.duringSceneGui += m_func;
        }

        void InitDebugData()
        {
            m_drawBoundsColor = EditorPrefs_GetColor($"{nameof(PlatformLayerScriptableInspector)}.{nameof(m_drawBoundsColor)}");

            if (target == null)
                return;
            var set = target.TilesSet;
            if (set == null)
                return;
            var sets = set.TileDataSets;
            if (sets == null)
                return;

            var count = sets.Count;
            Array.Resize(ref m_debugData, count);
            for (var i = 0; i < sets.Count; i++) m_debugData[i] = LoadDebugSettings(i);
        }

        public void OnDisable()
        {
            if (m_currentInspector!= this)
                return;
            if (m_func == null)
                return;

            // ReSharper disable once DelegateSubtraction
            SceneView.duringSceneGui -= m_func;
            m_func = null;
            m_currentInspector = null;
        }

        public void OnSceneGUI()
        {
            if (target.TilesSet == null)
                return;

            if (SceneView.lastActiveSceneView != null)
                m_2dMode = SceneView.lastActiveSceneView.in2DMode;

            SceneGUIDraw();
            SceneGUIEdit();
        }

        void SceneGUIEdit()
        {
            var e = Event.current;

            if (e.alt)
                return;
            if (e.button == 2) // middle mouse button
                return;
            // prevent default SceneGUI stuff part 1, e.Use() for part2
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(controlId);

            SizeSceneGUI(m_sizePositionCategory, target, k_roomSizeDat);

            var groundPlane = new Plane(m_2dMode ? Vector3.back : Vector3.up, target.PositionOffset.y * (m_2dMode? Vector3.back : Vector3.up));
            var ray = HandleUtility.GUIPointToWorldRay(new Vector2(e.mousePosition.x, e.mousePosition.y));
            // Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, sceneView.position.height - e.mousePosition.y - 16, 0.0f));
            if (!groundPlane.Raycast(ray, out var distance))
                return;
            var wPos = ray.GetPoint(distance);
            var cursorPos = wPos;
            if (m_2dMode)
            {
                var y = wPos.y;
                var z = wPos.z;
                wPos = new Vector3(wPos.x, z, y);
            }
            SceneGUI_TilesMode(Event.current, ref m_tileDrawingData, wPos, cursorPos);

            if (e.type != EventType.Repaint && e.type != EventType.Layout)
                SceneView.RepaintAll();
        }

        void SceneGUIDraw()
        {
            var count = Mathf.Min(m_debugData.Length, target.TilesSet.TileDataSets.Count);
            for (var i = 0; i < count; i++)
            {
                var set = target.TilesSet.TileDataSets[i];
                var usingSet = (set.Mask & m_tileDrawingData.TileTypesMask) != 0;
                var debug = m_debugData[i];
                if (debug.DrawOnlyWhenUsing && !usingSet)
                    continue;

                if (m_2dMode)
                {
                    debug.Settings.YMult = new Vector3Int(0,0,1);
                    debug.Settings.ZMult = Vector3Int.up;
                }
                DebugDraw.DrawGeometryType(target, set, debug.Settings);
            }

            if (m_2dMode)
                return;
            DebugDraw.DrawPlatformBounds(target, target.PositionOffset, target.GridSize, target.GridYSize, m_drawBoundsColor);
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_drawBoundsColor = EditorGUILayout.ColorField("Bounds-Color: ", m_drawBoundsColor);
                if (check.changed)
                    EditorPrefs_SetColor($"{nameof(PlatformLayerScriptableInspector)}.{nameof(m_drawBoundsColor)}", 
                        m_drawBoundsColor);
            }   
            
            base.OnInspectorGUI();

            GUILayout.Label($"Position: {target.Position} Size: {target.Size}", EditorStyles.helpBox);

            if (target.TilesSet == null)
                return;

            SizeToolsCategoryGUI(ref m_sizePositionCategory);
            //SizeToolsGUI(m_showSizeTools, target, target, k_roomSizeDat);

            var count = Mathf.Min(m_debugData.Length, target.TilesSet.TileDataSets.Count);
            for (var i = 0; i < count; i++)
            {
                var set = target.TilesSet.TileDataSets[i];
                bool active;
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(Screen.width)))
                {
                    //GUILayout.Label($"{set.TileConfig.name}: ",GUILayout.Width(150f));
                    active = TileTypesActiveGUI(ref m_tileDrawingData, set);
                    GUILayout.Space(15f);
                    m_debugData[i].Foldout = EditorGUILayout.Foldout(m_debugData[i].Foldout, "debug settings", true, MiniFoldoutStyle);
                    if (m_debugData[i].Foldout)
                    {
                        using (var check = new EditorGUI.ChangeCheckScope())
                        using (new EditorGUILayout.VerticalScope())
                        {
                            m_debugData[i].DrawOnlyWhenUsing =
                                EditorGUILayout.Toggle("Draw only when using", m_debugData[i].DrawOnlyWhenUsing);
                            DebugSettings(ref m_debugData[i].Settings);

                            if (check.changed) 
                                SaveDebugSettings(i, m_debugData[i]);
                        }
                    }
                }
                if (active)
                    TileSelectionGUI(ref m_tileDrawingData, set);
            }

            BrushTypeSizeGUI(ref m_tileDrawingData);
        }

        void DebugSettings(ref DebugDraw.DebugDrawSettings settings)
        {
            settings.DrawLabel = EditorGUILayout.Toggle("Draw Label", settings.DrawLabel);
            settings.DrawDots = EditorGUILayout.Toggle("Draw Dots", settings.DrawDots);
            settings.DrawDebugTiles =
                EditorGUILayout.Toggle("Draw Debug-Tiles", settings.DrawDebugTiles);
            //if (m_debugData[i].DrawDebugTiles)
            using (new EditorGUI.DisabledScope(!settings.DrawDebugTiles))
            {
                settings.DebugTileSize =
                    EditorGUILayout.FloatField("Debug TileSize: ", settings.DebugTileSize);
            }
            settings.ColorAlpha = EditorGUILayout.FloatField("Color Alpha: ", settings.ColorAlpha);
        }

        void SaveDebugSettings(int i, TilesSetDebugData debugData)
        {
            var pre = $"{nameof(PlatformLayerScriptableInspector)}.{nameof(m_debugData)}[{i}].";
            EditorPrefs.SetBool(pre + $"{nameof(TilesSetDebugData.DrawOnlyWhenUsing)}", debugData.DrawOnlyWhenUsing);
            EditorPrefs.SetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawDots)}", debugData.Settings.DrawDots);
            EditorPrefs.SetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawDebugTiles)}", debugData.Settings.DrawDebugTiles);
            EditorPrefs.SetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawLabel)}", debugData.Settings.DrawLabel);
            EditorPrefs.SetFloat(pre + $"{nameof(DebugDraw.DebugDrawSettings.DebugTileSize)}", debugData.Settings.DebugTileSize);
            EditorPrefs.SetFloat(pre + $"{nameof(DebugDraw.DebugDrawSettings.ColorAlpha)}", debugData.Settings.ColorAlpha);
        }
        TilesSetDebugData LoadDebugSettings(int i)
        {
            var pre = $"{nameof(PlatformLayerScriptableInspector)}." +
                      $"{nameof(m_debugData)}[{i}].";
            var loaded = TilesSetDebugData.Default;
            loaded.DrawOnlyWhenUsing = EditorPrefs.GetBool(pre + $"{nameof(TilesSetDebugData.DrawOnlyWhenUsing)}", TilesSetDebugData.Default.DrawOnlyWhenUsing);
            loaded.Settings.DrawDots = EditorPrefs.GetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawDots)}", DebugDraw.DebugDrawSettings.Default.DrawDots);
            loaded.Settings.DrawDebugTiles = EditorPrefs.GetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawDebugTiles)}", DebugDraw.DebugDrawSettings.Default.DrawDebugTiles);
            loaded.Settings.DrawLabel = EditorPrefs.GetBool(pre + $"{nameof(DebugDraw.DebugDrawSettings.DrawLabel)}", DebugDraw.DebugDrawSettings.Default.DrawLabel);
            loaded.Settings.DebugTileSize = EditorPrefs.GetFloat(pre + $"{nameof(DebugDraw.DebugDrawSettings.DebugTileSize)}", DebugDraw.DebugDrawSettings.Default.DebugTileSize);
            loaded.Settings.ColorAlpha = EditorPrefs.GetFloat(pre + $"{nameof(DebugDraw.DebugDrawSettings.ColorAlpha)}", DebugDraw.DebugDrawSettings.Default.ColorAlpha);
            return loaded;
        }
    }

    public struct TilesSetDebugData
    {
        public bool Foldout;
        public bool DrawOnlyWhenUsing;
        public DebugDraw.DebugDrawSettings Settings;

        public static TilesSetDebugData Default = new TilesSetDebugData()
        {
            Foldout = false,
            DrawOnlyWhenUsing = true,
            Settings = DebugDraw.DebugDrawSettings.Default
        };
    }
}