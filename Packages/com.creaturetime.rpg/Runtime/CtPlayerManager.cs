
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
    public class CtPlayerManager : CtAbstractSignal
    {
        [SerializeField, HideInInspector] private CtPlayerDef[] playerDefs;
        [SerializeField] private CtAvatarSnapshot avatarSnapshot;

        public CtPlayerDef LocalPlayerDef { get; private set; }

        [SerializeField, HideInInspector] private RenderTexture[] playerRenderTextures;
        private DataList playerRenderTexturesToUpdate = new DataList();

        public void Init()
        {
            // Do nothing?
        }

        public CtPlayerDef GetPlayerDefById(ushort playerId)
        {
            for (int i = 0; i < playerDefs.Length; i++)
            {
                CtPlayerDef playerDef = playerDefs[i];
                if (!playerDef)
                    continue;

                LogDebug($"[GetPlayerDefById] PlayerDef (playerId={playerDef.PlayerId}).");

                if (playerDef.PlayerId == playerId)
                    return playerDef;
            }

            return null;
        }

        public CtPlayerDef GetPlayerDefByIndex(int index)
        {
            return playerDefs[index];
        }

        public void OnPlayerDestroyed(CtPlayerDef playerDef)
        {
            Client_OnPlayerRemoved(playerDef);

            int index = Array.IndexOf(playerDefs, playerDef);
            playerDefs[index] = null;

            CtLogger.LogDebug("Player Manager",
                $"Player destroyed (displayName={playerDef.DisplayName}, playerId={playerDef.PlayerId})");
        }

        public void Client_OnPlayerAdded(CtPlayerDef playerDef)
        {
            CtLogger.LogDebug("Player Manager",
                $"Player added (displayName={playerDef.DisplayName}).");

            int index = Array.IndexOf(playerDefs, null);
            if (index == -1)
            {
                CtLogger.LogCritical("Player Manager", "Could not find available player definition.");
                return;
            }

            playerDef.SetRenderTexture(playerRenderTextures[index]);

            if (playerDef.IsLocal)
            {
                LocalPlayerDef = playerDef;
                this.Emit(EPlayerManagerSignal.LocalPlayerChanged);
            }

            playerDefs[index] = playerDef;

            SetArgs.Add(index);
            this.Emit(EPlayerManagerSignal.PlayerAdded);

            QueueUpdatePlayerAvatar(playerDef.PlayerId);
        }

        public void Client_OnPlayerRemoved(CtPlayerDef playerDef)
        {
            int index = Array.IndexOf(playerDefs, playerDef);

            SetArgs.Add(index);
            this.Emit(EPlayerManagerSignal.PlayerRemoved);

            if (playerDef.IsLocal)
            {
                LocalPlayerDef = null;
                this.Emit(EPlayerManagerSignal.LocalPlayerChanged);
            }

            playerRenderTextures[index].Release();

            CtLogger.LogDebug("Player Manager",
                $"Player removed (displayName={playerDef.DisplayName}).");
        }

        public void UpdatePlayerAvatar()
        {
            for (int i = 0; i < playerRenderTexturesToUpdate.Count; i++)
            {
                var playerDef = GetPlayerDefById(playerRenderTexturesToUpdate[i].UShort);
                if (playerDef)
                    avatarSnapshot.UpdatePlayerIcon(playerDef);
            }
            playerRenderTexturesToUpdate.Clear();
        }

        public void QueueUpdatePlayerAvatar(ushort playerId)
        {
            playerRenderTexturesToUpdate.Add(playerId);
            SendCustomEventDelayedSeconds(nameof(UpdatePlayerAvatar), 5);
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            QueueUpdatePlayerAvatar((ushort)player.playerId);
        }
    }
}