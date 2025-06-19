using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleEndState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtRpgGame rpgGame;

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
            rpgGame.EndBattle(battleState);

            return ENodeStatus.Success;
        }
    }
}