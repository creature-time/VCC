
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNextTurnState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleLootState lootState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            if (battleState.IsAllyTeamDead() || battleState.IsEnemyTeamDead())
                return lootState;
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

            if (battleState.TimeLeft > 0)
                return ENodeStatus.Running;

            var identifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(identifier, out var entity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find entity (identifier={identifier}).");
#endif
                return ENodeStatus.Failure;
            }

            if (entity.HasAttackReady())
                return ENodeStatus.Running;

            battleState.BeginTickBlock();
            var isDead = entity.ProcessStatusTick();
            battleState.EndTickBlock();

            if (isDead && battleState.IsAllyTeamDead() || battleState.IsEnemyTeamDead())
                return ENodeStatus.Success;

            battleState.NextTurn();
            identifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(identifier, out entity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find next turn's entity (identifier={identifier}).");
#endif
                return ENodeStatus.Failure;
            }

            var initiatives = battleState.Initiatives;
            for (var i = 0; i < initiatives.Length; i++)
            {
                if (!battleState.TryGetEntity(initiatives[i], out var otherEntity))
                    return ENodeStatus.Failure;
                otherEntity.RemoveExpiredStatusEffects(entity);
            }

            entity.UpdateStatsAndSkills();

            return ENodeStatus.Success;
        }
    }
}