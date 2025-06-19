
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtHeadLookFeature : CtNpcFeature
    {
        [SerializeField] private float resetTimer = 1.0f;
        [SerializeField] private Vector2 minMaxEyeAngle = new Vector2(-60f, 60f);
        [SerializeField] private float lookDistance = 2.5f;
        [SerializeField] private float lookSpeed = 5.0f;
        [SerializeField] private float resetLookSpeed = 5.0f;

        private bool _isWithinBounds;
        private bool _isLooking;
        private Transform _headBone;
        private Quaternion _targetRotation;

        public override void Init(CtNpcController controller)
        {
            if (controller.HeadBone)
            {
                _headBone = controller.HeadBone;
            }
#if DEBUG_LOGS
            else
            {
                CtLogger.LogWarning("Dialogue Character", "Head transform was null.");
            }
#endif
        }

        public override void ExecuteLateUpdate(CtNpcController controller)
        {
            if (!_headBone)
                return;

            Vector3 headPosition;
            if (controller.LookTarget)
            {
                headPosition = controller.LookTarget.position;
            }
            else
            {
                if (Networking.LocalPlayer == null)
                    return;
                headPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            }

            Vector3 eyeHeightPosition = new Vector3(controller.transform.position.x, _headBone.position.y, controller.transform.position.z);
            Vector3 direction = headPosition - eyeHeightPosition;
            float angle = Vector3.SignedAngle(direction.normalized, controller.transform.forward, transform.up);
            if (angle > minMaxEyeAngle.x && angle < minMaxEyeAngle.y && direction.magnitude < lookDistance)
            {
                if (!_isLooking)
                {
                    _isLooking = true;
                    _targetRotation = _headBone.rotation;
                }

                Quaternion targetRotation = Quaternion.LookRotation(headPosition - _headBone.position);
                float t = 1.0f - Mathf.Exp(-lookSpeed * Time.deltaTime);
                _targetRotation = Quaternion.Slerp(_targetRotation, targetRotation, t);

                _headBone.rotation = _targetRotation;
            }
            else if (_isLooking)
            {
                if (Quaternion.Angle(_headBone.rotation, _targetRotation) < 0.01f)
                {
                    _isLooking = false;
                }
                else
                {
                    float t = 1.0f - Mathf.Exp(-resetLookSpeed * Time.deltaTime);
                    _targetRotation = Quaternion.Slerp(_targetRotation, _headBone.rotation, t);
                    _headBone.rotation = _targetRotation;
                }
            }
        }
    }
}