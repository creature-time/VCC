
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestFlagObjective : CtAbstractQuestObjective
    {
        private const string EVENT_TYPE = "Flag";

        public override string EventType => EVENT_TYPE;

        [SerializeField] private string description;

        public override string GetFormattedDescription(CtPlayerProgressionDatabase progressionData)
        {
            if (!progressionData.TryGetProgressionData(Quest.Identifier, out var data))
                return "...";

            var value = CtDataBlock.GetQuestObjective(Array.IndexOf(Quest.Objectives, this), data);

            return $"{value}/1 Set flag {Flag}.";
        }

        public override EProgressionState GetState(CtPlayerProgressionDatabase playerProgressionDatabase)
        {
            if (!playerProgressionDatabase.TryGetProgressionData(Quest.Identifier, out var data))
            {
#if DEBUG_LOGS
                LogDebug("Failed to find quest when attempting to check objective status " +
                         $"(objectiveId={Array.IndexOf(Quest.Objectives, this)}, playerDatabase={playerProgressionDatabase})");
#endif
                return EProgressionState.Failed;
            }

            var objectiveIndex = Array.IndexOf(Quest.Objectives, this);
            var objectiveValue = CtDataBlock.GetQuestObjective(objectiveIndex, data);

#if DEBUG_LOGS
            LogDebug($"Checking objective target values (objectiveValue={objectiveValue})");
#endif

            if (objectiveValue != 0)
                return EProgressionState.Completed;

            return EProgressionState.Active;
        }

        public override void UpdateObjective(CtPlayerProgressionDatabase progressionData, DataDictionary eventData)
        {
#if DEBUG_LOGS
            LogDebug($"Updating flag objective (eventType={eventData["eventType"].String}, " +
                     $"objectiveId={Array.IndexOf(Quest.Objectives, this)}, playerDatabase={progressionData})");
#endif

            if (eventData["flag"].String != Flag)
                return;

            var value = eventData["value"].Boolean;
#if DEBUG_LOGS
            LogDebug($"Updating objective (objectiveId={Array.IndexOf(Quest.Objectives, this)}, playerDatabase={progressionData}, value={value})");
#endif

            if (!progressionData.TryGetProgressionData(Quest.Identifier, out var data))
            {
#if DEBUG_LOGS
                LogCritical("Failed to find quest when attempting to update objective " +
                            $"(objectiveId={Array.IndexOf(Quest.Objectives, this)}, playerDatabase={progressionData}, value={value})");
#endif
                return;
            }

            var objectiveValue = CtDataBlock.GetQuestObjective(Array.IndexOf(Quest.Objectives, this), data);
            if (objectiveValue != 0) return;

            data = CtDataBlock.UpdateQuestObjective(Array.IndexOf(Quest.Objectives, this), 1, data);
            progressionData.SetProgressionData(Quest.Identifier, data);
        }

        public static DataDictionary CreateEventData(string flag, bool value)
        {
            var eventData = new DataDictionary();
            eventData.Add("eventType", EVENT_TYPE);
            eventData.Add("flag", flag);
            eventData.Add("value", value);
            return eventData;
        }
    }
}
