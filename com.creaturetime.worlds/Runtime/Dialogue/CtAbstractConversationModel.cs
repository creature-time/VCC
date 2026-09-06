
using UnityEngine;

namespace CreatureTime
{
    public enum EConversationState
    {
        Processing,
        Completed
    }

    public abstract class CtAbstractConversationModel : CtAbstractSignal
    {
        [SerializeField] protected CtDialogueDatabase dialogueDatabase;

        protected EConversationState State = EConversationState.Completed;

        public abstract ushort Identifier { get; set; }

        public bool IsComplete => State != EConversationState.Processing;
        public void SetComplete() => State = EConversationState.Completed;

        public abstract void UpdateConversation();
    }
}