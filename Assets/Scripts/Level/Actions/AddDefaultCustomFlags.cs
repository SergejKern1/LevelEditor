using System;
using Level.ScriptableUtility;
using Level.Texture;
using Level.Tiles.MarchingSquares;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode;
using XNode.Attributes;
using static Level.Tiles.TileProcessing;

namespace Level.Tiles.Actions
{
    public class AddDefaultCustomFlags : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal TilesSetListConfig m_tilesSetListConfig;
        [SerializeField, Input]
        internal TileTypeIdentifier m_floorNode;
        [SerializeField, Input]
        internal LevelTextureConfig m_texTypeConfig;

        [SerializeField, Input]
        internal TilesSetListConfig m_tilesEditConfig;
        [SerializeField, Input]
        internal HardEdgeConfig m_hardEdgeConfig;
        [SerializeField, Input]
        internal CountNodesConfig m_floorTexTypesConfig;
        [SerializeField, Input]
        internal CountNodesConfig m_activeFloorNodesConfig;

        [SerializeField, Input]
        internal ScriptableVector3 m_currentPosition;

        [SerializeField, Input]
        internal ScriptableGrid m_sourceGrid;
        [SerializeField, Input]
        internal ScriptableGrid m_targetGrid;

        public override string Name => nameof(AddDefaultCustomFlags);
        public static Type StaticFactoryType => typeof(AddDefaultCustomFlagsAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            var pos = new Vector3Reference(ctx, m_currentPosition);
            var grid = new GridReference(ctx, m_sourceGrid);
            var editGrid = new GridReference(ctx, m_targetGrid);

            var hardEdgeSet = m_tilesEditConfig.GetSet(m_hardEdgeConfig);
            var floorTexTypesSet = m_tilesEditConfig.GetSet(m_floorTexTypesConfig);
            var activeFloorNodesSet = m_tilesEditConfig.GetSet(m_activeFloorNodesConfig);

            return new AddDefaultCustomFlagsAction(m_tilesSetListConfig, m_floorNode, m_texTypeConfig, pos, grid, 
                editGrid, hardEdgeSet, floorTexTypesSet, activeFloorNodesSet);
        }
    }

    public class AddDefaultCustomFlagsAction : IDefaultAction
    {
        readonly TilesSetListConfig m_tilesSetListConfig;
        readonly TileTypeIdentifier m_floorNode;
        readonly LevelTextureConfig m_texTypeConfig;

        readonly Vector3Reference m_pos;
        readonly GridReference m_sourceGrid;
        readonly GridReference m_targetGrid;
        readonly TilesSetData m_hardEdgeSet;
        readonly TilesSetData m_floorTexTypesSet;
        readonly TilesSetData m_activeFloorNodesSet;

        public AddDefaultCustomFlagsAction(TilesSetListConfig config, TileTypeIdentifier floorNode, LevelTextureConfig texTypeConfig,
            Vector3Reference pos,
            GridReference sourceGrid, 
            GridReference targetGrid, 
            TilesSetData hardEdgeSet,
            TilesSetData floorTexTypesSet, 
            TilesSetData activeFloorNodesSet)
        {
            m_tilesSetListConfig = config;
            m_floorNode = floorNode;
            m_texTypeConfig = texTypeConfig;

            m_pos = pos;
            m_sourceGrid = sourceGrid;
            m_targetGrid = targetGrid;

            m_hardEdgeSet = hardEdgeSet;
            m_floorTexTypesSet = floorTexTypesSet;
            m_activeFloorNodesSet = activeFloorNodesSet;
        }

        public void Invoke()
        {
            var pos = m_pos.Value;
            // compares each of the 4 nodes geo-type (6 comparisons), 4 types = 0, 3 types = 1, 2 types = 2, 1 type = 6
            var equalNodesGeoType = 0;
            // how many 0-4 nodes are of type floor
            var activeFloorNodes = 0;
            // how many 0-4 different texture-types do we have for floor nodes:
            var floorTexTypes = 4;
            var forceHardEdge = false;

            CollectNodeInfo(pos, ref activeFloorNodes, ref forceHardEdge, ref floorTexTypes, ref equalNodesGeoType);

            var flag = GetHardEdgeState(equalNodesGeoType, floorTexTypes, activeFloorNodes, forceHardEdge);
            UpdateGrid(pos, flag, floorTexTypes, activeFloorNodes);
        }

        void CollectNodeInfo(Vector3 pos, ref int activeFloorNodes, ref bool forceHardEdge, ref int floorTexTypes, ref int equalNodesGeoType)
        {
            var x = (int) pos.x;
            var y = (int) pos.y;
            var z = (int) pos.z;

            var sourceGrid = m_sourceGrid.Value;
            
            var geoDatSet = m_tilesSetListConfig.GetSet(m_floorNode.Config.Result);
            var texSet = m_tilesSetListConfig.GetSet(m_texTypeConfig);

            for (var nodeAOffIdx = 0; nodeAOffIdx < IterationOffset.Length; nodeAOffIdx++)
            {
                var offA = IterationOffset[nodeAOffIdx];
                var nodeA = sourceGrid[x + offA.x, y + offA.y, z + offA.z];

                var equalsInTexType = false;

                var nodeAGeoType = geoDatSet.GetTileIdx(nodeA);
                var nodeATexType = ushort.MaxValue;
                if (nodeAGeoType == m_floorNode.TileIdx)
                {
                    ++activeFloorNodes;
                    nodeATexType = texSet.GetTileIdx(nodeA);
                    if (m_texTypeConfig != null)
                        forceHardEdge |= m_texTypeConfig.IsForce(nodeATexType);
                }
                else --floorTexTypes;

                for (var nodeBOffIdx = nodeAOffIdx + 1; nodeBOffIdx < IterationOffset.Length; nodeBOffIdx++)
                {
                    var offB = IterationOffset[nodeBOffIdx];
                    var nodeB = sourceGrid[x + offB.x, y + offB.y, z + offB.z];

                    var nodeBGeoType = geoDatSet.GetTileIdx(nodeB);
                    var nodeBTexType = texSet.GetTileIdx(nodeB);

                    if (nodeBGeoType == m_floorNode.TileIdx)
                        equalsInTexType |= (nodeATexType == nodeBTexType);
                    if (nodeAGeoType == nodeBGeoType)
                        ++equalNodesGeoType;
                }

                if (equalsInTexType)
                    --floorTexTypes;
            }
        }

        void UpdateGrid(Vector3 pos, HardEdgeState flag, int floorTexTypes, int activeFloorNodes)
        {
            var x = (int) pos.x;
            var y = (int) pos.y;
            var z = (int) pos.z;

            var tileData = m_targetGrid.Value[x, y, z];
            tileData = m_hardEdgeSet.GetCombinedTile(tileData, (uint) flag);
            tileData = m_floorTexTypesSet.GetCombinedTile(tileData, (uint) floorTexTypes);
            tileData = m_activeFloorNodesSet.GetCombinedTile(tileData, (uint) activeFloorNodes);

            m_targetGrid.Value[x, y, z] = tileData;
        }

        static HardEdgeState GetHardEdgeState(int equalNodesGeoType, int floorTexTypes, int activeFloorNodes,
            bool forceHardEdge)
        {
            var allNodesEqualGeoType = equalNodesGeoType == 6;
            var multipleTexAndGeometryTypes = floorTexTypes > 1 && !allNodesEqualGeoType;
            var tooManyFloorTexTypes = floorTexTypes > 2;
            var tooManyGeoTypes = equalNodesGeoType <= 1;

            var hard = tooManyFloorTexTypes || multipleTexAndGeometryTypes || tooManyGeoTypes;
            // when we have 2 activeFloorNodes or all node types are equal we don't have diagonal edges anyway, so it breaks the force-hard-edge propagation
            var breakHard = !hard && (activeFloorNodes == 2 || allNodesEqualGeoType);

            var flag = GetHardEdgeState(hard, forceHardEdge, breakHard);
            return flag;
        }

        static HardEdgeState GetHardEdgeState(bool hard, bool forceHardEdge, bool breakHard)
        {
            var flag = HardEdgeState.Default;
            if (hard || forceHardEdge)
                flag |= HardEdgeState.Hard;
            if (breakHard)
                flag |= HardEdgeState.Break;
            return flag;
        }
    }
}
