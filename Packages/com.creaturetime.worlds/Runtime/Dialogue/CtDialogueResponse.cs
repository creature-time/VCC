
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EDialogueChoiceType
    {
        Default,
        Recruit,
        Shop,
        Trainer,
        QuestAccept,
        QuestTurnIn
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueResponse : CtBlackboard
    {
        [SerializeField] private string displayText;
        [SerializeField] private EDialogueChoiceType choiceType;
        [SerializeField] private CtResponseCondition[] conditions;
        [SerializeField] private CtResponseConsequence[] consequences;
        [SerializeField] private ushort nextId = CtConstants.InvalidId;

        public string DisplayText => displayText;
        public EDialogueChoiceType ChoiceType => choiceType;
        public CtResponseCondition[] Conditions => conditions;
        public CtResponseConsequence[] Consequences => consequences;
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