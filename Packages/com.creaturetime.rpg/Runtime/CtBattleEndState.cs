
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleEndState : CtStateBase
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtQuestSystem questSystem;
        [SerializeField] private CtBattleState battleState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return null;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.End;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            var allyParty = battleState.AllyParty;
            if (!allyParty)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find ally party for battle state (battleState={battleState}).");
                return ENodeStatus.Failure;
#endif
            }

            var enemyParty = battleState.EnemyParty;
            if (!enemyParty)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find enemy party for battle state (battleState={battleState}).");
                return ENodeStatus.Failure;
#endif
            }

            if (battleState.IsEnemyTeamDead() && !battleState.IsAllyTeamDead())
            {
                allyParty.Map.SetCompleted();
            }

            rpgGame.EndBattle(battleState);

            return ENodeStatus.Success;
        }
    }
}