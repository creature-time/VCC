
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEyeLookFeature : CtNpcFeature
    {
        [SerializeField] private Vector2 minMaxEyeAngle = new Vector2(-35f, 35f);
        [SerializeField] private float lookDistance = 3.0f;
        [SerializeField] private float lookSpeed = 15.0f;

        #region Eye Left
        private Transform _eyeLeft;
        private Quaternion _eyeResetLeft;
        private Quaternion _eyeRotationLeft;
        private Quaternion _eyeTargetRotationLeft;
        #endregion

        #region Eye Right
        private Transform _eyeRight;
        private Quaternion _eyeResetRight;
        private Quaternion _eyeRotationRight;
        private Quaternion _eyeTargetRotationRight;
        #endregion

        public override void Init(CtNpcController controller)
        {
            if (controller.EyeBoneL)
            {
                _eyeLeft = controller.EyeBoneL;
                _eyeRotationLeft = _eyeLeft.localRotation;
            }
            else
            {
                CtLogger.LogWarning("Dialogue Character", "Left eye transform was null.");
            }

            if (controller.EyeBoneR)
            {
                _eyeRight = controller.EyeBoneR;
                _eyeRotationRight = _eyeRight.localRotation;
            }
            else
            {
                CtLogger.LogWarning("Dialogue Character", "Right eye transform was null.");
            }
        }

        public override void ExecuteLateUpdate(CtNpcController controller)
        {
            if (!_eyeLeft || !_eyeRight)
                return;

            Quaternion targetEyeLeft = _eyeRotationLeft;
            Quaternion targetEyeRight = _eyeRotationRight;
            Vector3 targetLookPosition;
            if (controller.LookTarget)
            {
                targetLookPosition = controller.LookTarget.position;
            }
            else
            {
                if (Networking.LocalPlayer == null)
                    return;
                targetLookPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            }

            Vector3 eyePosition = (_eyeLeft.position + _eyeRight.position) / 2;
            Vector3 worldLookDirection = targetLookPosition - eyePosition;
            Vector3 headLookDirection = controller.HeadBone.forward;

            float angle = Vector3.SignedAngle(worldLookDirection, headLookDirection, Vector3.up);
            if (angle > minMaxEyeAngle.x && angle < minMaxEyeAngle.y && worldLookDirection.magnitude <= lookDistance)
            {
                Quaternion worldRotation;
                if (_eyeLeft)
                {
                    worldRotation = Quaternion.LookRotation(worldLookDirection) * _eyeRotationLeft;
                    targetEyeLeft = Quaternion.Inverse(_eyeLeft.parent.rotation) * worldRotation;
                }

                if (_eyeRight)
                {
                    worldRotation = Quaternion.LookRotation(worldLookDirection) * _eyeRotationRight;
                    targetEyeRight = Quaternion.Inverse(_eyeRight.parent.rotation) * worldRotation;
                }
            }

            if (_eyeLeft)
                _UpdateEyeLook(_eyeLeft, targetEyeLeft, ref _eyeTargetRotationLeft);
            if (_eyeRight)
                _UpdateEyeLook(_eyeRight, targetEyeRight, ref _eyeTargetRotationRight);
        }

        private void _UpdateEyeLook(Transform eyeTransform, Quaternion targetRotation, ref Quaternion lastRotation)
        {
            float t = 1.0f - Mathf.Exp(-lookSpeed * Time.deltaTime);
            lastRotation = Quaternion.Slerp(lastRotation, targetRotation, t);
            eyeTransform.localRotation = lastRotation;
        }
    }
}