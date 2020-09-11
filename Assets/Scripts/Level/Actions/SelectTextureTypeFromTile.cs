using System;
using Core.Unity.Extensions;
using Level.ScriptableUtility;
using Level.Texture;
using Level.Tiles;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode.Attributes;

namespace Level.Actions
{
    public class SelectTextureTypeFromTile : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal TilesSetListConfig m_sourceConfig;
        [SerializeField, Input]
        internal LevelTextureConfig m_texConfig;

        [SerializeField, Input]
        internal ScriptableGrid m_grid;
        [SerializeField, Input]
        internal ScriptableVector3 m_currentPosition;
        [SerializeField, Input]
        internal ScriptableTilesSetFilter m_tileSetFilter;

        public override string Name => nameof(SelectTextureTypeFromTile);
        public static Type StaticFactoryType => typeof(SelectTextureTypeFromTileAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);
            var tileSetFilter = new TilesSetFilterReference(ctx, m_tileSetFilter);

            var texConfigSetData = m_sourceConfig.GetSet(m_texConfig);

            return new SelectTextureTypeFromTileAction(texConfigSetData, 
                grid, currentPos,
                tileSetFilter);
        }
    }

    public class SelectTextureTypeFromTileAction : IDefaultAction
    {
        readonly GridReference m_grid;
        readonly Vector3Reference m_currentPos;
        readonly TilesSetData m_texConfigSetData;

        TilesSetFilterReference m_tilesSetFilter;

        public SelectTextureTypeFromTileAction(
            TilesSetData setData,
            GridReference grid,
            Vector3Reference currentPos,
            TilesSetFilterReference tsf)
        {
            m_grid = grid;
            m_currentPos = currentPos;
            m_tilesSetFilter = tsf;
            m_texConfigSetData = setData;
        }

        public void Invoke()
        {
            var grid = m_grid.Value;
            var pos = m_currentPos.Value.Vector3Int();
            var tile = grid[pos.x, pos.y, pos.z];
            var texType = m_texConfigSetData.GetTileIdx(tile);

            m_tilesSetFilter.SetValue(new TilesSetFilter()
            {
                Data = m_texConfigSetData,
                FilterIdx = texType
            });
        }
    }
}
