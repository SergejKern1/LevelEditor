using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Level/Grid")]
    public class ScriptableGrid : ScriptableVariable<ushort[,,]> {
        public override ushort[,,] GetValue(OverrideValue value)
        {
            var vars = value.StringValue.Split(',');
            var dimX = ushort.Parse(vars[0]);
            var dimY = ushort.Parse(vars[1]);
            var dimZ = ushort.Parse(vars[2]);

            var arr = new ushort[dimX, dimY, dimZ];
            int x=0, y=0, z=0;
            for (var i = 3; i < vars.Length; i++)
            {
                arr[x, y, z]= ushort.Parse(vars[i]);

                x++;
                if (x >= dimX)
                {
                    x = 0;
                    y++;
                }

                if (y >= dimY)
                {
                    y = 0;
                    z++;
                }
                if (z >= dimZ)
                    break;
            }

            return arr;
        }

        public override OverrideValue GetValue(ushort[,,] value)
        {
            var xLength = value.GetLength(0);
            var yLength = value.GetLength(1);
            var zLength = value.GetLength(2);

            var stringRep = $"{xLength},{yLength},{zLength},";
            for (var z = 0; z < zLength; z++)
            for (var y = 0; y < yLength; y++)
            for (var x = 0; x < xLength; x++)
            {
                stringRep += $"{value[x, y, z]},";
            }
            return new OverrideValue() {StringValue = stringRep };
        }
    }
}