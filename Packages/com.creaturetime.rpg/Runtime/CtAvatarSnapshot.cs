
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtAvatarSnapshot : UdonSharpBehaviour
    {
        [SerializeField] public Camera captureCamera;

        [Range(0.1f, 100.0f)]
        public float distance = 1.0f;

        private void Start()
        {
            // Move it to root so we can properly assign the position and rotation for taking the photo.
            captureCamera.transform.parent = null;
        }

        public void UpdatePlayerIcon(CtPlayerDef playerStats)
        {
            int playerMask = LayerMask.NameToLayer("Player");
            int mirrorReflectionMask = LayerMask.NameToLayer("MirrorReflection");

            captureCamera.targetTexture = (RenderTexture)playerStats.Icon;
            captureCamera.cullingMask = playerStats.IsLocal ? 1 << mirrorReflectionMask : 1 << playerMask;

            var player = VRCPlayerApi.GetPlayerById(playerStats.PlayerId);
            if (player == null)
                return;

            Vector3 position = player.GetBonePosition(HumanBodyBones.Head);
            Quaternion rotation = player.GetRotation();

            var cameraTransform = captureCamera.transform;
            cameraTransform.position = position;
            cameraTransform.rotation = Quaternion.Euler(0, 135, 0) * rotation;
            cameraTransform.position -= cameraTransform.forward * distance;

            captureCamera.Render();
        }
    }
}
