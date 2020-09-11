using System;
using System.Linq;
using Core.Unity.Extensions;
using Level.ScriptableUtility;
using Level.Tiles.Interface;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.ActionConfigs.Attributes;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode.Attributes;
using static Level.Tiles.TileProcessing;

namespace Level.Tiles.Actions
{
    public class TileTypeIterator : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal TilesSetListConfig m_sourceConfig;
        [SerializeField, Input]
        internal RefITileConfig m_iterateOnTileConfig;
        [SerializeField, Input]
        internal TileTypeIdentifier[] m_filter;

        [SerializeField, Input]
        internal ScriptableGrid m_grid;
        [SerializeField, Input]
        internal ScriptableVector3 m_currentPosition;
        [SerializeField, Input]
        internal ScriptableTilesSetFilter m_currentTilesSetFilter;

        [SerializeField, HideInInspector, ActionOutput] 
        internal ScriptableBaseAction m_continueWith;

        public override string Name => nameof(TileTypeIterator);
        public static Type StaticFactoryType => typeof(TileTypeIteratorAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var continueAction = m_continueWith.CreateAction(ctx) as IDefaultAction;
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);
            var filter = new TilesSetFilterReference(ctx, m_currentTilesSetFilter);

            return new TileTypeIteratorAction(m_sourceConfig, m_iterateOnTileConfig.Result, m_filter,
                grid, currentPos, filter, continueAction);
        }

#if UNITY_EDITOR
        public const string Editor_ContinueWithPropName = nameof(m_continueWith);
#endif
    }

    public class TileTypeIteratorAction : IDefaultAction
    {
        readonly TilesSetListConfig m_sourceConfig;
        readonly ITileConfig m_iterateOnTileConfig;
        readonly TileTypeIdentifier[] m_filter;

        readonly GridReference m_grid;
        readonly Vector3Reference m_currentPos;
        TilesSetFilterReference m_tilesSetFilter;
        readonly IDefaultAction m_continueAction;

        public TileTypeIteratorAction(TilesSetListConfig sourceConfig, 
            ITileConfig iterateOnTileConfig, 
            TileTypeIdentifier[] filter, 
            GridReference grid, 
            Vector3Reference currentPos,
            TilesSetFilterReference tilesSetFilter, 
            IDefaultAction continueAction)
        {
            m_sourceConfig = sourceConfig;
            m_iterateOnTileConfig = iterateOnTileConfig;
            m_filter = filter;
            m_grid = grid;
            m_currentPos = currentPos;
            m_tilesSetFilter = tilesSetFilter;
            m_continueAction = continueAction;
        }

        public void Invoke()
        {
            var iterationSet = m_sourceConfig.GetSet(m_iterateOnTileConfig);

            var filters = new TilesSetFilter[m_filter.Length];
            for (var i = 0; i < m_filter.Length; i++)
            {
                var filterSet = m_sourceConfig.GetSet(m_filter[i].Config.Result);
                filters[i] = new TilesSetFilter() { Data = filterSet, FilterIdx = m_filter[i].TileIdx };
            }

            var startNode = 0;
            ushort[] texTypesDone = { ushort.MaxValue, ushort.MaxValue, ushort.MaxValue };
            var texTypesDoneIdx = 0;

            ushort next;
            while ((next = NextType(iterationSet, filters, ref startNode,
                       ref texTypesDone, ref texTypesDoneIdx)) != ushort.MaxValue)
            {
                m_tilesSetFilter.SetValue(new TilesSetFilter(){Data = iterationSet, FilterIdx = next});
                m_continueAction.Invoke();
            }
        }

        public ushort NextType(TilesSetData iterationSet, TilesSetFilter[] filters, 
            ref int startNode, ref ushort[] texTypeDone, ref int texTypesDoneIdx)
        {
            var grid = m_grid.Value;
            var pos = m_currentPos.Value.Vector3Int();
            for (; startNode < IterationOffset.Length; ++startNode)
            {
                var off = IterationOffset[startNode];
                var tile = grid[pos.x + off.x, pos.y + off.y, pos.z + off.z];

                var ignoreTile = filters.Any(filter => !filter.IsTileActive(tile));
                if (ignoreTile)
                    continue;

                var type = iterationSet.GetTileIdx(tile);

                if (texTypeDone.Any(t => t == tile))
                    continue;
                texTypeDone[texTypesDoneIdx] = tile;
                texTypesDoneIdx++;

                return type;
            }
            return ushort.MaxValue;
        }
    }
}
