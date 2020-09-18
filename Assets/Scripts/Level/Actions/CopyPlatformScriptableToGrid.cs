using System;

using UnityEngine;

using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using Level.Room;
using Level.ScriptableUtility;
using XNode.Attributes;
using static Level.PlatformLayer.Operations;

namespace Level.Tiles.Actions
{
    public class CopyPlatformScriptableToGrid : ScriptableBaseAction
    {
        //[SerializeField, Input]
        //internal ScriptableUnityObject m_platform;
        [SerializeField, Input]
        internal ScriptableGrid m_grid;

        public override string Name => nameof(CopyPlatformScriptableToGrid);
        public static Type StaticFactoryType => typeof(CopyPlatformScriptableToGridAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx) 
            => new CopyPlatformScriptableToGridAction(Graph as RoomBuilder
                , new GridReference(ctx, m_grid));
    }

    public class CopyPlatformScriptableToGridAction : IDefaultAction
    {
        readonly RoomBuilder m_builder;
        GridReference m_grid;

        public CopyPlatformScriptableToGridAction(RoomBuilder builder, GridReference grid)
        {
            m_builder = builder;
            m_grid = grid;
        }

        public void Invoke()
        {
            //var platforms = m_builder.Layer;
            //var platformA = platforms[0];

            //var defaultData = new ushort[platformA.TileDim.x, platforms.Length, platformA.TileDim.y];

            //for (var y = 0; y < platforms.Length; ++y)
            //for (var x = 0; x < platformA.TileDim.x; ++x)
            //for (var z = 0; z < platformA.TileDim.y; ++z)
            //    defaultData[x, y, z] = GetTile(platforms[y], platforms[y], x, z);

            //m_grid.SetValue(defaultData);
        }
    }
}
