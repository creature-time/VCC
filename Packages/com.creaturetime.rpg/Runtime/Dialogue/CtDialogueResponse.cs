
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EDialogueChoiceType
    {
        Default,
        Recruit
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueResponse : UdonSharpBehaviour
    {
        [SerializeField] private ushort index;
        [SerializeField] private string displayText;
        [SerializeField] private EDialogueChoiceType choiceType;
        [SerializeField] private CtResponseCondition[] conditions;
        [SerializeField] private CtResponseConsequence[] consequences;
        [SerializeField] private ushort nextId;

        public ushort Index => index;
        public string DisplayText => displayText;
        public EDialogueChoiceType ChoiceType => choiceType;
        public ushort NextId => nextId;

        public bool IsValid()
        {
            foreach (CtResponseCondition condition in conditions)
                if (!condition.IsValid())
                    return false;
            return true;
        }

        public void Execute()
        {
            foreach (CtResponseConsequence consequence in consequences)
                consequence.Execute();
        }
    }
}