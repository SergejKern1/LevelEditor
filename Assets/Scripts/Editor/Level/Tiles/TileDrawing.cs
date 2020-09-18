using System;
using Core.Editor.Interface;
#if LUDIQ_PEEK
using System.Reflection;
#endif

using Core.Editor.Utility;
using Core.Events;
using Core.Extensions;
using Core.Unity.Extensions;
using Level.Enums;
using Level.PlatformLayer;

//using Level.Operations;
using Level.Tiles;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

using static Level.PlatformLayer.Operations;

namespace Editor.Level.Tiles
{
    public static class TileDrawing
    {
        public struct TilesDrawn
        {

        }

        #region Editor Prefs
        //const string _EditorPrefPaintTileTypeLeft = "RoomEditor.PaintTileTypeLeft";
        //const string _EditorPrefPaintTileTypeRight = "RoomEditor.PaintTileTypeRight";
        const string m_editorPrefShowLevelSettingEditor = "RoomEditor.ShowLevelSettingEditor";
        #endregion

        //todo: depending on GridSize and Setting?
        const float k_minBrushSize = 0.1f;
        const float k_maxBrushSize = 10f;

        internal static void Init_TilesMode(ref TileDrawingOpData opDat, IRepaintable r)
        {
            opDat.PaintTileTypeLeft = 0; //EditorPrefs.GetInt(_EditorPrefPaintTileTypeLeft, (int)Tiles.Type.Floor);
            opDat.PaintTileTypeRight = 0; //EditorPrefs.GetInt(_EditorPrefPaintTileTypeRight, (int)Tiles.Type.AutoNonFloor);
            opDat.BrushSize = 1;
            opDat.ShowLevelSettingEditor = new AnimBool(false);

            opDat.ShowLevelSettingEditor.valueChanged.RemoveAllListeners();
            opDat.ShowLevelSettingEditor.valueChanged.AddListener(r.Repaint);

            opDat.ShowLevelSettingEditor.target = EditorPrefs.GetBool(m_editorPrefShowLevelSettingEditor, true);
        }

        static void DrawRect(ref TileDrawingOpData opDat, Vector3 currentPos)
        {
            if (!opDat.StartPos.HasValue)
                return;
            var startPosV = opDat.StartPos.Value;
            //var startGridPos = new Vector2Int(Mathf.FloorToInt(startPosV.x), Mathf.FloorToInt(startPosV.z));
            //var starGridPosVec3 = startGridPos.Vector3(startPosV.y);

            Handles.DrawLine(currentPos, new Vector3(currentPos.x, currentPos.y, startPosV.z));
            Handles.DrawLine(currentPos, new Vector3(startPosV.x, currentPos.y, currentPos.z));
            Handles.DrawLine(startPosV, new Vector3(currentPos.x, currentPos.y, startPosV.z));
            Handles.DrawLine(startPosV, new Vector3(startPosV.x, currentPos.y, currentPos.z));
        }

        const float k_sqrt2 = 1.4142135623730950488016887242097f;
        static void DrawCursor(ref TileDrawingOpData opDat, Vector3 currentPos)
        {
            var brush = opDat.Brush;
            var brushSize = opDat.BrushSize;

            var currentGridPos = new Vector2Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.z));
            var currentGridPosVec3 = currentGridPos.Vector3(currentPos.y);

            // todo: Fix later
            //if (opDat.IsVariationOnly && opDat.Brush == BrushType.PointBrush)
            //{
            //    brush = BrushType.RectangleBrush;
            //    brushSize = 0.5f;
            //    currentPos = new Vector3(Mathf.CeilToInt(currentPos.x) - 0.5f, 0.0f, Mathf.CeilToInt(currentPos.z) - 0.5f);
            //}

            var prevCol = Handles.color;
            Handles.color = Color.green;

