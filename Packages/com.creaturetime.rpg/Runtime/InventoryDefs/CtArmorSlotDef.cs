
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtArmorSlotDef : CtInventoryItemDef
    {
        [SerializeField] private CtArmorSetDef armorSet;
        // [SerializeField] private EArmorSlot armorSlot = EArmorSlot.None;
        // Heavy armor: 25-80 armor
        // Medium armor: 10-70 armor
        // Light armor: 5-60 armor
        [SerializeField] private int armorRating = 0;
        // Heavy bonus: Armor +20, Armor +20, Armor +20, Armor +20, Armor +20
        // Medium bonus: n/a, Energy +5, n/a, Energy Recovery, n/a
        // Light bonus: n/a, Energy +5, Energy +5, Energy Recover, Energy Recover
        // public EBonusType bonusType = EBonusType.None;
        // public int bonusValue = -1;

        public CtArmorSetDef ArmorSet => armorSet;
        // public EArmorSlot ArmorSlot => armorSlot;
        public int ArmorRating => armorRating;

        public void GetFormattedStats(ulong dataBlock, ref string equipmentName, ref string stats, ref EItemRarity rarity, ref int requirement)
        {
            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;
            switch (rarity)
            {
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
                    LogCritical($"Rarity not implemented (rarity={rarity}).");
                    break;
            }

            equipmentName = $"<color={color}>{DisplayName}</color>";

            stats = string.Empty;

            stats = stats.Trim();
        }
    }
}