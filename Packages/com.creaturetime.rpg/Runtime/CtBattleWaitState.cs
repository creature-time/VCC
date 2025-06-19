
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleWaitState : CtStateBase
    {
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleAttackState attackState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return attackState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Wait;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
            battleState.TryGetEntity(entityIdentifier, out var entity);
            if (entity.IsPlayer)
                entity.EntityDef.GetComponent<CtPlayerTurn>().Submit(CTBattleInteractType.Attack, -1, battleState.EnemyParty.GetMemberId(0));
            return 
                entity.TryGetAttack(out var skillIndex, out var targetId) ? ENodeStatus.Success : ENodeStatus.Running;
        }
    }
}