            switch (brush)
            {
                case BrushType.Fill:
                case BrushType.PointBrush:
                    DrawCursorPointBrush(opDat, currentGridPosVec3);
                    break;
                case BrushType.RectangleBrush:
                    Handles.RectangleHandleCap(-1, currentPos, Quaternion.Euler(90, 0, 0), brushSize, EventType.Repaint);
                    break;
                case BrushType.CircleBrush:
                    Handles.CircleHandleCap(-1, currentPos, Quaternion.Euler(90, 0, 0), brushSize, EventType.Repaint);
                    break;
                case BrushType.DragLine:
                    DrawCursorDragLine(opDat, currentPos, brushSize);
                    break;
                case BrushType.DragRectangle:
                    DrawRect(ref opDat, currentPos);
                    break;
                case BrushType.DragEllipseRect:
                case BrushType.DragEllipseDiagonal:
                    DrawCursorDragEllipseDiagonal(ref opDat, currentPos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Handles.color = prevCol;
        }

        static void DrawCursorDragEllipseDiagonal(ref TileDrawingOpData opDat, Vector3 currentPos)
        {
            if (!opDat.StartPos.HasValue)
                return;

            var startPosV = opDat.StartPos.Value;
            var halfSize = 0.5f * (currentPos - startPosV);
            var centerPos = startPosV + halfSize;
            var v2 = opDat.Brush == BrushType.DragEllipseRect;

            if (v2)
                DrawRect(ref opDat, currentPos);
            else
            {
                Handles.DrawLine(startPosV, startPosV + halfSize.normalized);
                Handles.DrawLine(currentPos, currentPos - halfSize.normalized);
            }

            var multi = v2 ? 1.0f : k_sqrt2;
            float x = 0;
            float z = 0;
            for (var i = 0; i < 21; ++i)
            {
                var nextX = Mathf.Cos(Mathf.Deg2Rad * i * 18) * multi * halfSize.x + centerPos.x;
                var nextZ = Mathf.Sin(Mathf.Deg2Rad * i * 18) * multi * halfSize.z + centerPos.z;
                if (i != 0)
                    Handles.DrawLine(new Vector3(x, 0, z), new Vector3(nextX, 0, nextZ));

                x = nextX;
                z = nextZ;
            }
        }

        static void DrawCursorDragLine(TileDrawingOpData opDat, Vector3 currentPos, float brushSize)
        {
            Handles.CircleHandleCap(-1, currentPos, Quaternion.Euler(90, 0, 0), brushSize, EventType.Repaint);
            if (!opDat.StartPos.HasValue)
                return;

            var startPosV = opDat.StartPos.Value;
            Handles.DrawLine(startPosV, currentPos);
            Handles.CircleHandleCap(-1, startPosV, Quaternion.Euler(90, 0, 0), brushSize, EventType.Repaint);
        }

        static void DrawCursorPointBrush(TileDrawingOpData opDat, Vector3 currentGridPosVec3)
        {
            var gridRealWPos = currentGridPosVec3;
            Handles.DrawLine(gridRealWPos + Vector3.forward * 0.5f, gridRealWPos - Vector3.forward * 0.5f);
            Handles.DrawLine(gridRealWPos + Vector3.right * 0.5f, gridRealWPos - Vector3.right * 0.5f);
            if (opDat.Brush != BrushType.Fill)
                return;
            Handles.DrawLine(gridRealWPos + Vector3.forward * 0.5f + Vector3.right * 0.5f,
                gridRealWPos - Vector3.forward * 0.5f - Vector3.right * 0.5f);
            Handles.DrawLine(gridRealWPos - Vector3.forward * 0.5f + Vector3.right * 0.5f,
                gridRealWPos + Vector3.forward * 0.5f - Vector3.right * 0.5f);
        }

        internal static void SceneGUI_TilesMode(Event e, ref TileDrawingOpData opDat, Vector3 pos, Vector3 cursor)
        {
            DrawCursor(ref opDat, cursor);

            if (e.control && e.type == EventType.ScrollWheel)
            {
                opDat.BrushSize = Mathf.Clamp(opDat.BrushSize - 0.1f * e.delta.y, 0.5f, 10.0f);
                e.Use();

                //Debug.Log("Repainting");
                SceneView.RepaintAll();
            }

            var useEvent = e.type.IsAny(EventType.MouseDown, EventType.MouseDrag, EventType.MouseUp);
            if (!useEvent)
                return;

            ProcessMouseEvent(e, ref opDat, pos, out var doSetTilesNow);
            opDat.FireEvent = e.type == EventType.MouseUp;

            e.Use();

#if LUDIQ_PEEK
            PeekFix();
#endif

            if (!doSetTilesNow)
                return;

            ApplyBrush(ref opDat, pos);
        }

#if LUDIQ_PEEK
        static FieldInfo m_peekPressPosition;
        static void PeekFix()
        {
            if (m_peekPressPosition == null)
            {
                var type = typeof(Ludiq.Peek.Probe);
                m_peekPressPosition = type.GetField("pressPosition", BindingFlags.NonPublic | BindingFlags.Static);
            }
            m_peekPressPosition?.SetValue(null, null);
        }
#endif

        static void ApplyBrush(ref TileDrawingOpData opDat, Vector3 pos)
        {
            var e = Event.current;
            // todo: Fix Later
            //if (opDat.IsVariationOnly)
            //{
            //    pos -= Vector3.one * 0.5f;
            //    opDat.StartPos -= Vector3.one * 0.5f;
            //}
            //TODO ROOM: Fix this
            //Undo.RecordObject(_Room, "Paint");

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (e.button == 0)
                ApplyBrush(ref opDat, pos, true);
            else if (e.button == 1)
                ApplyBrush(ref opDat, pos, false);

            //TODO ROOM: FIX!!
            //MeshOperationData.Room = _RoomOpDat;
            //GenerateMesh(MeshOperationData);
            if (opDat.FireEvent)
                EventMessenger.TriggerEvent(new TilesDrawn());

            opDat.StartPos = null;
        }

        static void ProcessMouseEvent(Event e, ref TileDrawingOpData opDat, Vector3 pos, out bool doSetTilesNow)
        {
            doSetTilesNow = false;
            if (opDat.IsBrushType)
            {
                doSetTilesNow = true;
                return;
            }

            if (e.type == EventType.MouseDown)
                opDat.StartPos = pos;

            if (opDat.Brush == BrushType.Fill && opDat.StartPos.HasValue)
                opDat.StartPos = pos;

            if (e.type == EventType.MouseUp && opDat.StartPos.HasValue)
                doSetTilesNow = true;
        }

        static void CircleAndRectangleBrush(ref TileDrawingOpData opDat, Vector3 pos, uint tileType)
        {
            var brushMin = Mathf.FloorToInt(-opDat.BrushSize);
            var brushMax = Mathf.CeilToInt(opDat.BrushSize);

            for (var x = brushMin; x <= brushMax; ++x)
            for (var z = brushMin; z <= brushMax; ++z)
            {
                var gridPos = new Vector2Int(Mathf.RoundToInt(pos.x + x), Mathf.RoundToInt(pos.z + z));
                if (gridPos.x < pos.x - opDat.BrushSize) continue;
                if (gridPos.y < pos.z - opDat.BrushSize) continue;
                if (gridPos.x > pos.x + opDat.BrushSize) continue;
                if (gridPos.y > pos.z + opDat.BrushSize) continue;

                if (opDat.Brush == BrushType.CircleBrush)
                {
                    var rX = gridPos.x - pos.x;
                    var rZ = gridPos.y - pos.z;
                    if (rX * rX + rZ * rZ > opDat.BrushSize * opDat.BrushSize) continue;
                }

                ModifyTileAtPos(ref opDat, new Vector2Int(gridPos.x, gridPos.y), tileType);
            }
        }

        static void ApplyBrush(ref TileDrawingOpData opDat, Vector3 currentPos, bool left)
        {
            // var gridStartPos = new Vector2Int(Mathf.FloorToInt(startPos.x), Mathf.FloorToInt(startPos.z));

            var tileType = left ? opDat.PaintTileTypeLeft : opDat.PaintTileTypeRight;
            //var texType = left? opDat._PaintTexLeft : opDat._PaintTexRight;
            //var variation = left? opDat._PaintVariationLeft : opDat._PaintVariationRight;

            switch (opDat.Brush)
            {
                case BrushType.PointBrush:
                    {
                        var currentGridPos = new Vector2Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.z));
                        ModifyTileAtPos(ref opDat, currentGridPos, tileType);
                    }
                    break;
                case BrushType.CircleBrush:
                case BrushType.RectangleBrush:
                    CircleAndRectangleBrush(ref opDat, currentPos, tileType);
                    break;
                case BrushType.DragLine:
                    ApplyCircleAndRectangleBrush(ref opDat, currentPos, tileType);
                    break;
                case BrushType.DragRectangle:
                    ApplyDragRectangleBrush(ref opDat, currentPos, tileType);
                    break;
                case BrushType.DragEllipseRect:
                case BrushType.DragEllipseDiagonal:
                    ApplyDragEllipseDiagonalBrush(ref opDat, currentPos, tileType);
                    break;
                case BrushType.Fill:
                    ApplyFillBrush(ref opDat, currentPos, tileType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static void ApplyFillBrush(ref TileDrawingOpData opDat, Vector3 currentPos, uint tileType)
        {
            var currentGridPos = new Vector2Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.z));
            //var level = _Room.PlatformLayer[CurrentLevel];
            var platform = opDat.Platform;
            var tiles = GetRegionTiles(platform, opDat.Tiles, currentGridPos.x - platform.Position.x,
                currentGridPos.y - platform.Position.y, compareMask: opDat.TileTypesMask);

            foreach (var tile in tiles)
            {
                //TODO ROOM:  Fix this
                ModifyTileAtPos(ref opDat, tile + platform.Position, tileType);
            }
        }

