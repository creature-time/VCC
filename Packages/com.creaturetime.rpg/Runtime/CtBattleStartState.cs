
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleStartState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleWaitState waitState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return waitState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Start;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (battleState.ArePlayersLoaded())
                return ENodeStatus.Success;
            return ENodeStatus.Running;
        }
    }
}