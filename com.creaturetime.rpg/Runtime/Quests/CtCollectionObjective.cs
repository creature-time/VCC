
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtCollectionObjective : CtAbstractQuestObjective
    {
        private const string EVENT_TYPE = "Collection";

        public override string EventType => EVENT_TYPE;

        [SerializeField] private CtGameData gameData;
        [SerializeField] private ushort itemId;
        [SerializeField] private int count;

        public override string GetFormattedDescription(CtPlayerProgressionDatabase progressionData)
        {
            if (!progressionData.TryGetProgressionData(Quest.Identifier, out var data))
                return "...";

            var value = CtDataBlock.GetQuestObjective(Array.IndexOf(Quest.Objectives, this), data);

            // var itemDef = gameData.GetItemDef();
            // var itemName = itemDef.DisplayName;
            var itemName = "TODO items!";

            return $"{value}/{count} Collect {count} {itemName}.";
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
            LogDebug($"Checking objective target values (objectiveValue={objectiveValue}, itemCount={count})");
#endif

            if (objectiveValue >= count)
                return EProgressionState.Completed;

            return EProgressionState.Active;
        }

        public override void UpdateObjective(CtPlayerProgressionDatabase progressionData, DataDictionary eventData)
        {
#if DEBUG_LOGS
            LogDebug($"Updating collection objective (eventType={eventData["eventType"].String}, " +
                     $"objectiveId={Array.IndexOf(Quest.Objectives, this)}, playerDatabase={progressionData})");
#endif

            if (eventData["itemId"].UShort != itemId) return;

            var value = eventData["value"].Int;
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
            if (objectiveValue != count) return;

            objectiveValue = Mathf.Min(objectiveValue + value, count);

            data = CtDataBlock.UpdateQuestObjective(Array.IndexOf(Quest.Objectives, this), objectiveValue, data);
            progressionData.SetProgressionData(Quest.Identifier, data);
        }

        public static DataDictionary CreateEventData(ushort itemId, int value)
        {
            var eventData = new DataDictionary();
            eventData.Add("eventType", EVENT_TYPE);
            eventData.Add("itemId", itemId);
            eventData.Add("value", value);
            return eventData;
        }
    }
}
