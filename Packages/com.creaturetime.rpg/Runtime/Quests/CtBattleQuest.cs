
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleQuest : CtAbstractQuest
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtSquadDef squadDef;

        public override void Execute(CtParty party)
        {
            rpgGame.StartBattle(party, squadDef);
        }
    }
}