using Level.Tiles.Interface;
using UnityEngine;

namespace Level.Tiles.MarchingSquares
{
    [CreateAssetMenu(menuName = "Level/Tiles/CountNodesConfig")]
    public class CountNodesConfig : ScriptableObject, ITileConfig
    {
        [SerializeField] string m_name;

        public string Name => m_name;
        public ushort ReserveBits => 4;
        public int Count => 5;

        // todo: don't make me implement editor shit
        public ITilesData this[int idx] => null;
    }
}