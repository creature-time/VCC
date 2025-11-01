
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueEntry : CtAbstractSignal
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private ushort conversationId = CtConstants.InvalidId;
        [SerializeField] private ushort nextId = CtConstants.InvalidId;
 
        [SerializeField] private CtDialogueActor actor;
        [SerializeField] private CtDialogueActor conversant;
        [SerializeField] private string dialogueText;
        [SerializeField] private CtDialogueResponse[] responses;

        public ushort Identifier => identifier;
        public ushort ConversationId => conversationId;
        public CtDialogueActor Actor => actor;
        public CtDialogueActor Conversant => conversant;
        public string DialogueText => dialogueText;
        public CtDialogueResponse[] Responses => responses;
        public ushort NextId => nextId;
    }
}