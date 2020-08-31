using System;
using Core.Extensions;
using Core.Unity.Extensions;
using Level.ScriptableUtility;
using Level.Tiles;
using Level.Tiles.Interface;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;

namespace Level.Actions
{
    public class TileTypeSwitch : ScriptableBaseAction
    {
        [SerializeField, HideInInspector] internal string m_name;
        [SerializeField] internal TilesSetListConfig m_sourceConfig;
        [SerializeField] internal RefITileConfig m_tileConfig;

        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_currentPosition;

        [SerializeField, HideInInspector] internal ScriptableBaseAction[] m_tileActions;

        public override string Name => m_name.IsNullOrEmpty()? nameof(TileTypeCheck) : m_name;
        public static Type StaticFactoryType => typeof(TileTypeCheckAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);

            var actions = new IDefaultAction[m_tileActions.Length];
            for (var i = 0; i < m_tileActions.Length; i++)
                actions[i] = (IDefaultAction) m_tileActions[i].CreateAction(ctx);

            return new TileTypeSwitchAction(m_sourceConfig, m_tileConfig.Result, grid, currentPos, actions);
        }

#if UNITY_EDITOR
        public static string Editor_NameProperty => nameof(m_name);
        public static string Editor_ActionProperty => nameof(m_tileActions);

        public ITileConfig Editor_TileConfig => m_tileConfig?.Result;
#endif
    }

    public class TileTypeSwitchAction : IDefaultAction
    {
        readonly TilesSetData m_setData;
        readonly GridReference m_grid;
        readonly Vector3Reference m_currentPos;

        readonly IDefaultAction[] m_actions;
        
        public TileTypeSwitchAction(TilesSetListConfig sourceConfig, 
            ITileConfig config, 
            GridReference grid, 
            Vector3Reference currentPos,
            IDefaultAction[] actions)
        {
            m_setData = sourceConfig.GetSet(config);

            m_grid = grid;
            m_currentPos = currentPos;

            m_actions = actions;
        }

        public void Invoke()
        {
            var grid = m_grid.Value;
            var pos = m_currentPos.Value.Vector3Int();
            var tile = grid[pos.x, pos.y, pos.z];

            var idx = m_setData.GetTileIdx(tile);
            //Debug.Log($"TileType Switch {pos} combined: {tile} idx: {idx}");
            if (m_actions.IsIndexInRange(idx))
                m_actions[idx]?.Invoke();
        }

    }
}
