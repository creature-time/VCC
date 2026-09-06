
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerWorldPersistenceData playerWorldPersistenceData;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        private CtPlayerTurn _playerTurn;
        private string _playerGuid;

        public override ushort EntityId => CtConstants.InvalidId;
        public override bool IsPlayer => true;

        public override Transform RootTransform => rootTransform;
        public override Transform HeadTransform => headTransform;
        public override Transform LeftHandTransform => leftHandTransform;
        public override Transform RightHandTransform => rightHandTransform;

        public CtPlayerProgressionDatabase PrimaryQuestProgression => PlayerDef.PrimaryQuestProgression;
        public CtPlayerProgressionDatabase SecondaryQuestProgression => PlayerDef.SecondaryQuestProgression;
        public CtPlayerInventory PlayerInventory => PlayerDef.PlayerInventory;
        public CtPlayerWallet PlayerWallet => PlayerDef.PlayerWallet;
        public CtPlayerRoll PlayerRoll => PlayerDef.PlayerRoll;
        public CtPlayerTurn PlayerTurn => PlayerDef.PlayerTurn;

        [UdonSynced, FieldChangeCallback(nameof(_Callback_LocationId))] public ushort _locationId = CtConstants.InvalidId;

        public ushort _Callback_LocationId
        {
            get => _locationId;
            set
            {
                _locationId = value;
                if (PlayerDef && Networking.IsOwner(PlayerDef.gameObject))
                    PlayerDef.LocationId = _locationId;
            }
        }

        public ushort LocationId
        {
            get => _Callback_LocationId;
            set
            {
                _Callback_LocationId = value;
                RequestSerialization();
            }
        }

        public CtPlayerDef PlayerDef
        {
            get => (CtPlayerDef)EntityDef;
            set
            {
                EntityDef = value;
                _playerTurn = value ? value.PlayerTurn : null;
                if (_playerGuid != playerWorldPersistenceData.PlayerGuid)
                {
                    Reset();
                    _playerGuid = playerWorldPersistenceData.PlayerGuid;
                }
                LocationId = PlayerDef.LocationId;
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

        public override bool IsReadyToLeave()
        {
            return _playerTurn.InteractType == CTBattleInteractType.Leave;
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

        public int InvSize => PlayerInventory.Count;

        private CtBattleState _battleState;

        public override CtBattleState BattleState
        {
            get => _battleState;
            set => _battleState = value;
        }
    }
}