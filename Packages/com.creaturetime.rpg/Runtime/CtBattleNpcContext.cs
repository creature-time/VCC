
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNpcContext : CtNpcContext
    {
        public override CtBehaviorTreeNodeBase[] GetActions()
        {
            return null;
        }
    }
}
