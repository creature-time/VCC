
using System;
using CreatureTime.RpgGame;
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
        BattleChanged,
        RollStarted,
        RollFinished
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtParty : CtAbstractSignal
    {
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtPartyManager partyManager;

        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        [SerializeField, HideInInspector] private CtPartySlot[] slots;
        private DataList _entityCache = new DataList();
        private CtPlayerEntity[] _players = { };

        [SerializeField] private CtMap map;

        [SerializeField] private CtLootRollSession[] rollSessions;
        [UdonSynced] private ushort[] _rollQueue = { };
        private DataDictionary _rollSessions = new DataDictionary();

        public CtPlayerEntity[] Players => _players;

        public CtMap Map => map;

        public bool HasValidMap => map.Nodes.Length > 0;

        [UdonSynced] private int _lootIndex = -1;

        public bool TryGetNextLootPlayer(out CtPlayerEntity playerEntity)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                _lootIndex++;
                if (_lootIndex == slots.Length)
                    _lootIndex = 0;
                RequestSerialization();

                var entity = slots[_lootIndex].EntityCache;
                if (!entity) continue;
                if (!entity.IsPlayer) continue;

                playerEntity = (CtPlayerEntity)entity;
                return true;
            }

            playerEntity = null;
            return false;
        }

        public void GenerateMap(CtLocationDef locationDef)
        {
            map.GenerateMap(locationDef, 7, 7, locationDef.PathCount, locationDef.MaxNodeCount);
        }

        [UdonSynced, FieldChangeCallback(nameof(BattleCallback))]
        private ushort _battleId = CtConstants.InvalidId;

        public ushort BattleCallback
        {
            get => _battleId;
            set
            {
                _battleId = value;

                SetArgs.Add(_battleId);
                this.Emit(EPartySignal.BattleChanged);
            }
        }

        public ushort Battle
        {
            get => BattleCallback;
            set
            {
                BattleCallback = value;
                RequestSerialization();
            }
        }

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
                var prevEntity = slot.EntityCache;
                if (prevEntity.IsPlayer)
                    CtArrayUtils.Remove(ref _players, (CtPlayerEntity)prevEntity);
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
            if (entity)
            {
                if (_entityCache.Count == 0)
                {
                    this.Emit(EPartySignal.Started);
                }

                if (entity.IsPlayer)
                    CtArrayUtils.Add(ref _players, (CtPlayerEntity)entity);
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

            if (Count == 0)
            {
                _lootIndex = -1;
                RequestSerialization();
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

        public void AddRollItemWithQueue(ushort itemId)
        {
            if (_AddRollItem(itemId)) return;
            if (Array.IndexOf(_rollQueue, itemId) != -1) return;

#if DEBUG_LOGS
            LogWarning($"Adding item to roll queue (itemId={itemId}).");
#endif

            CtArrayUtils.Add(ref _rollQueue, itemId);
            RequestSerialization();
        }

        private bool _AddRollItem(ushort itemId)
        {
#if DEBUG_LOGS
            LogDebug($"Adding roll item (itemId={itemId}, party={Identifier}).");
#endif

            foreach (var rollSession in rollSessions)
            {
                if (_rollSessions.ContainsValue(rollSession)) continue;
                rollSession.StartSession(itemId, Players);
                return true;
            }

            return false;
        }

        public bool TryGetRollSession(ushort itemId, out CtLootRollSession rollSession)
        {
            if (!_rollSessions.TryGetValue(itemId, out var token))
            {
                rollSession = null;
                return false;
            }

            rollSession = (CtLootRollSession)token.Reference;
            return true;
        }

        public void AddToCache(CtLootRollSession rollSession)
        {
#if DEBUG_LOGS
            LogDebug($"Adding to session cache (itemId={rollSession.ItemId}).");
#endif

            var itemId = rollSession.ItemId;
            _rollSessions.Add(itemId, rollSession);
            SetArgs.Add(itemId);
            this.Emit(EPartySignal.RollStarted);
        }

        public void RemoveFromCache(CtLootRollSession rollSession)
        {
#if DEBUG_LOGS
            LogDebug($"Removing from session cache (itemId={rollSession.ItemId}).");
#endif

            var itemId = rollSession.ItemId;
            SetArgs.Add(itemId);
            this.Emit(EPartySignal.RollFinished);
            _rollSessions.Remove(itemId);
        }

        public void AddNextQueuedItem()
        {
            // Queue up next in roll queue if we have any...
            if (_rollQueue.Length == 0) return;

            var nextItemId = CtArrayUtils.Pop(ref _rollQueue, 0);
            RequestSerialization();

#if DEBUG_LOGS
            LogDebug($"Popping item from roll queue (itemId={nextItemId}).");
#endif

            if (!_AddRollItem(nextItemId))
            {
#if DEBUG_LOGS
                LogCritical("Failed to add roll item to the queue after last roll was complete " +
                            $"(nextItemId={nextItemId}, party={Identifier}).");
#endif
            }
        }

        public void StopRollSessions(ushort itemId)
        {
            var index = Array.IndexOf(_rollQueue, itemId);
            if (index != -1)
                CtArrayUtils.Pop(ref _rollQueue, index);

            foreach (var rollSession in rollSessions)
            {
                if (rollSession.ItemId == itemId)
                    rollSession.StopSession();
            }
        }

        private void Update()
        {
            var tokens = _rollSessions.GetValues();
            for (var i = 0; i < tokens.Count; i++)
            {
                var rollSession = (CtLootRollSession)tokens[i].Reference;
#if DEBUG_LOGS
                var timeLeft = rollSession.TimeLeft;
                LogDebug($"Updating time for roll session (itemId={rollSession.ItemId}, timeLeft={timeLeft}).");
#endif

                if (rollSession.TimeLeft > 0) continue;
                rollSession.Resolve();
            }
        }
    }
}