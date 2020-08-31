﻿using System;
using System.Collections.Generic;
using Level.Tiles.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level.Tiles
{
    [CreateAssetMenu(menuName = "Level/Tiles/DataConfig")]
    public class TileConfig : ScriptableObject, ITileConfig
    {
        [Serializable]
        public class TilesData : ITilesData
        {
            [FormerlySerializedAs("TileName"), SerializeField]
            string m_tileName;
            [FormerlySerializedAs("TileColor"), SerializeField]
            Color m_tileColor;

            public string TileName
            {
                get => m_tileName;
                set => m_tileName = value;
            }
            public Color TileColor
            {
                get => m_tileColor;
                set => m_tileColor = value;
            }
            public Texture2D PreviewTex { get; set; }
        }

        // todo Tiles: limit maximum TileTypes
        [FormerlySerializedAs("TileTypes"), SerializeField] 
        List<TilesData> m_tileTypes;

        [FormerlySerializedAs("ReserveBits"), SerializeField] 
        ushort m_reserveBits;

        public string Name => name;
        public ushort ReserveBits => m_reserveBits;

        public int Count => m_tileTypes.Count;
        public ITilesData this[int idx] => m_tileTypes[idx];
    }
}
