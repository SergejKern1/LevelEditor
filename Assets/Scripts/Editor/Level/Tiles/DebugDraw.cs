using UnityEditor;
using UnityEngine;

using Core.Editor.Utility;

using Level.PlatformLayer.Interface;
using Level.Tiles;

using static Level.PlatformLayer.Operations;

namespace Editor.Level.Tiles
{
    public static class DebugDraw
    {
        public struct DebugDrawSettings
        {
            public bool DrawLabel;
            public bool DrawDots;
            public bool DrawDebugTiles;
            public float DebugTileSize;
            public float ColorAlpha;

            public Vector3Int YMult;
            public Vector3Int ZMult;

            public static DebugDrawSettings Default = new DebugDrawSettings()
            {
                DrawLabel = false,
                DrawDots = false,
                DrawDebugTiles = true,
                DebugTileSize = 0.45f,
                ColorAlpha = 0.5f,

                YMult = Vector3Int.up,
                ZMult = new Vector3Int(0,0,1)
            };
        }

        public static void DrawGeometryType(IPlatformLayer layer,
            TilesSetData tilesSetData, DebugDrawSettings settings)
        {
            var posOffset = layer.PositionOffset;
            var gridSize = layer.GridSize;

            for (var x = 0; x < layer.TileDim.x; ++x)
            for (var z = 0; z < layer.TileDim.y; ++z)
            {
                var pos = new Vector3(posOffset.x + layer.Position.x, posOffset.y, posOffset.z + layer.Position.y);
                var rx = gridSize * x + pos.x;
                var rz = gridSize * z + pos.z;

                var tile = tilesSetData.GetTileIdx(GetTile(layer, layer, x, z));

                if (tile >= tilesSetData.Count)
                    return;

                DrawTile(tilesSetData, settings, rx, pos, rz, tile);
            }
        }

        static void DrawTile(TilesSetData tilesSetData, DebugDrawSettings settings, float rx, Vector3 pos, float rz, ushort tile)
        {
            var center = new Vector3(rx, 
                pos.y * settings.YMult.y + rz * settings.ZMult.y, 
                rz * settings.ZMult.z + pos.y * settings.YMult.z);

            GetDebugDrawVertices(settings, center, out var v1, out var v2, out var v3, out var v4);
            GetDebugDrawColor(settings, tilesSetData, tile, out var faceCol, out var outlineCol);

            var previewTex = tilesSetData[tile].PreviewTex;
            if (settings.DrawDebugTiles)
                Handles.DrawSolidRectangleWithOutline(new[] {v1, v2, v3, v4}, faceCol, outlineCol);

            var label = settings.DrawLabel ? tilesSetData[tile].TileName : "";
            var image = settings.DrawDots ? previewTex : null;

            if (settings.DrawDots || settings.DrawLabel)
                Handles.Label(center, new GUIContent() {text = label, image = image});
        }

        static void GetDebugDrawColor(DebugDrawSettings settings, TilesSetData tilesSetData, ushort tile, out Color faceCol, out Color outlineCol)
        {
            faceCol = tilesSetData[tile].TileColor;
            faceCol.a *= settings.ColorAlpha;
            outlineCol = tilesSetData[tile].TileColor;
        }

        static void GetDebugDrawVertices(DebugDrawSettings settings, Vector3 center, out Vector3 v1, out Vector3 v2, out Vector3 v3,
            out Vector3 v4)
        {
            var debugTileSize = settings.DebugTileSize;
            v1 = center + new Vector3(-debugTileSize, -debugTileSize * settings.ZMult.y, -debugTileSize * settings.ZMult.z);
            v2 = center + new Vector3(debugTileSize, -debugTileSize * settings.ZMult.y, -debugTileSize * settings.ZMult.z);
            v3 = center + new Vector3(debugTileSize, debugTileSize * settings.ZMult.y, debugTileSize * settings.ZMult.z);
            v4 = center + new Vector3(-debugTileSize, debugTileSize * settings.ZMult.y, debugTileSize * settings.ZMult.z);
        }

        internal static void DrawPlatformBounds(IPlatformLayerSpatialData platform,
            Vector3Int posOffset, float gridSize, float gridYSize, Color drawCol)
        {
            var bounds = GetBounds(platform, posOffset, gridSize, gridYSize);
            CustomHandleDraw.DrawBounds(bounds, drawCol);
        }
    }
}
