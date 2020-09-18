using System;
using Core.Interface;
using Core.Unity.Extensions;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.ActionConfigs.Attributes;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using UnityEngine;
using XNode.Attributes;
using static Level.Tiles.TileProcessing;

namespace Level.Tiles.Actions
{
    public class MarchingSquareIterator : ScriptableBaseAction
    {
        [SerializeField, Input]
        internal Vector3Reference m_pos;
        [SerializeField, Input]
        internal Vector3Reference m_cornerPos;
        [SerializeField, Input]
        internal IntReference m_idxOffset;
        [SerializeField, Input]
        internal IntReference m_iteratorIdx;
        [SerializeField, ActionOutput]
        internal ScriptableBaseAction m_continueWith;

        public override string Name => nameof(MarchingSquareIterator);
        public static Type StaticFactoryType => typeof(MarchingSquareIteratorAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            m_pos.Init(nameof(m_pos),this, ctx);
            m_cornerPos.Init(nameof(m_cornerPos), this, ctx);
            m_idxOffset.Init(nameof(m_idxOffset), this, ctx);
            m_iteratorIdx.Init(nameof(m_iteratorIdx), this, ctx);
            var continueAction = (IDefaultAction) m_continueWith.CreateAction(ctx);

            return new MarchingSquareIteratorAction(m_pos, m_idxOffset, m_iteratorIdx, m_cornerPos, continueAction);
        }
    }

    public class MarchingSquareIteratorAction : IDefaultAction
    {
        readonly Vector3Reference m_pos;
        readonly IntReference m_idxOffset;

        IntReference m_iteratorIdx;
        Vector3Reference m_cornerPos;

        readonly IDefaultAction m_continue;

        public MarchingSquareIteratorAction(Vector3Reference pos, 
            IntReference idxOffset, 
            IntReference iteratorIdx, 
            Vector3Reference cornerPos, 
            IDefaultAction action)
        {
            m_pos = pos;
            m_idxOffset = idxOffset;
            m_iteratorIdx = iteratorIdx;
            m_cornerPos = cornerPos;
            m_continue = action;
        }

        public void Invoke()
        {
            var pos = m_pos.Value.Vector3Int();
            for (var idx = m_idxOffset.Value; idx < IterationOffset.Length; idx++)
            {
                var offset = IterationOffset[idx];
                m_iteratorIdx.SetValue(idx);
                m_cornerPos.SetValue(new Vector3(pos.x + offset.x, pos.y + offset.y, pos.z + offset.z));

                m_continue.Invoke();
            }
        }
    }
}
