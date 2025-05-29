
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcExpertTest : CtNpcExpert
    {
        public CtBehaviorTreeNodeBase[] nodes = {};

        public override int GetInsistence(CtNpcContext blackboard)
        {
            return 0;
        }

        public override void Execute(CtNpcContext blackboard)
        {
            // TODO: Add actions to blackboard.
        }

        public override CtBehaviorTreeNodeBase[] GetActions()
        {
            return nodes;
        }
    }
}