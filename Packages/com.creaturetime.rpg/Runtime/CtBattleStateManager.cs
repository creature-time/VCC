
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleStateManager : CtAbstractSignal
    {
        [SerializeField] private CtBattleState[] battleStates;

        public bool TryCreateBattleState(CtParty allyParty, CtParty enemyParty, out CtBattleState battleState)
        {
            battleState = null;
            foreach (var bs in battleStates)
            {
                if (bs.InProgress)
                    continue;
                bs.InProgress = true;

                bs.AllyId = allyParty.Identifier;
                bs.EnemyId = enemyParty.Identifier;

                int index = 0;
                int count = allyParty.Count + enemyParty.Count;
                ushort[] temp = new ushort[count];

                for (int i = 0; i < allyParty.MaxCount; i++)
                {
                    var identifier = allyParty.GetMemberId(i);
                    if (identifier == CtConstants.InvalidId)
                        continue;
                    temp[index++] = identifier;
                }

                for (int i = 0; i < enemyParty.MaxCount; i++)
                {
                    var identifier = enemyParty.GetMemberId(i);
                    if (identifier == CtConstants.InvalidId)
                        continue;
                    temp[index++] = identifier;
                }

                if (index != count)
                    LogCritical($"Index did not match count (index={index}, count={count}).");

                bs.Initiatives = temp;

                bs.ResetTurns();

                battleState = bs;
                return true;
            }
            return false;
        }

        public void ReleaseBattleState(CtBattleState battleState)
        {
            battleState.AllyId = CtConstants.InvalidId;
            battleState.EnemyId = CtConstants.InvalidId;
            battleState.Initiatives = new ushort[] {};
            battleState.InProgress = false;
        }
    }
}