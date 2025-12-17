
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerWorldPersistenceData playerWorldPersistenceData;

        private CtPlayerTurn _playerTurn;

        public override ushort EntityId => CtConstants.InvalidId;
        public override Vector3 Position => playerWorldPersistenceData.PlayerPersistenceData.RootTransform.position;
        public override Quaternion Rotation => playerWorldPersistenceData.PlayerPersistenceData.RootTransform.rotation;
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