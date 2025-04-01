
namespace CreatureTime
{
    public enum EDataType
    {
        Weapon,
        Equipment,
        OffHand
    }

    public static class CtDataBlock
    {
        public const ulong InvalidData = 0xFFFFFFFFFFFFFFFF;
        private const int DataTypeBitMask = 0x000000000000000F;

        public static EDataType GetDataType(ulong data) => (EDataType)(data & DataTypeBitMask);
        public static bool IsValid(ulong data) => data != InvalidData;

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
                CtLogger.LogCritical("Data Mangle", $"Data was invalid (data={data}).");
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
                CtLogger.LogCritical("Data Mangle", $"Data was invalid (data={data}).");
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
                CtLogger.LogCritical("Data Mangle", $"Identifier greater than mask allowed (identifier={identifier}).");
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
                ((ulong)EDataType.Weapon & DataTypeBitMask); // Data type
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
                CtLogger.LogCritical("Data Mangle", $"Data was invalid (data={data}).");
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
                CtLogger.LogCritical("Data Mangle", $"Data was invalid (data={data}).");
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
                CtLogger.LogCritical("Data Mangle", $"Identifier greater than mask allowed (identifier={identifier}).");
                return InvalidData;
            }

            if (modifierStat < 1 || modifierStat > 16)
            {
                CtLogger.LogCritical("Data Mangle", $"Modifier stat must be between 1 and 16 (modifierStat={modifierStat}).");
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
                ((ulong)EDataType.OffHand & DataTypeBitMask); // Data type
        }

        # endregion

        # region Equipment

        private const ulong EquipmentUnusedMask = 0xFFFFFFFFFFF00000;

        private const int EquipmentIdBitShift = 4;
        private const ulong EquipmentIdBitMask = 0x00000000000FFFF0;
        private const ulong EquipmentIdBitShiftMask = EquipmentIdBitMask >> EquipmentIdBitShift;

        public static ushort GetEquipmentIdentifier(ulong data) => 
            (ushort)((data >> EquipmentIdBitShift) & EquipmentIdBitShiftMask);

