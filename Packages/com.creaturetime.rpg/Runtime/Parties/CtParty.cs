
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EPartySignal
    {
        Started,
        Disbanded,
        MemberAdded,
        MemberRemoved,
        QuestChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtParty : CtAbstractSignal
    {
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtPartyManager partyManager;

        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        [SerializeField, HideInInspector] private CtPartySlot[] slots;
        private DataList _entityCache = new DataList();

        [UdonSynced, FieldChangeCallback(nameof(QuestCallback))]
        private ushort _questId = 0;

        public ushort QuestCallback
        {
            get => _questId;
            set
            {
                _questId = value;
                this.Emit(EPartySignal.QuestChanged);
            }
        }

        public ushort Quest
        {
            get => QuestCallback;
            set
            {
                QuestCallback = value;
                RequestSerialization();
            }
        }

        private void Start()
        {
            foreach (var slot in slots)
            {
                slot.Connect(EPartySlotSignal.IdentifierChanged, this, nameof(_OnPartySlotIdentifierChanged));
            }
        }

        public void _OnPartySlotIdentifierChanged()
        {
            var slot = (CtPartySlot)Sender;
            int index = Array.IndexOf(slots, slot);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogWarning($"Slot should be found within party (slot={slot}).");
#endif
                return;
            }

            var slotIdentifier = GetArgs[0].UShort;

            // Get entity; return if failed.
            if (!entityManager.TryGetEntity(slotIdentifier, out var entity)) return;

            // No need to remove then add again if the entities are the same.
            if (slot.EntityCache == entity) return;

            if (slot.EntityCache)
            {
                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberRemoved);

                _entityCache.Remove(slot.EntityCache);

                if (_entityCache.Count == 0)
                {
                    this.Emit(EPartySignal.Disbanded);
                }
            }

            slot.EntityCache = entity;
            if (slot.EntityCache)
            {
                if (_entityCache.Count == 0)
                {
                    this.Emit(EPartySignal.Started);
                }

                _entityCache.Add(slot.EntityCache);

                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberAdded);
            }
        }

        public CtEntity GetEntity(int slotIndex)
        {
            return slots[slotIndex].EntityCache;
        }

        public int GetMemberIndex(CtEntity entity)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].EntityCache == entity)
                    return i;

#if DEBUG_LOGS
            LogWarning($"Failed to find index by entity (party={this}, entity={entity}).");
#endif

            return -1;
        }

        public ushort Identifier => identifier;
        public bool IsEmpty => _entityCache.Count == 0;
        public bool IsFull => _entityCache.Count == slots.Length;
        public int Count => _entityCache.Count;
        public int MaxCount => slots.Length;

        public void Join(CtEntity entity)
        {
#if DEBUG_LOGS
            LogDebug($"Joining party (party={this}, entity={entity}).");
#endif

            CtPartySlot freeSlot = null;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.EntityCache) continue;
                if (slot.HasDisconnectedAlias)
                {
                    if (!freeSlot)
                        freeSlot = slot;
                    continue;
                }

                freeSlot = slot;
                break;
            }

            if (!freeSlot)
            {
#if DEBUG_LOGS
                LogCritical("Cannot add anymore members to party.");
#endif
                return;
            }

            freeSlot.Identifier = entity.Identifier;
        }

        public bool HasMember(CtEntity entity)
        {
            return _entityCache.Contains(entity);
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Length; ++i)
                if (slots[i].EntityCache)
                    slots[i].EntityCache = null;
        }

        private bool _FindDisconnectedIndex(string alias, out int index)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.DisconnectedUuid == alias)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public bool WasConnectedToParty(CtEntity entity)
        {
            return _FindDisconnectedIndex(entity.DisplayName, out var index);
        }

        public void Reconnected(CtEntity entity)
        {
            if (!_FindDisconnectedIndex(entity.DisplayName, out var index))
            {
#if DEBUG_LOGS
                LogCritical($"Cannot find member to reconnect to party (uuid={entity.DisplayName}).");
#endif
                return;
            }

#if DEBUG_LOGS
            LogDebug($"Reconnecting to party (partyId={identifier}, uuid={entity.DisplayName}).");
#endif

            slots[index].Reconnected(entity.DisplayName);
            slots[index].EntityCache = entity;
        }

        public void Disconnected(CtEntity entity)
        {
#if DEBUG_LOGS
            LogCritical($"Cannot find member to disconnect from party (uuid={entity.DisplayName}).");
#endif
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.EntityCache == entity)
                {
#if DEBUG_LOGS
                    LogDebug($"Disconnecting from party (partyId={identifier}, uuid={entity.DisplayName}).");
#endif

                    slot.Disconnected(entity.DisplayName);
                    slots[i].EntityCache = null;
                    return;
                }
            }
        }

        public void Leave(CtEntity entity)
        {
#if DEBUG_LOGS
            LogDebug($"Leaving party (party={this}, entity={entity}).");
#endif

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.EntityCache != entity) continue;
                slots[i].Identifier = CtConstants.InvalidId;
                return;
            }

#if DEBUG_LOGS
            LogCritical("Cannot find member to remove from party.");
#endif
        }
    }
}