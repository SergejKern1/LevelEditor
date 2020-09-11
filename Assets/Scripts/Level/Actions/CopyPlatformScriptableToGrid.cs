using System;

using UnityEngine;

using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;

using Level.PlatformLayer;
using Level.ScriptableUtility;
using XNode;
using XNode.Attributes;
using static Level.PlatformLayer.Operations;

namespace Level.Tiles.Actions
{
    public class CopyPlatformScriptableToGrid : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal PlatformLayerScriptable m_platform;
        [SerializeField, Input]
        internal ScriptableGrid m_grid;

        public override string Name => nameof(CopyPlatformScriptableToGrid);
        public static Type StaticFactoryType => typeof(CopyPlatformScriptableToGridAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx) => new CopyPlatformScriptableToGridAction(m_platform, new GridReference(ctx, m_grid));
    }

    public class CopyPlatformScriptableToGridAction : IDefaultAction
    {
        readonly PlatformLayerScriptable m_platform;
        GridReference m_grid;

        public CopyPlatformScriptableToGridAction(PlatformLayerScriptable platform, GridReference grid)
        {
            m_platform = platform;
            m_grid = grid;
        }

        public void Invoke()
        {
            var defaultData = new ushort[m_platform.TileDim.x, 1, m_platform.TileDim.y];

            for (var x = 0; x < m_platform.TileDim.x; ++x)
            for (var z = 0; z < m_platform.TileDim.y; ++z)
                defaultData[x, 0, z] = GetTile(m_platform, m_platform, x, z);

            m_grid.SetValue(defaultData);
        }
    }
}
