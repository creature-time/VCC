
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EPlayerManagerSignal
    {
        PlayerAdded,
        PlayerRemoved,
        LocalPlayerChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPlayerManager : CtSingleton
    {
        [SerializeField, HideInInspector] private CtPlayerDef[] playerDefs;
        [SerializeField] private CtAvatarSnapshot avatarSnapshot;

        public CtPlayerDef LocalPlayerDef { get; private set; }
        public ushort LocalPlayerId { get; private set; }

        [SerializeField, HideInInspector] private RenderTexture[] playerRenderTextures;
        private DataList playerRenderTexturesToUpdate = new DataList();

        public void Init()
        {
            // Do nothing?
        }

        public CtPlayerDef GetPlayerDefById(int playerId)
        {
            var index = GetIndexById(playerId);
            var playerDef = playerDefs[index];
            if (playerDef)
                return playerDef;
            return null;
        }

        public int GetIndexById(int playerId)
        {
            for (int i = 0; i < playerDefs.Length; i++)
            {
                CtPlayerDef playerDef = playerDefs[i];
                if (!playerDef)
                    continue;

#if DEBUG_LOGS
                LogDebug($"[GetPlayerDefById] PlayerDef (playerId={playerDef.PlayerId}).");
#endif

                if (playerDef.PlayerId == playerId)
                    return i;
            }

            return 0;
        }

        public CtPlayerDef GetPlayerDef(ushort playerId)
        {
            return playerDefs[playerId];
        }

        public void Client_OnPlayerAdded(CtPlayerDef playerDef)
        {
#if DEBUG_LOGS
            LogDebug($"Player added (displayName={playerDef.DisplayName}).");
#endif

            int index = Array.IndexOf(playerDefs, null);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical("Could not find available player definition.");
#endif
                return;
            }

            playerDef.SetRenderTexture(playerRenderTextures[index]);

            if (playerDef.IsLocal)
            {
                LocalPlayerDef = playerDef;
                LocalPlayerId = (ushort)index;
                this.Emit(EPlayerManagerSignal.LocalPlayerChanged);
            }

            playerDefs[index] = playerDef;

            SetArgs.Add((ushort)index);
            this.Emit(EPlayerManagerSignal.PlayerAdded);

            QueueUpdatePlayerAvatar(index);
        }

        public void Client_OnPlayerRemoved(CtPlayerDef playerDef)
        {
            int index = Array.IndexOf(playerDefs, playerDef);

            SetArgs.Add((ushort)index);
            this.Emit(EPlayerManagerSignal.PlayerRemoved);

            playerDefs[index] = null;

            if (playerDef.IsLocal)
            {
                LocalPlayerDef = null;
                LocalPlayerId = CtConstants.InvalidId;
                this.Emit(EPlayerManagerSignal.LocalPlayerChanged);
            }

            playerRenderTextures[index].Release();

#if DEBUG_LOGS
            LogDebug($"Player removed (displayName={playerDef.DisplayName}).");
#endif
        }

        public void UpdatePlayerAvatar()
        {
            for (int i = 0; i < playerRenderTexturesToUpdate.Count; i++)
            {
                var index = playerRenderTexturesToUpdate[i].Int;
                var playerDef = playerDefs[index];
                if (playerDef)
                    avatarSnapshot.UpdatePlayerIcon(playerDef);
            }
            playerRenderTexturesToUpdate.Clear();
        }

        public void QueueUpdatePlayerAvatar(int index)
        {
            playerRenderTexturesToUpdate.Add(index);
            SendCustomEventDelayedSeconds(nameof(UpdatePlayerAvatar), 5);
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            QueueUpdatePlayerAvatar(GetIndexById(player.playerId));
        }
    }
}