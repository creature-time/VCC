
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EQuestSystemSignal
    {
        QuestsUpdated,
        QuestAccepted,
        QuestTurnIn,
        QuestCancelled
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestSystem : CtSingleton
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtGameData gameData;

        DataDictionary _actorQuestObjectiveCache = new DataDictionary();

        private DataDictionary _pickUp = new DataDictionary();
        private DataDictionary _turnIn = new DataDictionary();

        public override void Init()
        {
            rpgGame.Connect(ERpgGameSignal.LocalPlayerChanged, this, nameof(_OnLocalPlayerChanged));

            foreach (var questDef in rpgGame.GameData.QuestDefinitions)
            {
                if (!_pickUp.ContainsKey(questDef.PickUpActor.Identifier))
                    _pickUp.Add(questDef.PickUpActor.Identifier, new DataList());

                var questList = _pickUp[questDef.PickUpActor.Identifier].DataList;
                questList.Add(questDef);

                if (!_turnIn.ContainsKey(questDef.TurnInActor.Identifier))
                    _turnIn.Add(questDef.TurnInActor.Identifier, new DataList());

                questList = _turnIn[questDef.TurnInActor.Identifier].DataList;
                questList.Add(questDef);

                var objectives = questDef.Objectives;
                for (var i = 0; i < objectives.Length; i++)
                {
                    var objective = objectives[i];
                    var talkToObjective = (CtQuestTalkToObjective)objective;
                    if (talkToObjective.GetUdonTypeID() != GetUdonTypeID<CtQuestTalkToObjective>()) continue;

                    var actorId = talkToObjective.Actor.Identifier;
                    if (!_actorQuestObjectiveCache.ContainsKey(actorId))
                        _actorQuestObjectiveCache.Add(actorId, new DataDictionary());
                    var questLookup = _actorQuestObjectiveCache[actorId].DataDictionary;

                    if (!questLookup.ContainsKey(questDef.Identifier))
                        questLookup.Add(questDef.Identifier, new DataList());
                    var objectiveLookup = questLookup[questDef.Identifier].DataList;

                    objectiveLookup.Add(i);
                }
            }
        }

        private CtPlayerEntity _playerEntity;
        private CtPlayerProgressionDatabase _primaryProgression;
        private CtPlayerProgressionDatabase _secondaryProgression;

        public void _OnLocalPlayerChanged()
        {
            if (_playerEntity)
            {
                _primaryProgression.Disconnect(EPlayerDatabaseSignal.ProgressionChanged, this, nameof(_Signal_OnPrimaryProgressionChanged));
                _secondaryProgression.Disconnect(EPlayerDatabaseSignal.ProgressionChanged, this, nameof(_Signal_OnSecondaryProgressionChanged));

                _primaryProgression = null;
                _secondaryProgression = null;
            }

            _playerEntity = rpgGame.LocalEntity;
            if (_playerEntity)
            {
                _primaryProgression = rpgGame.LocalEntity.PrimaryQuestProgression;
                _secondaryProgression = rpgGame.LocalEntity.SecondaryQuestProgression;

                _Signal_OnPrimaryProgressionChanged();
                _Signal_OnSecondaryProgressionChanged();

                _primaryProgression.Connect(EPlayerDatabaseSignal.ProgressionChanged, this, nameof(_Signal_OnPrimaryProgressionChanged));
                _secondaryProgression.Connect(EPlayerDatabaseSignal.ProgressionChanged, this, nameof(_Signal_OnSecondaryProgressionChanged));
            }
        }

        public CtQuestDef[] ActivePrimaryQuests
        {
            get;
            private set;
        } = { };
        
        public CtQuestDef[] ActiveSecondaryQuests
        {
            get;
            private set;
        } = { };

        public void _Signal_OnPrimaryProgressionChanged()
        {
            var activeProgression = _primaryProgression.ActiveProgression;
            ActivePrimaryQuests = new CtQuestDef[_primaryProgression.ActiveProgression.Length];
            for (var i = 0; i < ActivePrimaryQuests.Length; i++)
            {
                var data = CtDataBlock.Deserialize(activeProgression[i]);
                var identifier = CtDataBlock.GetProgressionIdentifier(data);
                if (!gameData.TryGetQuestDef(identifier, out var questDef)) continue;
            
                ActivePrimaryQuests[i] = questDef;
            }

            this.Emit(EQuestSystemSignal.QuestsUpdated);
        }

        public void _Signal_OnSecondaryProgressionChanged()
        {
            var activeProgression = _primaryProgression.ActiveProgression;
            ActiveSecondaryQuests = new CtQuestDef[_primaryProgression.ActiveProgression.Length];
            for (var i = 0; i < ActiveSecondaryQuests.Length; i++)
            {
                var data = CtDataBlock.Deserialize(activeProgression[i]);
                var identifier = CtDataBlock.GetProgressionIdentifier(data);
                if (!gameData.TryGetQuestDef(identifier, out var questDef)) continue;
            
                ActiveSecondaryQuests[i] = questDef;
            }

            this.Emit(EQuestSystemSignal.QuestsUpdated);
        }

        public bool HasAvailableQuests(CtPlayerProgressionDatabase playerProgressionDatabase, ushort actorId, bool isPrimaryQuest)
        {
#if DEBUG_LOGS
            LogDebug($"Has available quests (progressionData={playerProgressionDatabase}, " +
                     $"actorId={actorId}, isPrimaryQuest={isPrimaryQuest}).");
#endif

            if (!_pickUp.TryGetValue(actorId, out var token))
                return false;

            var questList = token.DataList;
            for (var i = 0; i < questList.Count; i++)
            {
                var quest = (CtQuestDef)questList[i].Reference;
                if (quest.IsPrimaryQuest != isPrimaryQuest) continue;
                if (playerProgressionDatabase.IsCompleted(quest.Identifier)) continue;

                var state = GetState(playerProgressionDatabase, quest);
#if DEBUG_LOGS
                LogDebug($"Checking state (actorId={actorId}, quest={quest}, state={state}).");
#endif

                if (state == EProgressionState.Available)
                    return true;
            }

            return false;
        }

        public bool HasTurnInQuests(CtPlayerProgressionDatabase playerProgressionDatabase, ushort actorId, bool isPrimaryQuest)
        {
            if (!_turnIn.TryGetValue(actorId, out var token))
                return false;

            var questList = token.DataList;
            for (var i = 0; i < questList.Count; i++)
            {
                var quest = (CtQuestDef)questList[i].Reference;
                if (quest.IsPrimaryQuest != isPrimaryQuest) continue;

                if (GetState(playerProgressionDatabase, quest) == EProgressionState.Completed)
                    return true;
            }

            return false;
        }

        public bool IsActorQuestObjective(CtPlayerProgressionDatabase playerProgressionDatabase, ushort actorId)
        {
#if DEBUG_LOGS
            LogDebug($"Is actor quest objective (progressionData={playerProgressionDatabase}, actorId={actorId}).");
#endif

            if (!_actorQuestObjectiveCache.TryGetValue(actorId, out var questLookupToken)) return false;

#if DEBUG_LOGS
            LogDebug($"Checking if any of the quests are active (progressionData={playerProgressionDatabase}, actorId={actorId}).");
#endif

            foreach (var activeProgression in playerProgressionDatabase.ActiveProgression)
            {
                var data = CtDataBlock.Deserialize(activeProgression);
                var questId = CtDataBlock.GetProgressionIdentifier(data);
                if (!questLookupToken.DataDictionary.TryGetValue(questId, out var objectivesLookupToken)) continue;
                if (!gameData.TryGetQuestDef(questId, out var questDef)) continue;

                var objectives = questDef.Objectives;
                var objectiveIndexes = objectivesLookupToken.DataList;
                for (var i = 0; i < objectiveIndexes.Count; i++)
                {
                    var objectiveIndex = objectiveIndexes[i].Int;
                    var objective = objectives[objectiveIndex];

#if DEBUG_LOGS
                    LogDebug("Checking objective value " +
                             $"(data={data:x16}, objective={objective.Flag}, actorId={actorId}).");
#endif
                    if (!questDef.TryGetObjectiveValue(data, objective.Flag, out var value)) continue;

#if DEBUG_LOGS
                    LogDebug("Retrieved objective value " +
                             $"(data={data:x16}, objective={objective.Flag}, actorId={actorId}, value={value}).");
#endif
                    if (value == 0)
                        return true;
                }
            }

            return false;
        }

        private bool _CheckPrerequisites(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
            if (playerProgressionDatabase.IsCompleted(questDef.Identifier)) return true;

            foreach (var prerequisite in questDef.Prerequisites)
            {
                var prerequisiteResult = prerequisite.IsValid(playerProgressionDatabase);
#if DEBUG_LOGS
                LogDebug("Checking Prerequisite " +
                         $"(quest={questDef}, prerequisite={prerequisite}, result={prerequisiteResult}).");
#endif

                if (prerequisiteResult) continue;
                return false;
            }

            return true;
        }

        public EProgressionState GetState(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
#if DEBUG_LOGS
            LogDebug($"Getting quest state (progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif

            if (playerProgressionDatabase.TryGetProgressionData(questDef.Identifier, out var data))
                return CtDataBlock.GetQuestState(data);
            if (_CheckPrerequisites(playerProgressionDatabase, questDef)) return EProgressionState.Available;
            return EProgressionState.Locked;
        }

        public bool CanAccept(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
#if DEBUG_LOGS
            LogDebug($"Can accept quest (progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif
            if (playerProgressionDatabase.IsCompleted(questDef.Identifier)) return false;

            return GetState(playerProgressionDatabase, questDef) == EProgressionState.Available;
        }

        public bool TryAcceptQuest(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
#if DEBUG_LOGS
            LogDebug($"Accepting quest (progressionData={playerProgressionDatabase}, count={playerProgressionDatabase.MaxProgressiveCount}, questId={questDef.Identifier}).");
#endif

            if (GetState(playerProgressionDatabase, questDef) != EProgressionState.Available)
            {
#if DEBUG_LOGS
                LogWarning("Could not accept quest due to quest was not available " +
                           $"(progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif
                return false;
            }

            if (!playerProgressionDatabase.TryAddProgression(questDef.Identifier, out var data))
            {
#if DEBUG_LOGS
                LogCritical("Failed to add progression data to player " +
                           $"(progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif
                return false;
            }

            var state = EProgressionState.Active;
            if (questDef.Objectives.Length == 0)
                state = EProgressionState.Completed;

            data = CtDataBlock.SetQuestState(data, state);
            playerProgressionDatabase.SetProgressionData(questDef.Identifier, data);

            this.Emit(EQuestSystemSignal.QuestAccepted);

            return true;
        }

        public bool TryGetObjectiveValue(CtPlayerProgressionDatabase playerProgressionDatabase, string checkFlag, out int value)
        {
#if DEBUG_LOGS
            LogDebug($"Getting quest progression (progressionData={playerProgressionDatabase}, flag={checkFlag}).");
#endif

            foreach (var progression in playerProgressionDatabase.ActiveProgression)
            {
                var data = CtDataBlock.Deserialize(progression);
                if (!CtDataBlock.IsValid(data)) continue;
                var questId = CtDataBlock.GetProgressionIdentifier(data);
                if (!gameData.TryGetQuestDef(questId, out var questDef)) continue;

                if (questDef.TryGetObjectiveValue(data, checkFlag, out value))
                    return true;
            }

            value = -1;
            return false;
        }

        private bool _ValidateQuestCompleted(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
            foreach (var objective in questDef.Objectives)
            {
                if (objective.GetState(playerProgressionDatabase) != EProgressionState.Completed)
                    return false;
            }

            return true;
        }

        public void UpdateQuests(CtPlayerProgressionDatabase playerProgressionDatabase, DataDictionary eventData)
        {
#if DEBUG_LOGS
            LogDebug($"Updating quest progression (progressionData={playerProgressionDatabase}).");
#endif

            var activeProgression = playerProgressionDatabase.ActiveProgression;
            for (var i = 0; i < activeProgression.Length; i++)
            {
                var progression = activeProgression[i];

                var data = CtDataBlock.Deserialize(progression);
                if (!CtDataBlock.IsValid(data)) continue;

                var questId = CtDataBlock.GetProgressionIdentifier(data);
                if (!gameData.TryGetQuestDef(questId, out var questDef))
                {
#if DEBUG_LOGS
                    LogWarning("Failed to find quest def to set objective value " +
                               $"(progressionData={playerProgressionDatabase}, questId={questId}).");
#endif

                    continue;
                }

                if (CtDataBlock.GetQuestState(data) != EProgressionState.Active) continue;

                var objectives = questDef.Objectives;
                for (var j = 0; j < objectives.Length; j++)
                {
                    var objective = objectives[j];
                    if (eventData["eventType"].String == objective.EventType)
                    {
                        objective.UpdateObjective(playerProgressionDatabase, eventData);
                    }
                }

                if (_ValidateQuestCompleted(playerProgressionDatabase, questDef))
                {
                    // TODO: Can we remove this redundant second lookup?
                    data = CtDataBlock.Deserialize(activeProgression[i]);
                    data = CtDataBlock.SetQuestState(data, EProgressionState.Completed);
                    activeProgression[i] = CtDataBlock.Serialize(data);
                }

                this.Emit(EQuestSystemSignal.QuestsUpdated);
            }
        }

        public bool IsReadyToTurnIn(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
#if DEBUG_LOGS
            LogDebug($"Is ready to turn in quest (progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif

            return GetState(playerProgressionDatabase, questDef) == EProgressionState.Completed;
        }

        public bool TryCompleteQuest(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
#if DEBUG_LOGS
            LogDebug($"Completing quest (progressionData={playerProgressionDatabase}, questId={questDef.Identifier}).");
#endif

            if (!IsReadyToTurnIn(playerProgressionDatabase, questDef))
            {
#if DEBUG_LOGS
                LogWarning("Failed to turn in quest because quest was not complete yet " +
                           $"(playerDatabase={playerProgressionDatabase}, questDef={questDef}).");
#endif
                return false;
            }

            if (!playerProgressionDatabase.TryGetProgressionData(questDef.Identifier, out var index))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find quest index (playerDatabase={playerProgressionDatabase}, questDef={questDef}).");
#endif
                return false;
            }

            foreach (var reward in questDef.Rewards)
            {
                reward.GrantRewards();
            }

            if (!playerProgressionDatabase.TryCompleteProgression(questDef.Identifier))
            {
                playerProgressionDatabase.TryRemoveProgression(questDef.Identifier);
                return false;
            }

            this.Emit(EQuestSystemSignal.QuestTurnIn);

            return true;
        }

        public bool TryCancelQuest(CtPlayerProgressionDatabase playerProgressionDatabase, CtQuestDef questDef)
        {
            if (!playerProgressionDatabase.TryRemoveProgression(questDef.Identifier))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to cancel quest (playerDatabase={playerProgressionDatabase}, questDef={questDef}).");
#endif
                return false;
            }

            this.Emit(EQuestSystemSignal.QuestCancelled);
            return true;
        }

        // public bool TryGetQuestTurnIn(CtQuestDef questDef, out ushort turnInId)
        // {
        //     turnInId = CtConstants.InvalidId;
        //     var index = Array.IndexOf(gameData.QuestDefinitions, questDef);
        //     if (index == -1) return false;
        //
        //     turnInId = turnIn[index];
        //     return true;
        // }

        public bool TryGetQuestDef(ushort identifier, out CtQuestDef questDef) => 
            gameData.TryGetQuestDef(identifier, out questDef);
    }
}