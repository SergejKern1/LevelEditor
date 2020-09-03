using System;
using Core.Interface;
using Level.ScriptableUtility;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;
using ScriptableUtility.Variables.Reference;
using ScriptableUtility.Variables.Scriptable;
using UnityEngine;

namespace Level.Actions
{
    public class MeshSetIterator : ScriptableBaseAction
    {
        [SerializeField] internal ScriptableMeshSet m_meshSet;
        [SerializeField] internal ScriptableUnityObject m_mesh;

        [SerializeField] internal  ScriptableBaseAction m_continueWith;

        public override string Name => nameof(MeshSetIterator);
        public static Type StaticFactoryType => typeof(MeshSetIteratorAction);
        public override Type FactoryType => StaticFactoryType;

#if UNITY_EDITOR
        public static string Editor_ContinueWithPropName => nameof(m_continueWith);
#endif

        public override IBaseAction CreateAction(IContext ctx)
        {
            var continueAction = m_continueWith.CreateAction(ctx) as IDefaultAction;
            return new MeshSetIteratorAction(new MeshSetReference(ctx, m_meshSet), 
                new ObjectReference(ctx, m_mesh),
                continueAction);
        }
    }

    public class MeshSetIteratorAction : IDefaultAction
    {
        readonly MeshSetReference m_meshSet;
        ObjectReference m_mesh;
        readonly IDefaultAction m_continue;

        public MeshSetIteratorAction(MeshSetReference meshSet, ObjectReference mesh, IDefaultAction continueAction)
        {
            m_meshSet = meshSet;
            m_mesh = mesh;
            m_continue = continueAction;
        }

        public void Invoke()
        {
            var data = m_meshSet.Value;
            foreach (var m in data.Meshes)
            {
                m_mesh.SetValue((UnityEngine.Object) m);
                m_continue.Invoke();
            }
        }
    }
}
