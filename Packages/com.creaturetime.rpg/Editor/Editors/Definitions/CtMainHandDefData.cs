
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtMainHandUserData
    {
        public CtUserData userData;
        public Material material;
        [Range(0, 15)] public int palette;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "mainHandDefData", menuName = "CreatureTime/Rpg/Main Hand Definition", order = 1)]
    public class CtMainHandDefData : ScriptableObject
    {
        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
        [SerializeField] public Texture icon;
        [SerializeField] public EWeaponType weaponType;
        [SerializeField] public ushort attributeType = CtConstants.InvalidId;
        [SerializeField] [Range(0, 9)] private int attributeRequirement = 0;
        [SerializeField] public EDamageType damageType = EDamageType.Piercing;
        [SerializeField] [Range(0, 32)] public int damageMin = 15;
        [SerializeField] [Range(0, 32)] public int damageMax = 26;
        [SerializeField] public EItemRarity rarity = EItemRarity.None;
        [SerializeField] public CtMainHandUserData userData;
    }
}