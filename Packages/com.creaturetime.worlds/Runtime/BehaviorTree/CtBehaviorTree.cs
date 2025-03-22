
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtBehaviorTree : CtLoggerUdonScript
    {
        [SerializeField] private CtNpcContext context;

        [SerializeField] private CtBehaviorTreeNodeBase[] children = { };

        public CtNpcContext Context => context;

        public ENodeStatus Process()
        {
            foreach (var child in children)
            {
                var status = child.Process(context);
                if (status != ENodeStatus.Success)
                    return status;
            }
            return ENodeStatus.Success;
        }
    }
}