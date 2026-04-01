
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EBattleLootSignal
    {
        LootReset,
        LootUpdated,
        WinnersReset,
        WinnerUpdated
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleLoot : CtAbstractSignal
    {
        [SerializeField] private CtRpgGame rpgGame;

        [CtItem, SerializeField] private string testItem = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        [UdonSynced] private ulong[] _loot = { };
        private ulong[] _cmpLoot = { };

        [UdonSynced] private ERollType[] _highestRollType = { };
        [UdonSynced] private ushort[] _winners = { };
        private ushort[] _cmpWinners = { };

        public bool HasLoot => _loot.Length > 0;
        public int LootCount => _loot.Length;

        public void _Test_AddingItems()
        {
            var data = CtDataBlock.Deserialize(testItem);
            AddItem(data);
            AddItem(data);
            AddItem(data);
        }

        public void AddItem(ulong item)
        {
#if DEBUG_LOGS
            LogDebug($"Adding loot item (itemData={item:x16}).");
#endif

            CtArrayUtils.Add(ref _loot, item);
            CtArrayUtils.Add(ref _winners, CtConstants.InvalidId);
            CtArrayUtils.Add(ref _highestRollType, ERollType.None);
            RequestSerialization();
            OnDeserialization();
        }

        public bool TryGetItem(int index, out ulong data)
        {
            if (index >= _loot.Length)
            {
                data = CtDataBlock.InvalidData;
                return false;
            }

            data = _loot[index];
            return true;
        }

        public bool TryGetWinner(int index, out ushort winner)
        {
            if (index >= _winners.Length)
            {
                winner = CtConstants.InvalidId;
                return false;
            }

            winner = _winners[index];
            return true;
        }

        private void _GetRolls(CtParty party, int index, out ERollType highestRollType, out int[] playerIndexes, out int[] rolls)
        {
            highestRollType = ERollType.Pass;
            playerIndexes = new int[] { };
            rolls = new int[] { };

            for (var i = 0; i < party.MaxCount; i++)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (!entity.IsPlayer) continue;
                var playerEntity = (CtPlayerEntity)entity;
                var playerRoll = playerEntity.PlayerRoll;
                var rollType = playerRoll.GetRollType(index);

                if (Convert.ToInt32(highestRollType) < Convert.ToInt32(rollType))
                {
                    playerIndexes = new int[] { };
                    rolls = new int[] { };
                    highestRollType = rollType;
                }

                if (highestRollType != rollType) continue;

                CtArrayUtils.Add(ref playerIndexes, i);
                CtArrayUtils.Add(ref rolls, CtRandomizer.GetIntValue(0, 10) + 1);
            }
        }

        public bool HasRolled(int index) => _highestRollType[index] != ERollType.None;

        public void Roll(CtParty party, int index)
        {
            _GetRolls(party, index, out var highestRollType, out var players, out var rolls);
            var winnerIndex = CtRandomizer.GetRandomFromArrayInt(rolls);
            _highestRollType[index] = highestRollType;
            _winners[index] = party.GetEntity(players[winnerIndex]).Identifier;
            RequestSerialization();
            OnDeserialization();
        }

        public void Clear()
        {
            _loot = new ulong[] { };
            _highestRollType = new ERollType[] { };
            _winners = new ushort[] { };
        }

        public override void OnDeserialization()
        {
            if (_loot.Length != _cmpLoot.Length)
            {
                CtArrayUtils.Resize(ref _cmpLoot, _loot.Length, CtDataBlock.InvalidData);
                Array.Copy(_loot, _cmpLoot, _loot.Length);
                this.Emit(EBattleLootSignal.LootReset);
            }

            for (var i = 0; i < _loot.Length; i++)
            {
                if (_loot[i] != _cmpLoot[i])
                {
                    _cmpLoot[i] = _loot[i];
                    SetArgs.Add(i);
                    this.Emit(EBattleLootSignal.LootUpdated);
                }
            }

            if (_winners.Length != _cmpWinners.Length)
            {
                CtArrayUtils.Resize(ref _cmpWinners, _winners.Length);
                Array.Copy(_winners, _cmpWinners, _winners.Length);
                this.Emit(EBattleLootSignal.WinnersReset);
            }
            else
            {
                for (var i = 0; i < _winners.Length; i++)
                {
                    if (_winners[i] != _cmpWinners[i])
                    {
                        _cmpWinners[i] = _winners[i];
                        SetArgs.Add(i);
                        this.Emit(EBattleLootSignal.WinnerUpdated);
                    }
                }
            }

            var localEntity = rpgGame.LocalEntity;
            for (var i = 0; i < _winners.Length; i++)
            {
                if (_winners[i] == localEntity.Identifier)
                {
                    var playerRoll = localEntity.PlayerRoll;
                    if (!playerRoll.HasTakenLoot(i))
                    {
                        var loot = _loot[i];
                        localEntity.PlayerInventory.TryGiveItem(loot);
                        playerRoll.TakeLoot(i);
                    }
                }
            }
        }
    }
}