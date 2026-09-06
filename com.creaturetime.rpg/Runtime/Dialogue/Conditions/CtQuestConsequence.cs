
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    public enum EQuestDialogueAction
    {
        PickUp,
        TurnIn
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestConsequence : CtResponseConsequence
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtQuestSystem questSystem;

        [SerializeField] private CtQuestDef quest;
        [SerializeField] private EQuestDialogueAction action;

        public override void Execute()
        {
            var primaryPlayerDatabase = rpgGame.LocalEntity.PrimaryQuestProgression;
            var secondaryPlayerDatabase = rpgGame.LocalEntity.SecondaryQuestProgression;
            switch (action)
            {
                case EQuestDialogueAction.PickUp:
                    if (!questSystem.CanAccept(quest.IsPrimaryQuest ? primaryPlayerDatabase : secondaryPlayerDatabase, quest))
                    {
#if DEBUG_LOGS
                        LogCritical($"The quest that should be available is no longer available (questId={quest}).");
#endif
                        return;
                    }

                    questSystem.TryAcceptQuest(quest.IsPrimaryQuest ? primaryPlayerDatabase : secondaryPlayerDatabase, quest);
                    break;
                case EQuestDialogueAction.TurnIn:
                    questSystem.TryCompleteQuest(quest.IsPrimaryQuest ? primaryPlayerDatabase : secondaryPlayerDatabase, quest);
                    break;
            }
        }
    }
}