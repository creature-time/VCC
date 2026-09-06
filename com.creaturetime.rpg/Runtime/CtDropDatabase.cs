
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EBattleLootSignal
    {
        DropAdded,
        DropRemoved,
        OwnerUpdated
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtDropDatabase : CtSingleton
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPartyManager partyManager;

        [UdonSynced] private ushort _genId;

        [UdonSynced] private ushort[] _itemIds = { };
        [UdonSynced] private ulong[] _drop = { };
        [UdonSynced] private ushort[] _entityIds = { };
        [UdonSynced] private Vector3[] _position = { };
        [UdonSynced] private ushort[] _partyId = { };
        [UdonSynced] private ushort[] _ownerId = { };
        [UdonSynced] private bool[] _rolled = { };

        private ushort[] _cmpItemIds = { };
        private DataDictionary _cmpOwner = new DataDictionary();
        private CtPlayerEntity[] _players;

        public bool HasLoot => _itemIds.Length > 0;

        [CtItem, SerializeField] private string testItem = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public void _Test_AddingItems()
        {
            var data = CtDataBlock.Deserialize(testItem);
            // for (var i = 0; i < 32 * 4; i++)
            for (var i = 0; i < 3; i++)
            {
                AddDrop(data, CtConstants.InvalidId, new Vector3(0, 1, i), CtConstants.InvalidId, rpgGame.LocalEntity.Identifier);
            }
        }

        public static Vector3 RandomSpawnLocation(float minDropDistance, float maxDropDistance)
        {
            var angle = (float)(2.0 * Mathf.PI * CtRandomizer.GetDoubleValue(1));
            var distance = (float)CtRandomizer.GetDoubleValue(minDropDistance, maxDropDistance);
            return new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
        }

        public void AddDrop(ulong item, ushort entityId, Vector3 position, ushort partyId, ushort owner)
        {
            if (owner == CtConstants.InvalidId && partyId == CtConstants.InvalidId)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to add item due to no entity owner or party owner (itemData={item:x16}).");
#endif
                return;
            }

#if DEBUG_LOGS
            LogDebug($"Adding loot item (itemData={item:x16}).");
#endif

            if (!CtDataBlock.IsValid(item))
            {
                LogCritical($"Item data was invalid (item={item:x16})");
                return;
            }

            var itemId = _genId++;
            if (_genId == CtConstants.InvalidId)
                _genId = 0;

            var rolled = false;
            if (partyId != CtConstants.InvalidId)
            {
                var itemRarity = _GetItemRarity(item);
                switch (itemRarity)
                {
                    case EItemRarity.None:
                    case EItemRarity.Common:
                        owner = CtConstants.InvalidId;
                        rolled = true;
                        break;
                }
            }

            CtArrayUtils.Add(ref _itemIds, itemId);
            CtArrayUtils.Add(ref _drop, item);
            CtArrayUtils.Add(ref _entityIds, entityId);
            CtArrayUtils.Add(ref _position, position);
            CtArrayUtils.Add(ref _partyId, partyId);
            CtArrayUtils.Add(ref _ownerId, owner);
            CtArrayUtils.Add(ref _rolled, rolled);

            RequestSerialization();
            OnDeserialization();
        }

        public void ClearDrops(ushort partyId)
        {
            for (var i = _itemIds.Length - 1; i >= 0; i--)
            {
                if (_partyId[i] == partyId)
                    _RemoveDrop(i);
            }

            OnDeserialization();
        }

        private void _RemoveDrop(int index)
        {
            var itemId = _itemIds[index];

            if (!partyManager.TryGetParty(_partyId[index], out var party))
            {
#if DEBUG_LOGS
                LogCritical($"Could not find party for taking drop item (itemId={itemId}).");
#endif
                return;
            }

            party.StopRollSessions(itemId);

            CtArrayUtils.Pop(ref _itemIds, index);
            CtArrayUtils.Pop(ref _drop, index);
            CtArrayUtils.Pop(ref _entityIds, index);
            CtArrayUtils.Pop(ref _position, index);
            CtArrayUtils.Pop(ref _partyId, index);
            CtArrayUtils.Pop(ref _ownerId, index);
            CtArrayUtils.Pop(ref _rolled, index);

            RequestSerialization();
        }

        private void _GiveDrop(ushort entityId, ushort itemId)
        {
#if DEBUG_LOGS
            LogDebug($"Give item (entityId={entityId}, itemId={itemId}).");
#endif

            var index = Array.IndexOf(_itemIds, itemId);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to take item by identifier (itemId={itemId}).");
#endif
                return;
            }

            rpgGame.RequestGiveItem(entityId, _drop[index]);

            _RemoveDrop(index);

            OnDeserialization();
        }

        public bool TryGetDrop(ushort itemId, out ulong data, out ushort entityId, out Vector3 position, out ushort partyId, out ushort owner)
        {
            var index = Array.IndexOf(_itemIds, itemId);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find item by identifier (itemId={itemId}).");
#endif
                data = CtDataBlock.InvalidData;
                entityId = CtConstants.InvalidId;
                position = Vector3.zero;
                partyId = CtConstants.InvalidId;
                owner = CtConstants.InvalidId;
                return false;
            }

            if (index >= _itemIds.Length)
            {
#if DEBUG_LOGS
                LogCritical($"Item index was out of bounds (index={index}, length={_itemIds.Length}).");
#endif
                data = CtDataBlock.InvalidData;
                entityId = CtConstants.InvalidId;
                position = Vector3.zero;
                partyId = CtConstants.InvalidId;
                owner = CtConstants.InvalidId;
                return false;
            }

            data = _drop[index];
            entityId = _entityIds[index];
            position = _position[index];
            partyId = _partyId[index];
            owner = _ownerId[index];
            return true;
        }

        public void Clear()
        {
            _itemIds = new ushort[] { };
            _drop = new ulong[] { };
            _position = new Vector3[] { };
            RequestSerialization();
            OnDeserialization();
        }

        public void TryTakeItem(ushort itemId, ushort entityId)
        {
//             if (_rollSessions.ContainsKey(itemId))
//             {
// #if DEBUG_LOGS
//                 LogWarning($"Item already being rolled (itemId={itemId}).");
// #endif
//                 return;
//             }

            var index = Array.IndexOf(_itemIds, itemId);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find item by identifier (itemId={itemId}).");
#endif
                return;
            }

            var ownerId = _ownerId[index];
            var partyId = _partyId[index];

            // Handle if drop is not in a party...
            if (partyId == CtConstants.InvalidId)
            {
                if (entityId != ownerId)
                {
#if DEBUG_LOGS
                    LogCritical($"Attempting to pick up drop that is not in party and not the owner (itemId={itemId}).");
#endif
                    return;
                }

                _GiveDrop(entityId, itemId);
                return;
            }

            // This should not happen, but we check anyways...
            if (!partyManager.TryGetParty(partyId, out var party))
            {
#if DEBUG_LOGS
                LogCritical($"Could not find party for taking drop item (itemId={itemId}).");
#endif
                return;
            }

            // If we are the owner or we only have 1 player party member, then we just give the item to the player.
            if (entityId == ownerId || party.Players.Length == 1)
            {
                _GiveDrop(entityId, itemId);
                return;
            }

            if (_rolled[index])
                _GiveDrop(entityId, itemId);
            else
                party.AddRollItemWithQueue(itemId);
        }

        private EItemRarity _GetItemRarity(ulong item)
        {
            var dataType = CtDataBlock.GetDataType(item);
            switch (dataType)
            {
                case EDataType.Weapon:
                    return CtDataBlock.GetWeaponRarity(item);
                case EDataType.Equipment:
                    var armorId = CtDataBlock.GetEquipmentIdentifier(item);
                    var armorDef = gameData.GetArmorDef(armorId);
                    return armorDef.Rarity;
                case EDataType.OffHand:
                    return CtDataBlock.GetOffHandRarity(item);
                case EDataType.Item:
                    return CtDataBlock.GetOffHandRarity(item);
                default:
                    LogCritical($"Unknown data type (dataType={dataType}).");
                    return EItemRarity.None;
            }
        }

