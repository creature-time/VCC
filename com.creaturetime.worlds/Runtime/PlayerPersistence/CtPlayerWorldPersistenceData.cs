
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EPlayerWorldPersistenceSignal
    {
        PlayerPersistenceDataChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerWorldPersistenceData : CtAbstractSignal
    {
        [SerializeField] private CtPlayerPersistenceManager playerPersistenceManager;

        private CtPlayerPersistenceData _playerPersistenceData;

        public CtPlayerPersistenceData PlayerPersistenceData
        {
            get => _playerPersistenceData;
            set
            {
#if DEBUG_LOGS
                if (_playerPersistenceData == value)
                    LogWarning($"Persistence data was already set (data={_playerPersistenceData}).");
#endif

                if (_playerPersistenceData)
                {
                    playerPersistenceManager.OnPlayerRemoved(this);
                }

#if DEBUG_LOGS
                LogDebug($"Persistence data was updated (prev={_playerPersistenceData}, data={value}).");
#endif
                _playerPersistenceData = value;
                if (_playerPersistenceData)
                {
                    playerPersistenceManager.OnPlayerAdded(this);
                }

                this.Emit(EPlayerWorldPersistenceSignal.PlayerPersistenceDataChanged);
            }
        }

        // Handle reconnecting players to their world persistent data.
        [UdonSynced] private long _playerLeftTimeStamp;
        public long PlayerLeftTimeStamp => _playerLeftTimeStamp;

        // Use a player guid over player id.
        [UdonSynced, FieldChangeCallback(nameof(PlayerGuidCallback))] private string _playerGuid;

        public string PlayerGuidCallback
        {
            get => _playerGuid;
            set
            {
#if DEBUG_LOGS
                if (_playerGuid == value)
                    LogWarning($"Persistence world player guid was already set (data={_playerPersistenceData}).");
#endif

#if DEBUG_LOGS
                LogDebug($"Player World Persistence Guid updated (prev={_playerGuid}, playerGuid={value}).");
#endif
                _playerGuid = value;

                playerPersistenceManager.HandlePlayerPersistenceData(this);
            }
        }

        public string PlayerGuid
        {
            get => PlayerGuidCallback;
            set
            {
#if DEBUG_LOGS
                if (!Networking.IsOwner(gameObject))
                    LogCritical($"Please make sure that player guid is only set by the owner (playerGuid={value}.");
#endif

                if (!string.IsNullOrEmpty(value))
                    _playerLeftTimeStamp = DateTime.Now.ToBinary();
                PlayerGuidCallback = value;
                RequestSerialization();
            }
        }
    }
}