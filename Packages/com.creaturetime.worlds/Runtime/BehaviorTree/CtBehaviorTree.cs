
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtBehaviorTree : CtLoggerUdonScript
    {
        [SerializeField] private CtNpcContext context;

        [SerializeField] private CtBehaviorTreeNodeBase[] _children = { };

        public CtNpcContext Context => context;

        public CtBehaviorTreeNodeBase[] Children
        {
            get => _children;
            set
            {
                foreach (var child in _children)
                    child.OnExit(context);
                _children = value;
                foreach (var child in _children)
                    child.OnEnter(context);
            }
        }

        public ENodeStatus Process()
        {
            foreach (var child in _children)
            {
                var status = child.Process(context);
                if (status != ENodeStatus.Success)
                    return status;
            }
            return ENodeStatus.Success;
        }
    }
}