using System;
using System.Collections.Generic;
using ScriptableUtility;
using ScriptableUtility.Variables;
using UnityEngine;

namespace Level.ScriptableUtility
{
    [Serializable]
    public struct GridReference : IContextVariableReference<ushort[,,]>
    {
        public GridReference(ushort[,,] value)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Intrinsic;

            m_scriptable = null;
            m_contextProvider = null;
            m_intrinsicValue = value;
        }
        public GridReference(IContext ctx, ScriptableGrid value)
        {
            m_context = ctx;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = value;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }
        public GridReference(ScriptableGrid scriptable)
        {
            m_context = null;
            m_contextType = ReferenceContextType.Scriptable;

            m_scriptable = scriptable;
            m_contextProvider = null;
            m_intrinsicValue = default;
        }

        [SerializeField] ReferenceContextType m_contextType;
        [SerializeField] ScriptableGrid m_scriptable;
        [SerializeField] GameObject m_contextProvider;
        [SerializeField] ushort[,,] m_intrinsicValue;
        IContext m_context;

		public Type VariableType => typeof(int);
        public ushort[,,] Value => this.GetValue();

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public ReferenceContextType ContextType => m_contextType;
        public ScriptableVariable<ushort[,,]> Scriptable => m_scriptable;

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

        public ushort[,,] IntrinsicValue
        {
            get => m_intrinsicValue;
            set => m_intrinsicValue = value;
        }
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public static implicit operator ushort[,,](GridReference reference) =>
            reference.Value;
        public static implicit operator GridReference(ushort[,,] val) =>
            new GridReference(val);

		// todo: let CodeGen nest things depending on checkbox
		// public static GridReference operator +(GridReference _ref, int val) => _ref.SetValue(_ref.Value + val);
        // public static GridReference operator -(GridReference _ref, int val) => _ref.SetValue(_ref.Value - val);

        public static void SetContext(ref GridReference @ref, IContext ctx) => @ref.m_context = ctx;
    }

    public static class GridReferenceExt
    {
	    public static void Init(this IList<GridReference> list, IContext ctx)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var c = list[i];
                c.Init(ctx);
                list[i] = c;
            }
        }

        public static void Init(this ref GridReference @ref, IContext ctx)
		{
            if (@ref.ContextType != ReferenceContextType.Scriptable)
                return;

            if (@ref.Scriptable == null)
            {
                Debug.LogError("Scriptable null!");
                return;
            }

            if (@ref.Scriptable.ContextType == ScriptableContextType.GameObjectContext)
                GridReference.SetContext(ref @ref, ctx);
        }

        public static GridReference SetValue(this ref GridReference @ref, ushort[,,] value)
        {
            if (@ref.ContextType == ReferenceContextType.Intrinsic)
            {
                @ref.IntrinsicValue = value;
                return @ref;
            }

            if (@ref.Scriptable == null)
            {
                Debug.LogError("Scriptable null!");
                return default;
            }

            if (@ref.Scriptable.ContextType == ScriptableContextType.GlobalContext)
                @ref.Scriptable.GlobalValue = value;
            else if (@ref.Context != null)
                @ref.Scriptable.SetWithContext(@ref.Context, value);
            else
                Debug.LogError("Context null!");

            return @ref;
        }
    }
}
