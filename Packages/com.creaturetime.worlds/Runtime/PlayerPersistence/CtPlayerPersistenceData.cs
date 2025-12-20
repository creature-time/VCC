
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerPersistenceData : CtAbstractSignal
    {
        [SerializeField] private CtPlayerPersistenceManager playerPersistenceManager;
        [SerializeField] private UdonSharpBehaviour extension;

        public UdonSharpBehaviour Extension => extension;

        // [UdonSynced] private Vector3 _version;

        [UdonSynced, FieldChangeCallback(nameof(PlayerGuidCallback))] private string _playerGuid;

        public string PlayerGuidCallback
        {
            get => _playerGuid;
            set
            {
#if DEBUG_LOGS
                if (_playerGuid == value)
                    LogWarning($"Player persistence player guid was already set (playerGuid={_playerGuid}).");
#endif

                _playerGuid = value;
#if DEBUG_LOGS
                LogDebug($"Player Persistence Guid Updated (playerGuid={_playerGuid})");
#endif

                if (PlayerId != 0)
                    playerPersistenceManager.OnPlayerAdded(this);
            }
        }

        public string PlayerGuid
        {
            get => PlayerGuidCallback;
            private set
            {
                PlayerGuidCallback = value;
                RequestSerialization();
            }
        }

        public string DisplayName { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsLocal { get; private set; }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (PlayerId != 0) return;
            if (!player.IsOwner(gameObject)) return;

#if DEBUG_LOGS
            LogDebug($"Player Restored (name={gameObject.name}, displayName={player.displayName}, playerId={player.playerId}, isLocal={player.isLocal}, playerGuid={PlayerGuid}, extension={Extension})");
#endif

            DisplayName = $"{player.displayName} [{player.playerId}] [{player.isLocal}]";
            IsLocal = player.isLocal;
            PlayerId = (ushort)player.playerId;

            if (string.IsNullOrEmpty(PlayerGuid))
            {
                if (IsLocal)
                {
                    // _version = new Vector3(0, 1, 0);
                    PlayerGuid = Guid.NewGuid().ToString();
                }
            }
            else
            {
                playerPersistenceManager.OnPlayerAdded(this);
            }
        }

        public void OnDestroy()
        {
            playerPersistenceManager.OnPlayerRemoved(this);

#if DEBUG_LOGS
            LogDebug($"Player persistence data destroyed (playerGuid={PlayerGuid})");
#endif
        }
    }
}