
using UdonSharp;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtWeaponDef : CtInventoryItemDef
    {
        [SerializeField] private EWeaponType weaponType;
        [SerializeField] private EAttributeType attributeType = EAttributeType.None;
        [SerializeField] [Range(0, 9)] private int attributeRequirement = 0;
        [SerializeField] private EDamageType damageType = EDamageType.Piercing;
        [SerializeField] private int damageMin = 15;
        [SerializeField] private int damageMax = 26;
        [SerializeField] private EItemRarity rarity = EItemRarity.None;
        [SerializeField] private CtUserData userData;

        public EWeaponType WeaponType => weaponType;
        public EAttributeType AttributeType => attributeType;
        public int AttributeRequirement => attributeRequirement;
        public EDamageType DamageType => damageType;
        public int DamageMin => damageMin;
        public int DamageMax => damageMax;
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
                    CtLogger.LogCritical("Weapon Definition", 
                        $"Prefix display name was not defined (prefix={prefix}).");
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
                    CtLogger.LogCritical("Weapon Definition",
                        $"Suffix display name was not defined (suffix={suffix}).");
                    return "<Invalid>";
            }
        }

        public ulong GenerateWeapon()
        {
            int req = attributeRequirement;
            int generatedRarity = (int)rarity;
            int rolledRarity = CtRandomizer.GetIntValue(generatedRarity + 1);
            req += generatedRarity - rolledRarity;

            return CtInventoryData.CreateWeaponData(
                Identifier,
                EWeaponPrefix.None,
                EWeaponSuffix.None,
                req,
                (EItemRarity)rolledRarity);
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

        private float _CritChance(int sourceLevel, int targetLevel, int weaponAttributeLevel)
        {
            int levelA = sourceLevel;
            int levelD = targetLevel;
            int weaponSkill = weaponAttributeLevel;

            float a = 8 * levelA;
            float b = 4 * weaponSkill;
            float c = 6 * Mathf.Min(weaponSkill, (levelA + 4) / 2);
            float d = 15 * levelD;
            float baseCriticalChance =
                (0.05f * Mathf.Pow(2, (a + b + c - d - 100) / 40) * (1.0f - weaponSkill * 0.01f)) +
                weaponSkill * 0.01f;
            return baseCriticalChance;
        }

        private bool _IsCritical(int sourceLevel, int targetLevel, int weaponAttributeLevel)
        {
            float criticalChance = _CritChance(sourceLevel, targetLevel, weaponAttributeLevel);
            return Random.Range(0, 1000) < criticalChance * 1000;
        }

        public int CalcDamage(int weaponAttributeLevel, int sourceWeaponAttributeLevel, int sourceLevel, 
            int targetLevel, int targetArmorRating, out bool isCritical)
        {
            isCritical = _IsCritical(sourceLevel, targetLevel, weaponAttributeLevel);

            int weaponDamage = isCritical
                ? (int)(damageMax * 1.2f)
                : Random.Range(damageMin, damageMax);

            int attributeThreshold = (sourceLevel + 4) / 2;
            int strikeLevel = isCritical
                ? weaponAttributeLevel + 20
                : 5 * Mathf.Min(weaponAttributeLevel, attributeThreshold) +
                  2 * Mathf.Max(0, weaponAttributeLevel - attributeThreshold);
            int damageTotal = CtEntityDef.CalculateDamage(weaponDamage, strikeLevel, targetArmorRating);
            damageTotal = Mathf.Max(0, damageTotal);

            // If source does not meet requirements to use the weapon.
            if (sourceWeaponAttributeLevel < weaponAttributeLevel)
                damageTotal = (int)(damageTotal * (1.0f / 3.0f));

            return damageTotal;
        }

        public void GetFormattedStats(ulong dataBlock, ref string weaponName, ref string stats, ref EItemRarity rarity, ref int requirement)
        {
            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;

            rarity = CtInventoryData.GetWeaponRarity(dataBlock);
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
                    CtLogger.LogCritical("Weapon Definition", $"Item rarity not supported (rarity={rarity}).");
                    break;
            }

            requirement = CtInventoryData.GetWeaponRequirement(dataBlock);

            weaponName = $"<color={color}>{DisplayName}</color>";

            stats = string.Empty;

            // if (prefix != EWeaponPrefix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponPrefix), prefix)}</color>\n";

            string damageTypeName = "???";
            switch (damageType)
            {
                case EDamageType.Slashing:
                    damageTypeName = "Slashing";
                    break;
                case EDamageType.Blunt:
                    damageTypeName = "Blunt";
                    break;
                case EDamageType.Piercing:
                    damageTypeName = "Piercing";
                    break;
                case EDamageType.Earth:
                    damageTypeName = "Earth";
                    break;
                case EDamageType.Fire:
                    damageTypeName = "Fire";
                    break;
                case EDamageType.Air:
                    damageTypeName = "Air";
                    break;
                case EDamageType.Water:
                    damageTypeName = "Water";
                    break;
                case EDamageType.Smiting:
                    damageTypeName = "Holy";
                    break;
                default:
                    CtLogger.LogCritical("Weapon Definition", 
                        $"Not supported weapon damage type (damageType={damageType}).");
                    break;
            }

            stats += $"{damageTypeName} dmg {damageMin}-{damageMax} (Requirement {requirement})\n";

            // if (suffix != EWeaponSuffix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponSuffix), suffix)}</color>\n";

            stats = stats.Trim();
        }
    }
}