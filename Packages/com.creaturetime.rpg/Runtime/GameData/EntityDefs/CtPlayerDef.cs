
using System;
using CreatureTime.RpgGame.Ui;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerDef : CtEntityDef
    {
        private const int MaxInventoryCount = 16;

        [SerializeField] private CtPlayerPersistenceData playerPersistenceData;
        [SerializeField] private CtPlayerTurn playerTurn;
        [SerializeField] private CtItemSpawner itemSpawner;

        private int _playerId;

        public CtItemSpawner ItemSpawner => itemSpawner;

        public void Setup(Texture texture)
        {
            displayName = playerPersistenceData.DisplayName;
            icon = texture;
        }

        public void TearDown()
        {
            icon = null;
        }

        public void WeaponAttack(CtEntity target)
        {
            playerTurn.Submit(CTBattleInteractType.Attack, CtConstants.InvalidId, target.Identifier);
        }

        public void UseSkill(ushort skillId, CtEntity target)
        {
            playerTurn.Submit(CTBattleInteractType.Attack, skillId, target.Identifier);
        }

        public void Run()
        {
            playerTurn.Submit(CTBattleInteractType.Run, CtConstants.InvalidId, CtConstants.InvalidId);
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(_BarksCallback))]
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

        public CtPlayerTurn PlayerTurn => playerTurn;

        [SerializeField, UdonSynced] private ulong[] inventory = new ulong[MaxInventoryCount]
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

        public int InvSize => inventory.Length;

        public int InvCountOf(ushort identifier)
        {
            int count = 0;
            for (int i = 0; i < inventory.Length; ++i)
            {
                if ((inventory[i] & 0x0000FFFF) == identifier)
                    count++;
            }
        
            return count;
        }

        public int InvIndexOf(ushort identifier, int start = 0)
        {
            for (int i = start; i < inventory.Length; ++i)
            {
                if ((inventory[i] & 0x0000FFFF) == identifier)
                    return i;
            }

            return -1;
        }

        public int InvIndexOfEmpty()
        {
            return Array.IndexOf(inventory, CtDataBlock.InvalidData);
        }

        public void InvAddTo(int index, ulong data)
        {
            SetInventoryData(index, data);
        }

        public ulong InvDataAtSlot(int index)
        {
            return inventory[index];
        }

        public void InvRemoveFrom(int index)
        {
            SetInventoryData(index, CtDataBlock.InvalidData);
        }

        private void SetInventoryData(int index, ulong data)
        {
            inventory[index] = data;
            RequestSerialization();
            OnDeserialization();
        }

        private void _OnInventoryChanged(int index)
        {
            _cmpInventory[index] = inventory[index];
            this.Emit(EEntityStatsSignal.InventoryChanged);
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();

            for (int i = 0; i < inventory.Length; ++i)
            {
                if (_cmpInventory[i] != inventory[i])
                    _OnInventoryChanged(i);
            }
        }
    }
}