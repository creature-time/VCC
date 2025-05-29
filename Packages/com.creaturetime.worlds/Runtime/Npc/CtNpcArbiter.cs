
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtNpcArbiter : CtLoggerUdonScript
    {
        [SerializeField] private CtNpcExpert[] experts;

        public CtNpcExpert BestExpert { get; private set; }

        public bool ValidateExpert(CtNpcContext blackboard)
        {
            CtNpcExpert bestExpert = null;
            int heighestInsistence = Int32.MinValue;
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

            if (bestExpert != BestExpert)
            {
                BestExpert = bestExpert;
                return true;
            }

            return false;
        }
    }
}