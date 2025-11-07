
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
        [SerializeField] private CtDialogueTrigger[] onEnterTriggers;
        [SerializeField] private CtDialogueTrigger[] onExitTriggers;

        public ushort Identifier => identifier;
        public ushort ConversationId => conversationId;
        public CtDialogueActor Actor => actor;
        public CtDialogueActor Conversant => conversant;
        public string DialogueText => dialogueText;
        public CtDialogueResponse[] Responses => responses;
        public ushort NextId => nextId;

        public void OnEnterTriggers()
        {
            Debug.Log($"OnEnterTriggers {gameObject}");
            foreach (var trigger in onEnterTriggers)
                trigger.target.SendCustomEvent(trigger.eventTrigger);
        }

        public void OnExitTriggers()
        {
            Debug.Log($"OnExitTriggers {gameObject}");
            foreach (var trigger in onExitTriggers)
                trigger.target.SendCustomEvent(trigger.eventTrigger);
        }
    }
}