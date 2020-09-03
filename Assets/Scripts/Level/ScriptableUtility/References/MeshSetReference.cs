using System;
using System.Collections.Generic;
using Level.Data;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [Serializable]
    public struct MeshSetReference : IContextVariableReference<TileMeshConfig.MeshSet>
    {
        public MeshSetReference(TileMeshConfig.MeshSet value)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Intrinsic;

            m_scriptable = null;
            m_contextProvider = null;
            m_intrinsicValue = value;
        }
        public MeshSetReference(IContext ctx, ScriptableMeshSet value)
        {
            m_context = ctx;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = value;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }
        public MeshSetReference(ScriptableMeshSet scriptable)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = scriptable;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }

        [SerializeField] ReferenceContextType m_contextType;
        [SerializeField] ScriptableMeshSet m_scriptable;
        [SerializeField] GameObject m_contextProvider;
        [SerializeField] TileMeshConfig.MeshSet m_intrinsicValue;
        IContext m_context;

		public Type VariableType => typeof(TileMeshConfig.MeshSet);
        public TileMeshConfig.MeshSet Value => this.GetValue();

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public ReferenceContextType ContextType => m_contextType;
        public ScriptableVariable<TileMeshConfig.MeshSet> Scriptable => m_scriptable;

        public IContext Context
        {
            get
            {
                if (m_context != null)
                    return m_context;

                if (m_contextProvider == null)
                    return null;

                var cp = m_contextProvider.GetComponent<IContextProvider>();
                return cp?.Context;
            }
        }

        public TileMeshConfig.MeshSet IntrinsicValue
        {
            get => m_intrinsicValue;
            set => m_intrinsicValue = value;
        }
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public static implicit operator TileMeshConfig.MeshSet(MeshSetReference reference) =>
            reference.Value;
        public static implicit operator MeshSetReference(TileMeshConfig.MeshSet val) =>
            new MeshSetReference(val);

		// todo: let CodeGen nest things depending on checkbox
		// public static MeshSetReference operator +(MeshSetReference _ref, MeshSet val) => _ref.SetValue(_ref.Value + val);
        // public static MeshSetReference operator -(MeshSetReference _ref, MeshSet val) => _ref.SetValue(_ref.Value - val);

        public static void SetContext(ref MeshSetReference @ref, IContext ctx) => @ref.m_context = ctx;
    }

    public static class MeshSetReferenceExt
    {
	    public static void Init(this IList<MeshSetReference> list, IContext ctx)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var c = list[i];
                c.Init(ctx);
                list[i] = c;
            }
        }

        public static void Init(this ref MeshSetReference @ref, IContext ctx)
		{
            if (@ref.ContextType != ReferenceContextType.Scriptable)
                return;

            if (@ref.Scriptable == null)
            {
                Debug.LogError("Scriptable null!");
                return;
            }

            if (@ref.Scriptable.ContextType == ScriptableContextType.GameObjectContext)
                MeshSetReference.SetContext(ref @ref, ctx);
        }

        public static MeshSetReference SetValue(this ref MeshSetReference @ref, TileMeshConfig.MeshSet val)
        {
			@ref = (MeshSetReference) @ref.SetValueInternal(val);
            return @ref;
        }
    }
}
