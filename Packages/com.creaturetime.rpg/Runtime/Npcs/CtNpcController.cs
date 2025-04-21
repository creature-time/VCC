
using UdonSharp;
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

    public enum ECharacterSignal
    {
        MovementSpeedChanged,
        DialogueChanged,
        SequenceChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcController : CtAbstractSignal
    {
        private const int CharacterFlagsHasDialogue = 1 << 0;
        private const int CharacterFlagsHasSequence = 1 << 1;
        private const int CharacterFlagsHalt = 1 << 2;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        [Header("Character")]
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private string displayName;
        [SerializeField] private string subTitle;
        [SerializeField] private AudioClip babbleClip;
        // TODO: Use a global audio source manager.
        public AudioSource BabbleSource { get; set; }

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 1.4f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 7.0f;

        public ushort Identifier => identifier;
        public string DisplayName => displayName;
        public string SubTitle => subTitle;

        [Header("Characteristics")]
        [SerializeField] private CtNpcBrain brain;
        [SerializeField] private CtNpcFeature[] features = {};

        [Header("Skeleton References")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

        public Transform HeadBone => headBone;
        public Transform EyeBoneL => eyeBoneL;
        public Transform EyeBoneR => eyeBoneR;

        private ENpcMovementSpeed _npcMovementSpeed = ENpcMovementSpeed.Walk;

        public ENpcMovementSpeed NpcMovementSpeed
        {
            get => _npcMovementSpeed;
            set
            {
                _npcMovementSpeed = value;
                switch (_npcMovementSpeed)
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
                this.Emit(ECharacterSignal.MovementSpeedChanged);
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
                        LogWarning($"Unknown expression (expression={_expression}).");
                        break;
                }
            }
        }

        private int _flags;

        public bool HasSequence
        {
            get => (_flags & CharacterFlagsHasSequence) != 0;
            set
            {
                if (value)
                    _flags |= CharacterFlagsHasSequence;
                else
                    _flags &= ~CharacterFlagsHasSequence;
                this.Emit(ECharacterSignal.SequenceChanged);
            }
        }

        public bool HasDialogue
        {
            get => (_flags & CharacterFlagsHasDialogue) != 0;
            set
            {
                if (value)
                    _flags |= CharacterFlagsHasDialogue;
                else
                    _flags &= ~CharacterFlagsHasDialogue;                
                this.Emit(ECharacterSignal.DialogueChanged);
            }
        }

        public bool IsHalted
        {
            get => (_flags & CharacterFlagsHalt) != 0;
            set
            {
                if (value)
                {
                    _flags |= CharacterFlagsHalt;
                }
                else
                {
                    _flags &= ~CharacterFlagsHalt;
                }
            }
        }

        public CtNpcController Target { get; set; }

        private Transform _lookTarget;
        public Transform LookTarget
        {
            get => _lookTarget ? _lookTarget : Target ? Target.HeadBone.transform : null;
            set => _lookTarget = value;
        }

        private void Start()
        {
            if (!headBone)
                LogWarning("Head transform was null.");

            NpcMovementSpeed = ENpcMovementSpeed.Walk;

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

        public void Babble()
        {
            if (!BabbleSource.isPlaying)
            {
                BabbleSource.clip = babbleClip;
                BabbleSource.Play();
                BabbleSource.pitch = Random.Range(0.75f, 1.25f);
            }
        }
    }
}