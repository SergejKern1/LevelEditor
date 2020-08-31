using UnityEngine;

using Level.PlatformLayer.Interface;
using Level.Tiles;

namespace Level.PlatformLayer
{
    [CreateAssetMenu(menuName = "Level/Room/PlatformLayer")]
    public class PlatformLayerScriptable : ScriptableObject, IPlatformLayer
    {
        [SerializeField] float m_gridSize = 1;
        [SerializeField] float m_gridYSize = 1;
        [SerializeField] TilesSetListConfig m_tilesSet;

        [SerializeField] Vector3Int m_positionOffset;
        [SerializeField, HideInInspector] Vector2Int m_position;
        [SerializeField, HideInInspector] Vector2Int m_size = Vector2Int.one;

        // ReSharper disable ConvertToAutoProperty
        public float GridSize => m_gridSize;
        public float GridYSize => m_gridYSize;
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

        // for undo we need list of ushort
        [SerializeField, HideInInspector]
        ushort[] m_tiles = new ushort[4];

        // todo: put to editor? (set by room)
        public Vector3Int PositionOffset
        {
            get => m_positionOffset;
            set => m_positionOffset = value;
        }

        public bool IsDirty { get; private set; }

        public ushort[] Tiles => m_tiles;
        public void SetTiles(ushort[] tiles) => m_tiles = tiles;

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
        public Vector2Int TileDim => Size + new Vector2Int(1, 1);
    }
}