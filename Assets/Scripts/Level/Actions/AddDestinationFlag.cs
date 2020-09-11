using System;
using Core.Unity.Extensions;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using XNode;
using XNode.Attributes;

namespace Level.Tiles.Actions
{
    public class AddDestinationFlag : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal TileTypeIdentifier m_resultTag;
        [SerializeField, Input]
        internal TilesSetListConfig m_destinationConfig;

        [SerializeField, Input]
        internal ScriptableGrid m_resultGrid;
        [SerializeField, Input]
        internal ScriptableVector3 m_currentPosition;

        public override string Name => nameof(AddDestinationFlag);
        public static Type StaticFactoryType => typeof(AddDestinationFlagAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            return new AddDestinationFlagAction(m_resultTag, m_destinationConfig, 
                new GridReference(ctx, m_resultGrid), 
                new Vector3Reference(ctx, m_currentPosition));
        }
    }

    public class AddDestinationFlagAction : IDefaultAction
    {
        readonly TileTypeIdentifier m_resultTag;
        readonly TilesSetListConfig m_destinationConfig;
        readonly GridReference m_resultGrid;
        readonly Vector3Reference m_currentPosition;

        public AddDestinationFlagAction(TileTypeIdentifier resultTag, 
            TilesSetListConfig destinationConfig, 
            GridReference resultGrid, Vector3Reference currentPosition)
        {
            m_resultTag = resultTag;
            m_destinationConfig = destinationConfig;
            m_resultGrid = resultGrid;
            m_currentPosition = currentPosition;
        }

        public void Invoke()
        {
            var resultData = m_resultGrid.Value;
            var pos = m_currentPosition.Value.Vector3Int();
            var x = pos.x;
            var y = pos.y;
            var z = pos.z;
            var set = m_destinationConfig.GetSet(m_resultTag.Config.Result);
            resultData[x, y, z] = set.GetCombinedTile(resultData[x, y, z], m_resultTag.TileIdx);
        }
    }
}
