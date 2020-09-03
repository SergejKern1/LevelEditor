using System;
using Level.ScriptableUtility;
using Level.Texture;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using UnityEngine;

namespace Level.Actions
{
    public class TextureTypeUVsForMeshUVs : ScriptableBaseAction
    {
        [SerializeField] internal LevelTextureConfig m_texConfig;
        [SerializeField] internal ScriptableMeshData m_meshData;
        [SerializeField] internal ScriptableTilesSetFilter m_tileSetFilter;

        public override string Name => nameof(TextureTypeUVsForMeshUVs);
        public static Type StaticFactoryType => typeof(TextureTypeUVsForMeshUVsAction);
        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var meshData = new MeshDataReference(ctx, m_meshData);
            var tileSetFilter = new TilesSetFilterReference(ctx, m_tileSetFilter);

            return new TextureTypeUVsForMeshUVsAction(meshData, tileSetFilter,
                m_texConfig);
        }
    }

    public class TextureTypeUVsForMeshUVsAction : IDefaultAction
    {
        readonly MeshDataReference m_meshData;
        readonly TilesSetFilterReference m_tilesSetFilter;

        readonly LevelTextureConfig m_texConfig;

        public TextureTypeUVsForMeshUVsAction(MeshDataReference mdr,
            TilesSetFilterReference tsf,
            LevelTextureConfig texConf)
        {
            m_meshData = mdr;
            m_tilesSetFilter = tsf;
            m_texConfig = texConf;
        }

        public void Invoke()
        {
            Vector2 uvStart = default;
            Vector2 uvEnd = default;
            m_texConfig.GetFloorUVCoords(m_tilesSetFilter.Value.FilterIdx, 0, ref uvStart, ref uvEnd);
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
