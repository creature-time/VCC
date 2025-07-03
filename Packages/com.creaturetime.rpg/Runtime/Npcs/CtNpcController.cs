
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;

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
        SequenceChanged,
        DamageTrigger,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcController : CtAbstractSignal
    {
        private const int CharacterFlagsHasDialogue = 1 << 0;
        private const int CharacterFlagsHasSequence = 1 << 1;
        private const int CharacterFlagsHalt = 1 << 2;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        [Header("Character")]
        [SerializeField] private string displayName;
        [SerializeField] private string subTitle;
        [SerializeField] private AudioClip babbleClip;

        [Header("Movement")]
        [SerializeField] private ENpcMovementSpeed npcMovementSpeed = ENpcMovementSpeed.Walk;

        [SerializeField] private float walkSpeed = 1.4f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 7.0f;

        public string DisplayName => displayName;
        public string SubTitle => subTitle;

        [Header("Characteristics")]
        [SerializeField] private CtNpcBrain brain;
        [SerializeField] private CtNpcFeature[] features = {};

        [Header("Skeleton References")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

        public AudioClip BabbleClip => babbleClip;
        public CtNpcBrain Brain => brain;

        public Transform HeadBone => headBone;
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
#if DEBUG_LOGS
                        LogWarning($"Unknown expression (expression={_expression}).");
#endif
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
        public Transform HomePosition { get; set; }

        private DataDictionary _damageValues = new DataDictionary();

        public DataDictionary DamageValues => _damageValues;

        private bool IsChargingMelee
        {
            // get
            // {
            //     brain.Context.TryGetBool("Expert/IsChargingMelee", out var value);
            //     return value;
            // }
            set => brain.Context.SetBool("Expert/IsChargingMelee", value);
        }

        private bool IsAttackingMelee
        {
            // get
            // {
            //     brain.Context.TryGetBool("Expert/IsAttackingMelee", out var value);
            //     return value;
            // }
            set => brain.Context.SetBool("Expert/IsAttackingMelee", value);
        }

        private bool IsDoneAttackingMelee
        {
            // get
            // {
            //     brain.Context.TryGetBool("Expert/IsAttackingMelee", out var value);
            //     return value;
            // }
            set => brain.Context.SetBool("Expert/IsDoneAttackingMelee", value);
        }

        public void MeleeAttack()
        {
            IsAttackingMelee = true;
            animator.SetTrigger("MeleeAttack");

            // TODO: Get the attack animation length?
            SendCustomEventDelayedSeconds(nameof(_FinishedAttacking), 1.5f);
        }

        public void _FinishedAttacking()
        {
            IsDoneAttackingMelee = true;
        }

        public void InitiateAttack(ushort targetId)
        {
            brain.Context.SetUShort("TargetId", targetId);
            IsChargingMelee = true;
        }

        public void ResetAttack()
        {
            IsChargingMelee = false;
            IsAttackingMelee = false;
        }

        private Transform _lookTarget;
        public Transform LookTarget
        {
            get => _lookTarget ? _lookTarget : Target ? Target.HeadBone.transform : null;
            set => _lookTarget = value;
        }

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

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            animator.SetTrigger("IsPushed");
        }
    }
}