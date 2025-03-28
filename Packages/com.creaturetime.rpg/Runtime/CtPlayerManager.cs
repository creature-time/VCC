
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
        [SerializeField] private int maxPlayerCount;
        [SerializeField] private CtEntityDef playerDefaultDef;
        [SerializeField] private CtAvatarSnapshot avatarSnapshot;

        public CtPlayerDef LocalPlayerDef { get; private set; }
        public CtPlayerDef[] PlayerDefinitions { get; private set; }

        [SerializeField, HideInInspector] private RenderTexture[] playerRenderTextures;
        private DataList playerRenderTexturesToUpdate = new DataList();

        public CtPlayerDef GetPlayerDef(ushort playerId)
        {
            for (int i = 0; i < PlayerDefinitions.Length; i++)
            {
                CtPlayerDef playerDef = PlayerDefinitions[i];
                if (playerDef)
                    if (playerDef.PlayerId == playerId)
                        return playerDef;
            }

            return null;
        }

        private void OnEnable()
        {
            PlayerDefinitions = new CtPlayerDef[maxPlayerCount];
        }

        public void SetupPlayer(VRCPlayerApi player, CtPlayerDef playerDef)
        {
            if (playerDef.CharacterLevel == 0)
            {
                playerDef.Copy(playerDefaultDef);

                // Testing
                playerDef.InvAddTo(0, 18446744024585911168);
                playerDef.InvAddTo(1, 18446744024585863200);
                playerDef.InvAddTo(2, 18446744024585863216);

                playerDef.InvAddTo(3, 18446744073708503057);
                playerDef.InvAddTo(4, 18446744073708503073);
                playerDef.InvAddTo(5, 18446744073708503089);
                playerDef.InvAddTo(6, 18446744073708503105);
                playerDef.InvAddTo(7, 18446744073708503121);
            }

            CtLogger.LogDebug("Player Manager",
                "Player joined " +
                $"(displayName={playerDef.DisplayName}, playerId={player.playerId})");
        }

        public void OnPlayerDestroyed(CtPlayerDef playerDef)
        {
            Client_OnPlayerRemoved(playerDef);

            int index = Array.IndexOf(PlayerDefinitions, playerDef);
            PlayerDefinitions[index] = null;

            CtLogger.LogDebug("Player Manager",
                $"Player destroyed (displayName={playerDef.DisplayName}, playerId={playerDef.PlayerId})");
        }

        public void Client_OnPlayerAdded(CtPlayerDef playerDef)
        {
            CtLogger.LogDebug("Player Manager",
                $"Player added (displayName={playerDef.DisplayName}).");

            int index = Array.IndexOf(PlayerDefinitions, null);
            if (index == -1)
            {
                CtLogger.LogCritical("Player Manager", "Could not find available player definition.");
                return;
            }

            Debug.Log(playerRenderTextures[index]);
            playerDef.SetRenderTexture(playerRenderTextures[index]);

            if (playerDef.IsLocal)
            {
                LocalPlayerDef = playerDef;
                this.Emit(EPlayerManagerSignal.LocalPlayerChanged);
            }

            PlayerDefinitions[index] = playerDef;

            SetArgs.Add(index);
            this.Emit(EPlayerManagerSignal.PlayerAdded);

            QueueUpdatePlayerAvatar(playerDef.PlayerId);
        }

        public void Client_OnPlayerRemoved(CtPlayerDef playerDef)
        {
            int index = Array.IndexOf(PlayerDefinitions, playerDef);

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
                var playerDef = GetPlayerDef(playerRenderTexturesToUpdate[i].UShort);
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