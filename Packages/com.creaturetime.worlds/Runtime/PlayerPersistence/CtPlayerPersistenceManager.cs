
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

        private DataList playerPersistenceDataArray = new DataList();

        public void Init()
        {
        }

//         public bool TryGetPlayerPersistenceData(string playerGuid, out CtPlayerPersistenceData playerPersistenceData)
//         {
//             playerPersistenceData = null;
//
// #if DEBUG_LOGS
//             if (string.IsNullOrEmpty(playerGuid))
//                 LogWarning("Player guid was null. Please valid player guid before calling function.");
// #endif
//
//             for (int i = 0; i < playerPersistenceDataArray.Count; i++)
//             {
//                 var data = (CtPlayerPersistenceData)playerPersistenceDataArray[i].Reference;
//                 if (data.PlayerGuid == playerGuid)
//                 {
//                     playerPersistenceData = data;
//                     return true;
//                 }
//             }
//
// #if DEBUG_LOGS
//             LogCritical($"Failed to get player persistence data (playerGuid={playerGuid}).");
// #endif
//
//             return false;
//         }
//
//         public void _OnPlayerPersistenceDataChanged()
//         {
//             var sender = (CtPlayerWorldPersistenceData)Sender;
//             if (sender.PlayerPersistenceData.IsLocal)
//             {
//                 SetArgs.Add(sender);
//                 this.Emit(EPlayerPersistenceManagerSignal.LocalPlayerChanged);
//             }
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
            LogDebug($"Player added (displayName={playerPersistenceData.PlayerGuid}).");
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

            playerPersistenceDataArray.Add(playerPersistenceData);
            playerWorldPersistenceDataArray[index].PlayerPersistenceData = playerPersistenceData;

            if (Networking.IsMaster)
                playerWorldPersistenceData.PlayerGuid = playerPersistenceData.PlayerGuid;

            SetArgs.Add(playerWorldPersistenceData);
            this.Emit(EPlayerPersistenceManagerSignal.PlayerAdded);

            if (playerPersistenceData.IsLocal)
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
                LogCritical($"Failed to find player persistence data to remove (playerGuid={playerPersistenceData.PlayerGuid}).");
#endif
                return;
            }

            if (playerPersistenceData.IsLocal)
            {
                SetArgs.Add((CtPlayerWorldPersistenceData)null);
                this.Emit(EPlayerPersistenceManagerSignal.LocalPlayerChanged);
            }

            var playerWorldPersistenceData = playerWorldPersistenceDataArray[index];

            SetArgs.Add(playerWorldPersistenceData);
            this.Emit(EPlayerPersistenceManagerSignal.PlayerRemoved);

            playerWorldPersistenceData.PlayerPersistenceData = null;
            playerPersistenceDataArray.Remove(playerPersistenceData);

#if DEBUG_LOGS
            LogDebug($"Player removed (displayName={playerPersistenceData.PlayerGuid}).");
#endif
        }
    }
}