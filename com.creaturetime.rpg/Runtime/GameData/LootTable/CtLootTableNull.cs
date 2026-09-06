
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLootTableNull : CtAbstractLootTableObject
    {
        public override bool IsNull => true;
    }
}