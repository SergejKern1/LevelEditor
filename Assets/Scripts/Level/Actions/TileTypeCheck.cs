using System;
using Core.Unity.Extensions;
using Level.ScriptableUtility;
using Level.Tiles;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;

namespace Level.Actions
{
    public class TileTypeCheck : ScriptableBaseAction
    {
        [SerializeField] internal TilesSetListConfig m_sourceConfig;
        [SerializeField] internal TileTypeIdentifier m_tileIdentifier;

        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_currentPosition;

        [SerializeField] internal ScriptableBaseAction m_trueAction;
        [SerializeField] internal ScriptableBaseAction m_falseAction;

        public override string Name => nameof(TileTypeCheck);
        public static Type StaticFactoryType => typeof(TileTypeCheckAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);

            var trueAction = (IDefaultAction) m_trueAction.CreateAction(ctx);
            var falseAction = (IDefaultAction) m_falseAction.CreateAction(ctx);

            return new TileTypeCheckAction(m_sourceConfig, m_tileIdentifier, grid, currentPos, trueAction, falseAction);
        }
    }

    public class TileTypeCheckAction : IDefaultAction
    {
        readonly TilesSetFilter m_filter;

        readonly GridReference m_grid;
        readonly Vector3Reference m_currentPos;

        readonly IDefaultAction m_trueAction;
        readonly IDefaultAction m_falseAction;
        
        public TileTypeCheckAction(TilesSetListConfig sourceConfig, 
            TileTypeIdentifier identifier, 
            GridReference grid, 
            Vector3Reference currentPos,
            IDefaultAction trueAction,
            IDefaultAction falseAction)
        {
            var iterationSet = sourceConfig.GetSet(identifier.Config.Result);

            m_filter = new TilesSetFilter() {Data = iterationSet, FilterIdx = identifier.TileIdx};

            m_grid = grid;
            m_currentPos = currentPos;

            m_trueAction = trueAction;
            m_falseAction = falseAction;
        }

        public void Invoke()
        {
            var grid = m_grid.Value;
            var pos = m_currentPos.Value.Vector3Int();
            var tile = grid[pos.x, pos.y, pos.z];

            if (m_filter.IsTileActive(tile))
                m_trueAction?.Invoke();
            else 
                m_falseAction?.Invoke();
        }

    }
}
