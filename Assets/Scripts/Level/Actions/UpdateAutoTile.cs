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
using XNode.Attributes;

namespace Level.Actions
{
    public class UpdateAutoTile : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal ScriptableGrid m_grid;
        [SerializeField, Input]
        internal ScriptableVector3 m_currentPosition;

        [SerializeField] 
        internal TilesSetListConfig m_setListConfig;
        [SerializeField] internal TileTypeIdentifier m_autoTile;
        [SerializeField] internal TileTypeIdentifier m_floorTile;
        [SerializeField] internal TileTypeIdentifier m_wallTile;
        [SerializeField] internal TileTypeIdentifier m_emptyTile;

        public override string Name => nameof(UpdateAutoTile);
        public static Type StaticFactoryType => typeof(UpdateAutoTileAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            var setData = m_setListConfig.GetSet(m_autoTile.Config.Result);

            return new UpdateAutoTileAction(new GridReference(ctx, m_grid), 
                new Vector3Reference(ctx, m_currentPosition), setData, m_autoTile, m_floorTile, m_wallTile, m_emptyTile);
        }
    }

    public class UpdateAutoTileAction : IDefaultAction
    {
        public UpdateAutoTileAction(GridReference gr, Vector3Reference gridPos, 
            TilesSetData setData,
            TileTypeIdentifier auto, TileTypeIdentifier floor,
            TileTypeIdentifier wall, TileTypeIdentifier empty)
        {
            m_gridReference = gr;
            m_gridPos = gridPos;
            m_setData = setData;
            m_auto = auto;
            m_floor = floor;
            m_wall = wall;
            m_empty = empty;
        }

        GridReference m_gridReference;
        readonly Vector3Reference m_gridPos;
        readonly TilesSetData m_setData;

        readonly TileTypeIdentifier m_auto;
        readonly TileTypeIdentifier m_floor;
        readonly TileTypeIdentifier m_wall;
        readonly TileTypeIdentifier m_empty;

        public void Invoke()
        {
            var grid = m_gridReference.Value;
            var pos = m_gridPos.Value.Vector3Int();
            var gridTile = m_setData.GetTileIdx(grid[pos.x, pos.y, pos.z]);
            if (gridTile != m_auto.TileIdx)
                return;
            
            var belowTile = m_wall.TileIdx;
            var hasBelow = pos.y > 0;
            if (hasBelow) 
                belowTile = m_setData.GetTileIdx(m_gridReference.Value[pos.x, pos.y - 1, pos.z]);

            if (belowTile == m_floor.TileIdx || belowTile == m_empty.TileIdx)
                grid[pos.x, pos.y, pos.z] = m_setData.GetCombinedTile(gridTile, m_empty.TileIdx);
            else if (belowTile == m_wall.TileIdx)
                grid[pos.x, pos.y, pos.z] = m_setData.GetCombinedTile(gridTile, m_floor.TileIdx);
        }
    }
}
