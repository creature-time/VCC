
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleAttackState : CtStateBase
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleNextTurnState nextTurnState;
        [SerializeField] private CtBattleEndState endState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(entityIdentifier, out var entity))
                return endState;

            if (!entity.HasAttackReady())
                return endState;

            return nextTurnState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Attack;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
            {
#if DEBUG_LOGS
                LogError("Battle state was not longer in progress.");
#endif
                return ENodeStatus.Failure;
            }

            var identifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(identifier, out var entity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find entity (identifier={identifier}).");
#endif
                return ENodeStatus.Success;
            }

            if (!entity.HasAttackReady())
            {
#if DEBUG_LOGS
                LogCritical("Attack should be ready at this point.");
#endif
                return ENodeStatus.Success;
            }

            if (!entity.TryGetAttack(out var skillId, out var targetId))
            {
#if DEBUG_LOGS
                LogError("Should be able to get an attack at this point.");
#endif
                return ENodeStatus.Failure;
            }

            if (!battleState.TryGetEntity(targetId, out var targetEntity))
            {
#if DEBUG_LOGS
                LogError($"Failed to get target entity (targetId={targetId}).");
#endif
                return ENodeStatus.Success;
            }

            battleState.BeginDamageBlock(battleState, entity, targetEntity, skillId);

            if (skillId == CtConstants.InvalidId)
            {
                CtSkillDef.MeleeAttack(gameData, targetEntity, entity);
            }
            else
            {
                var skillDef = gameData.GetSkillDef(skillId);
                skillDef.OnUse(gameData, targetEntity, entity);
            }

            battleState.EndDamageBlock();

            return ENodeStatus.Success;
        }
    }
}