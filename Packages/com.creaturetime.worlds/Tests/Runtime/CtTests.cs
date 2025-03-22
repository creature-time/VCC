
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtTests : UdonSharpBehaviour
    {
        [Header("State Machine Tests")]
        [SerializeField] private CtStateMachine stateMachine;
        [SerializeField] private CtStateBase stateMachineTest0;

        public void _RunStateMachineTest0()
        {
            var identifier = stateMachine.Process(stateMachineTest0);
            Debug.Log($"Running test state machine 0 {identifier}");
        }

        [Header("Sequence Manager Tests")]
        [SerializeField] private CtSequenceManager sequenceManager;
        [SerializeField] private CtSequenceNodeBase sequenceManagerTest0;

        public void _RunSequenceManagerTest0()
        {
            var identifier = sequenceManager.Process(sequenceManagerTest0);
            Debug.Log($"Running test sequence manager 0 {identifier}");
        }

        [Header("Behavior Tree Tests")]
        [SerializeField] private CtBehaviorTree behaviorTree;
        [SerializeField] private CtBehaviorTreeNodeBase[] behaviorTreeTest0;

        public void _RunBehaviorTreeTest0()
        {
        }

        private void Update()
        {
            behaviorTree.Process();
        }
    }
}