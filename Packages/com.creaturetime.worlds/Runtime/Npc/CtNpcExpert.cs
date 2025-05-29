using UdonSharp;

namespace CreatureTime
{
    public abstract class CtNpcExpert : UdonSharpBehaviour
    {
        public abstract int GetInsistence(CtNpcContext blackboard);
        public abstract void Execute(CtNpcContext blackboard);
        public abstract CtBehaviorTreeNodeBase[] GetActions();
    }
}