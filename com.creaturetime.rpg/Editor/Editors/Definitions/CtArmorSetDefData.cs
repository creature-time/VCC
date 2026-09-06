
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreatureTime
{
    [Serializable]
    public struct CtArmorSlotData
    {
        [SerializeField] public string suffix;
        [SerializeField] public Texture icon;
        [SerializeField] public int baseValue;
        [SerializeField] public int armorRating;
        [SerializeField] public int armorRatingBonus;
        [SerializeField] public EArmorRatingBonusType armorRatingBonusType;
        [SerializeField] public int energyRegenerationBonus;
        [SerializeField] public int energyIncreaseBonus;
        [SerializeField] public int healthIncreaseBonus;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "armorDefData", menuName = "CreatureTime/Rpg/Armor Definition", order = 1)]
    public class CtArmorSetDefData : CtAbstractDefData
    {
        [Flags]
        public enum EAllowedProfessionFlags
        {
            Melee = 1 << 0,
            Caster = 1 << 1,
            Ranged = 1 << 2,
            Healer = 1 << 3
        }

        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public EAllowedProfessionFlags allowedProfessionFlags;
        [SerializeField] public EItemRarity rarity;
        [SerializeField] public CtArmorSlotData headSlot;
        [SerializeField] public CtArmorSlotData chestSlot;
        [SerializeField] public CtArmorSlotData handsSlot;
        [SerializeField] public CtArmorSlotData legsSlot;
        [SerializeField] public CtArmorSlotData feetSlot;
    }
}