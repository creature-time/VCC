
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtVrController : UdonSharpBehaviour
    {
        [SerializeField] private CtSelectionModel friendlySelectionModel;
        [SerializeField] private CtSelectionModel enemySelectionModel;
        [SerializeField] private LineRenderer lineRenderer;

        private ushort _hovered = CtConstants.InvalidId;

        private void FixedUpdate()
        {
            var localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            Vector3 origin;
            Vector3 direction;

            var isUserInVr = localPlayer.IsUserInVR();
            if (isUserInVr)
            {
                var trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                origin = trackingData.position;
                direction = trackingData.rotation * Quaternion.Euler(0, 45f, 0) * Vector3.forward;
            }
            else
            {
                var trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                origin = trackingData.position;
                direction = trackingData.rotation * Vector3.forward;
            }

            // Debug.DrawLine(origin, origin + direction * 50, Color.red, 1f);

            var result = Physics.Raycast(new Ray(origin, direction), out var hitInfo, 30f);
            result = result && hitInfo.collider;
            // result = isUserInVr && result && hitInfo.collider;
            _hovered = CtConstants.InvalidId;
            if (result)
            {
                var npcUserData = hitInfo.collider.GetComponent<CtNpcUserData>();
                result = npcUserData;
                if (npcUserData)
                {
                    lineRenderer.SetPosition(0, origin);
                    lineRenderer.SetPosition(1, hitInfo.point);

                    _hovered = npcUserData.TargetId;
                }
            }

            lineRenderer.enabled = result;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (args.handType == HandType.RIGHT && args.boolValue)
            {
                if (_hovered != CtConstants.InvalidId)
                {
                    var curr = enemySelectionModel.Selection;
                    if (curr.Count > 0 && curr[0].UShort == _hovered)
                        enemySelectionModel.Clear();
                    else
                        enemySelectionModel.SetSelection(_hovered, ESelectionFlags.ClearSelection);
                    _hovered = CtConstants.InvalidId;
                }
            }
        }
    }
}