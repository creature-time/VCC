
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNextTurnState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleEndState endState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            if (battleState.IsAllyTeamDead() || battleState.IsEnemyTeamDead())
                return endState;
            return waitState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.NextTurn;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            battleState.TryGetEntity(entityIdentifier, out var entity);
            if (entity.IsPlayer)
                entity.EntityDef.GetComponent<CtPlayerTurn>().ResetToWait();
            battleState.NextTurn();

            return ENodeStatus.Success;
        }
    }
}