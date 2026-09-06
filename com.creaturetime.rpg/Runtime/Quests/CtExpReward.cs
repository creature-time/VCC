
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtExpReward : CtAbstractQuestReward
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private int expReward;

        public int ExpReward => expReward;

        public override void GrantRewards()
        {
            rpgGame.LocalEntity.PlayerDef.Exp += expReward;
        }
    }
}
