
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerManager playerManager;

        private CtPlayerTurn _playerTurn;

        public override ushort EntityId => PlayerDef.PlayerId;
        public override bool IsPlayer => true;

        public CtPlayerDef PlayerDef
        {
            private get => (CtPlayerDef)EntityDef;
            set
            {
                EntityDef = value;
                _playerTurn = value ? value.PlayerTurn : null;
            }
        }

        public override void OnStartBattle()
        {
            _playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, "ResetToWait");
            base.OnStartBattle();
        }

        public override bool IsReady()
        {
            return _playerTurn.InteractType != CTBattleInteractType.None;
        }

        public override bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            return _playerTurn.TryGetAttack(out skillId, out targetId);
        }

        public override void OnEndBattle()
        {
            _playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, "Reset");
            base.OnEndBattle();
        }
    }
}