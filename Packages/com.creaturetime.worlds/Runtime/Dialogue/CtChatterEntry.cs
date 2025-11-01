
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChatterEntry : CtAbstractSignal
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private ushort chatterId = CtConstants.InvalidId;
        [SerializeField] private ushort nextId = CtConstants.InvalidId;
 
        [SerializeField] private CtDialogueActor actor;
        [SerializeField] private CtDialogueActor conversant;
        [SerializeField] private string dialogueText;
        [SerializeField, Range(0, 30)] private float duration;

        public ushort Identifier => identifier;
        public ushort ChatterId => chatterId;
        public ushort NextId => nextId;
        public CtDialogueActor Actor => actor;
        public CtDialogueActor Conversant => conversant;
        public string DialogueText => dialogueText;
        public float Duration => duration;
    }
}