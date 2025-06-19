
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtOffHandDef : CtInventoryItemDef
    {
        [SerializeField] private EOffHandType offHandType = EOffHandType.None;
        [SerializeField] private EAttributeType attributeType = EAttributeType.None;
        [SerializeField] [Range(0, 9)] private int attributeRequirement = 0;
        [SerializeField] private int minModifierStat = 8;
        [SerializeField] private int maxModifierStat = 16;
        [SerializeField] private EItemRarity rarity = EItemRarity.None;
        [SerializeField] private CtUserData userData;

        public EOffHandType OffHandType => offHandType;
        public EAttributeType AttributeType => attributeType;
        public int AttributeRequirement => attributeRequirement;
        public int MinModifierStat => minModifierStat;
        public int MaxModifierStat => maxModifierStat;
        public EItemRarity Rarity => rarity;
        public CtUserData UserData => userData;

        public static string GetPrefixName(EWeaponPrefix prefix)
        {
            switch (prefix)
            {
                case EWeaponPrefix.None:
                    return string.Empty;
                case EWeaponPrefix.Barbed:
                    return "Barbed";
                case EWeaponPrefix.Ebon:
                    return "Ebon";
                case EWeaponPrefix.Fiery:
                    return "Fiery";
                case EWeaponPrefix.Shocking:
                    return "Shocking";
                case EWeaponPrefix.Icy:
                    return "Icy";
                default:
#if DEBUG_LOGS
                    CtLogger.LogCritical("Off-Hand Definition", 
                        $"Prefix display name was not defined (prefix={prefix}).");
#endif
                    return "<Invalid>";
            }
        }

        public static string GetSuffixName(EWeaponSuffix suffix)
        {
            switch (suffix)
            {
                case EWeaponSuffix.None:
                    return string.Empty;
                case EWeaponSuffix.Defense:
                    return "Defense";
                case EWeaponSuffix.Shelter:
                    return "Shelter";
                case EWeaponSuffix.Warding:
                    return "Warding";
                case EWeaponSuffix.Enchanting:
                    return "Enchanting";
                default:
#if DEBUG_LOGS
                    CtLogger.LogCritical("Off-Hand Definition", 
                        $"Suffix display name was not defined (suffix={suffix}).");
#endif
                    return "<Invalid>";
            }
        }

        public ulong Generate()
        {
            int req = attributeRequirement;
            int generatedRarity = (int)rarity;
            int rolledRarity = CtRandomizer.GetIntValue(generatedRarity + 1);
            req += generatedRarity - rolledRarity;
            int modifierStats = CtRandomizer.GetIntValue(minModifierStat, maxModifierStat);

            return CtDataBlock.CreateOffHandData(
                Identifier,
                EOffHandPrefix.None,
                EOffHandSuffix.None,
                req,
                (EItemRarity)rolledRarity,
                modifierStats);
        }

        public string BuildFullDisplayName(EWeaponPrefix prefix, EWeaponSuffix suffix)
        {
            string fullName = DisplayName;
            if (prefix != EWeaponPrefix.None)
            {
                string prefixName = GetPrefixName(prefix);
                if (!string.IsNullOrEmpty(prefixName))
                    fullName = $"{prefixName} {fullName}";
            }

            if (suffix != EWeaponSuffix.None)
            {
                string suffixName = GetSuffixName(suffix);
                if (!string.IsNullOrEmpty(suffixName))
                    fullName = $"{fullName} of {suffixName}";
            }

            return fullName;
        }

        public void GetFormattedStats(ulong dataBlock, ref string weaponName, ref string stats, ref EItemRarity rarity, 
            ref int modifierStat, ref int requirement)
        {
            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;

            rarity = CtDataBlock.GetOffHandRarity(dataBlock);
            switch (rarity)
            {
                case EItemRarity.None:
                    color = RarityDefaultColor;
                    break;
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
#if DEBUG_LOGS
                    CtLogger.LogCritical("Off-Hand Definition", $"Item rarity not supported (rarity={rarity}).");
#endif
                    break;
            }

            requirement = CtDataBlock.GetOffHandRequirement(dataBlock);
            modifierStat = CtDataBlock.GetOffHandModifierStat(dataBlock);

            weaponName = $"<color={color}>{DisplayName}</color>";

            stats = string.Empty;

            // if (prefix != EWeaponPrefix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponPrefix), prefix)}</color>\n";

            // if (suffix != EWeaponSuffix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponSuffix), suffix)}</color>\n";

            stats = stats.Trim();
        }
    }
}