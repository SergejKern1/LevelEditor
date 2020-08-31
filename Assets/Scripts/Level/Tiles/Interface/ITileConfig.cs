using System;
using Core.Unity.Types;

namespace Level.Tiles.Interface
{
    public interface ITileConfig
    {
        string Name { get; }
        ushort ReserveBits { get; }

        int Count { get; }
        ITilesData this[int idx] { get; }
    }

    [Serializable]
    public class RefITileConfig: InterfaceContainer<ITileConfig>{}
}
