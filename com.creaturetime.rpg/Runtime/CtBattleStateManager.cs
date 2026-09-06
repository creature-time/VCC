
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleStateManager : CtSingleton
    {
        [SerializeField] private CtBattleState[] battleStates;

        public CtBattleState[] BattleStates => battleStates;

        public bool TryCreateBattleState(CtSquadDef squadDef, CtParty allyParty, CtParty enemyParty, out CtBattleState battleState)
        {
            battleState = null;
            foreach (var bs in battleStates)
            {
                if (bs.InProgress)
                    continue;
                bs.InProgress = true;
                bs.SquadId = squadDef.Identifier;
                bs.AllyId = allyParty.Identifier;
                bs.EnemyId = enemyParty.Identifier;

                int index = 0;
                int count = allyParty.Count + enemyParty.Count;
                ushort[] temp = new ushort[count];

                for (int i = 0; i < allyParty.MaxCount; i++)
                {
                    var entity = allyParty.GetEntity(i);
                    if (!entity) continue;
#if DEBUG_LOGS
                    LogDebug($"Adding ally entity to initiative (index={index}, entity={entity}).");
#endif
                    temp[index++] = entity.Identifier;
                }

                for (int i = 0; i < enemyParty.MaxCount; i++)
                {
                    var entity = enemyParty.GetEntity(i);
                    if (!entity) continue;
#if DEBUG_LOGS
                    LogDebug($"Adding enemy entity to initiative (index={index}, entity={entity}).");
#endif
                    temp[index++] = entity.Identifier;
                }

                bs.Initiatives = temp;

                bs.ResetTurns();

                battleState = bs;
                return true;
            }

            return false;
        }

        public bool TryGetBattleState(CtParty party, out CtBattleState battleState)
        {
            battleState = null;
            foreach (var bs in battleStates)
            {
                if (bs.AllyParty != party) continue;

                battleState = bs;
                return true;
            }
    
            return false;
        }

        public void ReleaseBattleState(CtBattleState battleState)
        {
            battleState.Initiatives = new ushort[] {};
            battleState.State = EBattleState.Start;
            battleState.AllyId = CtConstants.InvalidId;
            battleState.EnemyId = CtConstants.InvalidId;
            battleState.SquadId = CtConstants.InvalidId;
            battleState.InProgress = false;
        }
    }
}