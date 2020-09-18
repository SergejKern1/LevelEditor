using Core.Editor.Attribute;
using Core.Editor.Extensions;
using Core.Editor.Interface;
using Core.Types;
using Editor.Level.Room.States;
using JetBrains.Annotations;
using Level;
using Level.Room;
using Level.Room.Operations;
using Level.Tiles;
using UnityEditor;
using UnityEngine;

namespace Editor.Level.Room.Editors
{
    [EditorState(typeof(RoomEditorHeaderState))]
    [UsedImplicitly]
    public class RoomPropertiesEditor : IEditor
    {
        public string Name => "Room Properties";

        static bool RoomInitialized => RoomEditor.CurrentEditor?.RoomInitialized == true;
        static ref RoomInstanceData RoomInstance => ref RoomEditor.CurrentEditor.RoomInstance;
        public object ParentContainer { get; private set; }

        static ref RoomData GetRoomData()
        {
            if (RoomInitialized)
                return ref RoomInstance.GetRoomData();
            return ref m_newRoomData;
        }
        static RoomData m_newRoomData;
        public static RoomData NewRoomData => m_newRoomData;

        #region Editor Prefs
        static readonly string k_editorPref_roomSize = $"{nameof(RoomEditor)}.{nameof(RoomData.Size)}";
        static readonly string k_editorPref_defaultGridSettings = $"{nameof(RoomEditor)}.{nameof(RoomSettings.Grid)}";
        static readonly string k_editorPref_defaultTileSet = $"{nameof(RoomEditor)}.{nameof(RoomSettings.TileSet)}";
        static readonly string k_editorPref_defaultBuilder = $"{nameof(RoomEditor)}.{nameof(RoomSettings.Builder)}";
        static readonly string k_editorPref_defaultFloorAndWallMaterial = $"{nameof(RoomEditor)}.{nameof(RoomSettings.FloorAndWallMaterial)}";
        static readonly string k_editorPref_defaultCeilingMaterial = $"{nameof(RoomEditor)}.{nameof(RoomSettings.CeilingMaterial)}";
        #endregion

        public void Init(object parentContainer)
        {
            ParentContainer = parentContainer;
            LoadDefaultRoomSettings();
        }

        public void Terminate() { }

        public void OnGUI(float width)
        {
            var data = GetRoomData();

            if (RoomDataGUI(width, ref data) != ChangeCheck.Changed) 
                return;
            if (RoomInitialized)
            {
                ref var roomInst = ref RoomInstance;
                Undo.RecordObjects(new Object[] { roomInst.RoomConfig, roomInst.GameObject },
                    "Room-Data");
                roomInst.UpdateData(data);
            }
            else m_newRoomData = data;
        }

        ChangeCheck RoomDataGUI(float width, ref RoomData data)
        {
            var changed = ChangeCheck.NotChanged;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Space(15.0f);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name: ", GUILayout.Width(50));
                    data.Name = EditorGUILayout.TextField(data.Name);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(5.0f);
                var gs = data.Settings.Grid;
                var size = data.Size;
                if (gs != null)
                {
                    // size of room
                    size = new Vector3Int(data.Size.x / gs.RoomChunkSize, data.Size.y / gs.RoomYChunkSize,
                        data.Size.z / gs.RoomChunkSize);
                }

                var settings = data.Settings;
                if (RoomDataSettingsGUI(ref settings) == ChangeCheck.Changed)
                {
                    changed = ChangeCheck.Changed;
                    data.Settings = settings;
                }

                if (RoomSizeGUI(ref size) == ChangeCheck.Changed)
                {
                    changed = ChangeCheck.Changed;
                    var newSize = new Vector3Int(size.x * gs.RoomChunkSize, size.y * gs.RoomYChunkSize,
                        size.z * gs.RoomChunkSize);
                    data.Size = newSize;
                }

                if (check.changed)
                    changed = ChangeCheck.Changed;

                return changed;
            }
        }

