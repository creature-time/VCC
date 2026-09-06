
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtShopSystem : CtAbstractShopSystem
    {
        [SerializeField] private CtGameData gameData;

        public override bool TryGetBuyPrice(ulong data, CtShop shop, out int price)
        {
            price = int.MaxValue;

            var dataType = CtDataBlock.GetDataType(data);
            CtInventoryItemDef inventoryItem;
            switch (dataType)
            {
                case EDataType.Weapon:
                    var weaponId = CtDataBlock.GetWeaponIdentifier(data);
                    inventoryItem = gameData.GetWeaponDef(weaponId);
                    break;
                case EDataType.Equipment:
                    var armorSetId = CtDataBlock.GetEquipmentIdentifier(data);
                    var armorSetDef = gameData.GetArmorDef(armorSetId);
                    var armorSlot = CtDataBlock.GetEquipmentSlot(data);
                    inventoryItem = armorSetDef.GetArmorSlot(armorSlot);
                    break;
                case EDataType.OffHand:
                    var offHandId = CtDataBlock.GetWeaponIdentifier(data);
                    inventoryItem = gameData.GetOffHandDef(offHandId);
                    break;
                default:
#if DEBUG_LOGS
                    LogCritical($"Invalid data type (dataType={dataType}).");
#endif
                    return false;
            }

            var result = (float)inventoryItem.BaseValue;

            inventoryItem.ApplyBuyModifiers(ref result, data);
            shop.ApplyBuyModifiers(ref result, data);
            // _ApplyPlayerModifiers(ref result, player?);

            price = Mathf.RoundToInt(result);
            return true;
        }

        public override bool TryGetSellPrice(ulong data, CtShop shop, out int price)
        {
            price = 0;

            var dataType = CtDataBlock.GetDataType(data);
            CtInventoryItemDef inventoryItem;
            switch (dataType)
            {
                case EDataType.Weapon:
                    var weaponId = CtDataBlock.GetWeaponIdentifier(data);
                    inventoryItem = gameData.GetWeaponDef(weaponId);
                    break;
                case EDataType.Equipment:
                    var armorSetId = CtDataBlock.GetEquipmentIdentifier(data);
                    var armorSetDef = gameData.GetArmorDef(armorSetId);
                    var armorSlot = CtDataBlock.GetEquipmentSlot(data);
                    inventoryItem = armorSetDef.GetArmorSlot(armorSlot);
                    break;
                case EDataType.OffHand:
                    var offHandId = CtDataBlock.GetWeaponIdentifier(data);
                    inventoryItem = gameData.GetOffHandDef(offHandId);
                    break;
                default:
#if DEBUG_LOGS
                    LogCritical($"Invalid data type (dataType={dataType}).");
#endif
                    return false;
            }

            var result = (float)inventoryItem.BaseValue;

            inventoryItem.ApplySellModifiers(ref result, data);
            shop.ApplySellModifiers(ref result, data);
            // _ApplyPlayerModifiers(ref result, player?);

            price = Mathf.RoundToInt(result);
            return true;
        }
    }
}