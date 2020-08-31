using System;
using Core.Types;
using Level.ScriptableUtility;
using Level.Tiles.Interface;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;

using static Level.Tiles.TileProcessing;

namespace Level.Tiles.Actions
{
    public class MatchPattern : ScriptableBaseAction
    {
        [SerializeField] internal TilesSetListConfig m_sourceConfig;

        [SerializeField] internal ScriptableTilesSetFilter m_filter;
        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_pos;

        internal TileTypeIdentifier Identifier;
        internal int Configuration;

        public bool InvertSourceConfiguration;

        [Tooltip("AND = contained?/ Configuration as Minimum \n" +
                 "OR = allowed?/ Configuration as Maximum \n" +
                 "None = direct equality comparison")]
        public BinaryOperation Operation;
        //public bool OnlyTwoTileTypes;
        //public string Tag;

        public override string Name => nameof(MatchPattern);
        public static Type StaticFactoryType => typeof(MatchPatternAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            return new MatchPatternAction(m_sourceConfig, 
                new TilesSetFilterReference(ctx, m_filter),
                new GridReference(ctx, m_grid),
                new Vector3Reference(ctx, m_pos),
                Identifier, Configuration, InvertSourceConfiguration, Operation);
        }
    }

    public class MatchPatternAction : IBoolAction
    {
        bool m_result;

        readonly TilesSetListConfig m_sourceConfig;
        readonly TilesSetFilterReference m_tilesSetFilter;
        readonly GridReference m_grid;
        readonly Vector3Reference m_pos;
        readonly TileTypeIdentifier m_identifier;
        readonly int m_configuration;
        readonly bool m_invertSourceConfiguration;
        readonly BinaryOperation m_binaryOperation;

        public MatchPatternAction(TilesSetListConfig sourceConfig, 
            TilesSetFilterReference tilesSetFilter, 
            GridReference grid, 
            Vector3Reference pos, 
            TileTypeIdentifier identifier, 
            int configuration, 
            bool invertSourceConfiguration,
            BinaryOperation binOp)
        {
            m_sourceConfig = sourceConfig;
            m_tilesSetFilter = tilesSetFilter;
            m_grid = grid;
            m_pos = pos;
            m_identifier = identifier;
            m_configuration = configuration;
            m_invertSourceConfiguration = invertSourceConfiguration;
            m_binaryOperation = binOp;
        }

        public void Invoke()
        {
            m_result = false;

            var set = m_sourceConfig.GetSet(m_identifier.Config.Result);
            if (set.TileConfig == null)
            {
                Debug.LogError($"{nameof(ITileConfig)} {m_identifier.Config} not found in given {nameof(TilesSetListConfig)}");
                return;
            }

            var tiles = m_grid.Value;
            var x = (int) m_pos.Value.x;
            var y = (int) m_pos.Value.y;
            var z = (int) m_pos.Value.z;

            //tileTypes:
            var config = GetConfiguration(new TilesSetFilter() { Data = set, FilterIdx = m_identifier.TileIdx }, 
                m_tilesSetFilter, tiles, x, y, z).Configuration;

            if (m_invertSourceConfiguration)
                config = ~config;

            switch (m_binaryOperation)
            {
                case BinaryOperation.AND:
                    config &= m_configuration; break;
                case BinaryOperation.OR:
                    config |= m_configuration; break;
                case BinaryOperation.None: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            m_result = m_configuration == config;
        }

        public void Invoke(out bool result)
        {
            Invoke();
            result = m_result;
        }
    }
}
