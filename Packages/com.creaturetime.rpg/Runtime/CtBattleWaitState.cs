
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
            var identifier = battleState.Initiatives[battleState.TurnIndex];
            if (!battleState.TryGetEntity(identifier, out var entity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find entity (identifier={identifier}).");
#endif
                return endState;
            }

            if (!entity.HasAttackReady())
            {
#if DEBUG_LOGS
                LogCritical($"Entity should have attack ready.");
#endif
                return endState;
            }

            return attackState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Wait;
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

            if (entity.TryGetAttack(out var skillId, out var targetId))
                return ENodeStatus.Success;

            return ENodeStatus.Running;
        }
    }
}