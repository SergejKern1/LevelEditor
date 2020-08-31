using System;
using ScriptableUtility;
using ScriptableUtility.ActionConfigs;
using ScriptableUtility.Actions;

namespace Level.Actions
{
    public class AutoFloorMesh : ScriptableBaseAction
    {
        public override string Name { get; }
        public override Type FactoryType { get; }
        public override IBaseAction CreateAction(IContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
