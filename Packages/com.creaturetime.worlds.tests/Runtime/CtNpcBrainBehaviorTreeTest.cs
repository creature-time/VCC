
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcBrainBehaviorTreeTest : CtNpcBrain
    {
        [SerializeField] private CtBlackboardEntryData entryData;
        [SerializeField] private CtBehaviorTree behaviorTree;
        [SerializeField] private CtNpcArbiter arbiter;

        // Translate game state to world state.
        public override void Sense()
        {
            foreach (var action in arbiter.BlackboardIteration(behaviorTree.Context))
            {
                // Blackboard actions to set up blackboard.
                // action.Execute(behaviorTree.Context);
            }
        }

        public override void Think()
        {
            behaviorTree.Process();
        }

        private void Update()
        {
            Sense();
            Think();
        }
    }
}