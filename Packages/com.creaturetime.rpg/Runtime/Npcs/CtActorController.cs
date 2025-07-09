
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

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
        private const int CharacterFlagsHalt = 1 << 2;

        [Header("Actor Controller")]
        [SerializeField] private CtDialogueActor dialogueActor;
        [SerializeField] private string subTitle;
        [SerializeField] private AudioClip babbleClip;

        public override string DisplayName => dialogueActor.ActorName;
        public string SubTitle => subTitle;
        public AudioClip BabbleClip => babbleClip;

        public bool HasSequence
        {
            get => (Flags & CharacterFlagsHasSequence) != 0;
            set
            {
                if (value)
                    Flags |= CharacterFlagsHasSequence;
                else
                    Flags &= ~CharacterFlagsHasSequence;
                this.Emit(ENpcActorControllerSignal.SequenceChanged);
            }
        }

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

        public bool IsHalted
        {
            get => (Flags & CharacterFlagsHalt) != 0;
            set
            {
                if (value)
                {
                    Flags |= CharacterFlagsHalt;
                }
                else
                {
                    Flags &= ~CharacterFlagsHalt;
                }
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            animator.SetTrigger("IsPushed");
        }
    }
}