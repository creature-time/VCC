
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    public enum ENpcMovementSpeed
    {
        Walk,
        Run,
        Sprint
    }

    public enum ENpcExpression
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Fearful,
        Surprised,
        Disgusted,
        Flirt
    }

    public enum ENpcControllerSignal
    {
        MovementSpeedChanged,
        Extensions
    }

    public abstract class CtNpcController : CtAbstractSignal
    {
        [SerializeField] protected CtRpgGame rpgGame;

        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected Animator animator;

        public abstract string DisplayName { get; }

        [Header("Movement")]
        [SerializeField] private ENpcMovementSpeed npcMovementSpeed = ENpcMovementSpeed.Walk;

        [SerializeField] private float walkSpeed = 1.4f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 7.0f;

        [Header("Characteristics")]
        [SerializeField] private CtNpcBrain brain;

        public CtNpcBrain Brain => brain;

        [Header("Skeleton References")]
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform handBoneL;
        [SerializeField] private Transform handBoneR;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

        public Transform RootTransform => rootTransform;
        public Transform HeadBone => headBone;
        public Transform HandBoneL => handBoneL;
        public Transform HandBoneR => handBoneR;
        public Transform EyeBoneL => eyeBoneL;
        public Transform EyeBoneR => eyeBoneR;

        public ENpcMovementSpeed NpcMovementSpeed
        {
            get => npcMovementSpeed;
            set
            {
                npcMovementSpeed = value;
                switch (npcMovementSpeed)
                {
                    case ENpcMovementSpeed.Run:
                        agent.speed = runSpeed;
                        break;
                    case ENpcMovementSpeed.Sprint:
                        agent.speed = sprintSpeed;
                        break;
                    default:
                        agent.speed = walkSpeed;
                        break;
                }
                this.Emit(ENpcControllerSignal.MovementSpeedChanged);
            }
        }

        private ENpcExpression _expression;

        public ENpcExpression Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnExpressionChanged();
            }
        }

        protected virtual void OnExpressionChanged() {}

        protected int Flags;

        [Header("Blink")]
        [SerializeField] private SkinnedMeshRenderer blinkingSkinnedMesh;
        [SerializeField] private Vector2 minMaxBlinkDelta = new Vector2(4.0f, 6.0f);
        [SerializeField] private float blinkHoldTimer = 0.1f;
        [SerializeField] private float blinkSpeed = 30.0f;
        [SerializeField] private int blinkEyeLeft = -1;
        [SerializeField] private int blinkEyeRight = -1;

        private float _blinkTimer;
        private float _blinkTargetValue;

        [Header("Head Look")]
        [SerializeField] private Vector2 headLookMinMaxAngle = new Vector2(-60f, 60f);
        [SerializeField] private float headLookDistance = 2.5f;
        [SerializeField] private float headLookSpeed = 5.0f;
        [SerializeField] private float headLookResetSpeed = 5.0f;

        private bool _isWithinHeadLookBounds;
        private bool _isHeadLooking;
        private Quaternion _targetHeadLookRotation;

        [Header("Eye Look")]
        [SerializeField] private Vector2 eyeLookMinMaxEyeAngle = new Vector2(-35f, 35f);
        [SerializeField] private float eyeLookDistance = 3.0f;
        [SerializeField] private float eyeLookSpeed = 15.0f;
        [SerializeField] private Vector2 minMaxEyeContactRange = new Vector2(2f, 2f);
        [SerializeField] private Vector2 minMaxEyeContactDuration = new Vector2(1f, 2f);
        [SerializeField] private Quaternion defaultEyeRotationLeft;
        [SerializeField] private Quaternion defaultEyeRotationRight;

        #region Eye Left
        private Quaternion _eyeResetLeft;
        private Quaternion _eyeTargetRotationLeft;
        #endregion

        #region Eye Right
        private Quaternion _eyeResetRight;
        private Quaternion _eyeTargetRotationRight;
        #endregion

        private float _eyeContactTimer;
        private Quaternion _eyeContactOffset = Quaternion.identity;

        public Transform LookTarget { get; set; }

        protected virtual void Start()
        {
#if DEBUG_LOGS
            if (!headBone)
                LogWarning("Head transform was null.");
#endif

            NpcMovementSpeed = npcMovementSpeed;
        }

        private void _HandleAnimator()
        {
            var velocity = RootTransform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("RightVelocity", velocity.x / sprintSpeed);
            animator.SetFloat("ForwardVelocity", velocity.z / sprintSpeed);
        }

        private void _HandleEyeLookUpdate()
        {
            if (!EyeBoneL || !EyeBoneL) return;

            _eyeContactTimer -= Time.deltaTime;
            if (_eyeContactTimer <= 0)
            {
                _eyeContactTimer = Random.Range(minMaxEyeContactDuration.x, minMaxEyeContactDuration.y);
                _eyeContactOffset = Quaternion.Euler(Random.Range(-minMaxEyeContactRange.x, minMaxEyeContactRange.x), 0.0f, Random.Range(-minMaxEyeContactRange.y, minMaxEyeContactRange.y));
            }
        }

        private void _HandleBlinkUpdate()
        {
            if (!blinkingSkinnedMesh) return;

            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0)
                _blinkTimer = Random.Range(minMaxBlinkDelta.x, minMaxBlinkDelta.y);

            var t = 1.0f - Mathf.Exp(-blinkSpeed * Time.deltaTime);
            _blinkTargetValue = Mathf.Lerp(_blinkTargetValue, _blinkTimer < blinkHoldTimer ? 100.0f : 0.0f, t);

            if (blinkEyeLeft != -1)
                blinkingSkinnedMesh.SetBlendShapeWeight(blinkEyeLeft, _blinkTargetValue);
            if (blinkEyeRight != -1)
                blinkingSkinnedMesh.SetBlendShapeWeight(blinkEyeRight, _blinkTargetValue);
        }

        protected virtual void Update()
        {
            if (brain)
            {
                brain.Sense();
                brain.Think();
            }

            _HandleAnimator();

            _HandleEyeLookUpdate();
            _HandleBlinkUpdate();
        }

        private void _HandleHeadLookLateUpdate()
        {
            if (!HeadBone)
                return;

            if (LookTarget)
            {
                var headPosition = LookTarget.position;

                Vector3 eyeHeightPosition = new Vector3(transform.position.x, HeadBone.position.y,
                    transform.position.z);
                Vector3 direction = headPosition - eyeHeightPosition;

                var angle = Vector3.SignedAngle(direction.normalized, transform.forward, transform.up);
                if (angle > headLookMinMaxAngle.x && angle < headLookMinMaxAngle.y && direction.magnitude < headLookDistance)
                {
                    if (!_isHeadLooking)
                    {
                        _isHeadLooking = true;
                        _targetHeadLookRotation = HeadBone.rotation;
                    }

                    var targetRotation = Quaternion.LookRotation(headPosition - HeadBone.position);
                    var t = 1.0f - Mathf.Exp(-headLookSpeed * Time.deltaTime);
                    _targetHeadLookRotation = Quaternion.Lerp(_targetHeadLookRotation, targetRotation, t);

                    HeadBone.rotation = _targetHeadLookRotation;

                    return;
                }
            }

            if (_isHeadLooking)
            {
                if (Quaternion.Angle(HeadBone.rotation, _targetHeadLookRotation) < 0.01f)
                {
                    _isHeadLooking = false;
                }
                else
                {
                    var t = 1.0f - Mathf.Exp(-headLookResetSpeed * Time.deltaTime);
                    _targetHeadLookRotation = Quaternion.Slerp(_targetHeadLookRotation, HeadBone.rotation, t);
                    HeadBone.rotation = _targetHeadLookRotation;
                }
            }
        }

        private void _HandleEyeLookLateUpdate()
        {
            if (!EyeBoneL || !EyeBoneR) return;

            var targetEyeLeft = defaultEyeRotationLeft;
            var targetEyeRight = defaultEyeRotationRight;
            
            if (rpgGame.LocalEntity)
            {
                var targetLookPosition = rpgGame.LocalEntity.HeadTransform.position;
                if (LookTarget)
                {
                    targetLookPosition = LookTarget.position;
                }

                var eyePosition = (EyeBoneL.position + EyeBoneR.position) / 2;
                eyePosition.x = transform.position.x;
                eyePosition.z = transform.position.z;
                var worldLookDirection = targetLookPosition - eyePosition;
                var headLookDirection = HeadBone.forward;

                var angle = Vector3.SignedAngle(worldLookDirection, headLookDirection, Vector3.up);
                if (angle > eyeLookMinMaxEyeAngle.x && angle < eyeLookMinMaxEyeAngle.y &&
                    worldLookDirection.magnitude <= eyeLookDistance)
                {
                    Quaternion worldRotation;
                    if (EyeBoneL)
                    {
                        worldRotation = Quaternion.LookRotation(worldLookDirection) * defaultEyeRotationLeft;
                        targetEyeLeft = Quaternion.Inverse(EyeBoneL.parent.rotation) * worldRotation;
                    }
            
                    if (EyeBoneR)
                    {
                        worldRotation = Quaternion.LookRotation(worldLookDirection) * defaultEyeRotationRight;
                        targetEyeRight = Quaternion.Inverse(EyeBoneR.parent.rotation) * worldRotation;
                    }
                }
            }

            if (EyeBoneL)
                _UpdateEyeLook(EyeBoneL, targetEyeLeft * _eyeContactOffset, ref _eyeTargetRotationLeft);
            if (EyeBoneR)
                _UpdateEyeLook(EyeBoneR, targetEyeRight * _eyeContactOffset, ref _eyeTargetRotationRight);
        }

        private void _UpdateEyeLook(Transform eyeTransform, Quaternion targetRotation, ref Quaternion lastRotation)
        {
            var t = 1.0f - Mathf.Exp(-eyeLookSpeed * Time.deltaTime);
            lastRotation = Quaternion.Slerp(lastRotation, targetRotation, t);
            eyeTransform.localRotation = lastRotation;
        }

        protected virtual void LateUpdate()
        {
            _HandleHeadLookLateUpdate();
            _HandleEyeLookLateUpdate();
        }
    }
}