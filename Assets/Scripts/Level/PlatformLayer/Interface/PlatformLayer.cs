using UnityEngine;

namespace Level.PlatformLayer.Interface
{
    public interface IPlatformLayer : IPlatformLayerTiles, IPlatformLayerSpatialData, IGridData
    {
        Vector3Int PositionOffset { get; }
    }
}
