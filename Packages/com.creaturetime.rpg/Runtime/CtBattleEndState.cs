using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleEndState : CtStateBase
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtBattleState battleState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return null;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.End;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            rpgGame.EndBattle(battleState);

            return ENodeStatus.Success;
        }
    }
}