//         public void AddRollItemWithQueue(ushort itemId, CtParty party)
//         {
//             if (_AddRollItem(itemId, party)) return;
//
//             CtArrayUtils.Add(ref _rollQueue, itemId);
//             RequestSerialization();
//         }
//
//         private bool _AddRollItem(ushort itemId, CtParty party)
//         {
// #if DEBUG_LOGS
//             LogDebug($"Adding roll item (itemId={itemId}, party={party.Identifier}).");
// #endif
//
//             foreach (var rollSession in rollSessions)
//             {
//                 if (_rollSessions.ContainsValue(rollSession)) continue;
//                 rollSession.StartSession(itemId, party.Players);
//                 return true;
//             }
//
//             return false;
//         }
//
//         public bool TryGetRollSession(ushort itemId, out CtLootRollSession rollSession)
//         {
//             if (!_rollSessions.TryGetValue(itemId, out var token))
//             {
//                 rollSession = null;
//                 return false;
//             }
//
//             rollSession = (CtLootRollSession)token.Reference;
//             return true;
//         }

        public void OnUpdateRolledOwner(ushort itemId, ushort ownerId)
        {
            var index = Array.IndexOf(_itemIds, itemId);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to update owner item by identifier (itemId={itemId}).");
#endif
                return;
            }

            _ownerId[index] = ownerId;
            _rolled[index] = true;

            RequestSerialization();
            OnDeserialization();

            if (ownerId != CtConstants.InvalidId)
                _GiveDrop(ownerId, itemId);
        }

         public override void OnDeserialization()
         {
             for (var i = 0; i < _itemIds.Length; i++)
             {
                 var itemId = _itemIds[i];
                 if (Array.IndexOf(_cmpItemIds, itemId) == -1)
                 {
                     _cmpOwner.Add(itemId, _ownerId[i]);
                     SetArgs.Add(itemId);
                     this.Emit(EBattleLootSignal.DropAdded);
                 }
             }

             foreach (var itemId in _cmpItemIds)
                 if (Array.IndexOf(_itemIds, itemId) == -1)
                 {
                     _cmpOwner.Remove(itemId);
                     SetArgs.Add(itemId);
                     this.Emit(EBattleLootSignal.DropRemoved);
                 }

             _cmpItemIds = new ushort[_itemIds.Length];
             Array.Copy(_itemIds, _cmpItemIds, _itemIds.Length);

             for (var i = 0; i < _itemIds.Length; i++)
             {
                 if (!_cmpOwner.TryGetValue(_itemIds[i], out var token)) continue;

                 var owner = token.UShort;
                 if (owner == _ownerId[i]) continue;

                 _cmpOwner[i] = _ownerId[i];

                 SetArgs.Add(_itemIds[i]);
                 this.Emit(EBattleLootSignal.OwnerUpdated);
             }
         }

