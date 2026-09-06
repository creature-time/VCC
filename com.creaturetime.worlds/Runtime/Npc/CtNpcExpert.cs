using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcExpert : UdonSharpBehaviour
    {
        [SerializeField] private CtBehaviorTreeNodeBase[] nodes;

        public virtual int GetInsistence(CtNpcContext blackboard)
        {
            return 0;
        }

        public virtual void Execute(CtNpcContext blackboard)
        {
            // Do any setup here with the blackboard.
        }

        public virtual CtBehaviorTreeNodeBase[] GetActions()
        {
            return nodes;
        }
    }
}