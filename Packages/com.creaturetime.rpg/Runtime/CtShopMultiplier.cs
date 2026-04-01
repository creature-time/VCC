
using UdonSharp;

namespace CreatureTime
{
    public abstract class CtShopMultiplier : UdonSharpBehaviour
    {
        public abstract bool ApplyBuyMultiplier(ref float price);
        public abstract bool ApplySellMultiplier(ref float price);
    }
}