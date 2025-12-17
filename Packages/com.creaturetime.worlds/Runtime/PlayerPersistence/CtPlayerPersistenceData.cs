
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
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        public UdonSharpBehaviour Extension => extension;

        [UdonSynced, FieldChangeCallback(nameof(PlayerGuidCallback))] private string _playerGuid;

        public string PlayerGuidCallback
        {
            get => _playerGuid;
            set
            {
                _playerGuid = value;
#if DEBUG_LOGS
                LogDebug($"Player Persistence Guid Updated (playerGuid={_playerGuid})");
#endif

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

        public Transform RootTransform => rootTransform;
        public Transform HeadTransform => headTransform;
        public Transform LeftHandTransform => leftHandTransform;
        public Transform RightHandTransform => rightHandTransform;

        public string DisplayName { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsLocal { get; private set; }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (PlayerId != 0) return;
            if (!player.IsOwner(gameObject)) return;

#if DEBUG_LOGS
            LogDebug($"Player Restored (name={gameObject.name}, displayName={player.displayName}, playerId={player.playerId}, playerGuid={PlayerGuid}, extension={Extension})");
#endif

            DisplayName = player.displayName;
            IsLocal = player.isLocal;
            PlayerId = (ushort)player.playerId;

            if (player.isLocal && string.IsNullOrEmpty(PlayerGuid))
                PlayerGuid = Guid.NewGuid().ToString();
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