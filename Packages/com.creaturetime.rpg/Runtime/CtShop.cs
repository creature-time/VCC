
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtShop : CtAbstractDefinition
    {
        [CtItem, SerializeField] private string[] shopEntries;
        [SerializeField] private CtShopMultiplier[] multipliers;

        public string[] ShopEntries => shopEntries;
        public CtShopMultiplier[] Multipliers => multipliers;

        public void ApplyBuyModifiers(ref float price, ulong data)
        {
            foreach (var multiplier in multipliers)
                multiplier.ApplyBuyMultiplier(ref price);
        }

        public void ApplySellModifiers(ref float price, ulong data)
        {
            foreach (var multiplier in multipliers)
                multiplier.ApplySellMultiplier(ref price);
        }
    }
}