//         public void AddToCache(CtLootRollSession rollSession)
//         {
//             var itemId = rollSession.ItemId;
//             _rollSessions.Add(itemId, rollSession);
//             SetArgs.Add(itemId);
//             this.Emit(EBattleLootSignal.RollStarted);
//         }
//
//         public void RemoveFromCache(CtLootRollSession rollSession)
//         {
//             var itemId = rollSession.ItemId;
//             SetArgs.Add(itemId);
//             this.Emit(EBattleLootSignal.RollFinished);
//             _rollSessions.Remove(itemId);
//         }
//
//         private void Update()
//         {
//             var tokens = _rollSessions.GetValues();
//             for (var i = 0; i < tokens.Count; i++)
//             {
//                 var rollSession = (CtLootRollSession)tokens[i].Reference;
// #if DEBUG_LOGS
//                 var timeLeft = rollSession.TimeLeft;
//                 LogDebug($"Updating time for roll session (itemId={rollSession.ItemId}, timeLeft={timeLeft}).");
// #endif
//
//                 if (rollSession.TimeLeft > 0) continue;
//                 rollSession.Resolve();
//
//                 // Queue up next in roll queue if we have any...
//                 if (_rollQueue.Length == 0) continue;
//
//                 var itemId = CtArrayUtils.Pop(ref _rollQueue, 0);
//                 RequestSerialization();
//
//                 var index = Array.IndexOf(_itemIds, itemId);
//
//                 if (!partyManager.TryGetParty(_partyId[index], out var party)) continue;
//
//                 if (!_AddRollItem(itemId, party))
//                 {
// #if DEBUG_LOGS
//                     LogCritical("Failed to add roll item to the queue after last roll was complete " +
//                                 $"(itemId={itemId}, party={party.Identifier}).");
// #endif
//                 }
//             }
//         }
    }
}