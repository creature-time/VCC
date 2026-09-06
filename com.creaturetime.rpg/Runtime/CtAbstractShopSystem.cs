
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public abstract class CtAbstractShopSystem : CtSingleton
    {
        [SerializeField] private CtShop[] shops;

        private DataDictionary _shops = new DataDictionary();

        public override void Init()
        {
            foreach (var shop in shops)
            {
                _shops.Add(shop.Identifier, shop);
            }
        }

        public bool TryGetShop(ushort identifier, out CtShop shop) => this.TryGetDefinition(_shops, identifier, out shop);

        public abstract bool TryGetBuyPrice(ulong data, CtShop shop, out int price);
        public abstract bool TryGetSellPrice(ulong data, CtShop shop, out int price);
    }
}