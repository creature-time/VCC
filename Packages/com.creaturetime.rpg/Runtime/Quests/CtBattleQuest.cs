
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleQuest : CtAbstractQuest
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtNpcDef[] npcDefinitions;

        public override void Execute(CtParty party)
        {
            rpgGame.StartBattle(party, npcDefinitions);
        }
    }
}