
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtKillObjective : CtAbstractQuestObjective
    {
        private const string EVENT_TYPE = "Kill";

        public override string EventType => EVENT_TYPE;

        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtNpcDef targetNpc;
        [SerializeField] private int count;

        public override string GetFormattedDescription(CtPlayerProgressionDatabase progressionData)
        {
            if (!progressionData.TryGetProgressionData(Quest.Identifier, out var data))
                return "...";

            var value = CtDataBlock.GetQuestObjective(Array.IndexOf(Quest.Objectives, this), data);
            return $"{value}/{count} Kill {count} {targetNpc.DisplayName}.";
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
            LogDebug($"Checking objective target values (objectiveValue={objectiveValue}, targetCount={count})");
#endif


            if (objectiveValue == count)
                return EProgressionState.Completed;

            return EProgressionState.Active;
        }

        public override void UpdateObjective(CtPlayerProgressionDatabase progressionData, DataDictionary eventData)
        {
            var value = eventData["value"].Int;
            var killId = eventData["killId"].UShort;

#if DEBUG_LOGS
            LogDebug($"Updating kill objective (eventType={eventData["eventType"].String}, " +
                     $"objectiveId={Array.IndexOf(Quest.Objectives, this)}, targetNpc={targetNpc}, value={value}, killId={killId}).");
#endif

            if (targetNpc.Identifier != killId) return;

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
            if (objectiveValue == count) return;

            // TODO: Cache the status of the objective in the data block.
            //       Please refer to IsComplete for cleanup.

            objectiveValue = Mathf.Min(objectiveValue + value, count);
#if DEBUG_LOGS
            LogDebug($"Updated kill objective value (objectiveId={Array.IndexOf(Quest.Objectives, this)}, value={value})");
#endif

            data = CtDataBlock.UpdateQuestObjective(Array.IndexOf(Quest.Objectives, this), objectiveValue, data);
            progressionData.SetProgressionData(Quest.Identifier, data);
        }

        public static DataDictionary CreateEventData(ushort npcId, int value)
        {
            var eventData = new DataDictionary();
            eventData.Add("eventType", EVENT_TYPE);
            eventData.Add("killId", npcId);
            eventData.Add("value", value);
            return eventData;
        }
    }
}
