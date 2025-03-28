
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerDef : CtEntityDef
    {
        private const int MaxInventoryCount = 16;

        [SerializeField] private CtPlayerManager playerManager;

        [SerializeField]
        [UdonSynced]
        [FieldChangeCallback(nameof(_BarksCallback))]
        private ulong barks;

        public ulong _BarksCallback
        {
            get => barks;
            set
            {
                barks = value;
                this.Emit(EEntityStatsSignal.BarksChanged);
            }
        }

        public ulong Barks
        {
            get => _BarksCallback;
            private set
            {
                _BarksCallback = value;
                RequestSerialization();
            }
        }

        public bool IsLocal { get; set; } = false;
        public ushort PlayerId { get; set; } = CtConstants.InvalidId;

        [UdonSynced] public ulong[] _inventory = new ulong[MaxInventoryCount]
        {
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData
        };

        private ulong[] _cmpInventory = new ulong[MaxInventoryCount]
        {
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData
        };

        // public int InvCountOf(ushort identifier)
        // {
        //     int count = 0;
        //     for (int i = 0; i < _inventory.Length; ++i)
        //     {
        //         if ((_inventory[i] & 0x0000FFFF) == identifier)
        //             count++;
        //     }
        //
        //     return count;
        // }
        //
        // public int InvIndexOf(ushort identifier, int start = 0)
        // {
        //     for (int i = start; i < _inventory.Length; ++i)
        //     {
        //         if ((_inventory[i] & 0x0000FFFF) == identifier)
        //             return i;
        //     }
        //
        //     return -1;
        // }

        public int InvIndexOfEmpty()
        {
            return Array.IndexOf(_inventory, CtDataBlock.InvalidData);
        }

        public void InvAddTo(int index, ulong data)
        {
            SetInventoryData(index, data);
        }

        public ulong InvDataAtSlot(int index)
        {
            return _inventory[index];
        }

        public void InvRemoveFrom(int index)
        {
            SetInventoryData(index, CtDataBlock.InvalidData);
        }

        private void SetInventoryData(int index, ulong data)
        {
            _inventory[index] = data;
            RequestSerialization();
            OnDeserialization();
        }

        private void _OnInventoryChanged(int index)
        {
            _cmpInventory[index] = _inventory[index];
            this.Emit(EEntityStatsSignal.InventoryChanged);
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();

            for (int i = 0; i < _inventory.Length; ++i)
            {
                if (_cmpInventory[i] != _inventory[i])
                    _OnInventoryChanged(i);
            }
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            CtLogger.LogDebug("Player Stats",
                $"Player Restored (displayName={player.displayName}, playerId={player.playerId})");

            if (PlayerId != CtConstants.InvalidId)
                return;

            if (player.IsOwner(gameObject))
            {
                displayName = player.displayName;
                IsLocal = player.isLocal;
                PlayerId = (ushort)player.playerId;
                playerManager.Client_OnPlayerAdded(this);
            }

            if (!player.isLocal || !Networking.IsOwner(gameObject))
                return;

            playerManager.SetupPlayer(player, this);
        }

        private void OnDestroy()
        {
            playerManager.OnPlayerDestroyed(this);
        }
    }
}