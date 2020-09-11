using System;
using System.Collections.Generic;
using Level.Tiles;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;
using XNode;

namespace Level.ScriptableUtility
{
    [Serializable]
    public struct TilesSetFilterReference : IContextVariableReference<TilesSetFilter>
    {
        public TilesSetFilterReference(TilesSetFilter value)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Intrinsic;

            m_scriptable = null;
            m_contextProvider = null;
            m_intrinsicValue = value;
        }
        public TilesSetFilterReference(IContext ctx, ScriptableTilesSetFilter value)
        {
            m_context = ctx;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = value;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }
        public TilesSetFilterReference(ScriptableTilesSetFilter scriptable)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = scriptable;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }

        [SerializeField] ReferenceContextType m_contextType;
        [SerializeField] ScriptableTilesSetFilter m_scriptable;
        [SerializeField] GameObject m_contextProvider;
        [SerializeField] TilesSetFilter m_intrinsicValue;
        IContext m_context;

		public Type VariableType => typeof(TilesSetFilter);
        public TilesSetFilter Value => this.GetValue();

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public ReferenceContextType ContextType => m_contextType;
        public ScriptableVariable<TilesSetFilter> Scriptable => m_scriptable;

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

        public TilesSetFilter IntrinsicValue
        {
            get => m_intrinsicValue;
            set => m_intrinsicValue = value;
        }
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public static implicit operator TilesSetFilter(TilesSetFilterReference reference) =>
            reference.Value;
        public static implicit operator TilesSetFilterReference(TilesSetFilter val) =>
            new TilesSetFilterReference(val);

		// todo: let CodeGen nest things depending on checkbox
		// public static TilesSetFilterReference operator +(TilesSetFilterReference _ref, TilesSetFilter val) => _ref.SetValue(_ref.Value + val);
        // public static TilesSetFilterReference operator -(TilesSetFilterReference _ref, TilesSetFilter val) => _ref.SetValue(_ref.Value - val);

        public static void SetContext(ref TilesSetFilterReference @ref, IContext ctx) => @ref.m_context = ctx;
    }

    public static class TilesSetFilterReferenceExt
    {
	    public static void Init(this IList<TilesSetFilterReference> list, IContext ctx)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var c = list[i];
                c.Init(ctx);
                list[i] = c;
            }
        }

        public static void Init(this ref TilesSetFilterReference @ref, IContext ctx) => 
            @ref = @ref.InitInternal<TilesSetFilterReference, TilesSetFilter>(ctx);
        public static void Init(this ref TilesSetFilterReference @ref, string fieldName, Node n, IContext ctx) => 
            @ref = @ref.InitInternal<TilesSetFilterReference, TilesSetFilter>(fieldName, n, ctx);

        public static TilesSetFilterReference SetValue(this ref TilesSetFilterReference @ref, TilesSetFilter val)
        {
			@ref = (TilesSetFilterReference) @ref.SetValueInternal(val);
            return @ref;
        }
    }
}
