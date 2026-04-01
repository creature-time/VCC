
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestTalkToObjective : CtQuestFlagObjective
    {
        [SerializeField] private CtDialogueActor actor;
        public CtDialogueActor Actor => actor;

        public override string GetFormattedDescription(CtPlayerProgressionDatabase progressionData)
        {
            if (!progressionData.TryGetProgressionData(Quest.Identifier, out var data))
                return "...";

            var value = CtDataBlock.GetQuestObjective(Array.IndexOf(Quest.Objectives, this), data);

            return $"{value}/1 Speak with {Actor.ActorName}.";
        }
    }
}
