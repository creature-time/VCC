
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "offHandDefData", menuName = "CreatureTime/Rpg/Off Hand Definition", order = 1)]
    public class CtOffHandDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public Texture icon;
        [SerializeField] public EOffHandType offHandType = EOffHandType.None;
        [SerializeField] public CtAttributeDefData attributeType;
        [SerializeField] [Range(0, 9)] public int attributeRequirement;
        [SerializeField] public int minModifierStat = 8;
        [SerializeField] public int maxModifierStat = 16;
        [SerializeField] public EItemRarity rarity = EItemRarity.None;
    }
}