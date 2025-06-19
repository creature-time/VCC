
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

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return nextTurnState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Attack;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            battleState.TryGetEntity(entityIdentifier, out var entity);
            entity.TryGetAttack(out var skillIndex, out var targetId);
            battleState.TryGetEntity(targetId, out var targetEntity);

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

            LogDebug(
                $"Testing (turnIndex={battleState.TurnIndex}, skillIndex={skillIndex}, targetId={targetId}, targetHealth={targetEntity.Health})");

            return ENodeStatus.Success;
        }
    }
}