
using System;
using CreatureTime.RpgGame;
using UnityEngine;

namespace CreatureTime
{
    public enum EDataType
    {
        None = 0,
        Weapon = 1,
        Equipment = 2,
        OffHand = 3,
        Item = 4
    }

    public static class CtDataBlock
    {
        public const ulong InvalidData = 0xFFFFFFFFFFFFFFFF;

        public static string Serialize(ulong data) => $"{data:X16}";
        public static ulong Deserialize(string data) => Convert.ToUInt64(data, 16);

        public static bool IsValid(ulong data) => data != InvalidData;

        # region InventoryData

        // private const ulong UniqueIdMask = 0x0000000000000FFF;
        //
        // private const int UniqueIdBitShift = 0;
        // private const ulong UniqueIdBitMask = 0x0000000000000FFF;
        // private const ulong UniqueIdBitShiftMask = UniqueIdBitMask >> UniqueIdBitShift;
        //
        // public static ushort GetUniqueIdentifier(ulong data) => 
        //     (ushort)((data >> UniqueIdBitShift) & UniqueIdBitShiftMask);

        private const int TypeIdBitShift = 0;
        private const ulong TypeIdBitMask = 0x000000000000000F;
        private const ulong TypeIdBitShiftMask = TypeIdBitMask >> TypeIdBitShift;

        public static EDataType GetDataType(ulong data) => 
            (EDataType)((data >> TypeIdBitShift) & TypeIdBitShiftMask);

        # region Weapons

        private const ulong WeaponUnusedMask = 0xFFFFFFF000000000;

        private const int WeaponIdBitShift = 4;
        private const ulong WeaponIdBitMask = 0x00000000000FFFF0;
        private const ulong WeaponIdBitShiftMask = WeaponIdBitMask >> WeaponIdBitShift;

        private const int WeaponPrefixShiftBit = 20;
        private const ulong WeaponPrefixBitMask = 0x0000000000F00000;
        private const ulong WeaponPrefixBitShiftMask = WeaponPrefixBitMask >> WeaponPrefixShiftBit;

        private const int WeaponSuffixShiftBit = 24;
        private const ulong WeaponSuffixBitMask = 0x000000000F000000;
        private const ulong WeaponSuffixBitShiftMask = WeaponSuffixBitMask >> WeaponSuffixShiftBit;

        private const int WeaponReqShiftBit = 28;
        private const ulong WeaponReqBitMask = 0x00000000F0000000;
        private const ulong WeaponReqBitShiftMask = WeaponReqBitMask >> WeaponReqShiftBit;

        private const int WeaponRarityShiftBit = 32;
        private const ulong WeaponRarityBitMask = 0x0000000F00000000;
        private const ulong WeaponRarityBitShiftMask = WeaponRarityBitMask >> WeaponRarityShiftBit;

        public static ushort GetWeaponIdentifier(ulong data) => 
            (ushort)((data >> WeaponIdBitShift) & WeaponIdBitShiftMask);

        public static EWeaponPrefix GetWeaponPrefix(ulong data) =>
            (EWeaponPrefix)((data >> WeaponPrefixShiftBit) & WeaponPrefixBitShiftMask);

        public static void SetWeaponPrefix(EWeaponPrefix prefix, ref ulong data)
        {
            if (!IsValid(data))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={data}).");
#endif
                return;
            }

            int p = (int)prefix;
            data = ((ulong)p & WeaponPrefixBitMask) << WeaponPrefixShiftBit | data & ~WeaponPrefixBitMask;
        }

        public static EWeaponSuffix GetWeaponSuffix(ulong data) =>
            (EWeaponSuffix)((data >> WeaponSuffixShiftBit) & WeaponSuffixBitShiftMask);

        public static void SetWeaponSuffix(EWeaponSuffix suffix, ref ulong data)
        {
            if (!IsValid(data))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={data}).");
