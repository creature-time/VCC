
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtArmorSlotData
    {
        [SerializeField] public string suffix;
        [SerializeField] public Texture icon;
        [SerializeField] public int armorRating;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "armorDefData", menuName = "CreatureTime/Rpg/Armor Definition", order = 1)]
    public class CtArmorSetDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public EItemRarity rarity;
        [SerializeField] public CtArmorSlotData headSlot;
        [SerializeField] public CtArmorSlotData chestSlot;
        [SerializeField] public CtArmorSlotData handsSlot;
        [SerializeField] public CtArmorSlotData legsSlot;
        [SerializeField] public CtArmorSlotData feetSlot;
    }
}