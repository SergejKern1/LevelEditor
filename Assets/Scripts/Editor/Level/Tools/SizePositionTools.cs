using System;
using Core.Types;
using Core.Unity.Utility.GUITools;
using Level.Data;
using Level.Data.Interface;
using Level.Enums;
using Level.PlatformLayer;
using Level.PlatformLayer.Interface;
using Level.Room.Ext;

using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using static Core.Editor.Utility.GUIStyles.CustomStyles;
using static Core.Editor.Utility.CustomGUI;
using static Level.PlatformLayer.Operations;

using Object = UnityEngine.Object;

namespace Editor.Level.Tools
{
    public static class SizePositionTools
    {
        static bool m_sizeToolsAffectRoom;

        #region Editor Prefs
        const string k_editorPrefShowSizeTools = "RoomEditor.ShowSizeTools";
        const string k_editorPrefSizeToolsAffectRoom = "RoomEditor.SizeToolsAffectRoom";
        #endregion

        public static void Init_SizePositionTools(AnimBool showTools, Object editorObj)
        {
            showTools.valueChanged.RemoveAllListeners();

            var editor = editorObj as UnityEditor.Editor;
            var editorW = editorObj as EditorWindow;

            if (editorW != null)
                showTools.valueChanged.AddListener(editorW.Repaint);
            else if (editor != null)
                showTools.valueChanged.AddListener(editor.Repaint);
            else throw new ArgumentException($"{editorObj} expected to be of type {nameof(Editor)} or {nameof(EditorWindow)}");

            showTools.target = EditorPrefs.GetBool(k_editorPrefShowSizeTools, false);
            m_sizeToolsAffectRoom = EditorPrefs.GetBool(k_editorPrefSizeToolsAffectRoom, false);
        }

        public static void SizeToolsGUI(AnimBool showTools, IPlatformLayerSpatialData platform, IPlatformLayerTiles tiles, SizeData roomSize)
        {
            if (SizeToolsGUI(showTools, out var mod, false) != ChangeCheck.Changed)
                return;

            ApplySizeTool(platform, tiles, roomSize, mod);
        }

        public static void SizeToolsCategoryGUI(ref SizePositionCategory cat) => cat = (SizePositionCategory) EditorGUILayout.EnumPopup("Size/Position: ", cat);

        //public static void SizeToolsGUI(AnimBool showTools, ref RoomOpData roomData, int currentLvl, int roomGridSize)
        //{
        //    if (SizeToolsGUI(showTools, out var mod, true)!= ChangeCheck.Changed)
        //        return;
        //    if (mod.AffectRoom)
        //    {
        //        if (!Equals(Vector2Int.zero, mod.SizeModifier))
        //        {
        //            mod.SizeModifier *= roomGridSize;
        //            mod.PosModifier *= roomGridSize;
        //            mod.TileModifier *= roomGridSize;
        //        }
        //        var newSize = roomData.SizeData.Size + mod.SizeModifier.Vector3Int();
        //        if (newSize.x <= 0 || newSize.z <= 0)
        //            return;
        //        roomData.SizeData.Size = newSize;
        //        SetRoomPositioning(Vector3Int.zero, ref roomData);
        //        if (mod.SpecialCase)
        //        {
        //            mod.PosModifier = mod.SizeModifier;
        //            mod.TileModifier = Vector2Int.zero;
        //        }
        //        for (var i = 0; i < roomData.PlatformLayer.Count; i++)
        //        {
        //            var lvl = roomData.PlatformLayer[i];
        //            mod.SizeModifier = Vector2Int.zero;
        //            ApplySizeTool(lvl.LayerDat, lvl.Tiles, roomData.SizeData, mod);
        //        }
        //    }
        //    else if (currentLvl >= 0 && currentLvl < roomData.PlatformLayer.Count)
        //    {
        //        if (mod.SpecialCase)
        //            mod.TileModifier = (mod.PosModifier * -1);
        //        var lvl = roomData.PlatformLayer[currentLvl];
        //        ApplySizeTool(lvl.LayerDat, lvl.Tiles, roomData.SizeData, mod);
        //    }
        //    //TODO ROOM:  FIX
        //    roomData.UpdateResultingDataBuffer.Add(currentLvl);
        //    if (currentLvl > 0)
        //        roomData.UpdateResultingDataBuffer.Add(0);
        //    if (roomData.PlatformLayer.Count > (currentLvl + 1))
        //        roomData.UpdateResultingDataBuffer.Add(currentLvl + 1);
        //    SetRoomDirty();
        //}

        public struct SizePositionMod
        {
            public Vector2Int SizeModifier;
            public Vector2Int PosModifier;
            public Vector2Int TileModifier;
            public bool SpecialCase;
            public bool AffectRoom;
        }