#endif
                return;
            }

            int s = (int)suffix;
            data = ((ulong)s & WeaponSuffixBitMask) << WeaponSuffixShiftBit | (data & ~WeaponSuffixBitMask);
        }

        public static int GetWeaponRequirement(ulong data) =>
            (int)((data >> WeaponReqShiftBit) & WeaponReqBitShiftMask);

        public static EItemRarity GetWeaponRarity(ulong data) =>
            (EItemRarity)((data >> WeaponRarityShiftBit) & WeaponRarityBitShiftMask);

        public static ulong CreateWeaponData(ushort identifier, EWeaponPrefix prefix, EWeaponSuffix suffix, 
            int requirement, EItemRarity rarity)
        {
            if (identifier >= WeaponIdBitShiftMask && identifier != 0xFFFF)
            {
#if DEBUG_LOGS
                Debug.LogError($"Identifier greater than mask allowed (identifier={identifier}).");
#endif
                return InvalidData;
            }

            int ra = (int)rarity;
            int re = requirement;
            int s = (int)suffix;
            int p = (int)prefix;
            return
                WeaponUnusedMask | // Unused
                ((ulong)ra & WeaponRarityBitShiftMask) << WeaponRarityShiftBit | // Rarity
                ((ulong)re & WeaponReqBitShiftMask) << WeaponReqShiftBit | // Suffix
                ((ulong)s & WeaponSuffixBitShiftMask) << WeaponSuffixShiftBit | // Suffix
                ((ulong)p & WeaponPrefixBitShiftMask) << WeaponPrefixShiftBit | // Prefix
                ((ulong)identifier & WeaponIdBitShiftMask) << WeaponIdBitShift | // Identifier
                ((ulong)EDataType.Weapon & TypeIdBitMask); // Data type
        }

        # endregion

        # region OffHand

        private const ulong OffHandUnusedMask = 0xFFFFFF0000000000;

        private const int OffHandIdBitShift = 4;
        private const ulong OffHandIdBitMask = 0x00000000000FFFF0;
        private const ulong OffHandIdBitShiftMask = OffHandIdBitMask >> OffHandIdBitShift;

        private const int OffHandPrefixShiftBit = 20;
        private const ulong OffHandPrefixBitMask = 0x0000000000F00000;
        private const ulong OffHandPrefixBitShiftMask = OffHandPrefixBitMask >> OffHandPrefixShiftBit;

        private const int OffHandSuffixShiftBit = 24;
        private const ulong OffHandSuffixBitMask = 0x000000000F000000;
        private const ulong OffHandSuffixBitShiftMask = OffHandSuffixBitMask >> OffHandSuffixShiftBit;

        private const int OffHandReqShiftBit = 28;
        private const ulong OffHandReqBitMask = 0x00000000F0000000;
        private const ulong OffHandReqBitShiftMask = OffHandReqBitMask >> OffHandReqShiftBit;

        private const int OffHandRarityShiftBit = 32;
        private const ulong OffHandRarityBitMask = 0x0000000F00000000;
        private const ulong OffHandRarityBitShiftMask = OffHandRarityBitMask >> OffHandRarityShiftBit;

        private const int OffHandModifierShiftBit = 36;
        private const ulong OffHandModifierBitMask = 0x000000F000000000;
        private const ulong OffHandModifierBitShiftMask = OffHandModifierBitMask >> OffHandModifierShiftBit;

        public static ushort GetOffHandIdentifier(ulong data) => 
            (ushort)((data >> OffHandIdBitShift) & OffHandIdBitShiftMask);

        public static EOffHandPrefix GetOffHandPrefix(ulong data) =>
            (EOffHandPrefix)((data >> OffHandPrefixShiftBit) & OffHandPrefixBitShiftMask);

        public static void SetOffHandPrefix(EOffHandPrefix prefix, ref ulong data)
        {
            if (!IsValid(data))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={data}).");
#endif
                return;
            }

            int p = (int)prefix;
            data = ((ulong)p & OffHandPrefixBitMask) << OffHandPrefixShiftBit | data & ~OffHandPrefixBitMask;
        }

        public static EOffHandSuffix GetOffHandSuffix(ulong data) =>
            (EOffHandSuffix)((data >> OffHandSuffixShiftBit) & OffHandSuffixBitShiftMask);

        public static void SetOffHandSuffix(EOffHandSuffix suffix, ref ulong data)
        {
            if (!IsValid(data))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={data}).");