        public static ulong CreateEquipmentData(ushort identifier)
        {
            if (identifier >= EquipmentIdBitShiftMask)
            {
                CtLogger.LogCritical("Data Mangle", $"Identifier greater than mask allowed (identifier={identifier}).");
                return InvalidData;
            }

            return
                EquipmentUnusedMask | // Unused
                (identifier & EquipmentIdBitShiftMask) << EquipmentIdBitShift | // Identifier
                ((ulong)EDataType.Equipment & DataTypeBitMask); // Data type
        }

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
                CtLogger.LogCritical("Data Mangle", "Bit shift should not be greater than the size of the data block!");
                return AttributeStartBitShift;
            }
            return AttributeStartBitShift + PerAttributeBitShift * index;
        } 

        public static ulong SetProfession(ushort profession, ushort attributeCount)
        {
            if (attributeCount > MaxAttributes)
            {
                CtLogger.LogCritical("Data Mangle", "Attribute count was more than max allowed count " +
                                     $"(attributeCount={attributeCount}, allowed={MaxAttributes}).");
                return InvalidData;
            }

            if (profession > ProfessionIdBitMask)
            {
                CtLogger.LogCritical("Data Mangle", "Profession greater than mask allowed " +
                                     $"(profession={profession}, allowed={ProfessionIdBitMask}).");
                return InvalidData;
            }

            int bitShift = CalcAttributeBitShiftByIndex(attributeCount);
            ulong unusedBitMask = 0xFFFFFFFFFFFFFFFF << bitShift;
            return
                unusedBitMask | // Unused while setting remaining attribute data to zero.
                (attributeCount & AttributeCountBitShiftMask) << AttributeCountBitShift | // Atribute count
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

        public static ulong SetAttributeRank(int attributeIndex, ushort rank, ulong attributeData)
        {
            if (!IsValid(attributeData))
            {
                CtLogger.LogCritical("Data Mangle", $"Data was invalid (data={attributeData}).");
                return attributeData;
            }

            if (rank > AttributeRankBitShiftMask)
            {
                CtLogger.LogCritical("Data Mangle", "Attribute rank greater than mask allowed " +
                                  $"(rank={rank}, allowed={AttributeRankBitShiftMask}).");
                return attributeData;
            }

            int bitShift = CalcAttributeBitShiftByIndex(attributeIndex);
            bitShift += AttributeRankBitShift;
            ulong attributeBitMask = AttributeRankBitShiftMask << bitShift;
            return (rank & AttributeRankBitShiftMask) << bitShift | 
                   (attributeData & ~attributeBitMask);
        }

        public static int GetRankCostPerRank(int rank)
        {
            int[] rankCost = { 0, 1, 2, 3, 4, 5, 6, 7, 9, 11, 13, 16, 20 };
            if (rank >= rankCost.Length)
            {
                CtLogger.LogCritical("Data Mangle", $"Rank was out of bounds (rank={rank}).");
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
                CtLogger.LogCritical("Data Mangle", "Count was greater than the max allowed " +
                                     $"(count={count}, allowed={MaxAttributes}).");
                return result;
            }

            for (int i = 0; i < count; ++i)
            {
                result += AttributesPointsPerRank(GetAttributeRank(data, i));
            }
            return result;
        }

        #endregion

        private const ulong UtMaskValidationExpected = 0xFFFFFFFFFFFFFFFF;

        private const ushort UtWeaponIdentifierExpected = 1234;
        private const EWeaponPrefix UtWeaponPrefixExpected = EWeaponPrefix.Fiery;
        private const EWeaponSuffix UtWeaponSuffixExpected = EWeaponSuffix.Defense;
        private const int UtWeaponReqExpected = 9;
        private const EItemRarity UtWeaponRarityExpected = EItemRarity.Rare;

        private const ushort UtEquipmentIdentifierExpected = 1337;

        private const ushort UtProfessionExpected = 15;
        private const ushort UtAttributeCountExpected = MaxAttributes;

        public static void UnitTests()
        {
            ulong data;

            data = WeaponUnusedMask |
                   WeaponReqBitMask |
                   WeaponSuffixBitMask |
                   WeaponSuffixShiftBit |
                   WeaponPrefixBitMask |
                   WeaponIdBitMask |
                   WeaponRarityBitMask |
                   DataTypeBitMask;
            if (data != UtMaskValidationExpected)
                CtLogger.LogError("Data Mangle", "Weapon data masks did not match expected " +
                                                 $"(given={data:x16}, expected={UtMaskValidationExpected:x16})");

            data = CreateWeaponData(
                UtWeaponIdentifierExpected,
                EWeaponPrefix.Fiery,
                EWeaponSuffix.Defense,
                UtWeaponReqExpected,
                UtWeaponRarityExpected);

            // EItemRarity rarity = GetWeaponRarity(data);
            // if (rarity != UtWeaponRarityExpected)
            //     Debug.LogError("Weapon rarity did not match expected " +
            //                    $"(given={rarity}, expected={UtWeaponRarityExpected})");

            ushort weaponIdentifier = GetWeaponIdentifier(data);
            if (weaponIdentifier != UtWeaponIdentifierExpected)
                CtLogger.LogError("Data Mangle", "Weapon identifier did not match expected " +
                                                 $"(given={weaponIdentifier}, expected={UtWeaponIdentifierExpected})");

            EWeaponPrefix prefix = GetWeaponPrefix(data);
            if (prefix != UtWeaponPrefixExpected)
                CtLogger.LogError("Data Mangle", "Weapon prefix did not match expected " +
                                                 $"(given={prefix}, expected={UtWeaponPrefixExpected})");

            EWeaponSuffix suffix = GetWeaponSuffix(data);
            if (suffix != UtWeaponSuffixExpected)
                CtLogger.LogError("Data Mangle", "Weapon suffix did not match expected " +
                                                 $"(given={suffix}, expected={UtWeaponSuffixExpected})");

            int req = GetWeaponRequirement(data);
            if (req != UtWeaponReqExpected)
                CtLogger.LogError("Data Mangle", "Weapon requirement did not match expected " +
                                                 $"(given={req}, expected={UtWeaponReqExpected})");

            data = EquipmentUnusedMask |
                   EquipmentIdBitMask |
                   DataTypeBitMask;
            if (data != UtMaskValidationExpected)
                CtLogger.LogError("Data Mangle", "Equipment data masks did not match expected " +
                                                 $"(given={data:x16}, expected={UtMaskValidationExpected:x16})");

            data = CreateEquipmentData(UtEquipmentIdentifierExpected);

            ushort equipmentIdentifier = GetEquipmentIdentifier(data);
            if (equipmentIdentifier != UtEquipmentIdentifierExpected)
                CtLogger.LogError("Data Mangle",
                    "Equipment identifier did not match expected " +
                    $"(given={equipmentIdentifier:x16}, expected={UtEquipmentIdentifierExpected:x16})");

            // data = //WeaponUnusedMask |
            //        // WeaponSuffixBitMask |
            //        // WeaponSuffixShiftBit |
            //        // WeaponPrefixBitMask |
            //        AttributeCountBitMask |
            //        ProfessionIdBitMask;
            // if (data != UtMaskValidationExpected)
            //     Debug.LogError("Attribute data masks did not match expected " +
            //                    $"(given={data:x16}, expected={UtMaskValidationExpected:x16})");

            ulong attributeBitMask = AttributeTypeBitMask | AttributeRankBitMask;
            if (attributeBitMask != AttributeBitMask)
                CtLogger.LogError("Data Mangle", "Attribute masks are not masking correctly " +
                                                 $"(given={attributeBitMask}, expected={AttributeBitMask})");

            data = SetProfession(UtProfessionExpected, UtAttributeCountExpected);

            ushort value = 1;
            for (int i = 0; i < UtAttributeCountExpected; ++i)
            {
                // data = SetAttributeType(i, value, data);
                data = SetAttributeRank(i, value, data);
                value++;
            }

            ushort profession = GetProfession(data);
            if (profession != UtProfessionExpected)
                CtLogger.LogError("Data Mangle", "Profession was not returned correctly " +
                                                 $"(given={profession}, expected={UtProfessionExpected})");

            ushort attributeCount = GetAttributeCount(data);
            if (attributeCount != UtAttributeCountExpected)
                CtLogger.LogError("Data Mangle", "Attribute count was not returned correctly " +
                                                 $"(given={attributeCount}, expected={UtAttributeCountExpected})");

            ushort expectedValue = 1;
            for (int i = 0; i < attributeCount; ++i)
            {
                // ushort attributeType = GetAttributeType(data, i);
                // if (attributeType != expectedValue)
                //     Debug.LogError("Attribute type was not returned correctly " +
                //                    $"(given={attributeType}, expected={expectedValue})");
                ushort attributeRank = GetAttributeRank(data, i);
                if (attributeRank != expectedValue)
                    CtLogger.LogCritical("Data Mangle", "Attribute rank was not returned correctly " +
                                         $"(given={attributeRank}, expected={expectedValue})");
                expectedValue++;
            }
        }
    }
}