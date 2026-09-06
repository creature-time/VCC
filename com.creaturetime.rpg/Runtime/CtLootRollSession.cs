
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame
{
    public enum ELootRollSessionSignal
    {
        Updated
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtLootRollSession : CtAbstractSignal
    {
        private const int RollDuration = 10;

        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtDropDatabase dropDatabase;

        [SerializeField] private CtParty party;

        [UdonSynced, FieldChangeCallback(nameof(_Callback_ItemId))] private ushort _itemId = CtConstants.InvalidId;
        [UdonSynced] private ushort _partyId = CtConstants.InvalidId;
        [UdonSynced] private ushort[] _players = { };
        [UdonSynced] private ERollType[] _choices = { };
        [UdonSynced] private ERollType _highestRollType = ERollType.None;
        [UdonSynced, FieldChangeCallback(nameof(_Callback_EndTime))] private long _binaryEndTime;

        public ushort PartyId => _partyId;
        public int Count => _players.Length;
        public ERollType HighestRollType => _highestRollType;

        public ERollType PlayerChoice
        {
            get
            {
                var rollType = ERollType.None;
                if (rpgGame.LocalEntity)
                {
                    var index = Array.IndexOf(_players, rpgGame.LocalEntity.Identifier);
                    if (index != -1)
                        rollType = _choices[index];
                }

                return rollType;
            }
        }

        public DateTime EndTime { get; private set; }

        public long _Callback_EndTime
        {
            get => _binaryEndTime;
            set
            {
                _binaryEndTime = value;
                EndTime = DateTime.FromBinary(_binaryEndTime);
            }
        }

        public float TimeLeft => (float)(EndTime - DateTime.UtcNow).TotalSeconds;
        public float TimeLeftNormalized => Mathf.Clamp01(TimeLeft / RollDuration);

        public ushort _Callback_ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId != CtConstants.InvalidId)
                {
                    party.RemoveFromCache(this);
                }

                _itemId = value;
                if (_itemId != CtConstants.InvalidId)
                {
                    party.AddToCache(this);
                }
            }
        }

        public ushort ItemId
        {
            get => _Callback_ItemId;
            set
            {
                _Callback_ItemId = value;
                RequestSerialization();
            }
        }

        public void StartSession(ushort itemId, CtPlayerEntity[] players)
        {
#if DEBUG_LOGS
            LogDebug($"Starting roll session (itemId={itemId}).");
#endif

            _players = new ushort[players.Length];
            _choices = new ERollType[players.Length];
            // _rolls = new int[players.Length];
            for (var i = 0; i < players.Length; i++)
                _players[i] = players[i].Identifier;
            var endTime = DateTime.UtcNow + new TimeSpan(0, 0, RollDuration);
            _Callback_EndTime = endTime.ToBinary();
            ItemId = itemId;
            RequestSerialization();
        }

        public void StopSession()
        {
            _players = new ushort[] { };
            _choices = new ERollType[] { };
            _Callback_EndTime = 0;
            ItemId = CtConstants.InvalidId;
            RequestSerialization();

            party.AddNextQueuedItem();
        }

        public void SubmitChoice(CtPlayerEntity playerEntity, ERollType choice)
        {
#if DEBUG_LOGS
            LogDebug($"Submit choice for player (entityId={playerEntity.Identifier}, choice={choice}).");
#endif

            var index = Array.IndexOf(_players, playerEntity.Identifier);
            if (index == -1)
            {
                return;
            }

            // Notify passed if passed?

            _choices[index] = choice;
            RequestSerialization();

            if (_AllPlayersResponded())
                Resolve();
        }

        private bool _AllPlayersResponded()
        {
            foreach (var t in _choices)
                if (t == ERollType.None)
                    return false;

            return true;
        }

        public void Resolve()
        {
#if DEBUG_LOGS
            LogDebug($"Resolving session (session={this}).");
#endif

            _highestRollType = ERollType.Pass;

            var rollers = new DataDictionary();
            for (var i = 0; i < _choices.Length; i++)
            {
                var choice = _choices[i];
                if (choice == ERollType.Pass) continue;

                if (Convert.ToInt32(_highestRollType) < Convert.ToInt32(choice))
                {
                    _highestRollType = choice;
                    rollers.Clear();
                }

                if (_highestRollType != choice) continue;
                rollers.Add(_players[i], -1);
            }

            var winner = CtConstants.InvalidId;
            if (Convert.ToInt32(_highestRollType) > Convert.ToInt32(ERollType.Pass))
            {
                var values = new int[100];
                for (var i = 0; i < values.Length; i++)
                    values[i] = i + 1;

                var highestRoll = -1;
                var keys = rollers.GetKeys();
                for (var i = 0; i < keys.Count; i++)
                {
                    var rollerId = keys[i].UShort;
                    var rolledIndex = CtRandomizer.GetIntValue(0, values.Length - 1);
                    CtArrayUtils.Pop(ref values, rolledIndex);

                    // TODO: Add party to notify?
                    var rolledValue = values[rolledIndex];
                    dropDatabase.TryGetDrop(ItemId, out var itemData, out var entityId, out var position, out var partyId, out var owner);
                    rpgGame.NotifyRoll(itemData, rollerId, rolledValue);

                    if (rolledValue > highestRoll)
                    {
                        highestRoll = rolledValue;
                        winner = rollerId;
                    }
                }
            }

            dropDatabase.OnUpdateRolledOwner(_Callback_ItemId, winner);

            StopSession();
        }

        public void SetNeed()
        {
            rpgGame.RequestRollChoice(ItemId, ERollType.Need);
        }

        public void SetGreed()
        {
            rpgGame.RequestRollChoice(ItemId, ERollType.Greed);
        }

        public void SetPass()
        {
            rpgGame.RequestRollChoice(ItemId, ERollType.Pass);
        }
    }
}