        // ReSharper disable once TooManyDeclarations
        static void ApplyDragEllipseDiagonalBrush(ref TileDrawingOpData opDat, Vector3 currentPos, uint tileType)
        {
            if (!opDat.StartPos.HasValue)
                return;

            var startPosV = opDat.StartPos.Value;
            var halfSize = 0.5f * (currentPos - startPosV);
            var centerPos = startPosV + halfSize;

            var v2 = (opDat.Brush == BrushType.DragEllipseRect);

            var width = v2 ? Mathf.Abs(halfSize.x) : Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * 0) * k_sqrt2 * halfSize.x);
            var height = v2 ? Mathf.Abs(halfSize.z) : Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * 90) * k_sqrt2 * halfSize.z);

            var xMin = Mathf.FloorToInt(-width);
            var xMax = Mathf.CeilToInt(width);
            var zMin = Mathf.FloorToInt(-height);
            var zMax = Mathf.CeilToInt(height);

            for (var x = xMin; x <= xMax; ++x)
            for (var z = zMin; z <= zMax; ++z)
            {
                var currentGridPos = new Vector2Int(Mathf.RoundToInt(centerPos.x + x), Mathf.RoundToInt(centerPos.z + z));
                var eX = (currentGridPos.x - centerPos.x) / width;
                var eZ = (currentGridPos.y - centerPos.z) / height;
                if (eX * eX + eZ * eZ > 1)
                    continue;
                ModifyTileAtPos(ref opDat, new Vector2Int(currentGridPos.x, currentGridPos.y), tileType);
            }
        }

        static void ApplyDragRectangleBrush(ref TileDrawingOpData opDat, Vector3 currentPos, uint tileType)
        {
            if (!opDat.StartPos.HasValue)
                return;
            var startPosV = opDat.StartPos.Value;
            var xMin = Mathf.Min(startPosV.x, currentPos.x);
            var zMin = Mathf.Min(startPosV.z, currentPos.z);
            var xMax = Mathf.Max(startPosV.x, currentPos.x);
            var zMax = Mathf.Max(startPosV.z, currentPos.z);
            var xStart = Mathf.FloorToInt(xMin);
            var xEnd = Mathf.CeilToInt(xMax);
            var zStart = Mathf.FloorToInt(zMin);
            var zEnd = Mathf.CeilToInt(zMax);

            for (var x = xStart; x <= xEnd; ++x)
            for (var z = zStart; z <= zEnd; ++z)
            {
                var currentGridPos = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
                if (currentGridPos.x < xMin) continue;
                if (currentGridPos.y < zMin) continue;
                if (currentGridPos.x > xMax) continue;
                if (currentGridPos.y > zMax) continue;

                ModifyTileAtPos(ref opDat, new Vector2Int(currentGridPos.x, currentGridPos.y), tileType);
            }
        }

        static void ApplyCircleAndRectangleBrush(ref TileDrawingOpData opDat, Vector3 currentPos, uint tileType)
        {
            if (!opDat.StartPos.HasValue)
                return;

            var startPosV = opDat.StartPos.Value;
            var lineVec = (currentPos - startPosV);
            var lineDir = lineVec.normalized;
            var magnitude = Mathf.Abs(lineVec.magnitude);
            var length = Mathf.CeilToInt(magnitude);
            for (var i = 0; i <= length; ++i)
            {
                var step = Mathf.Min(i, magnitude);
                var addPos = step * lineDir;
                CircleAndRectangleBrush(ref opDat, startPosV + addPos, tileType);
            }
        }

        static void ModifyTileAtPos(ref TileDrawingOpData opDat, Vector2Int pos, uint tileType)
        {
            //if (tileType == -1)
            //    return;

            var lvlX = pos.x - opDat.Platform.Position.x;
            var lvlZ = pos.y - opDat.Platform.Position.y;
            if (opDat.Platform.IsInside(lvlX, lvlZ))
                SetTile(opDat.Platform, opDat.Tiles, lvlX, lvlZ, (ushort)tileType, opDat.TileTypesMask, action: OutOfLevelBoundsAction.IgnoreTile);

            // TODO: FIX for Room Editor
            //if (modifySurroundingLevels)
            //    ModifySurroundingLevel(pos.x, pos.y);
            //UpdateExits(pos);

            // TODO: FIX for Room Editor
            //if (texType!= -1)
            //    _Room.SetTile(CurrentLevel, pos.x, pos.y, (ushort)texType, mask: TexMask, shift: TexShift, action: OutOfLevelBoundsAction.IgnoreTile);
            //if (variation!= -1)
            //    _Room.SetTile(CurrentLevel, pos.x, pos.y, (ushort)variation, mask: VarMask, shift: VarShift, action: OutOfLevelBoundsAction.IgnoreTile);

            // TODO: FIX for Room Editor
            //SetRoomDirty();
        }

        // TODO: FIX for Room Editor
        //void UpdateExits(Vector2Int pos)
        //{
        //    var roomTileDim = _Room.SizeData.RoomTileDim();
        //    var editedRoomEdge = (pos.x == 0 || pos.y == 0 || pos.x == (roomTileDim.x - 1) || pos.y == (roomTileDim.y - 1));
        //    if (!editedRoomEdge)
        //        return;
        //    AddOpenExitsFromTiles(ref _Room);
        //}

        public static void BrushTypeSizeGUI(ref TileDrawingOpData opDat)
        {
            //if (brushTypeOptions == null || brushTypeOptions.Length < 1)
            //opDat.BrushTypeOptions = Enum.GetNames(typeof(BrushType));

            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i <= (int) BrushType.Fill; i++)
            {
                var tex = EditorGUIUtility.Load($"Draw/{(BrushType) i}.png") as Texture2D;
                //var toolText = $"{(BrushType) i}";
                if (CustomGUI.ActivityButton(opDat.Brush == (BrushType) i, "", tex, GUILayout.Width(45)))
                    opDat.Brush = (BrushType) i;
            }

            EditorGUILayout.EndHorizontal();

            opDat.BrushSize = EditorGUILayout.Slider("Brush Size", opDat.BrushSize, k_minBrushSize, k_maxBrushSize);
        }

        public static void TileSelectionGUI(ref TileDrawingOpData opDat, TilesSetData tileSetData)
        {
            //GUILayout.Label("Texture: ", GUILayout.Width(100));
            //if (ActivityButton2(_PaintTexLeft == -1, _PaintTexRight == -1, "None", GUILayout.Width(100)))
            //{
            //    if (Event.current.button == 1)
            //        _PaintTexRight = -1;
            //    else _PaintTexLeft = -1;
            //}

            ushort tIdx = 0;
            for (; tIdx < tileSetData.Count; ++tIdx)
            {
                var current = tileSetData.TileConfig?.Result[tIdx];
                if (current == null) continue;

                if (tIdx % 5 == 0)
                    EditorGUILayout.BeginHorizontal();
                var toolName = current.TileName;

                //Texture2D previewTex = null;
                //var floorVar = current.FloorVariations;
                var lIdx = tileSetData.GetTileIdx(opDat.PaintTileTypeLeft);
                var rIdx = tileSetData.GetTileIdx(opDat.PaintTileTypeRight);

                if (CustomGUI.ActivityButton2(lIdx == tIdx, rIdx == tIdx, toolName,
                    current.PreviewTex, GUILayout.Width(100)))
                {
                    //var prevLeft = _PaintTexLeft;
                    //var prevRight = _PaintTexRight;
                    if (Event.current.button == 1)
                        opDat.PaintTileTypeRight = tileSetData.GetCombinedTile(opDat.PaintTileTypeRight, tIdx);
                    else
                        opDat.PaintTileTypeLeft = tileSetData.GetCombinedTile(opDat.PaintTileTypeLeft, tIdx);

                    //if (_PaintTexLeft == _PaintTexRight && _PaintTileTypeLeft!= -1)
                    //{
                    //    if (Event.current.button == 1)
                    //        _PaintTexLeft = prevRight;
                    //    else _PaintTexRight = prevLeft;
                    //}
                }

                if (tIdx % 5 == 4)
                    EditorGUILayout.EndHorizontal();
            }

            if (tIdx % 5 != 0)
                EditorGUILayout.EndHorizontal();
        }

        public static bool TileTypesActiveGUI(ref TileDrawingOpData opDat, TilesSetData tileSetData)
        {
            var active = (opDat.TileTypesMask & tileSetData.Mask) != 0;
            if (GUILayout.Toggle(active, tileSetData.TileConfig.Result?.Name, "Button", GUILayout.Width(150f)) == active) 
                return active;

            active = !active;
            if (active)
                opDat.TileTypesMask |= tileSetData.Mask;
            else opDat.TileTypesMask &= (ushort) ~tileSetData.Mask;

            return active;
            //Debug.Log($"opDat-mask {opDat.TileTypesMask} tileSet-mask: {tileSetData.Mask} active: {active}");
        }

        //static LevelTextureConfig LevelTextureConfigGUI()
        //{
        // TODO: FIX for Room Editor
        //var newSetting = EditorGUILayout.ObjectField("Level Texture Setting", _Room.TexSettingData.Setting, typeof(LevelTextureSetting), false) as LevelTextureSetting;
        //if (_Room.TexSettingData.Setting!= newSetting)
        //{
        //    //TODO ROOM:  Fix this
        //    //Undo.RecordObject(_Room, "Setting changed");
        //    //_Room.Setting = newSetting;
        //}
        //using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        //    LevelSettingsEditorGUI();
        //var setting = _Room.TexSettingData.Setting;
        //    return null;
        //}

        //static void GeometryGUI()
        //{
        //GUILayout.Label("Geometry: ", GUILayout.Width(100));
        //using (new GUILayout.HorizontalScope())
        //{
        //    for (var gIdx = -1; gIdx <= (int)Tiles.Type.Floor; ++gIdx)
        //    {
        //        var toolName = ((Tiles.Type)gIdx).ToString();
        //        if (!ActivityButton2(opDat._PaintTileTypeLeft == gIdx, opDat._PaintTileTypeRight == gIdx, toolName,
        //            GUILayout.Width(100)))
        //            continue;
        //        //var prevLeft = _PaintTileTypeLeft;
        //        //var prevRight = _PaintTileTypeRight;
        //        if (Event.current.button == 1)
        //            opDat._PaintTileTypeRight = gIdx;
        //        else
        //            opDat._PaintTileTypeLeft = gIdx;
        //        //if (_PaintTileTypeLeft == _PaintTileTypeRight && _PaintTileTypeLeft!= -1)
        //        //{
        //        //    if (Event.current.button == 1)
        //        //        _PaintTileTypeLeft = prevRight;
        //        //    else _PaintTileTypeRight = prevLeft;
        //        //}
        //        //EditorPrefs.SetInt(_EditorPrefPaintTileTypeLeft, opDat._PaintTileTypeLeft);
        //        //EditorPrefs.SetInt(_EditorPrefPaintTileTypeRight, opDat._PaintTileTypeRight);
        //    }
        //}
        //}
        //void VariationGUI()
        //{
        //    m_paintVariationLeft = EditorGUILayout.IntField("Variation Left: ", m_paintVariationLeft);
        //    m_paintVariationRight = EditorGUILayout.IntField("Variation Right: ", m_paintVariationRight);
        //}
    }
}