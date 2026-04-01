
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtMainHandUserData
    {
        public GameObject userData;
        public Material material;
        [Range(0, 15)] public int palette;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "mainHandDefData", menuName = "CreatureTime/Rpg/Main Hand Definition", order = 1)]
    public class CtMainHandDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public Texture icon;
        [SerializeField] public int baseValue;
        [SerializeField] public EWeaponType weaponType;
        [SerializeField] public EWeaponAttackType attackType;
        [SerializeField] public CtAttributeDefData attributeType;
        [SerializeField] [Range(0, 9)] private int attributeRequirement = 0;
        [SerializeField] public EDamageType damageType = EDamageType.Piercing;
        [SerializeField] [Range(0, 32)] public int damageMin = 15;
        [SerializeField] [Range(0, 32)] public int damageMax = 26;
        [SerializeField] public EItemRarity rarity = EItemRarity.None;
        [SerializeField] public CtMainHandUserData userData;
    }
}