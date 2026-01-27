
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtArmorSlotDef : CtInventoryItemDef
    {
        [SerializeField] private CtArmorSetDef armorSet;

        // Heavy armor: 25-80 armor
        // Medium armor: 10-70 armor
        // Light armor: 5-60 armor
        [SerializeField] private int armorRating = 0;

        // Heavy bonus: Armor +20, Armor +20, Armor +20, Armor +20, Armor +20
        // Medium bonus: n/a, Energy +5, n/a, Energy Recovery, n/a
        // Light bonus: n/a, Energy +5, Energy +5, Energy Recover, Energy Recover
        [SerializeField] private int armorRatingBonus;
        [SerializeField] private EArmorRatingBonusType armorRatingBonusType;
        [SerializeField] private int energyRegenerationBonus;
        [SerializeField] private int energyIncreaseBonus;
        [SerializeField] private int healthIncreaseBonus;

        public CtArmorSetDef ArmorSet => armorSet;
        public int ArmorRating => armorRating;

        public int ArmorRatingBonus => armorRatingBonus;
        public EArmorRatingBonusType ArmorRatingBonusType => armorRatingBonusType;
        public int EnergyRegenerationBonus => energyRegenerationBonus;
        public int EnergyIncreaseBonus => energyIncreaseBonus;
        public int HealthIncreaseBonus => healthIncreaseBonus;

        public bool TryGetFormattedStats(out string displayName, out string stats)
        {
            displayName = string.Empty;
            stats = string.Empty;

            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;
            switch (ArmorSet.Rarity)
            {
                case EItemRarity.None:
                    // Do nothing...
                    break;
                case EItemRarity.Common:
                    color = RarityCommonColor;
                    break;
                case EItemRarity.Magical:
                    color = RarityMagicalColor;
                    break;
                case EItemRarity.Uncommon:
                    color = RarityUncommonColor;
                    break;
                case EItemRarity.Rare:
                    color = RarityRareColor;
                    break;
                default:
                    LogCritical($"Rarity not implemented (rarity={ArmorSet.Rarity}).");
                    return false;
            }

            displayName = $"<color={color}>{DisplayName}</color>";

            DataList statsArray = new DataList();

            if (ArmorRatingBonus > 0)
            {
                var armorRatingBonusType = string.Empty;
                switch (ArmorRatingBonusType)
                {
                    case EArmorRatingBonusType.None:
#if DEBUG_LOGS
                        LogCritical("Cannot have None type when armor rating is specified.");
#endif
                        return false;
                    case EArmorRatingBonusType.PhysicalDamage:
                        armorRatingBonusType = "Physical Damage";
                        break;
                    case EArmorRatingBonusType.ElementalDamage:
                        armorRatingBonusType = "Elemental Damage";
                        break;
                    default:
#if DEBUG_LOGS
                        LogWarning($"Unknown armor rating type (armorRatingBonusType={armorRatingBonusType}).");
#endif
                        return false;
                }

                statsArray.Add($"+{ArmorRatingBonus} {armorRatingBonusType}");
            }

            if (EnergyRegenerationBonus > 0)
            {
                statsArray.Add($"+{EnergyRegenerationBonus} Energy Regen");
            }

            if (EnergyIncreaseBonus > 0)
            {
                statsArray.Add($"+{EnergyIncreaseBonus} Energy");
            }

            if (HealthIncreaseBonus > 0)
            {
                statsArray.Add($"+{HealthIncreaseBonus} Health");
            }

            if (statsArray.Count > 0)
            {
                var stat = statsArray[0].String;
                stats = stat;

                for (int i = 1; i < statsArray.Count; i++)
                {
                    stat = statsArray[i].String;
                    stats += $"\n{stat}";
                }
            }

            return true;
        }

        public int CalcArmorRating(EDamageType damageType)
        {
            var result = ArmorRating;
            if (ArmorRatingBonus > 0)
            {
                switch (ArmorRatingBonusType)
                {
                    case EArmorRatingBonusType.None:
                        // TODO: Warning?
                        break;
                    case EArmorRatingBonusType.PhysicalDamage:
                        EDamageType[] physicalDamageTypes = { EDamageType.Slashing, EDamageType.Piercing, EDamageType.Blunt };
                        if (Array.IndexOf(physicalDamageTypes, damageType) != -1)
                            result += ArmorRatingBonus;
                        break;
                    case EArmorRatingBonusType.ElementalDamage:
                        EDamageType[] elementalDamageTypes = { EDamageType.Earth, EDamageType.Fire, EDamageType.Air, EDamageType.Water };
                        if (Array.IndexOf(elementalDamageTypes, damageType) != -1)
                            result += ArmorRatingBonus;
                        break;
                    default:
                        // TODO: Error?
                        break;
                }
            }
            return result;
        }
    }
}