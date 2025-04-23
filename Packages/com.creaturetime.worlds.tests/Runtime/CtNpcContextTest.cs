
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcContextTest : CtNpcContext
    {
        public override CtBehaviorTreeNodeBase[] GetActions() => new CtBehaviorTreeNodeBase[0];
    }
}