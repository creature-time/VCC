
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtMainHandDataBlock
    {
        public CtMainHandDefData mainHand;
        public EWeaponPrefix prefix;
        public EWeaponSuffix suffix;
        public int requirement;
        public EItemRarity rarity;

        public ulong DataBlock
        {
            get
            {
                if (!mainHand)
                    return CtDataBlock.InvalidData;
                if (mainHand.identifier == CtConstants.InvalidId)
                    return CtDataBlock.InvalidData;
                return CtDataBlock.CreateWeaponData(mainHand.identifier, prefix, suffix, requirement, rarity);
            }
        }
    }

    [Serializable]
    public struct CtOffHandDataBlock
    {
        public CtOffHandDefData offHand;
        public EOffHandPrefix prefix;
        public EOffHandSuffix suffix;
        public int requirement;
        public EItemRarity rarity;
        public int modifierStat;

        public ulong DataBlock {
            get
            {
                if (!offHand)
                    return CtDataBlock.InvalidData;
                if (offHand.identifier == CtConstants.InvalidId)
                    return CtDataBlock.InvalidData;
                return CtDataBlock.CreateOffHandData(
                    offHand.identifier, prefix, suffix, requirement, rarity, modifierStat);
            }
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NpcDefData", menuName = "CreatureTime/Rpg/Npc Definition", order = 1)]
    public class CtNpcDefData : ScriptableObject
    {
        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
        [SerializeField] public Texture icon;
        [SerializeField] public CtMainHandDataBlock mainHandDataBlock;
        [SerializeField] public CtOffHandDataBlock offHandDataBlock;

        // [SerializeField] public ??? defaultEquipment;
        // [SerializeField] public ??? skills;
        // [SerializeField] public ??? attributeData;

        // [SerializeField] public ??? customization;
    }
}