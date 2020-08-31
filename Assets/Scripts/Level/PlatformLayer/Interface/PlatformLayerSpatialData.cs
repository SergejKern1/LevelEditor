using System;
using UnityEngine;

namespace Level.PlatformLayer.Interface
{
    public interface IPlatformLayerSpatialData
    {
        Vector2Int Position { get; set; }
        Vector2Int Size { get; set; }
        Vector2Int Extends { get; }
        Vector2Int TileDim { get; }
    }
}