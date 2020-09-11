using System;
using System.Collections.Generic;
using Level.Data;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;
using XNode;

namespace Level.ScriptableUtility
{
    [Serializable]
    public struct MeshDataReference : IContextVariableReference<MeshData>
    {
        public MeshDataReference(MeshData value)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Intrinsic;

            m_scriptable = null;
            m_contextProvider = null;
            m_intrinsicValue = value;
        }
        public MeshDataReference(IContext ctx, ScriptableMeshData value)
        {
            m_context = ctx;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = value;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }
        public MeshDataReference(ScriptableMeshData scriptable)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = scriptable;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }

        [SerializeField] ReferenceContextType m_contextType;
        [SerializeField] ScriptableMeshData m_scriptable;
        [SerializeField] GameObject m_contextProvider;
        [SerializeField] MeshData m_intrinsicValue;
        IContext m_context;

		public Type VariableType => typeof(int);
        public MeshData Value => this.GetValue();

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public ReferenceContextType ContextType => m_contextType;
        public ScriptableVariable<MeshData> Scriptable => m_scriptable;

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
        public void __SetContext(IContext c) => m_context = c;

        public MeshData IntrinsicValue
        {
            get => m_intrinsicValue;
            set => m_intrinsicValue = value;
        }
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public static implicit operator MeshData(MeshDataReference reference) =>
            reference.Value;
        public static implicit operator MeshDataReference(MeshData val) =>
            new MeshDataReference(val);

		// todo: let CodeGen nest things depending on checkbox
		// public static MeshDataReference operator +(MeshDataReference _ref, int val) => _ref.SetValue(_ref.Value + val);
        // public static MeshDataReference operator -(MeshDataReference _ref, int val) => _ref.SetValue(_ref.Value - val);

        public static void SetContext(ref MeshDataReference @ref, IContext ctx) => @ref.m_context = ctx;
    }

    public static class MeshDataReferenceExt
    {
	    public static void Init(this IList<MeshDataReference> list, IContext ctx)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var c = list[i];
                c.Init(ctx);
                list[i] = c;
            }
        }

        public static void Init(this ref MeshDataReference @ref, IContext ctx) => 
            @ref = @ref.InitInternal<MeshDataReference, MeshData>(ctx);
        public static void Init(this ref MeshDataReference @ref, string fieldName, Node n, IContext ctx) => 
            @ref = @ref.InitInternal<MeshDataReference, MeshData>(fieldName, n, ctx);

        public static MeshDataReference SetValue(this ref MeshDataReference @ref, MeshData val)
        {
            @ref = (MeshDataReference) @ref.SetValueInternal(val);
            return @ref;
        }
    }
}
