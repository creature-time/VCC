
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EPlayerPersistenceManagerSignal
    {
        LocalPlayerChanged,
        PlayerAdded,
        PlayerRemoved
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPlayerPersistenceManager : CtSingleton
    {
        [SerializeField] private CtPlayerWorldPersistenceData[] playerWorldPersistenceDataArray;

        private DataDictionary _playerPersistenceDataLookup = new DataDictionary();

//         private bool _TryGetPlayerPersistenceData(string playerGuid, out CtPlayerPersistenceData playerPersistenceData)
//         {
// #if DEBUG_LOGS
//             if (string.IsNullOrEmpty(playerGuid))
//                 LogWarning("Player guid was null. Please valid player guid before calling function.");
// #endif
//
//             if (_playerPersistenceDataLookup.TryGetValue(playerGuid, out var token))
//             {
//                 playerPersistenceData = (CtPlayerPersistenceData)token.Reference;
//                 return true;
//             }
//
// #if DEBUG_LOGS
//             LogDebug($"Failed to get player persistence data (playerGuid={playerGuid}).");
// #endif
//
//             playerPersistenceData = null;
//             return false;
//         }

        private bool TryGetPlayerIndex(string playerGuid, out int index)
        {
            index = -1;

            // Check to see if they player was previously disconnected and oldest timestamp within single loop.
            var oldest = DateTime.Now;
            int oldestIndex = -1;
            int firstAvailableIndex = -1;
            for (int i = 0; i < playerWorldPersistenceDataArray.Length; i++)
            {
                var data = playerWorldPersistenceDataArray[i];

                var otherPlayerGuid = data.PlayerGuid;
                if (string.IsNullOrEmpty(otherPlayerGuid) && firstAvailableIndex == -1)
                    firstAvailableIndex = i;

                if (string.IsNullOrEmpty(playerGuid)) continue;

                // If there are no empty slots, then remove the oldest disconnected player.
                if (DateTime.FromBinary(data.PlayerLeftTimeStamp) < oldest)
                    oldestIndex = i;

                // Check if the player was previously disconnected and if there is currently not a player in that slot.
                if (otherPlayerGuid == playerGuid && !data.PlayerPersistenceData)
                {
                    // Found previously disconnected player slot.
                    index = i;
#if DEBUG_LOGS
                    LogDebug($"Found previously disconnected player slot (index={index}).");
#endif
                    return true;
                }
            }

            // If index was found after checking for empty player slots...
            if (firstAvailableIndex != -1)
            {
                index = firstAvailableIndex;
#if DEBUG_LOGS
                LogDebug($"Found empty player slot (index={index}).");
#endif
                return true;
            }

            // Use the oldest index by timestamp if available.
            if (oldestIndex != -1)
            {
                index = oldestIndex;
#if DEBUG_LOGS
                LogDebug($"Overriding oldest player disconnected slot (index={index}).");
#endif
                return true;
            }

#if DEBUG_LOGS
            LogCritical("Could not find available player definition slot.");
#endif

            return false;
        }

        public void OnPlayerAdded(CtPlayerPersistenceData playerPersistenceData)
        {
#if DEBUG_LOGS
            LogDebug($"Player persistence added (playerGuid={playerPersistenceData.PlayerGuid}).");
#endif

            if (!TryGetPlayerIndex(playerPersistenceData.PlayerGuid, out var index)) return;

            var playerWorldPersistenceData = playerWorldPersistenceDataArray[index];
            if (playerWorldPersistenceData.PlayerPersistenceData &&
                playerWorldPersistenceData.PlayerGuid != playerPersistenceData.PlayerGuid)
            {
#if DEBUG_LOGS
                LogCritical($"Player persistence data is being overriden (index={index}).");
#endif
                return;
            }

            _playerPersistenceDataLookup.Add(playerPersistenceData.PlayerGuid, playerPersistenceData);

            if (Networking.IsMaster)
                playerWorldPersistenceData.PlayerGuid = playerPersistenceData.PlayerGuid;

            HandlePlayerPersistenceData(playerWorldPersistenceData);
        }

        public void HandlePlayerPersistenceData(CtPlayerWorldPersistenceData playerWorldPersistenceData)
        {
            if (playerWorldPersistenceData.PlayerPersistenceData) return;
            var playerGuid = playerWorldPersistenceData.PlayerGuid;
            if (string.IsNullOrEmpty(playerGuid)) return;
            if (!_playerPersistenceDataLookup.TryGetValue(playerGuid, out var token)) return;

            playerWorldPersistenceData.PlayerPersistenceData = (CtPlayerPersistenceData)token.Reference;
        }

        public void OnPlayerAdded(CtPlayerWorldPersistenceData playerWorldPersistenceData)
        {
#if DEBUG_LOGS
            LogDebug($"Player world persistence added (displayName={playerWorldPersistenceData.PlayerGuid}).");
#endif

            SetArgs.Add(playerWorldPersistenceData);
            this.Emit(EPlayerPersistenceManagerSignal.PlayerAdded);

            if (playerWorldPersistenceData.PlayerPersistenceData.IsLocal)
            {
                SetArgs.Add(playerWorldPersistenceData);
                this.Emit(EPlayerPersistenceManagerSignal.LocalPlayerChanged);
            }
        }

        public void OnPlayerRemoved(CtPlayerPersistenceData playerPersistenceData)
        {
            var index = -1;
            for (int i = 0; i < playerWorldPersistenceDataArray.Length; i++)
            {
                if (playerWorldPersistenceDataArray[i].PlayerGuid == playerPersistenceData.PlayerGuid)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical(
                    $"Failed to find player persistence data to remove (playerGuid={playerPersistenceData.PlayerGuid}).");
#endif
                return;
            }

            var playerWorldPersistenceData = playerWorldPersistenceDataArray[index];
            _playerPersistenceDataLookup.Remove(playerWorldPersistenceData.PlayerPersistenceData.PlayerGuid);
            playerWorldPersistenceData.PlayerPersistenceData = null;

#if DEBUG_LOGS
            LogDebug($"Player persistence removed (playerGuid={playerWorldPersistenceData.PlayerGuid}).");
#endif
        }

        public void OnPlayerRemoved(CtPlayerWorldPersistenceData playerWorldPersistenceData)
        {
            if (!playerWorldPersistenceData.PlayerPersistenceData)
            {
#if DEBUG_LOGS
                LogCritical("Player world persistence data did not have a player persistence data attached " +
                            $"(playerGuid={playerWorldPersistenceData.PlayerGuid}).");
#endif
                return;
            }

            if (playerWorldPersistenceData.PlayerPersistenceData.IsLocal)
            {
                SetArgs.Add((CtPlayerWorldPersistenceData)null);
                this.Emit(EPlayerPersistenceManagerSignal.LocalPlayerChanged);
            }

            SetArgs.Add(playerWorldPersistenceData);
            this.Emit(EPlayerPersistenceManagerSignal.PlayerRemoved);

#if DEBUG_LOGS
            LogDebug($"Player world persistence removed (playerGuid={playerWorldPersistenceData.PlayerGuid}).");
#endif
        }
    }
}