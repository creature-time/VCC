
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerManager playerManager;
        private CtPlayerTurn playerTurn;

        protected override void _OnEntityIdChanged()
        {
            IsPlayer = true;

            var playerId = CtEntityManager.GetIdentifier(EntityId);
            EntityDef = playerManager.GetPlayerDefById(playerId);
            playerTurn = EntityDef.GetComponent<CtPlayerTurn>();
            SourceTransform = EntityDef.transform;
        }

        public override void OnStartBattle()
        {
            playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, "ResetToWait");
            base.OnStartBattle();
        }

        public override bool IsReady()
        {
            return playerTurn.InteractType != CTBattleInteractType.None;
        }

        public override bool TryGetAttack(out int skillIndex, out ushort targetId)
        {
            return playerTurn.TryGetAttack(out skillIndex, out targetId);
        }

        public override void OnEndBattle()
        {
            playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, "Reset");
            base.OnEndBattle();
        }
    }
}