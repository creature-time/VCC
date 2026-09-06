
using CreatureTime.Progression;
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestPrerequisite : CtAbstractPrerequisite
    {
        [SerializeField] private CtQuestSystem questSystem;
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtQuestDef reqQuest;

        public override bool IsValid(CtPlayerProgressionDatabase playerProgressionDatabase)
        {
            var localEntity = rpgGame.LocalEntity;
            if (reqQuest.IsPrimaryQuest)
                return localEntity.PrimaryQuestProgression.IsCompleted(reqQuest.Identifier);
            return localEntity.SecondaryQuestProgression.IsCompleted(reqQuest.Identifier);
        }
    }
}