
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
        Neutral
    }

    public enum ENpcControllerSignal
    {
        MovementSpeedChanged,
        Extensions
    }

    public abstract class CtNpcController : CtAbstractSignal
    {
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
        [SerializeField] private CtNpcFeature[] features = {};

        public CtNpcBrain Brain => brain;

        [Header("Skeleton References")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform handBoneL;
        [SerializeField] private Transform handBoneR;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

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
                switch (_expression)
                {
                    case ENpcExpression.Neutral:
                        // animator.SetTrigger("Speak");
                        break;
                    default:
#if DEBUG_LOGS
                        LogWarning($"Unknown expression (expression={_expression}).");
#endif
                        break;
                }
            }
        }

        protected int Flags;

        public Transform LookTarget { get; set; }

        private void Start()
        {
#if DEBUG_LOGS
            if (!headBone)
                LogWarning("Head transform was null.");
#endif

            NpcMovementSpeed = npcMovementSpeed;

            for (int i = 0; i < features.Length; i++)
                features[i].Init(this);
        }

        private void _HandleAnimator()
        {
            Vector3 velocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("RightVelocity", velocity.x / sprintSpeed);
            animator.SetFloat("ForwardVelocity", velocity.z / sprintSpeed);
        }

        private void Update()
        {
            if (brain)
            {
                brain.Sense();
                brain.Think();
            }

            _HandleAnimator();

            for (int i = 0; i < features.Length; i++)
                features[i].ExecuteUpdate(this);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < features.Length; i++)
                features[i].ExecuteLateUpdate(this);
        }
    }
}