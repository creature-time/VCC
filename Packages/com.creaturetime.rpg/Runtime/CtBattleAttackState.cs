
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

            entity.TryGetAttack(out var skillId, out var targetId);
            if (!battleState.TryGetEntity(targetId, out var targetEntity))
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
                return ENodeStatus.Failure;

            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(entityIdentifier, out var sourceEntity))
                return ENodeStatus.Success;

            sourceEntity.TryGetAttack(out var skillId, out var targetId);
            if (!battleState.TryGetEntity(targetId, out var targetEntity))
                return ENodeStatus.Success;

            battleState.BeginDamageBlock(battleState, sourceEntity, targetEntity, skillId);

            if (skillId == CtConstants.InvalidId)
            {
                CtSkillDef.MeleeAttack(gameData, targetEntity, sourceEntity);
            }
            else
            {
                var skillDef = gameData.GetSkillDef(skillId);
                skillDef.OnUse(gameData, targetEntity, sourceEntity);
            }

            battleState.EndDamageBlock();

            return ENodeStatus.Success;
        }
    }
}