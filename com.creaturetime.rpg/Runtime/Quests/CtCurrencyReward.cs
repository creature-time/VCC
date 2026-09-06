
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtCurrencyReward : CtAbstractQuestReward
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private int currencyReward;

        public int CurrencyReward => currencyReward;

        public override void GrantRewards()
        {
            rpgGame.LocalEntity.PlayerWallet.Add(currencyReward);
        }
    }
}
