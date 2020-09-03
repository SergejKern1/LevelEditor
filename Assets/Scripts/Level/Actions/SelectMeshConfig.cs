using System;
using Core.Interface;
using Level.Data;

using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Level.Actions
{
    // todo make general, use set Object
    public class SelectMeshConfig : ScriptableBaseAction
    {
        [SerializeField] internal TileMeshConfig m_meshConfig;
        [SerializeField] internal ObjectReference m_meshConfigVar;

        public override string Name => nameof(SelectMeshConfig);
        public static Type StaticFactoryType => typeof(SelectMeshConfigAction);
        public override Type FactoryType => StaticFactoryType;
        public override IBaseAction CreateAction(IContext ctx)
        {
            m_meshConfigVar.Init(ctx);
            return new SelectMeshConfigAction(m_meshConfig, m_meshConfigVar);
        }
    }

    public class SelectMeshConfigAction : IDefaultAction
    {
        readonly TileMeshConfig m_meshConfig;
        ObjectReference m_meshConfigVar;

        public SelectMeshConfigAction(TileMeshConfig meshConfig,
            ObjectReference meshConfigVar)
        {
            m_meshConfig = meshConfig;
            m_meshConfigVar = meshConfigVar;
        }

        public void Invoke() => m_meshConfigVar.SetValue(m_meshConfig as Object);
    }
}
