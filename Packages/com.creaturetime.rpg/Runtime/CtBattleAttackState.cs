
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleAttackState : CtStateBase
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;
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

            if (!battleState.TryGetEntity(targetId, out var target))
            {
#if DEBUG_LOGS
                LogError($"Failed to get target entity (targetId={targetId}).");
#endif
                return ENodeStatus.Success;
            }

            battleState.BeginDamageBlock(entity, target, skillId);

            if (skillId == CtConstants.InvalidId)
            {
                entity.UseWeapon(target);
            }
            else
            {
                if (!partyManager.TryGetEntityParty(target, out var party))
                {
#if DEBUG_LOGS
                    LogCritical($"Failed to get party for target entity (targetId={targetId}).");
#endif
                    return ENodeStatus.Failure;
                }

                // TODO: Make this reusable code...
                var adjacentTargets = new DataList();
                for (int i = 0; i < party.MaxCount; ++i)
                {
                    var otherEntity = party.GetEntity(i);
                    if (!otherEntity) continue;
                    adjacentTargets.Add(otherEntity);
                }

                entity.UseSkill(skillId, target, adjacentTargets);
            }

            battleState.EndDamageBlock();

            return ENodeStatus.Success;
        }
    }
}