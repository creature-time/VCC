
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;
using Vector3 = UnityEngine.Vector3;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleLootState : CtStateBase
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtDropDatabase dropDatabase;

        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleResultsState resultsState;

        public override CtStateBase GetNext(CtBlackboard context)
        {
            return resultsState;
        }

        public override void OnEnter(CtBlackboard context)
        {
            battleState.State = EBattleState.Loot;

            // if (battleState.IsEnemyTeamDead())
            // {
            //     var allyParty = battleState.AllyParty;
            //     var enemyParty = battleState.EnemyParty;
            //     for (var i = 0; i < enemyParty.MaxCount; i++)
            //     {
            //         var entity = enemyParty.GetEntity(i);
            //         if (!entity) continue;
            //         var npcEntity = (CtNpcEntity)entity;
            //         if (npcEntity.TryGenerateLoot(out var items))
            //         {
            //             foreach (var item in items)
            //             {
            //                 if (!allyParty.TryGetNextLootPlayer(out var player)) continue;
            //                 battleState.Loot.AddDrop(item, entity.RootTransform.position + Vector3.up * 1f, allyParty.Identifier, player.Identifier);
            //             }
            //         }
            //     }
            //
            //     for (var i = 0; i < allyParty.MaxCount; i++)
            //     {
            //         var entity = allyParty.GetEntity(i);
            //         if (!entity) continue;
            //         if (!entity.IsPlayer) continue;
            //         var playerEntity = (CtPlayerEntity)entity;
            //         var playerRoll = playerEntity.PlayerRoll;
            //         playerRoll.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(playerRoll.Reset));
            //     }
            // }
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            if (!battleState.InProgress)
                return ENodeStatus.Failure;

            // var loot = battleState.Loot;
            // if (loot.HasLoot)
            //     return ENodeStatus.Running;
            return ENodeStatus.Success;
        }

        public override void OnExit(CtBlackboard context)
        {
            dropDatabase.Clear();
        }
    }
}