using System;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using UnityEngine;

namespace Level.Actions
{
    public class PushMeshData : ScriptableBaseAction
    {
        [SerializeField] internal ScriptableMeshData m_pushData;
        [SerializeField] internal ScriptableMeshData m_targetData;

        public override string Name => nameof(PushMeshData);
        public static Type StaticFactoryType => typeof(PushMeshDataAction);

        public override Type FactoryType => StaticFactoryType;

        public override IBaseAction CreateAction(IContext ctx)
        {
            var pushData = new MeshDataReference(ctx, m_pushData);
            var targetData = new MeshDataReference(ctx, m_targetData);

            return new PushMeshDataAction(pushData, targetData);
        }
    }

    public class PushMeshDataAction : IDefaultAction
    {
        public PushMeshDataAction(MeshDataReference push, MeshDataReference target)
        {
            m_push = push;
            m_target = target;
        }

        MeshDataReference m_push;
        MeshDataReference m_target;

        public void Invoke()
        {
            var source = m_push.Value;
            var target = m_target.Value;

            var startCount = target.Vertices.Count;

            target.Vertices.AddRange(source.Vertices);
            target.UVs.AddRange(source.UVs);
            for (var i = 0; i < source.Triangles.Count; i++) 
                target.Triangles.Add(startCount + source.Triangles[i]);

            source.Vertices.Clear();
            source.UVs.Clear();
            source.Triangles.Clear();
        }
    }
}
