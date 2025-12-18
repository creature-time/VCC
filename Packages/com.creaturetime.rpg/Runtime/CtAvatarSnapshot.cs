
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtAvatarSnapshot : CtLoggerUdonScript
    {
        [SerializeField] public Camera captureCamera;

        [SerializeField] private RenderTexture[] playerRenderTextures;
        private DataDictionary _registry = new DataDictionary();
        private DataList _playerRenderTexturesToUpdate = new DataList();

        // [Range(0.1f, 100.0f)]
        // public float distance = 1.0f;

        private void Start()
        {
            // Move it to root so we can properly assign the position and rotation for taking the photo.
            transform.parent = null;
        }

        public bool Register(int playerId, out Texture renderTexture)
        {
            renderTexture = null;
            foreach (var rt in playerRenderTextures)
            {
                if (_registry.ContainsValue(rt)) continue;

#if DEBUG_LOGS
                LogDebug($"Registering render texture to player (playerId={playerId}, renderTexture={rt}).");
#endif

                _registry.Add(playerId, rt);
                _QueueSnapshot(playerId);

                renderTexture = rt;
                return true;
            }

#if DEBUG_LOGS
            LogCritical($"Failed to find render texture to register to player (playerId={playerId}).");
#endif

            return false;
        }

        public void Unregister(int playerId)
        {
            if (!_registry.TryGetValue(playerId, out var token))
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find player to unregister (playerId={playerId}).");
#endif
                return;
            }

            var renderTexture = (RenderTexture)token.Reference;
            renderTexture.Release();
            _registry.Remove(playerId);
        }

        private void _UpdatePlayerIcon(int playerId)
        {
            if (!_registry.TryGetValue(playerId, out var token))
            {
                return;
            }

            var renderTexture = (RenderTexture)token.Reference;

#if DEBUG_LOGS
            LogDebug($"Updating to update player avatar texture (playerId={playerId}).");
#endif

            var player = VRCPlayerApi.GetPlayerById(playerId);
            if (player == null) return;

            transform.position = player.GetBonePosition(HumanBodyBones.Head);;
            transform.rotation = player.GetRotation();

            int playerMask = LayerMask.NameToLayer("Player");
            int mirrorReflectionMask = LayerMask.NameToLayer("MirrorReflection");

            captureCamera.cullingMask = player.isLocal ? 1 << mirrorReflectionMask : 1 << playerMask;

            captureCamera.targetTexture = renderTexture;
            captureCamera.Render();
        }

        public void UpdatePlayerAvatar()
        {
            for (int i = 0; i < _playerRenderTexturesToUpdate.Count; i++)
            {
                _UpdatePlayerIcon(_playerRenderTexturesToUpdate[i].Int);
            }

            _playerRenderTexturesToUpdate.Clear();
        }

        private void _QueueSnapshot(int playerId)
        {
#if DEBUG_LOGS
            LogDebug($"Queued to update player avatar texture (playerId={playerId}).");
#endif

            if (_playerRenderTexturesToUpdate.Contains(playerId)) return;

            if (_playerRenderTexturesToUpdate.Count == 0)
                SendCustomEventDelayedSeconds(nameof(UpdatePlayerAvatar), 5);

            _playerRenderTexturesToUpdate.Add(playerId);
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            _QueueSnapshot(player.playerId);
        }
    }
}
