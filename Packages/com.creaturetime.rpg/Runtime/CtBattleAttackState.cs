
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

            entity.TryGetAttack(out var skillIndex, out var targetId);
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
            if (!battleState.TryGetEntity(entityIdentifier, out var entity))
                return ENodeStatus.Success;

            entity.TryGetAttack(out var skillIndex, out var targetId);
            if (!battleState.TryGetEntity(targetId, out var targetEntity))
                return ENodeStatus.Success;

            if (skillIndex == -1)
            {
                CtSkillDef.MeleeAttack(gameData, 0, targetEntity, entity);
            }
            else
            {
                var identifier = entity.EntityDef.GetSkill(skillIndex);
                var skillDef = gameData.GetSkillDef(identifier);
                skillDef.OnUse(gameData, 0, targetEntity, entity);
            }

            return ENodeStatus.Success;
        }
    }
}