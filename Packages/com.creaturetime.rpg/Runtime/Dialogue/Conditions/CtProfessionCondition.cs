
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtProfessionCondition : CtResponseCondition
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtProfessionDef professionDef;
        [SerializeField] private bool isProfession;

        public override bool IsValid()
        {
            var attributeData = rpgGame.PlayerManager.LocalPlayerDef.AttributeData;
            if (!CtDataBlock.IsValid(attributeData))
            {
                LogCritical("Invalid attribute data.");
                return false;
            }

            var professionId = CtDataBlock.GetProfession(attributeData);
            return isProfession == (professionId == professionDef.Identifier);
        }
    }
}