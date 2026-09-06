
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleResultsState : CtStateBase
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtDropDatabase dropDatabase;

        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleEndState endState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return endState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Results;

            if (battleState.IsEnemyTeamDead())
            {
                var allyParty = battleState.AllyParty;
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
                            if (!allyParty.TryGetNextLootPlayer(out var player)) continue;
                            var randomPoint = CtDropDatabase.RandomSpawnLocation(0, 1f);
                            dropDatabase.AddDrop(item, entity.Identifier, Vector3.up * 1f + randomPoint, allyParty.Identifier, player.Identifier);
                        }
                    }
                }

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
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            if (battleState.IsReadyToLeave())
                return ENodeStatus.Success;

            return ENodeStatus.Running;
        }

        public override void OnExit(CtBlackboard context)
        {
            var allyParty = battleState.AllyParty;
            dropDatabase.ClearDrops(allyParty.Identifier);
        }
    }
}