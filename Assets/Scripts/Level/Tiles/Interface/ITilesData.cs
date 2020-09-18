using System;
using Core.Unity.Types;
using UnityEngine;

namespace Level.Tiles.Interface
{
    public interface ITilesData
    {
        string TileName { get; }
        Color TileColor { get; }
        Texture2D PreviewTex { get; }
    }

    [Serializable]
    public class RefITilesData: InterfaceContainer<ITilesData>{}
}
