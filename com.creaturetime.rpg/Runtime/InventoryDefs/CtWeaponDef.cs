
using UdonSharp;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtWeaponDef : CtInventoryItemDef
    {
        [SerializeField] private CtGameData gameData;

        [SerializeField] private EWeaponType weaponType;
        [SerializeField] private EWeaponAttackType attackType;
        [SerializeField] private ushort attributeType = CtConstants.InvalidId;
        [SerializeField] [Range(0, 9)] private int attributeRequirement = 0;
        [SerializeField] private EDamageType damageType = EDamageType.Piercing;
        [SerializeField] private int damageMin = 15;
        [SerializeField] private int damageMax = 26;
        [SerializeField] private EItemRarity rarity = EItemRarity.None;
        [SerializeField] private CtUserData userData;

        public EWeaponType WeaponType => weaponType;
        public EWeaponAttackType AttackType => attackType;
        public ushort AttributeType => attributeType;
        public int AttributeRequirement => attributeRequirement;
        public EDamageType DamageType => damageType;
        public int DamageMin => damageMin;
        public int DamageMax => damageMax;
        public EItemRarity Rarity => rarity;
        public CtUserData UserData => userData;

        public ulong GenerateWeapon()
        {
            int req = attributeRequirement;
            int generatedRarity = (int)rarity;
            int rolledRarity = CtRandomizer.GetIntValue(generatedRarity + 1);
            req += generatedRarity - rolledRarity;

            return CtDataBlock.CreateWeaponData(
                Identifier,
                EWeaponPrefix.None,
                EWeaponSuffix.None,
                req,
                (EItemRarity)rolledRarity);
        }

        public int CalcDamage(int reqAttributeRank, int attributeRank, int sourceLevel, 
            int targetLevel, int targetArmorRating, bool isCritical)
        {
            var weaponDamage = CtRpgFormulas.CalcWeaponDamage(isCritical, damageMin, damageMax);
            var damageTotal = CtRpgFormulas.CalcWeaponDamageModWithAttributeLevel(
                weaponDamage, isCritical, sourceLevel, reqAttributeRank, attributeRank, targetArmorRating);

#if DEBUG_LOGS
            LogDebug("Calculating weapon damage " +
                     $"(reqAttrLevel={reqAttributeRank}, attributeLevel={attributeRank}, " +
                     $"sourceLevel={sourceLevel}, targetLevel={targetLevel}, targetArmorRating={targetArmorRating}, " +
                     $"isCritical={isCritical}, damageTotal={damageTotal})");
#endif

            return damageTotal;
        }

        public void GetFormattedStats(ulong dataBlock, ref string weaponName, ref string stats, ref EItemRarity rarity, ref int requirement)
        {
            const string RarityDefaultColor = "#808080";
            const string RarityCommonColor = "#ffffff";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string color = RarityDefaultColor;

            rarity = CtDataBlock.GetWeaponRarity(dataBlock);
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
                    Debug.LogError($"Item rarity not supported (rarity={rarity}).");
#endif
                    break;
            }

            requirement = CtDataBlock.GetWeaponRequirement(dataBlock);

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
                case EDamageType.Holy:
                    damageTypeName = "Holy";
                    break;
                default:
#if DEBUG_LOGS
                    Debug.LogError($"Not supported weapon damage type (damageType={damageType}).");
#endif
                    break;
            }

            stats += $"{damageTypeName} dmg {damageMin}-{damageMax} (Requirement {requirement})\n";

            // if (suffix != EWeaponSuffix.None)
            //     stats += $"<color=#008000>{Enum.GetName(typeof(EWeaponSuffix), suffix)}</color>\n";

            stats = stats.Trim();
        }

        private static bool DoesAttackMiss(CtEntity source, CtEntity target)
        {
            bool missed = source.IsBlind && Random.Range(0.0f, 1.0f) <= 0.9f;
#if DEBUG_LOGS
            Debug.LogWarning($"Does attack miss? (missed={missed}).");
#endif

            if (missed)
            {
                CtSkillDef._ApplyDamage(0, source, target, source.MainHand.Identifier, EDamageSourceType.Weapon, EDamageType.Missed, false);
                return true;
            }

            missed = target.Evasion > 0 && Random.Range(0.0f, 1.0f) <= target.Evasion;
#if DEBUG_LOGS
            Debug.LogWarning($"Does attack get evaded? (missed={missed}).");
#endif

            if (missed)
            {
                CtSkillDef._ApplyDamage(0, source, target, source.MainHand.Identifier, EDamageSourceType.Weapon, EDamageType.Missed, false);
                return true;
            }

            return false;
        }

        public bool TryCalcMeleeAttack(CtEntity source, CtEntity target, bool isCritical, out int damage)
        {
            if (DoesAttackMiss(source, target))
            {
                damage = 0;
                return false;
            }

            var weaponDefinition = source.MainHand;
            var attributeRank = source.TryGetAttributeLevelByAttributeType(weaponDefinition.AttributeType);

            isCritical = CtRpgFormulas.IsCritical(source.Level, target.Level, attributeRank, source.CriticalChanceMod);

            int armorRating = CtSkillDef.CalculateArmorRating(gameData, target, damageType);
            armorRating = (int)(armorRating * (1.0f - source.ArmorPenetrationMod));

            int weaponAttributeLevel = CtDataBlock.GetWeaponRequirement(source.EntityDef.MainHandWeapon);
            damage = weaponDefinition.CalcDamage(weaponAttributeLevel, attributeRank, source.Level,
                target.Level, armorRating, isCritical);

            source.GainAdrenaline(25);

            return true;
        }
    }
}