
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
        private ushort _questId = CtConstants.InvalidId;

        public ushort QuestCallback
        {
            get => _questId;
            set
            {
                _questId = value;

                SetArgs.Add(_questId);
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

        // private void Start()
        // {
        //     foreach (var slot in slots)
        //     {
        //         slot.Connect(EPartySlotSignal.IdentifierChanged, this, nameof(_OnPartySlotIdentifierChanged));
        //     }
        // }

        public void HandlePartySlotChanged(CtPartySlot slot)
        {
            int index = Array.IndexOf(slots, slot);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogWarning($"Slot should be found within party (slot={slot}).");
#endif
                return;
            }

            CtEntity entity = null;
            if (slot.Identifier != CtConstants.InvalidId)
            {
                if (!entityManager.TryGetEntity(slot.Identifier, out entity))
                    return;
            }

            if (slot.EntityCache)
            {
                _entityCache.Remove(slot.EntityCache);

                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberRemoved);

                if (_entityCache.Count == 0)
                {
                    this.Emit(EPartySignal.Disbanded);
                }
            }

#if DEBUG_LOGS
            LogDebug("Updating entity for slot " +
                     $"(party={this}, slot={slot}, prev={slot.EntityCache}, entity={entity}).");
#endif
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
                // if (slot.HasDisconnectedAlias)
                // {
                //     if (!freeSlot)
                //         freeSlot = slot;
                //     continue;
                // }

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

//         private bool _FindDisconnectedIndex(string playerGuid, out int index)
//         {
//             for (int i = 0; i < slots.Length; i++)
//             {
//                 var slot = slots[i];
//                 if (slot.DisconnectedUuid == playerGuid)
//                 {
//                     index = i;
//                     return true;
//                 }
//             }
//
//             index = -1;
//             return false;
//         }
//
//         public bool WasConnectedToParty(CtPlayerEntity entity)
//         {
//             return _FindDisconnectedIndex(entity.PlayerGuid, out var index);
//         }
//
//         public void Reconnected(CtPlayerEntity entity)
//         {
// #if DEBUG_LOGS
//             LogDebug($"Reconnecting to party (partyId={identifier}, uuid={entity.PlayerGuid}).");
// #endif
//
//             if (!_FindDisconnectedIndex(entity.PlayerGuid, out var index))
//             {
// #if DEBUG_LOGS
//                 LogCritical($"Cannot find member to reconnect to party (uuid={entity.PlayerGuid}).");
// #endif
//                 return;
//             }
//
//             slots[index].Reconnected(entity.PlayerGuid);
//         }
//
//         public void Disconnected(CtPlayerEntity entity)
//         {
//             foreach (var slot in slots)
//             {
//                 if (slot.EntityCache == entity)
//                 {
// #if DEBUG_LOGS
//                     LogDebug($"Disconnecting from party (partyId={identifier}, uuid={entity.PlayerGuid}).");
// #endif
//
//                     slot.Disconnected(entity.PlayerGuid);
//                     return;
//                 }
//             }
//
// #if DEBUG_LOGS
//             LogCritical($"Cannot find member to disconnect from party (uuid={entity.PlayerGuid}).");
// #endif
//         }

        public void Leave(CtEntity entity)
        {
#if DEBUG_LOGS
            LogDebug($"Leaving party (party={this}, entity={entity}).");
#endif

            foreach (var slot in slots)
            {
                if (slot.EntityCache != entity) continue;
                slot.Identifier = CtConstants.InvalidId;
                return;
            }

#if DEBUG_LOGS
            LogCritical("Cannot find member to remove from party.");
#endif
        }
    }
}