
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
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            var identifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(identifier, out var entity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find entity (identifier={identifier}).");
#endif
                return ENodeStatus.Success;
            }

            if (entity.HasAttackReady())
                return ENodeStatus.Running;

            battleState.NextTurn();
            return ENodeStatus.Success;
        }
    }
}