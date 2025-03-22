
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtNpcBrainBehaviorTreeTest : CtNpcBrain
    {
        [SerializeField] private CtBlackboardEntryData entryData;
        [SerializeField] private CtBehaviorTree behaviorTree;
        [FormerlySerializedAs("artbiter")] [SerializeField] private CtNpcArbiter arbiter;

        // Translate game state to world state.
        public override void Sense()
        {
            foreach (var action in arbiter.BlackboardIteration(behaviorTree.Context))
            {
                // Blackboard actions to set up blackboard.
            }
        }

        public override void Think()
        {
            behaviorTree.Process();
        }
    }
}