
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.Progression
{
    public abstract class CtAbstractObjective : CtLoggerUdonScript
    {
        [SerializeField] private string flag;

        public abstract string EventType { get; }

        public string Flag => flag;

        public abstract EProgressionState GetState(CtPlayerProgressionDatabase playerProgressionDatabase);

        public abstract void UpdateObjective(CtPlayerProgressionDatabase progressionData, DataDictionary eventData);
    }
}