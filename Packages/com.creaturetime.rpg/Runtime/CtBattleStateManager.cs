
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EBattleStateManagerSignal
    {
        LocalBattleStateChanged,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleStateManager : CtSingleton
    {
        [SerializeField] private CtBattleState[] battleStates;
        [SerializeField] private CtSquadDef[] squadDefs;

        public CtBattleState[] BattleStates => battleStates;

        private DataDictionary _squadDefLookup =  new DataDictionary();

        private void Start()
        {
            foreach(var squadDef in squadDefs)
                _squadDefLookup.Add(squadDef.Identifier, squadDef);
        }

        public bool TryGetSquadDef(ushort identifier, out CtSquadDef def)
        {
            def = null;
            if (_squadDefLookup.TryGetValue(identifier, out var token))
            {
                def = (CtSquadDef)token.Reference;
                return true;
            }

            return false;
        }

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

#if DEBUG_LOGS
                if (index != count)
                    LogCritical($"Index did not match count (index={index}, count={count}).");
#endif

                bs.Initiatives = temp;

                bs.ResetTurns();

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