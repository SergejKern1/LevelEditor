using System;
using System.Collections.Generic;
using Core.Unity.Interface;
using Level.Tiles.Interface;
using UnityEngine;

namespace Level.Tiles
{
    [CreateAssetMenu(menuName = "Level/Tiles/DataSetList")]
    public class TilesSetListConfig : ScriptableObject, IVerify
    {
        public List<TilesSetData> TileDataSets = new List<TilesSetData>();
        #if UNITY_EDITOR
        [NonSerialized] public ushort Editor_Digits;
        #endif
        public void UpdateMaskAndShift() => UpdateMaskAndShift(ref TileDataSets);

        static void UpdateMaskAndShift(ref List<TilesSetData> tileDataSets) => UpdateMaskAndShift(ref tileDataSets, out _);

        static void UpdateMaskAndShift(ref List<TilesSetData> tileDataSets, out ushort digits)
        {
            ushort lastShift = 0;
            for (var i = 0; i < tileDataSets.Count; i++)
            {
                var tilesData = tileDataSets[i];
                if (tilesData.TileConfig?.Result == null)
                    continue;
                var tileConfig = tilesData.TileConfig.Result;

                tilesData.Shift = lastShift;

                var thisLastTile = (uint)(tileConfig.Count - 1);

                ushort mask = 0;
                var shift = (ushort) Convert.ToString(thisLastTile, 2).Length;
                shift = Math.Max(shift, tileConfig.ReserveBits);
                for (var s = 0; s < shift; s++)
                    mask |= (ushort)(1 << s);

                tilesData.Mask = (ushort) (mask << lastShift);
                lastShift += shift;
                tileDataSets[i] = tilesData;
            }

            digits = lastShift;
        }

        public TilesSetData GetSet(ITileConfig config) 
            => TileDataSets.Find(set => set.TileConfig.Result == config);

        public void Verify(ref VerificationResult result)
        {
            var tileDataSetsCopy = new List<TilesSetData>(TileDataSets);
            UpdateMaskAndShift(ref tileDataSetsCopy, out Editor_Digits);

            for (var i = 0; i < TileDataSets.Count; i++)
            {
                var tileDataSet = TileDataSets[i];
                var tileDataSetCopy = tileDataSetsCopy[i];
                if (tileDataSet.Shift != tileDataSetCopy.Shift)
                {
                    result.ErrorLogs.Add(new LogData($"{nameof(tileDataSet.Shift)} should be {tileDataSetCopy.Shift} " +
                                                     $"but is {tileDataSet.Shift} at index {i}",this));
                    break;
                }
                if (tileDataSet.Mask != tileDataSetCopy.Mask)
                    result.ErrorLogs.Add(new LogData(
                        $"{nameof(tileDataSet.Mask)} should be {tileDataSetCopy.Mask} " +
                        $"but is {tileDataSet.Mask} at index {i}", this));
            }

            if (Editor_Digits > 16) result.ErrorLogs.Add(new LogData($"Too much tile-data for ushort", this));
        }
    }
}