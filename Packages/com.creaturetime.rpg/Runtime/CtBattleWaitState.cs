
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleWaitState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleAttackState attackState;
        [SerializeField] private CtBattleEndState endState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(entityIdentifier, out var entity))
                return endState;

            if (entity.TryGetAttack(out var skillId, out var targetId))
                return attackState;

            return endState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Wait;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(entityIdentifier, out var entity))
                return ENodeStatus.Success;

            if (entity.TryGetAttack(out var skillId, out var targetId))
                return ENodeStatus.Success;

            return ENodeStatus.Running;
        }
    }
}