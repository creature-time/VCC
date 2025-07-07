
using System;
using System.Collections.Generic;
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
    public struct CtArmorDataBlock
    {
        public CtArmorSetDefData armor;

        public ulong DataBlock {
            get
            {
                if (!armor)
                    return CtDataBlock.InvalidData;
                if (armor.identifier == CtConstants.InvalidId)
                    return CtDataBlock.InvalidData;
                return CtDataBlock.CreateEquipmentData(armor.identifier);
            }
        }
    }

    [Serializable]
    public struct CtProfessionDataBlock
    {
        private struct Rank
        {
            public ushort identifier;
            public ushort rank;
        }

        public CtProfessionDefData profession;
        public ushort attributeRank1;
        public ushort attributeRank2;
        public ushort attributeRank3;
        public ushort attributeRank4;
        public ushort attributeRank5;

        public ulong DataBlock {
            get
            {
                if (!profession)
                    return CtDataBlock.InvalidData;
                if (profession.identifier == CtConstants.InvalidId)
                    return CtDataBlock.InvalidData;

                List<Rank> ranks = new List<Rank>();
                if (profession.attributes1 && profession.attributes1.identifier != CtConstants.InvalidId)
                {
                    ranks.Add(new Rank {
                        identifier = profession.attributes1.identifier,
                        rank = attributeRank1
                    });
                }

                if (profession.attributes2 && profession.attributes2.identifier != CtConstants.InvalidId)
                {
                    ranks.Add(new Rank {
                        identifier = profession.attributes2.identifier,
                        rank = attributeRank2
                    });
                }

                if (profession.attributes3 && profession.attributes3.identifier != CtConstants.InvalidId)
                {
                    ranks.Add(new Rank {
                        identifier = profession.attributes3.identifier,
                        rank = attributeRank3
                    });
                }

                if (profession.attributes4 && profession.attributes4.identifier != CtConstants.InvalidId)
                {
                    ranks.Add(new Rank {
                        identifier = profession.attributes4.identifier,
                        rank = attributeRank4
                    });
                }

                if (profession.attributes5 && profession.attributes5.identifier != CtConstants.InvalidId)
                {
                    ranks.Add(new Rank {
                        identifier = profession.attributes5.identifier,
                        rank = attributeRank5
                    });
                }

                ranks.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var data = CtDataBlock.SetProfession(profession.identifier, (ushort)ranks.Count);
                for (int i = 0; i < ranks.Count; i++)
                    data = CtDataBlock.SetAttributeRank(i, ranks[i].rank, data);

                return data;
            }
        }
    }

    [Serializable]
    public struct CtSkillsDataBlock
    {
        public CtSkillDefData skillDef0;
        public CtSkillDefData skillDef1;
        public CtSkillDefData skillDef2;
        public CtSkillDefData skillDef3;
        public CtSkillDefData skillDef4;
        public CtSkillDefData skillDef5;
        public CtSkillDefData skillDef6;
        public CtSkillDefData skillDef7;
        public CtSkillDefData skillDef8;
        public CtSkillDefData skillDef9;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NpcDefData", menuName = "CreatureTime/Rpg/Npc Definition", order = 1)]
    public class CtNpcDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public Texture icon;
        [SerializeField, Range(1, 30)] public int characterLevel = 1;
        [SerializeField] public CtProfessionDataBlock professionDataBlock;
        [SerializeField] public CtMainHandDataBlock mainHandDataBlock;
        [SerializeField] public CtOffHandDataBlock offHandDataBlock;
        [SerializeField] public CtArmorDataBlock headArmorDataBlock;
        [SerializeField] public CtArmorDataBlock chestArmorDataBlock;
        [SerializeField] public CtArmorDataBlock handsArmorDataBlock;
        [SerializeField] public CtArmorDataBlock legsArmorDataBlock;
        [SerializeField] public CtArmorDataBlock feetArmorDataBlock;
        [SerializeField] public CtNpcBehaviorData behavior;
        [SerializeField] public CtSkillsDataBlock skillsBlock;

        // [SerializeField] public ??? defaultEquipment;
        // [SerializeField] public ??? skills;
        // [SerializeField] public ??? attributeData;

        // [SerializeField] public ??? customization;
    }
}