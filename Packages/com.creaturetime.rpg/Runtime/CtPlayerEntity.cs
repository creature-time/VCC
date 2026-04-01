
using UdonSharp;
using UnityEngine;
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

        // public int InvCountOf(ushort identifier)
        // {
        //     int count = 0;
        //     for (int i = 0; i < PlayerDef.InventorySize; ++i)
        //     {
        //         if (!PlayerDef.TryGetInventoryData(i, out var data)) continue;
        //         if ((data & 0x0000FFFF) == identifier)
        //             count++;
        //     }
        //
        //     return count;
        // }
        //
        // public int InvIndexOf(ushort identifier, int start = 0)
        // {
        //     for (int i = start; i < PlayerDef.InventorySize; ++i)
        //     {
        //         if (!PlayerDef.TryGetInventoryData(i, out var data)) continue;
        //         if ((data & 0x0000FFFF) == identifier)
        //             return i;
        //     }
        //
        //     return -1;
        // }

//         public bool TryGetInvIndexOfEmpty(out int index)
//         {
//             index = -1;
//             for (int i = 0; i < PlayerInventory.Count; ++i)
//             {
//                 if (!PlayerInventory.TryGetInventoryData(i, out var data)) continue;
//                 if (data != CtDataBlock.InvalidData) continue;
//
//                 index = i;
//                 return true;
//             }
//
// #if DEBUG_LOGS
//             LogCritical("Could not find an empty slot for inventory.");
// #endif
//
//             return false;
//         }
//
//         public void InvAddTo(int index, ulong data)
//         {
//             PlayerInventory.TrySetInventoryData(index, data);
//         }
//
//         public ulong InvDataAtSlot(int index)
//         {
//             return PlayerInventory.TryGetInventoryData(index, out var data) ? data : CtDataBlock.InvalidData;
//         }
//
//         public void InvRemoveFrom(int index)
//         {
//             PlayerInventory.TrySetInventoryData(index, CtDataBlock.InvalidData);
//         }
    }
}