
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtInventoryItemDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private Texture2D icon;
        [SerializeField] private int baseValue;
        [SerializeField] private float sellMultiplier = 0.5f;
        [SerializeField] private float buyMultiplier = 1f;

        public string DisplayName => displayName;
        public Texture2D Icon => icon;
        public int BaseValue => baseValue;
        // public float SellMultiplier => sellMultiplier;
        // public float BuyMultiplier => buyMultiplier;

        public virtual void ApplyBuyModifiers(ref float price, ulong data)
        {
            price *= buyMultiplier;
        }

        public virtual void ApplySellModifiers(ref float price, ulong data)
        {
            price *= sellMultiplier;
        }
    }
}
