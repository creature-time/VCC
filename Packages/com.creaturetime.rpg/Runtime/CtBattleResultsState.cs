
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleResultsState : CtStateBase
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleEndState endState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return endState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            if (battleState.IsEnemyTeamDead())
            {
                battleState.State = EBattleState.Successful;
                var enemyParty = battleState.EnemyParty;
                for (var i = 0; i < enemyParty.MaxCount; i++)
                {
                    var entity = enemyParty.GetEntity(i);
                    if (!entity) continue;
                    var npcEntity = (CtNpcEntity)entity;
                    if (npcEntity.TryGenerateLoot(out var items))
                    {
                        foreach (var item in items)
                        {
                            battleState.Loot.AddItem(item);
                        }
                    }
                }

                var allyParty = battleState.AllyParty;
                for (var i = 0; i < allyParty.MaxCount; i++)
                {
                    var entity = allyParty.GetEntity(i);
                    if (!entity) continue;
                    if (!entity.IsPlayer) continue;
                    var playerEntity = (CtPlayerEntity)entity;
                    var playerRoll = playerEntity.PlayerRoll;
                    playerRoll.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(playerRoll.Reset));
                }
            }
            else
                battleState.State = EBattleState.Failure;
        }

        private bool _AllPlayersRolled(CtParty party, int index)
        {
            for (var i = 0; i < party.MaxCount; i++)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (!entity.IsPlayer) continue;
                var playerEntity = (CtPlayerEntity)entity;
                var playerRoll = playerEntity.PlayerRoll;
                var rollType =  playerRoll.GetRollType(index);
                if (rollType == ERollType.None)
                    return false;
            }

            return true;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            // TODO: Move this to only do on signals and if all players have an option selected.
            var party = battleState.AllyParty;
            var loot = battleState.Loot;
            for (var i = 0; i < loot.LootCount; i++)
            {
                if (_AllPlayersRolled(party, i))
                {
                    if (!loot.HasRolled(i))
                        loot.Roll(party, i);
                }
            }

            if (battleState.IsReadyToLeave())
                return ENodeStatus.Success;

            return ENodeStatus.Running;
        }

        public override void OnExit(CtBlackboard context)
        {
            battleState.Loot.Clear();
        }
    }
}