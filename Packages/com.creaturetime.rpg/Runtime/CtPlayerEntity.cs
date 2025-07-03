
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerManager playerManager;

        private CtPlayerTurn _playerTurn;

        public override ushort EntityId => PlayerDef.PlayerId;

        public override Vector3 Position
        {
            get
            {
                var playerApi = VRCPlayerApi.GetPlayerById(PlayerDef.PlayerId);
                return playerApi.GetPosition();
            }
        }

        public override Quaternion Rotation
        {
            get
            {
                var playerApi = VRCPlayerApi.GetPlayerById(PlayerDef.PlayerId);
                return playerApi.GetRotation();
            }
        }

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
            _playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(_playerTurn.ResetToWait));
            base.OnStartBattle();
        }

        public override bool IsReady()
        {
            return _playerTurn.InteractType != CTBattleInteractType.None;
        }

        public override bool HasAttackReady()
        {
            return _playerTurn.InteractType == CTBattleInteractType.Attack;
        }

        public override bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            return _playerTurn.TryGetAttack(out skillId, out targetId);
        }

        public override void ResetAttack()
        {
            _playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(_playerTurn.ResetToWait));
        }

        public override void OnEndBattle()
        {
            _playerTurn.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(_playerTurn.Reset));
            base.OnEndBattle();
        }
    }
}