
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtArmorDef : CtInventoryItemDef
    {
        // Heavy armor: 25-80 armor
        // Medium armor: 10-70 armor
        // Light armor: 5-60 armor
        [SerializeField] private int armorRating = 0;
        [SerializeField] private EArmorSlot armorSlot = EArmorSlot.None;
        // Heavy bonus: Armor +20, Armor +20, Armor +20, Armor +20, Armor +20
        // Medium bonus: n/a, Energy +5, n/a, Energy Recovery, n/a
        // Light bonus: n/a, Energy +5, Energy +5, Energy Recover, Energy Recover
        // public EBonusType bonusType = EBonusType.None;
        // public int bonusValue = -1;

        public int ArmorRating => armorRating;
        public EArmorSlot ArmorSlot => armorSlot;

        public static int GetArmorIndex(EArmorSlot armorSlot)
        {
            switch (armorSlot)
            {
                case EArmorSlot.Head:
                    return 0;
                case EArmorSlot.Chest:
                    return 1;
                case EArmorSlot.Hands:
                    return 2;
                case EArmorSlot.Legs:
                    return 3;
                case EArmorSlot.Feet:
                    return 4;
                default:
                    CtLogger.LogError("Armor Definition", $"Unknown armor slot (armorSlot={armorSlot}).");
                    return -1;
            }
        }

        public static EArmorSlot RollArmorHit()
        {
            double roll = CtRandomizer.GetDoubleValue(100.0);
            if (roll < 12.5)
                return EArmorSlot.Head;
            if (roll < 25.0)
                return EArmorSlot.Hands;
            if (roll < 37.5)
                return EArmorSlot.Feet;
            if (roll < 75.0)
                return EArmorSlot.Legs;
            return EArmorSlot.Chest;
        }

        public void GetFormattedStats(ulong dataBlock, ref string equipmentName, ref string stats, ref EItemRarity rarity, ref int requirement)
        {
            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;

            // rarity = CTDataMangle.GetWeaponRarity(dataBlock);
            // switch (rarity)
            // {
            //     case EItemRarity.None:
            //         color = RarityDefaultColor;
            //         break;
            //     case EItemRarity.Common:
            //         color = RarityCommonColor;
            //         break;
            //     case EItemRarity.Magical:
            //         color = RarityMagicalColor;
            //         break;
            //     case EItemRarity.Uncommon:
            //         color = RarityUncommonColor;
            //         break;
            //     case EItemRarity.Rare:
            //         color = RarityRareColor;
            //         break;
            //     default:
            //         CTLogger.LogCritical($"Item rarity not supported (rarity={rarity}).");
            //         break;
            // }

            // requirement = CTDataMangle.GetWeaponRequirement(dataBlock);

            equipmentName = $"<color={color}>{DisplayName}</color>";

            stats = string.Empty;

            // if (prefix != EWeaponPrefix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponPrefix), prefix)}</color>\n";

            // string damageTypeName = "???";
            // switch (damageType)
            // {
            //     case EWeaponDamageType.Slashing:
            //         damageTypeName = "Slashing";
            //         break;
            //     case EWeaponDamageType.Blunt:
            //         damageTypeName = "Blunt";
            //         break;
            //     case EWeaponDamageType.Piercing:
            //         damageTypeName = "Piercing";
            //         break;
            //     case EWeaponDamageType.Earth:
            //         damageTypeName = "Earth";
            //         break;
            //     case EWeaponDamageType.Fire:
            //         damageTypeName = "Fire";
            //         break;
            //     case EWeaponDamageType.Air:
            //         damageTypeName = "Air";
            //         break;
            //     case EWeaponDamageType.Water:
            //         damageTypeName = "Water";
            //         break;
            //     case EWeaponDamageType.Smiting:
            //         damageTypeName = "Holy";
            //         break;
            //     default:
            //         CTLogger.LogCritical($"Not supported weapon damage type (damageType={damageType}).");
            //         break;
            // }

            // stats += $"{damageTypeName} dmg {damageMin}-{damageMax} (Requirement {requirement})\n";

            // if (suffix != EWeaponSuffix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponSuffix), suffix)}</color>\n";

            stats = stats.Trim();
        }
    }
}