#endif
                return;
            }

            int s = (int)suffix;
            data = ((ulong)s & OffHandSuffixBitMask) << OffHandSuffixShiftBit | (data & ~OffHandSuffixBitMask);
        }

        public static int GetOffHandRequirement(ulong data) =>
             (int)((data >> OffHandReqShiftBit) & OffHandReqBitShiftMask);

        // public static void SetOffHandModifier(int modifierStat, ref ulong data)
        // {
        //     if (!IsValid(data))
        //     {
        //         CTLogger.LogCritical("Data Mangle", $"Data was invalid (data={data}).");
        //         return;
        //     }
        //
        //     if (modifierStat < 1 || modifierStat > 16)
        //     {
        //         CTLogger.LogCritical("Data Mangle", $"Modifier stat must be between 1 and 16 (modifierStat={modifierStat}).");
        //         return;
        //     }
        //
        //     modifierStat -= 1;
        //
        //     data = ((ulong)modifierStat & OffHandModifierBitMask) << OffHandModifierShiftBit | (data & ~OffHandModifierBitMask);
        // }

        public static int GetOffHandModifierStat(ulong data)
        {
            int modifierStat = (int)((data >> OffHandModifierShiftBit) & OffHandModifierBitShiftMask);
            return modifierStat + 1;
        }

        public static EItemRarity GetOffHandRarity(ulong data) =>
            (EItemRarity)((data >> OffHandRarityShiftBit) & OffHandRarityBitShiftMask);

        public static ulong CreateOffHandData(ushort identifier, EOffHandPrefix prefix, EOffHandSuffix suffix, 
            int requirement, EItemRarity rarity, int modifierStat)
        {
            if (identifier >= OffHandIdBitShiftMask && identifier != 0xFFFF)
            {
#if DEBUG_LOGS
                Debug.LogError($"Identifier greater than mask allowed (identifier={identifier}).");
#endif
                return InvalidData;
            }

            if (modifierStat < 0 || modifierStat > 16)
            {
#if DEBUG_LOGS
                Debug.LogError($"Modifier stat must be between 1 and 16 (modifierStat={modifierStat}).");
#endif
                return InvalidData;
            }

            modifierStat -= 1;

            int ra = (int)rarity;
            int re = requirement;
            int s = (int)suffix;
            int p = (int)prefix;
            return
                OffHandUnusedMask | // Unused
                ((ulong)modifierStat & OffHandModifierBitShiftMask) << OffHandModifierShiftBit | // Modifier Stat
                ((ulong)ra & OffHandRarityBitShiftMask) << OffHandRarityShiftBit | // Rarity
                ((ulong)re & OffHandReqBitShiftMask) << OffHandReqShiftBit | // Suffix
                ((ulong)s & OffHandSuffixBitShiftMask) << OffHandSuffixShiftBit | // Suffix
                ((ulong)p & OffHandPrefixBitShiftMask) << OffHandPrefixShiftBit | // Prefix
                ((ulong)identifier & OffHandIdBitShiftMask) << OffHandIdBitShift | // Identifier
                ((ulong)EDataType.OffHand & TypeIdBitMask); // Data type
        }

        # endregion

        # region Equipment

        private const ulong EquipmentUnusedMask = 0xFFFFFFFFFF000000;

        private const int EquipmentIdBitShift = 4;
        private const ulong EquipmentIdBitMask = 0x00000000000FFFF0;
        private const ulong EquipmentIdBitShiftMask = EquipmentIdBitMask >> EquipmentIdBitShift;

        private const int EquipmentSlotBitShift = 20;
        private const ulong EquipmentSlotBitMask = 0x0000000000F00000;
        private const ulong EquipmentSlotBitShiftMask = EquipmentSlotBitMask >> EquipmentSlotBitShift;

        public static ushort GetEquipmentIdentifier(ulong data) => 
            (ushort)((data >> EquipmentIdBitShift) & EquipmentIdBitShiftMask);

        public static EArmorSlot GetEquipmentSlot(ulong data) => 
            (EArmorSlot)((data >> EquipmentSlotBitShift) & EquipmentSlotBitShiftMask);

        public static ulong CreateEquipmentData(ushort identifier, EArmorSlot slot)
        {
            var s = Convert.ToInt32(slot);

            return
                EquipmentUnusedMask | // Unused
                ((ulong)s & EquipmentSlotBitShiftMask) << EquipmentSlotBitShift | // Identifier
                (identifier & EquipmentIdBitShiftMask) << EquipmentIdBitShift | // Identifier
                ((ulong)EDataType.Equipment & TypeIdBitMask); // Data type
        }

        #endregion

        #endregion

        #region Attribute Data

        public const int MaxAttributes = 4;

        private const int ProfessionIdBitShift = 0;
        private const ulong ProfessionIdBitMask = 0x000000000000000F;

        private const int AttributeCountBitShift = 4;
        private const ulong AttributeCountBitMask = 0x00000000000007F0;
        private const ulong AttributeCountBitShiftMask = AttributeCountBitMask >> AttributeCountBitShift;

        private const int AttributeStartBitShift = 11;

        private const int PerAttributeBitShift = 11;
        private const ulong AttributeBitMask = 0x00000000000007FF;

        private const int AttributeTypeBitShift = 0;
        private const ulong AttributeTypeBitMask = 0x000000000000007F;
        private const ulong AttributeTypeBitShiftMask = AttributeTypeBitMask >> AttributeTypeBitShift;

        private const int AttributeRankBitShift = 7;
        private const ulong AttributeRankBitMask = 0x0000000000000780;
        private const ulong AttributeRankBitShiftMask = AttributeRankBitMask >> AttributeRankBitShift;

        public static ushort GetProfession(ulong attributeData)
        {
            return (ushort)((attributeData >> ProfessionIdBitShift) & ProfessionIdBitMask);
        }

        private static int CalcAttributeBitShiftByIndex(int index)
        {
            int bitShift = AttributeStartBitShift + PerAttributeBitShift * index;
            if (bitShift >= 64)
            {
#if DEBUG_LOGS
                Debug.LogError("Bit shift should not be greater than the size of the data block!");
#endif
                return AttributeStartBitShift;
            }
            return AttributeStartBitShift + PerAttributeBitShift * index;
        } 

        public static ulong SetProfession(ushort profession, int attributeCount)
        {
            if (attributeCount < 0)
            {
#if DEBUG_LOGS
                Debug.LogError("Attribute count was less than 0 " +
                               $"(attributeCount={attributeCount}, allowed={MaxAttributes}).");
#endif
                return InvalidData;
            }

            if (attributeCount > MaxAttributes)
            {
#if DEBUG_LOGS
                Debug.LogError("Attribute count was more than max allowed count " +
                               $"(attributeCount={attributeCount}, allowed={MaxAttributes}).");
#endif
                return InvalidData;
            }

            if (profession > ProfessionIdBitMask)
            {
#if DEBUG_LOGS
                Debug.LogError("Profession greater than mask allowed " +
                               $"(profession={profession}, allowed={ProfessionIdBitMask}).");
#endif
                return InvalidData;
            }

            int bitShift = CalcAttributeBitShiftByIndex(attributeCount);
            ulong unusedBitMask = 0xFFFFFFFFFFFFFFFF << bitShift;
            return
                unusedBitMask | // Unused while setting remaining attribute data to zero.
                ((ushort)attributeCount & AttributeCountBitShiftMask) << AttributeCountBitShift | // Attribute count
                (profession & AttributeTypeBitMask); // Profession;
        }

        public static ushort GetAttributeCount(ulong attributeData)
        {
            return (ushort)((attributeData >> AttributeCountBitShift) & AttributeCountBitShiftMask);
        }

        // public static ushort GetAttributeType(ulong attributeData, int attributeIndex)
        // {
        //     int bitShift = CalcAttributeBitShiftByIndex(attributeIndex);
        //     bitShift += AttributeTypeBitShift;
        //     return (ushort)((attributeData >> bitShift) & AttributeTypeBitShiftMask);
        // }
        //
        // public static ulong SetAttributeType(int attributeIndex, ushort attributeType, ulong attributeData)
        // {
        //     if (!IsValid(attributeData))
        //     {
        //         Debug.LogError($"Data was invalid (data={attributeData}).");
        //         return attributeData;
        //     }
        //
        //     if (attributeType > AttributeTypeBitShiftMask)
        //     {
        //         Debug.LogError($"Attribute type greater than mask allowed (attributeType={attributeType}).");
        //         return attributeData;
        //     }
        //
        //     int bitShift = CalcAttributeBitShiftByIndex(attributeIndex);
        //     bitShift += AttributeTypeBitShift;
        //     ulong attributeBitMask = AttributeTypeBitShiftMask << bitShift;
        //
        //     if (attributeBitMask >> bitShift != AttributeTypeBitShiftMask)
        //     {
        //         Debug.LogError("Attribute bitmask does not match attribute mask after bit shifting " +
        //                        $"(attributeBitMask={attributeBitMask >> bitShift:x0}, " +
        //                        $"AttributeTypeBitShiftMask={AttributeTypeBitShiftMask:x0}).");
        //         return attributeData;
        //     }
        //
        //     return (attributeType & AttributeTypeBitShiftMask) << bitShift | 
        //            (attributeData & ~attributeBitMask);
        // }

        public static ushort GetAttributeRank(ulong attributeData, int attributeIndex)
        {
            int bitShift = CalcAttributeBitShiftByIndex(attributeIndex);
            bitShift += AttributeRankBitShift;
            return (ushort)((attributeData >> bitShift) & AttributeRankBitShiftMask);
        }

        public static ulong SetAttributeRank(int attributeIndex, int rank, ulong attributeData)
        {
            var r = (ushort)rank;
            if (!IsValid(attributeData))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={attributeData}).");
