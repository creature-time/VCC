
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEyeLookFeature : CtNpcFeature
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private Vector2 minMaxEyeAngle = new Vector2(-35f, 35f);
        [SerializeField] private float lookDistance = 3.0f;
        [SerializeField] private float lookSpeed = 15.0f;
        [SerializeField] private Vector2 minMaxEyeContactRange = new Vector2(2.5f, 2.5f);
        [SerializeField] private Vector2 minMaxEyeContactDuration = new Vector2(1f, 2f);

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

        private float _eyeContactTimer;
        private Quaternion _eyeContactOffset;

        public override void Init(CtNpcController controller)
        {
            if (controller.EyeBoneL)
            {
                _eyeLeft = controller.EyeBoneL;
                _eyeRotationLeft = _eyeLeft.localRotation;
            }
#if DEBUG_LOGS
            else
            {
                LogWarning("Left eye transform was null.");
            }
#endif

            if (controller.EyeBoneR)
            {
                _eyeRight = controller.EyeBoneR;
                _eyeRotationRight = _eyeRight.localRotation;
            }
#if DEBUG_LOGS
            else
            {
                LogWarning("Right eye transform was null.");
            }
#endif
        }

        public override void ExecuteUpdate(CtNpcController controller)
        {
            _eyeContactTimer -= Time.deltaTime;
            if (_eyeContactTimer <= 0)
            {
                _eyeContactTimer = Random.Range(minMaxEyeContactDuration.x, minMaxEyeContactDuration.y);
                _eyeContactOffset = Quaternion.Euler(Random.Range(-minMaxEyeContactRange.x, minMaxEyeContactRange.x), 0.0f, Random.Range(-minMaxEyeContactRange.y, minMaxEyeContactRange.y));
            }
        }

        public override void ExecuteLateUpdate(CtNpcController controller)
        {
            if (!_eyeLeft || !_eyeRight)
                return;

            Quaternion targetEyeLeft = _eyeRotationLeft;
            Quaternion targetEyeRight = _eyeRotationRight;

            if (rpgGame.LocalEntity)
            {
                Vector3 targetLookPosition = rpgGame.LocalEntity.HeadTransform.position;
                if (controller.LookTarget)
                {
                    targetLookPosition = controller.LookTarget.position;
                }

                Vector3 eyePosition = (_eyeLeft.position + _eyeRight.position) / 2;
                eyePosition.x = controller.transform.position.x;
                eyePosition.z = controller.transform.position.z;
                Vector3 worldLookDirection = targetLookPosition - eyePosition;
                Vector3 headLookDirection = controller.HeadBone.forward;

                float angle = Vector3.SignedAngle(worldLookDirection, headLookDirection, Vector3.up);
                if (angle > minMaxEyeAngle.x && angle < minMaxEyeAngle.y &&
                    worldLookDirection.magnitude <= lookDistance)
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
            }

            if (_eyeLeft)
                _UpdateEyeLook(_eyeLeft, targetEyeLeft * _eyeContactOffset, ref _eyeTargetRotationLeft);
            if (_eyeRight)
                _UpdateEyeLook(_eyeRight, targetEyeRight * _eyeContactOffset, ref _eyeTargetRotationRight);
        }

        private void _UpdateEyeLook(Transform eyeTransform, Quaternion targetRotation, ref Quaternion lastRotation)
        {
            float t = 1.0f - Mathf.Exp(-lookSpeed * Time.deltaTime);
            lastRotation = Quaternion.Slerp(lastRotation, targetRotation, t);
            eyeTransform.localRotation = lastRotation;
        }
    }
}