        public enum SizePositionCategory
        {
            None,
            Size,
            Position,
            Tiles
        }

        enum SizePositionButton
        {
            None,
            Up,
            Left,
            Right,
            Down,

            SizeTopUp = Up,
            SizeLeftEdgeLeft = Left,
            SizeRightEdgeRight = Right,
            SizeBottomDown = Down,

            SizeTopDown,
            SizeLeftEdgeRight,
            SizeRightEdgeLeft,
            SizeBottomUp,
        }

        static void ApplySizeTool(IPlatformLayerSpatialData platform, IPlatformLayerTiles tiles, ISizeData roomSize,
            SizePositionMod mod)
        {
            if (mod.SpecialCase)
                mod.TileModifier = (mod.PosModifier * -1);

            var sizeModifier = mod.SizeModifier;
            var offsetModifier = mod.PosModifier;
            var tileModifier = mod.TileModifier;

            var newSize = platform.TileDim + sizeModifier;
            var newPos = platform.Position + offsetModifier;

            // level cannot begin before room begins, shrink size, add offset
            if (newPos.x < 0)
            {
                tileModifier += Vector2Int.right * newPos.x;
                newSize = new Vector2Int(Mathf.Max(0, newSize.x + newPos.x), newSize.y);
                newPos = new Vector2Int(0, newPos.y);
            }
            if (newPos.y < 0)
            {
                tileModifier += Vector2Int.up * newPos.y;
                newSize = new Vector2Int(newSize.x, Mathf.Max(0, newSize.y + newPos.y));
                newPos = new Vector2Int(newPos.x, 0);
            }

            if (ApplySizeTool_CheckLevelOutsideOfRoom(platform, tiles, roomSize, newPos, newSize, out var roomTileDim)) 
                return;

            CutIfOverflowing(newPos, ref newSize, roomTileDim);

            var newTiles = new ushort[newSize.x * newSize.y];
            //var newTiles = new PlatformLayerTilesArray() { Tiles_ = new PlatformLayerTilesBufferElement[newSize.x * newSize.y] };
            ApplySizeTool(platform, tiles, newSize, tileModifier, newTiles);
            platform.Position = newPos;
            platform.Size = newSize - new Vector2Int(1, 1);
            tiles.SetTiles(newTiles);

            SceneView.RepaintAll();
        }

        static bool ApplySizeTool_CheckLevelOutsideOfRoom(IPlatformLayerSpatialData platform, IPlatformLayerTiles tiles,
            ISizeData roomSize, Vector2Int newPos, Vector2Int newSize, out Vector2Int roomTileDim)
        {
            // level begins outside of room or a size-dimension is 0 -> empty level
            roomTileDim = roomSize.RoomTileDim();
            // ReSharper disable once ComplexConditionExpression
            if (newPos.x < roomTileDim.x && newPos.y < roomTileDim.y && newSize.x > 0 && newSize.y > 0)
                return false;
            platform.Position = Vector2Int.zero;
            platform.Size = Vector2Int.one;
            var newTiles = new ushort[platform.TileDim.x * platform.TileDim.y];
            tiles.SetTiles(newTiles);
            return true;
        }

        static void CutIfOverflowing(Vector2Int newPos, ref Vector2Int newSize, Vector2Int roomTileDim)
        {
            // level is not allowed to extend the roomSize, lvl is cut:
            var newExtends = newPos + newSize;
            if (newExtends.x > roomTileDim.x) 
                newSize = new Vector2Int(roomTileDim.x - newPos.x, newSize.y);

            if (newExtends.y > roomTileDim.y) 
                newSize = new Vector2Int(newSize.x, roomTileDim.y - newPos.y);
        }

        static void ApplySizeTool(IPlatformLayerSpatialData platform, IPlatformLayerTiles tiles, Vector2Int newSize,
            Vector2Int tileModifier, ushort[] newTiles)
        {
            for (var x = 0; x < newSize.x; ++x)
            {
                for (var z = 0; z < newSize.y; ++z)
                {
                    var oldX = x - tileModifier.x;
                    var oldZ = z - tileModifier.y;
                    var idx = z * newSize.x + x;

                    if (!platform.IsInside(oldX, oldZ))
                    {
                        newTiles[idx] = 0;
                        continue;
                    }

                    newTiles[idx] = GetTile(platform, tiles, oldX, oldZ, OutOfLevelBoundsAction.IgnoreTile);
                }
            }
        }

