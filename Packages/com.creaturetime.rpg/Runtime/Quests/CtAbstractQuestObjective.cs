
using CreatureTime.Progression;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractQuestObjective : CtAbstractObjective
    {
        [SerializeField] private CtQuestDef quest;

        public CtQuestDef Quest => quest;
        public abstract string GetFormattedDescription(CtPlayerProgressionDatabase progressionData);
    }
}