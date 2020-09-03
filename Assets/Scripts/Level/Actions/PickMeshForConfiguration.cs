using System;
using Level.Data;
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
    public class PickMeshForConfiguration : ScriptableBaseAction
    {
        [SerializeField] internal TilesSetListConfig m_primaryFilterSetListConfig;
        [SerializeField] internal TileTypeIdentifier m_primaryFilterIdentifier;

        [SerializeField] internal ScriptableTilesSetFilter m_filter;
        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_pos;

        [SerializeField] internal ScriptableMeshSet m_meshSet;

        // todo: restrict to MeshConfigs
        [SerializeField] internal ObjectReference m_meshConfig;
        [SerializeField]  bool m_invertConfiguration;

        public override string Name => nameof(PickMeshForConfiguration);
        public static Type StaticFactoryType => typeof(PickMeshForConfigurationAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            m_meshConfig.Init(ctx);

            if (!GetFilter(out var primaryFilter)) 
                return null;

            return new PickMeshForConfigurationAction(primaryFilter, 
                new TilesSetFilterReference(ctx, m_filter),
                new GridReference(ctx, m_grid),
                new Vector3Reference(ctx, m_pos),
                new MeshSetReference(ctx, m_meshSet), 
                m_invertConfiguration, m_meshConfig);
        }

        bool GetFilter(out TilesSetFilter primaryFilter)
        {
            primaryFilter = default;
            var primaryFilterSetData = m_primaryFilterSetListConfig.GetSet(m_primaryFilterIdentifier.Config.Result);
            if (primaryFilterSetData.TileConfig == null)
            {
                Debug.LogError(
                    $"{nameof(ITileConfig)} {m_primaryFilterIdentifier.Config} not found in given {nameof(TilesSetListConfig)}");
                return false;
            }

            primaryFilter = new TilesSetFilter() {Data = primaryFilterSetData, FilterIdx = m_primaryFilterIdentifier.TileIdx};
            return true;
        }
    }

    public class PickMeshForConfigurationAction : IDefaultAction
    {
        readonly TilesSetFilter m_primaryFilter;
        readonly TilesSetFilterReference m_tilesSetFilter;
        readonly GridReference m_grid;
        readonly Vector3Reference m_pos;
        MeshSetReference m_meshSet;

        readonly bool m_invertConfiguration;
        readonly ObjectReference m_meshConfig;

        public PickMeshForConfigurationAction(TilesSetFilter primaryFilter, 
            TilesSetFilterReference tilesSetFilter, 
            GridReference grid, 
            Vector3Reference pos,
            MeshSetReference meshSet,
            bool invertConfiguration,
            ObjectReference meshConfig)
        {
            m_primaryFilter = primaryFilter;
            m_tilesSetFilter = tilesSetFilter;
            m_grid = grid;
            m_pos = pos;
            m_meshSet = meshSet;
            m_invertConfiguration = invertConfiguration;

            m_meshConfig = meshConfig;
        }

        public void Invoke()
        {
            var tiles = m_grid.Value;
            var x = (int) m_pos.Value.x;
            var y = (int) m_pos.Value.y;
            var z = (int) m_pos.Value.z;

            //tileTypes:
            var config = GetConfiguration(m_primaryFilter, m_tilesSetFilter, tiles, x, y, z).Configuration;
            Debug.Log($"pos {m_pos.Value} config {config}");
            if (m_invertConfiguration)
                config = ~config;

            var meshConfig = m_meshConfig.Value as TileMeshConfig;
            if (meshConfig == null)
                return;
            var meshes = meshConfig.Get(config);
            m_meshSet.SetValue(meshes);
        }
    }
}
