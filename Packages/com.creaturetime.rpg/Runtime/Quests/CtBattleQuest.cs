
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleQuest : CtAbstractQuest
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtSquadCategory[] squadCategories;

        public override void Execute(CtParty party)
        {
            var category = squadCategories[CtRandomizer.GetIntValue(0, squadCategories.Length)];
            var squadDefs = category.SquadDefs;
            var squadDef = squadDefs[CtRandomizer.GetIntValue(0, squadDefs.Length)];
            rpgGame.StartBattle(party, squadDef);
        }
    }
}