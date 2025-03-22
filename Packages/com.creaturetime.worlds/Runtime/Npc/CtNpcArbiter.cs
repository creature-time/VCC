using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtNpcArbiter : CtLoggerUdonScript
    {
        [SerializeField] private CtNpcExpert[] experts;

        public CtBehaviorTreeNodeBase[] BlackboardIteration(CtNpcContext blackboard)
        {
            CtNpcExpert bestExpert = null;
            int heighestInsistence = 0;
            foreach (var expert in experts)
            {
                int insistence = expert.GetInsistence(blackboard);
                if (insistence > heighestInsistence)
                {
                    heighestInsistence = insistence;
                    bestExpert = expert;
                }
            }

            if (bestExpert)
            {
                bestExpert.Execute(blackboard);
            }

            return blackboard.GetNodes();
        }
    }
}