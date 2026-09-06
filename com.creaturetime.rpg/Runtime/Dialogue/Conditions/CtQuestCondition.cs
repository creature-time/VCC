
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Dialogue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestCondition : CtResponseCondition
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtQuestSystem questSystem;

        [SerializeField] private CtQuestDef quest;
        [SerializeField] private EQuestDialogueAction action;

        public override bool IsValid()
        {
            var primaryPlayerDatabase = rpgGame.LocalEntity.PrimaryQuestProgression;
            var secondaryPlayerDatabase = rpgGame.LocalEntity.SecondaryQuestProgression;
            switch (action)
            {
                case EQuestDialogueAction.PickUp:
                    return questSystem.CanAccept(quest.IsPrimaryQuest ? primaryPlayerDatabase : secondaryPlayerDatabase, quest);
                case EQuestDialogueAction.TurnIn:
                    return questSystem.IsReadyToTurnIn(quest.IsPrimaryQuest ? primaryPlayerDatabase : secondaryPlayerDatabase, quest);
            }

            return false;
        }
    }
}