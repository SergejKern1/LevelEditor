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

namespace Level.Actions
{
    public class TextureTypeUVsForMeshUVs : ScriptableBaseAction
    {
        [SerializeField] internal TilesSetListConfig m_sourceConfig;
        [SerializeField] internal LevelTextureConfig m_texConfig;

        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_currentPosition;
        [SerializeField] internal ScriptableMeshData m_meshData;

        public override string Name => nameof(TextureTypeUVsForMeshUVs);
        public static Type StaticFactoryType => typeof(TextureTypeUVsForMeshUVsAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var grid = new GridReference(ctx, m_grid);
            var currentPos = new Vector3Reference(ctx, m_currentPosition);
            var meshData = new MeshDataReference(ctx, m_meshData);

            return new TextureTypeUVsForMeshUVsAction(m_sourceConfig, grid, currentPos, meshData, 
                m_texConfig);
        }
    }

    public class TextureTypeUVsForMeshUVsAction : IDefaultAction
    {
        readonly GridReference m_grid;
        readonly Vector3Reference m_currentPos;

        readonly MeshDataReference m_meshData;
        readonly TilesSetData m_texConfigSetData;
        readonly LevelTextureConfig m_texConfig;

        public TextureTypeUVsForMeshUVsAction(TilesSetListConfig sourceConfig, 
            GridReference grid, 
            Vector3Reference currentPos,
            MeshDataReference mdr,
            LevelTextureConfig testUVs)
        {
            m_grid = grid;
            m_currentPos = currentPos;

            m_meshData = mdr;

            m_texConfig = testUVs;
            m_texConfigSetData = sourceConfig.GetSet(testUVs);
        }

        public void Invoke()
        {
            var grid = m_grid.Value;
            var pos = m_currentPos.Value.Vector3Int();
            var tile = grid[pos.x, pos.y, pos.z];

            var texType = m_texConfigSetData.GetTileIdx(tile);
            Vector2 uvStart = default;
            Vector2 uvEnd = default;
            m_texConfig.GetFloorUVCoords(texType, 0, ref uvStart, ref uvEnd);
            UpdateUVs(uvStart, uvEnd);
        }

        void UpdateUVs(Vector2 start, Vector2 end)
        {
            var md = m_meshData.Value;
            var uvs = md.UVs;
            var range = end - start;
            for (var i = 0; i < md.UVs.Count; i++)
            {
                var origUV = uvs[i];
                uvs[i] = new Vector2(start.x + origUV.x * range.x, start.y + origUV.y * range.y);
            }
        }
    }
}
