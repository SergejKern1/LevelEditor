using System;
using Core.Interface;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level.Tiles.Actions
{
    public class GridIterator : ScriptableBaseAction
    {
        [SerializeField] internal ScriptableGrid m_grid;
        [SerializeField] internal ScriptableVector3 m_currentPosition;
        // f.e. if grid pos x is compared with x+1 we reduce iteration by 1
        [SerializeField] internal Vector3Int m_reducedIteration;

        [FormerlySerializedAs("m_continueWith")]
        public ScriptableBaseAction ContinueWith;

        public override string Name => nameof(GridIterator);
        public static Type StaticFactoryType => typeof(GridIteratorAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            var continueAction = ContinueWith.CreateAction(ctx) as IDefaultAction;
            var reduce = new Vector3Int(Mathf.Abs(m_reducedIteration.x), 
                Mathf.Abs(m_reducedIteration.y), 
                Mathf.Abs(m_reducedIteration.z));
            
            return new GridIteratorAction(new GridReference(ctx, m_grid), 
                new Vector3Reference(ctx, m_currentPosition),
                continueAction, reduce);
        }
    }

    public class GridIteratorAction : IDefaultAction
    {
        readonly GridReference m_grid;
        Vector3Reference m_currentPosition;
        readonly IDefaultAction m_continue;
        readonly Vector3Int m_reducedIteration;

        public GridIteratorAction(GridReference gridReference, Vector3Reference vector3Reference, IDefaultAction continueAction,
            Vector3Int reducedIteration)
        {
            m_grid = gridReference;
            m_currentPosition = vector3Reference;
            m_continue = continueAction;
            m_reducedIteration = reducedIteration;
        }

        public void Invoke()
        {
            var data = m_grid.Value;
            for (var y = 0; y < data.GetLength(1)-m_reducedIteration.x; y++)
            for (var x = 0; x < data.GetLength(0)-m_reducedIteration.y; x++)
            for (var z = 0; z < data.GetLength(2)-m_reducedIteration.z; z++)
            {
                m_currentPosition.SetValue(new Vector3(x, y, z));
                //Debug.Log($"Setting Value: {m_currentPosition.Value}");
                m_continue.Invoke();
            }
            m_currentPosition.SetValue(new Vector3(-1, -1, -1));
        }
    }
}