#endif
                return attributeData;
            }

            if (r > AttributeRankBitShiftMask)
            {
#if DEBUG_LOGS
                Debug.LogError("Attribute rank greater than mask allowed " +
                               $"(rank={rank}, allowed={AttributeRankBitShiftMask}).");
#endif
                return attributeData;
            }

            int bitShift = CalcAttributeBitShiftByIndex(attributeIndex);
            bitShift += AttributeRankBitShift;
            ulong attributeBitMask = AttributeRankBitShiftMask << bitShift;
            return (r & AttributeRankBitShiftMask) << bitShift | 
                   (attributeData & ~attributeBitMask);
        }

        public static int GetRankCostPerRank(int rank)
        {
            int[] rankCost = { 0, 1, 2, 3, 4, 5, 6, 7, 9, 11, 13, 16, 20 };
            if (rank >= rankCost.Length)
            {
#if DEBUG_LOGS
                Debug.LogError($"Rank was out of bounds (rank={rank}).");
#endif
                return 0;
            }
            return rankCost[rank];
        }

        public static int AttributesPointsPerRank(int rank)
        {
            int total = 0;
            for (int i = 0; i <= rank; ++i)
            {
                total += GetRankCostPerRank(i);
            }
            return total;
        }

        public static int TotalPointsForAttributeRank(ulong data)
        {
            int result = 0;
            int count = GetAttributeCount(data);
            if (count > MaxAttributes)
            {
#if DEBUG_LOGS
                Debug.LogError("Count was greater than the max allowed " +
                               $"(count={count}, allowed={MaxAttributes}).");
#endif
                return result;
            }

            for (int i = 0; i < count; ++i)
            {
                result += AttributesPointsPerRank(GetAttributeRank(data, i));
            }
            return result;
        }

        #endregion

        # region Effect

        private const ulong EffectUnusedMask = 0xFFFFFF0000000000;

        private const int EffectIdBitShift = 0;
        private const ulong EffectIdBitMask = 0x000000000000FFFF;
        private const ulong EffectIdBitShiftMask = EffectIdBitMask >> EffectIdBitShift;

        private const int EffectSourceShiftBit = EffectIdBitShift + 16;
        private const ulong EffectSourceBitMask = 0x00000000FFFF0000;
        private const ulong EffectSourceBitShiftMask = EffectSourceBitMask >> EffectSourceShiftBit;

        private const int EffectTurnsShiftBit = EffectSourceShiftBit + 16;
        private const ulong EffectTurnsBitMask = 0x000000FF00000000;
        private const ulong EffectTurnsBitShiftMask = EffectTurnsBitMask >> EffectTurnsShiftBit;

        public static ushort GetEffectIdentifier(ulong data) => 
            (ushort)((data >> EffectIdBitShift) & EffectIdBitShiftMask);

        public static ushort GetEffectSource(ulong data) =>
            (ushort)((data >> EffectSourceShiftBit) & EffectSourceBitShiftMask);

        public static int GetEffectTurns(ulong data) =>
            (int)((data >> EffectTurnsShiftBit) & EffectTurnsBitShiftMask);

        public static void SetEffectTurns(int turns, ref ulong data)
        {
            if (!IsValid(data))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={data}).");
