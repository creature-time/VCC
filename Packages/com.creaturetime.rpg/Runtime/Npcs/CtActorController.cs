
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    public enum ENpcActorControllerSignal
    {
        DialogueChanged = ENpcControllerSignal.Extensions,
        SequenceChanged,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtActorController : CtNpcController
    {
        private const int CharacterFlagsHasDialogue = 1 << 0;
        private const int CharacterFlagsHasSequence = 1 << 1;

        [Header("Actor Controller")]
        [SerializeField] private CtDialogueActor dialogueActor;
        [SerializeField] private string subTitle;
        [SerializeField] private AudioClip blipClip;

        public override string DisplayName => dialogueActor.ActorName;
        public string SubTitle => subTitle;
        public AudioClip BlipClip => blipClip;

        public bool HasDialogue
        {
            get => (Flags & CharacterFlagsHasDialogue) != 0;
            set
            {
                if (value)
                    Flags |= CharacterFlagsHasDialogue;
                else
                    Flags &= ~CharacterFlagsHasDialogue;
                this.Emit(ENpcActorControllerSignal.DialogueChanged);
            }
        }

        protected override void OnExpressionChanged()
        {
            switch (Expression)
            {
                case ENpcExpression.Neutral:
                    // _expression = 0;
                    break;
                case ENpcExpression.Happy:
                    // _expression = 1;
                    break;
                case ENpcExpression.Sad:
                    // _expression = 2;
                    break;
                case ENpcExpression.Angry:
                    break;
                case ENpcExpression.Fearful:
                    break;
                case ENpcExpression.Surprised:
                    break;
                case ENpcExpression.Disgusted:
                    break;
                case ENpcExpression.Flirt:
                    break;
                default:
#if DEBUG_LOGS
                    LogWarning($"Unknown expression (expression={Expression}).");
#endif
                    break;
            }
        }

        public bool IsTalking
        {
            set => animator.SetBool("Babble", value);
        }

        [UdonSynced, FieldChangeCallback(nameof(HoldPathfindingTimerCallback))] private float _holdPathfindingTimer;

        public float HoldPathfindingTimerCallback
        {
            get => _holdPathfindingTimer;
            set
            {
                _holdPathfindingTimer = value;
#if DEBUG_LOGS
                LogDebug($"Hold Pathfinding Timer updated (_holdPathfindingTimer={_holdPathfindingTimer}).");
#endif
                if (_holdPathfindingTimer <= 0f)
                {
                    Brain.Context.SetBool("Pathfinding/Pause", false);
                }
                else
                {
                    Brain.Context.SetBool("Pathfinding/Pause", true);
                }
            }
        }

        private float HoldPathfindingTimer
        {
            get => HoldPathfindingTimerCallback;
            set
            {
                if (!Networking.IsOwner(gameObject)) return;

                HoldPathfindingTimerCallback = value;
                RequestSerialization();
            }
        }

        public void Event_HoldPathfinding()
        {
            HoldPathfindingTimer = 5;
        }

        private void _HandleTurning()
        {
            if (LookTarget && rpgGame.LocalEntity && LookTarget == rpgGame.LocalEntity.HeadTransform)
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Event_HoldPathfinding));
            }
            else
            {
                if (HoldPathfindingTimer > 0)
                    HoldPathfindingTimer -= Time.deltaTime;
            }
        }

        protected override void Update()
        {
            _HandleTurning();

            base.Update();
        }

//         [UdonSynced, FieldChangeCallback(nameof(HoldPathfindingTimerCallback))] private float _holdPathfindingTimer;
//
//         public float HoldPathfindingTimerCallback
//         {
//             get => _holdPathfindingTimer;
//             set
//             {
//                 _holdPathfindingTimer = value;
// #if DEBUG_LOGS
//                 LogDebug($"Hold Pathfinding Timer updated (_holdPathfindingTimer={_holdPathfindingTimer}).");
// #endif
//                 if (_holdPathfindingTimer <= 0f)
//                 {
//                     _hasDefaultRotation = false;
//                     Brain.Context.SetBool("Pathfinding/Pause", false);
//                 }
//                 else
//                 {
//                     Brain.Context.SetBool("Pathfinding/Pause", true);
//                 }
//             }
//         }
//
//         private float HoldPathfindingTimer
//         {
//             get => HoldPathfindingTimerCallback;
//             set
//             {
//                 if (!Networking.IsOwner(gameObject)) return;
//
//                 HoldPathfindingTimerCallback = value;
//                 RequestSerialization();
//             }
//         }
//
//         public void Event_HoldPathfinding()
//         {
//             HoldPathfindingTimer = 5;
//         }
//
//         private Quaternion _defaultBodyRotation;
//         private bool _hasDefaultRotation;
//         private Quaternion _previousTurnRotation = Quaternion.identity;
//         [SerializeField] private float turnSpeedToSpeak = 10f;
//
//         private void _HandleTurning()
//         {
//             var targetBodyRotation = RootTransform.rotation;
//
//             if (LookTarget && rpgGame.LocalEntity && LookTarget == rpgGame.LocalEntity.HeadTransform)
//             {
//                 SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Event_HoldPathfinding));
//
//                 if (!_hasDefaultRotation)
//                 {
//                     _defaultBodyRotation = RootTransform.rotation;
//                     _hasDefaultRotation = true;
//                 }
//
//                 var direction = rpgGame.LocalEntity.RootTransform.position - RootTransform.position;
//                 direction.y = 0;
//                 targetBodyRotation = Quaternion.LookRotation(direction, Vector3.up);
//             }
//             else
//             {
//                 if (HoldPathfindingTimer > 0)
//                     HoldPathfindingTimer -= Time.deltaTime;
//             }
//
//             float t = 1.0f - Mathf.Exp(-turnSpeedToSpeak * Time.deltaTime);
//             RootTransform.rotation = Quaternion.Slerp(targetBodyRotation, RootTransform.rotation, t);
//
//             var angle = Vector3.SignedAngle(RootTransform.forward, _previousTurnRotation * Vector3.forward, transform.up);
//             animator.SetFloat("Idle/Turn", angle, .2f, Time.deltaTime);
//             _previousTurnRotation = RootTransform.rotation;
//         }
//
//         protected override void Update()
//         {
//             _HandleTurning();
//
//             base.Update();
//         }
    }
}