        // ReSharper disable once FlagArgument
        static ChangeCheck SizeToolsGUI(AnimBool showTools, out SizePositionMod mod, bool roomOption)
        {
            mod.SizeModifier = Vector2Int.zero;
            mod.PosModifier = Vector2Int.zero;
            mod.TileModifier = Vector2Int.zero;
            mod.SpecialCase = false;
            mod.AffectRoom = false;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(Screen.width - 122)))
            {
                var show = EditorGUILayout.Foldout(showTools.target, "Size/Position Tools", true, MiniFoldoutStyle);
                if (show != showTools.target)
                {
                    showTools.target = show;
                    EditorPrefs.SetBool(k_editorPrefShowSizeTools, showTools.target);
                }

                using (var fade = new EditorGUILayout.FadeGroupScope(showTools.faded))
                {
                    if (fade.visible)
                    {
                        if (roomOption)
                            SizeToolsRoomOptionGUI(ref mod);

                        SizeToolsInnerGUI(out var cat, out var button);
                        SizeToolsButtonToData(cat, button, ref mod);
                    }
                }
            }

            if (Equals(Vector2Int.zero, mod.PosModifier) && (Equals(Vector2Int.zero, mod.SizeModifier)) && (Equals(Vector2Int.zero, mod.TileModifier)))
                return ChangeCheck.NotChanged;
            return ChangeCheck.Changed;
        }

        static void SizeToolsRoomOptionGUI(ref SizePositionMod mod)
        {
            using (new GUILayout.HorizontalScope())
            {
                mod.AffectRoom = ActivityButton(m_sizeToolsAffectRoom, "Room") || m_sizeToolsAffectRoom;
                if (ActivityButton(!m_sizeToolsAffectRoom, "Level"))
                    mod.AffectRoom = false;
                if (mod.AffectRoom == m_sizeToolsAffectRoom) return;
                m_sizeToolsAffectRoom = mod.AffectRoom;
                EditorPrefs.SetBool(k_editorPrefSizeToolsAffectRoom, m_sizeToolsAffectRoom);
            }
        }

        // ReSharper disable once MethodTooLong
        static SizePositionButton SizeToolsInnerGUI(out SizePositionCategory cat, out SizePositionButton button)
        {
            cat = SizePositionCategory.None;
            button = SizePositionButton.None;

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(57);
                        if (GUILayout.Button("^", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeTopUp; }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(57);
                        if (GUILayout.Button("v", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeTopDown; }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("<", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeLeftEdgeLeft; }
                        if (GUILayout.Button(">", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeLeftEdgeRight; }
                        GUILayout.Label("Size", GUILayout.Width(30));
                        if (GUILayout.Button("<", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeRightEdgeLeft; }
                        if (GUILayout.Button(">", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeRightEdgeRight; }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(57);
                        if (GUILayout.Button("^", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeBottomUp; }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(57);
                        if (GUILayout.Button("v", GUILayout.Width(20))) { cat = SizePositionCategory.Size; button = SizePositionButton.SizeBottomDown; }
                    }
                }

                //GUILayout.Space(100);
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(14);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(42);
                        if (GUILayout.Button("^", GUILayout.Width(20))) { cat = SizePositionCategory.Position; button = SizePositionButton.Up; }
                    }
                    GUILayout.Space(10);

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("<", GUILayout.Width(20))) { cat = SizePositionCategory.Position; button = SizePositionButton.Left; }
                        GUILayout.Label("Position", GUILayout.Width(50));
                        if (GUILayout.Button(">", GUILayout.Width(20))) { cat = SizePositionCategory.Position; button = SizePositionButton.Right; }
                    }
                    GUILayout.Space(10);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(42);
                        if (GUILayout.Button("v", GUILayout.Width(20))) { cat = SizePositionCategory.Position; button = SizePositionButton.Down; }
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(14);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(42);
                        if (GUILayout.Button("^", GUILayout.Width(20))) { cat = SizePositionCategory.Tiles; button = SizePositionButton.Up; }
                    }
                    GUILayout.Space(10);

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("<", GUILayout.Width(20))) { cat = SizePositionCategory.Tiles; button = SizePositionButton.Left; }
                        GUILayout.Label("  Tiles", GUILayout.Width(50));
                        if (GUILayout.Button(">", GUILayout.Width(20))) { cat = SizePositionCategory.Tiles; button = SizePositionButton.Right; }
                    }
                    GUILayout.Space(10);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(42);
                        if (GUILayout.Button("v", GUILayout.Width(20))) { cat = SizePositionCategory.Tiles; button = SizePositionButton.Down; }
                    }
                }
            }

            GUILayout.Space(10);
            return button;
        }

        static void SizeToolsButtonToData(SizePositionCategory cat, SizePositionButton button, ref SizePositionMod mod)
        {
            switch (cat)
            {
                case SizePositionCategory.Size: switch (button)
                {
                    case SizePositionButton.SizeTopUp:
                        mod.SizeModifier = Vector2Int.up;
                        break;
                    case SizePositionButton.SizeTopDown:
                        mod.SizeModifier = Vector2Int.down;
                        break;
                    case SizePositionButton.SizeLeftEdgeRight:
                        mod.SizeModifier = Vector2Int.left;
                        mod.PosModifier = Vector2Int.right;
                        mod.SpecialCase = true;
                        break;
                    case SizePositionButton.SizeLeftEdgeLeft:
                        mod.SizeModifier = Vector2Int.right;
                        mod.PosModifier = Vector2Int.left;
                        mod.SpecialCase = true;
                        break;
                    case SizePositionButton.SizeRightEdgeRight:
                        mod.SizeModifier = Vector2Int.right;
                        break;
                    case SizePositionButton.SizeRightEdgeLeft:
                        mod.SizeModifier = Vector2Int.left;
                        break;
                    case SizePositionButton.SizeBottomDown:
                        mod.SizeModifier = Vector2Int.up;
                        mod.PosModifier = Vector2Int.down;
                        mod.SpecialCase = true;
                        break;
                    case SizePositionButton.SizeBottomUp:
                        mod.SizeModifier = Vector2Int.down;
                        mod.PosModifier = Vector2Int.up;
                        mod.SpecialCase = true;
                        break;
                } break;

                case SizePositionCategory.Position: switch (button)
                {
                    case SizePositionButton.Left: mod.PosModifier = Vector2Int.left; break;
                    case SizePositionButton.Up: mod.PosModifier = Vector2Int.up; break;
                    case SizePositionButton.Down: mod.PosModifier = Vector2Int.down; break;
                    case SizePositionButton.Right: mod.PosModifier = Vector2Int.right; break;
                } break;

                case SizePositionCategory.Tiles: switch (button)
                {
                    case SizePositionButton.Left: mod.TileModifier = Vector2Int.left; break;
                    case SizePositionButton.Up: mod.TileModifier = Vector2Int.up; break;
                    case SizePositionButton.Down: mod.TileModifier = Vector2Int.down; break;
                    case SizePositionButton.Right: mod.TileModifier = Vector2Int.right; break;
                } break;
            }
        }

        public static void SizeSceneGUI(SizePositionCategory category, IPlatformLayer layer, ISizeData roomSize)
        {
            if (category == SizePositionCategory.None) 
                return;
            var bounds = GetBounds(layer);
            var button = DrawSizePositionHandles(category, bounds);
            SizePositionMod mod = default;
            SizeToolsButtonToData(category, button, ref mod);
            ApplySizeTool(layer, layer, roomSize, mod);
        }

        static SizePositionButton DrawSizePositionHandles(SizePositionCategory category, Bounds bounds)
        {
            var minX = bounds.min.x;
            var maxX = bounds.max.x;
            var minZ = bounds.min.z;
            var maxZ = bounds.max.z;
            var c = bounds.center;

            var isSizeTool = category == SizePositionCategory.Size;
            var size = 1.5f;
            var pickSize = 2f;
            var offset = isSizeTool ? 5f : 2f;
            var l = new Vector3(minX - offset, c.y, c.z);
            var r = new Vector3(maxX + offset, c.y, c.z);
            var t = new Vector3(c.x, c.y, maxZ + offset);
            var b = new Vector3(c.x, c.y, minZ - offset);

            var button = SizePositionButton.None;
            using (new ColorScope(Color.magenta))
                if (Handles.Button(l, Quaternion.Euler(0f, -90f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.Left;
            using (new ColorScope(Color.red))
                if (Handles.Button(r, Quaternion.Euler(0f, 90f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.Right;
            using (new ColorScope(Color.blue))
                if (Handles.Button(t, Quaternion.Euler(0f, 0f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.Up;
            using (new ColorScope(Color.cyan))
                if (Handles.Button(b, Quaternion.Euler(0f, 180f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.Down;

            if (!isSizeTool)
                return button;
            offset = 2f;
            l = new Vector3(minX - offset, c.y, c.z);
            r = new Vector3(maxX + offset, c.y, c.z);
            t = new Vector3(c.x, c.y, maxZ + offset);
            b = new Vector3(c.x, c.y, minZ - offset);

            using (new ColorScope(Color.magenta))
                if (Handles.Button(r, Quaternion.Euler(0f, -90f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.SizeRightEdgeLeft;

            using (new ColorScope(Color.red))
                if (Handles.Button(l, Quaternion.Euler(0f, 90f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.SizeLeftEdgeRight;
            
            using (new ColorScope(Color.blue))
                if (Handles.Button(b, Quaternion.Euler(0f, 0f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.SizeBottomUp;

            using (new ColorScope(Color.cyan))
                if (Handles.Button(t, Quaternion.Euler(0f, 180f, 0f), size, pickSize, Handles.ConeHandleCap))
                    button = SizePositionButton.SizeTopDown;

            return button;
        }

    }
}
