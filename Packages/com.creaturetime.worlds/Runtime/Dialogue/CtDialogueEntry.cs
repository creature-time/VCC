
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EDialogueEntrySignal
    {
        Enter,
        Exit
    }

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

        private void Start()
        {
            foreach (var trigger in onEnterTriggers)
                this.Connect(EDialogueEntrySignal.Enter, trigger.target, trigger.eventTrigger);
            foreach (var trigger in onExitTriggers)
                this.Connect(EDialogueEntrySignal.Exit, trigger.target, trigger.eventTrigger);
        }

        public void OnEnterTriggers()
        {
            SetArgs.Add(actor);
            this.Emit(EDialogueEntrySignal.Enter);
            // foreach (var trigger in onEnterTriggers)
            // {
            //     trigger.target.SetProgramVariable("actor", actor);
            //     trigger.target.SendCustomEvent(trigger.eventTrigger);
            // }
        }

        public void OnExitTriggers()
        {
            SetArgs.Add(actor);
            this.Emit(EDialogueEntrySignal.Exit);
            // foreach (var trigger in onExitTriggers)
            // {
            //     trigger.target.SetProgramVariable("actor", actor);
            //     trigger.target.SendCustomEvent(trigger.eventTrigger);
            // }
        }
    }
}