#endif
                return;
            }

            data = ((ulong)turns & EffectTurnsBitShiftMask) << EffectTurnsShiftBit | data & ~EffectTurnsBitMask;
        }

        public static ulong CreateEffectData(ushort identifier, ushort sourceId, int turns)
        {
            if (identifier >= EffectIdBitShiftMask)
            {
#if DEBUG_LOGS
                Debug.LogError($"Identifier greater than mask allowed (identifier={identifier}).");
#endif
                return InvalidData;
            }

            return
                EffectUnusedMask | // Unused
                ((ulong)turns & EffectTurnsBitShiftMask) << EffectTurnsShiftBit | // Turns
                (sourceId & EffectSourceBitShiftMask) << EffectSourceShiftBit | // Source Identifier
                (identifier & EffectIdBitShiftMask) << EffectIdBitShift; // Identifier
        }

        # endregion

        # region Quests

        private const ulong QuestUnusedMask = 0x0000000000000000;

        private const int QuestIdBitShift = 0;
        private const ulong QuestIdBitMask = 0x000000000000FFFF;
        private const ulong QuestIdBitShiftMask = QuestIdBitMask >> QuestIdBitShift;

        private const int QuestStateBitShift = QuestIdBitShift + 16;
        private const ulong QuestStateBitMask = 0x00000000000F0000;
        private const ulong QuestStateBitShiftMask = QuestStateBitMask >> QuestStateBitShift;

        private const int ObjectiveStartBitShift = QuestStateBitShift + 4;

        private const int PerObjectiveBitShift = 8;
        private const ulong PerObjectiveBitMask = 0x00000000000000FF;

        private const int ObjectiveBitShift = 0;
        private const ulong ObjectiveBitMask = 0x00000000000000FF;
        private const ulong ObjectiveBitShiftMask = ObjectiveBitMask >> ObjectiveBitShift;

        public static ushort GetProgressionIdentifier(ulong data) => 
            (ushort)((data >> QuestIdBitShift) & QuestIdBitShiftMask);

        public static EProgressionState GetQuestState(ulong data) => 
            (EProgressionState)((data >> QuestStateBitShift) & QuestStateBitShiftMask);

        public static ulong SetQuestState(ulong data, EProgressionState state)
        {
            var s = Convert.ToInt32(state);
            return ((ulong)s & QuestStateBitShiftMask) << QuestStateBitShift | (data & ~QuestStateBitMask);
        }

        private static int _CalcBitShiftByIndex(int index, int startBit, int offsetBit)
        {
            int bitShift = startBit + offsetBit * index;
            if (bitShift >= 64)
            {
#if DEBUG_LOGS
                Debug.LogError("Bit shift should not be greater than the size of the data block!");
#endif
                return startBit;
            }

            return startBit + offsetBit * index;
        }

        public static int GetQuestObjective(int objectiveIndex, ulong questData)
        {
            int bitShift = _CalcBitShiftByIndex(objectiveIndex, ObjectiveStartBitShift, PerObjectiveBitShift);
            bitShift += ObjectiveBitShift;
            return (int)((questData >> bitShift) & ObjectiveBitShiftMask);
        }

        public static ulong UpdateQuestObjective(int objectiveIndex, int value, ulong questData)
        {
            var v = (ushort)value;
            if (!IsValid(questData))
            {
#if DEBUG_LOGS
                Debug.LogError($"Data was invalid (data={questData}).");
#endif
                return questData;
            }

#if DEBUG_LOGS
            Debug.Log($"Updating quest objective (index={objectiveIndex}, value={value}).");
#endif

//             if (value > ObjectiveBitShiftMask)
//             {
// #if DEBUG_LOGS
//                 Debug.LogError("Objective value greater than mask allowed " +
//                                $"(value={value}, allowed={ObjectiveBitShiftMask}).");
// #endif
//                 return questData;
//             }

            int bitShift = _CalcBitShiftByIndex(objectiveIndex, ObjectiveStartBitShift, PerObjectiveBitShift);
            bitShift += ObjectiveBitShift;

            var objectiveBitMask = ObjectiveBitShiftMask << bitShift;
            return (v & ObjectiveBitShiftMask) << bitShift | 
                   (questData & ~objectiveBitMask);
        }

        public static ulong CreateQuestData(ushort identifier)
        {
            if (identifier >= QuestIdBitShiftMask)
            {
#if DEBUG_LOGS
                Debug.LogError($"Identifier greater than quest mask allowed (identifier={identifier}).");
#endif
                return InvalidData;
            }

            var s = Convert.ToInt32(EProgressionState.Active);

            return
                QuestUnusedMask | // Unused
                ((ulong)s & QuestStateBitShiftMask) << QuestStateBitShift | // State
                (identifier & QuestIdBitShiftMask) << QuestIdBitShift; // Identifier
        }

        # endregion
    }
}