using Core.Editor.Inspector;
using Editor.Level.Tiles;
using Level.PlatformLayer;
using UnityEditor;

namespace Editor.Level.PlatformLayer
{
    [CustomEditor(typeof(PlatformLayerConfig))]
    public class PlatformLayerConfigInspector : BaseInspector<PlatformEditor> { }

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