        static ChangeCheck RoomDataSettingsGUI(ref RoomSettings settings)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                settings.TileSet = (TilesSetListConfig)EditorGUILayout.ObjectField("Tile Set", settings.TileSet,
                    typeof(TilesSetListConfig), false);
                settings.Grid = (GridSettings)EditorGUILayout.ObjectField("Grid Settings", settings.Grid,
                    typeof(GridSettings), false);
                settings.Builder = (RoomBuilder)EditorGUILayout.ObjectField("Builder", settings.Builder,
                    typeof(RoomBuilder), false);

                settings.CeilingMaterial = (Material)EditorGUILayout.ObjectField("Ceiling Material", settings.CeilingMaterial,
                    typeof(Material), false);
                settings.FloorAndWallMaterial = (Material)EditorGUILayout.ObjectField("Floor/Wall Material", settings.FloorAndWallMaterial,
                    typeof(Material), false);

                if (!check.changed)
                    return ChangeCheck.NotChanged;
                SaveDefaultRoomSettings(ref settings);
                return ChangeCheck.Changed;
            }
        }

        void LoadDefaultRoomSettings()
        {
            m_newRoomData.Size = CustomEditorPrefs.GetVector3Int(k_editorPref_roomSize, Vector3Int.one);

            m_newRoomData.Settings.Grid = CustomEditorPrefs.GetAssetFromGuid<GridSettings>(k_editorPref_defaultGridSettings);
            m_newRoomData.Settings.TileSet = CustomEditorPrefs.GetAssetFromGuid<TilesSetListConfig>(k_editorPref_defaultTileSet);
            m_newRoomData.Settings.Builder = CustomEditorPrefs.GetAssetFromGuid<RoomBuilder>(k_editorPref_defaultBuilder);
            m_newRoomData.Settings.FloorAndWallMaterial = CustomEditorPrefs.GetAssetFromGuid<Material>(k_editorPref_defaultFloorAndWallMaterial);
            m_newRoomData.Settings.CeilingMaterial = CustomEditorPrefs.GetAssetFromGuid<Material>(k_editorPref_defaultCeilingMaterial);
        }
        static void SaveDefaultRoomSettings(ref RoomSettings settings)
        {
            CustomEditorPrefs.SetGuid(k_editorPref_defaultGridSettings, settings.Grid);
            CustomEditorPrefs.SetGuid(k_editorPref_defaultTileSet, settings.TileSet);
            CustomEditorPrefs.SetGuid(k_editorPref_defaultBuilder, settings.Builder);
            CustomEditorPrefs.SetGuid(k_editorPref_defaultFloorAndWallMaterial, settings.FloorAndWallMaterial);
            CustomEditorPrefs.SetGuid(k_editorPref_defaultCeilingMaterial, settings.CeilingMaterial);
        }

        ChangeCheck RoomSizeGUI(ref Vector3Int size)
        {
            ref var rd = ref GetRoomData();
            var changed = ChangeCheck.NotChanged;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Size-X", GUILayout.Width(50.0f));
                var xSize = Mathf.Max(1, EditorGUILayout.IntField(size.x, GUILayout.Width(25.0f)));
                EditorGUILayout.LabelField("Size-Y", GUILayout.Width(50.0f));
                var ySize = Mathf.Max(1, EditorGUILayout.IntField(size.y, GUILayout.Width(25.0f)));
                EditorGUILayout.LabelField("Size-Z", GUILayout.Width(50.0f));
                var zSize = Mathf.Max(1, EditorGUILayout.IntField(size.z, GUILayout.Width(25.0f)));

                if (xSize != size.x || zSize != size.z || ySize != size.y)
                {
                    changed = ChangeCheck.Changed;
                    size = new Vector3Int(xSize, ySize, zSize);
                }

                var gs = rd.Settings.Grid;
                if (gs != null)
                    EditorGUILayout.LabelField($"Actual size: x {xSize * gs.RoomChunkSize} y {ySize * gs.RoomYChunkSize} z { zSize * gs.RoomChunkSize}");
            }

            if (changed != ChangeCheck.Changed)
                return changed;

            CustomEditorPrefs.SetVector3Int(k_editorPref_roomSize, size);
            return changed;
        }
    }
}