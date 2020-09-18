using UnityEngine;

using Level.PlatformLayer.Interface;
using Level.Tiles;

namespace Level.PlatformLayer
{
    [CreateAssetMenu(menuName = "Level/Room/PlatformLayer")]
    public class PlatformLayerConfig : ScriptableObject, IPlatformLayer
    {
        // ReSharper disable ConvertToAutoProperty
        public float GridSize => m_gridSettings.TileSize;
        public float GridYSize => m_gridSettings.TileHeight;

        public TilesSetListConfig TilesSet => m_tilesSet;
        public Vector2Int Position
        {
            get => m_position;
            set => m_position = value;
        }
        public Vector2Int Size
        {
            get => m_size;
            set => m_size = value;
        }
        // ReSharper restore ConvertToAutoProperty

        // todo: put to editor? (set by room)
        public Vector3Int PositionOffset
        {
            get => m_positionOffset;
            set => m_positionOffset = value;
        }

        public bool IsDirty { get; private set; }

        public ushort[] Tiles => m_tiles;
        public int Count => m_tiles.Length;

        public ushort this[int index]
        {
            get => m_tiles[index];
            set
            {
                m_tiles[index] = value;
                IsDirty = true;
            }
        }

        public Vector2Int Extends => Position + Size;
        public Vector2Int TileDim => Size + m_gridSettings.ExtraDim * Vector2Int.one;

#pragma warning disable 0649 // wrong warnings for SerializeField
        // for undo we need list of ushort
        [SerializeField, HideInInspector]
        ushort[] m_tiles = new ushort[4];

        [SerializeField] GridSettings m_gridSettings;

        [SerializeField] TilesSetListConfig m_tilesSet;

        [SerializeField] Vector3Int m_positionOffset;
        [SerializeField, HideInInspector] Vector2Int m_position;
        [SerializeField, HideInInspector] Vector2Int m_size = Vector2Int.one;
#pragma warning restore 0649 // wrong warnings for SerializeField

        public void InitSize(Vector2Int size)
        {
            Size = size;
            m_tiles = new ushort[TileDim.x * TileDim.y];
        }
        public void SetTiles(ushort[] tiles) => m_tiles = tiles;

        public void SetTilesSet(TilesSetListConfig config) => m_tilesSet = config;
        public void SetGrid(GridSettings config) => m_gridSettings = config;
    }
}