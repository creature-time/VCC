
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EPlayerInventorySignal
    {
        InventoryChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerInventory : CtAbstractSignal
    {
        private const int StartingInventorySize = 16;

        [CtItem, SerializeField, UdonSynced] private string[] inventory = new string[StartingInventorySize]
        {
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData)
        };

        private string[] _cmpInventory = new string[StartingInventorySize]
        {
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData),
            CtDataBlock.Serialize(CtDataBlock.InvalidData), CtDataBlock.Serialize(CtDataBlock.InvalidData)
        };

        public int Count => inventory.Length;

        private bool _TryGetInvIndexOfEmpty(out int index)
        {
            index = -1;
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (!TryGetItem(i, out var data, out var count)) continue;
                if (data != CtDataBlock.InvalidData) continue;

                index = i;
                return true;
            }

#if DEBUG_LOGS
            LogCritical("Could not find an empty slot for inventory.");
#endif

            return false;
        }

        public bool TryGetItem(int index, out ulong data, out int count)
        {
            count = -1;
            data = CtDataBlock.InvalidData;
            if (index >= inventory.Length)
            {
#if DEBUG_LOGS
                LogCritical($"Attempting to get inventory data for slot (index={index}).");
#endif
                return false;
            }

            data = CtDataBlock.Deserialize(inventory[index]);
            return true;
        }

        public bool HasValidData(int index)
        {
            return CtDataBlock.IsValid(CtDataBlock.Deserialize(inventory[index]));
        }

        public bool TrySetItem(int index, ulong data, int count = -1)
        {
            if (index >= inventory.Length)
            {
#if DEBUG_LOGS
                LogCritical($"Attempting to set inventory data for invalid slot (index={index}).");
#endif
                return false;
            }

            inventory[index] = CtDataBlock.Serialize(data);
            RequestSerialization();
            _OnInventoryChanged(index);

            return true;
        }

        public bool TryGiveItem(ulong data, int count = -1)
        {
            if (!_TryGetInvIndexOfEmpty(out var index))
            {
#if DEBUG_LOGS
                LogCritical($"There are no empty slots for item (data={data:x16}).");
#endif
                return false;
            }

            return TrySetItem(index, data, count);
        }

        public bool TrySetOrGiveItem(int index, ulong data)
        {
            if (!HasValidData(index))
                return TrySetItem(index, data);
            return TryGiveItem(data);
        }

        public bool TryTakeItem(int index, out ulong data)
        {
            data = CtDataBlock.InvalidData;

            if (!TryGetItem(index, out var takeData, out var count)) return false;

            if (!CtDataBlock.IsValid(takeData))
            {
#if DEBUG_LOGS
                LogCritical($"Cannot take item at slot containing invalid data (index={index}).");
#endif
                return false;
            }

            inventory[index] = CtDataBlock.Serialize(CtDataBlock.InvalidData);
            RequestSerialization();
            _OnInventoryChanged(index);

            data = takeData;

            return true;
        }

        private void _OnInventoryChanged(int index)
        {
            _cmpInventory[index] = inventory[index];

#if DEBUG_LOGS
            LogDebug($"Player inventory updated (index={index}, data={inventory[index]}).");
#endif
            SetArgs.Add(index);
            this.Emit(EPlayerInventorySignal.InventoryChanged);
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