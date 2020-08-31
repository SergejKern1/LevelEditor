using Level.PlatformLayer.Interface;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Editor.Level.Tiles
{
    public struct TileDrawingOpData 
    {
        // todo: save/ load
        internal BrushType Brush;

        internal bool IsBrushType => Brush == BrushType.PointBrush || Brush == BrushType.CircleBrush || Brush == BrushType.RectangleBrush;
        //bool IsDragType => _Brush == BrushType.DragEllipseDiagonal || _Brush == BrushType.DragRectangle;
        //internal bool IsVariationOnly => (PaintTileTypeLeft + PaintTileTypeRight + _PaintTexLeft + _PaintTexRight == -4 
        //&& _PaintVariationLeft + _PaintVariationRight!= -2);

        internal Vector3? StartPos;
        // save/ load
        internal float BrushSize;

        internal uint PaintTileTypeLeft;
        internal uint PaintTileTypeRight;

        internal ushort TileTypesMask;
        internal bool FireEvent;

        //internal int _PaintTexLeft;
        //internal int _PaintTexRight;
        //internal int _PaintVariationLeft;
        //internal int _PaintVariationRight;

        //internal string[] BrushTypeOptions;

        internal AnimBool ShowLevelSettingEditor;

        internal IPlatformLayerSpatialData Platform;
        internal IPlatformLayerTiles Tiles;
    }
}