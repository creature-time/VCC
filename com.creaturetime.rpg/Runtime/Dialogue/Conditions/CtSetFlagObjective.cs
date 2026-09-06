
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Dialogue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSetFlagObjective : CtResponseConsequence
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtQuestSystem questSystem;

        [SerializeField] private string flag;
        [SerializeField] private int value;

        public override void Execute()
        {
            var localPlayer = rpgGame.LocalEntity;
            questSystem.UpdateQuests(localPlayer.PrimaryQuestProgression, CtQuestFlagObjective.CreateEventData(flag, value != 0));
            questSystem.UpdateQuests(localPlayer.SecondaryQuestProgression, CtQuestFlagObjective.CreateEventData(flag, value != 0));
        }
    }
}