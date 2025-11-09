
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtProfessionConsequence : CtResponseConsequence
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtProfessionDef professionDef;

        public override void Execute()
        {
            rpgGame.RequestProfession(professionDef);
        }
    }
}