
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtSkillDef : CtInventoryItemDef
    {
        protected const string ValueColor = "#008000";

        protected const float PreCalcOneThirds = 1.0f / 3.0f;
        protected const float PreCalcTwoThirds = 1.0f / 3.0f;

        [SerializeField] private ushort attributeType;
        [SerializeField, HideInInspector] private ECombatEffectFlags flags;

        public ushort AttributeType => attributeType;
        public ECombatEffectFlags Flags => flags;

        public bool HasUse => ((int)flags & (int)ECombatEffectFlags.Use) != 0;
        public bool HasPersistentEffect => ((int)flags & (int)ECombatEffectFlags.PersistentEffect) != 0;
        public bool HasSkillUsedEffect => ((int)flags & (int)ECombatEffectFlags.SkillUsedEffect) != 0;
        public bool HasTickEffect => ((int)flags & (int)ECombatEffectFlags.TickEffect) != 0;

        public virtual bool IsBeneficial => false;
        public virtual ESkillType Type => ESkillType.Energy;
        public virtual int Cost => 0;
        public virtual int RechargeTime => 0;
        public virtual ETargetType TargetType => ETargetType.None;

        public int Value => (Type == ESkillType.Adrenaline) ? Cost * 25 : Cost;

        public virtual void OnUse(CtGameData gameData, CtEntity target, CtEntity source) {}
        public virtual void OnEntryEffect(CtEntity target, CtEntity source) {}
        public virtual void OnPersistentEffect(CtEntity target, CtEntity source) {}
        public virtual void OnSkillUsed(CtGameData gameData, CtEntity target, CtEntity source, CtSkillDef usedSkill) {}
        public virtual void OnTickEffect(CtEntity target, CtEntity source) {}
        public virtual void OnLeaveEffect(CtEntity target, CtEntity source) {}

        public virtual string GetDescription(int attributeRank) => "<Invalid Description>";

        public static int CalcSkillValue(float baseValue, float valuePerAttribute, int attributeRank)
        {
            return Mathf.RoundToInt(baseValue + valuePerAttribute * attributeRank);
        }

        public static int CalcValue(int skillValue, int characterLevel, int targetArmorLevel)
        {
            int strikeLevel = 3 * characterLevel;
            int damageTotal = CtEntityDef.CalculateDamage(skillValue, strikeLevel, targetArmorLevel);
            return damageTotal;
        }

        public static int CalcSkillValueWithStrikeLevel(int baseDamage, int strikeLevel, int targetArmorLevel)
        {
            return (int)(baseDamage * Mathf.Pow(2, (strikeLevel - targetArmorLevel) / 40.0f));
        }

        public static int CalcDamage(float baseValue, float valuePerAttribute, int attributeRank,
            int characterLevel, int targetArmorLevel)
        {
            int skillValue = CalcSkillValue(baseValue, valuePerAttribute, attributeRank);
            return CalcValue(skillValue, characterLevel, targetArmorLevel);
        }

        public static int CalcHeal(float baseValue, float valuePerAttribute, int attributeRank)
        {
            int skillValue = CalcSkillValue(baseValue, valuePerAttribute, attributeRank);
            int damageTotal = CalcSkillValueWithStrikeLevel(skillValue, 0, 0);
            return damageTotal;
        }

        public static void HealingSkill(CtGameData gameData, CtEntity target, CtEntity source, 
            ushort attributeType, ushort identifier, int healingBase, float healingPerAttribute)
        {
            int attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, attributeType);
            int skillValue = CalcSkillValue(-healingBase, healingPerAttribute, attributeRank);
            target.ApplyDamage(skillValue, EDamageType.Healing, EDamageSourceType.Skill, identifier, source, false);
        }

        public static void MeleeAttack(CtGameData gameData, CtEntity target, CtEntity source)
        {
            int damage = _CalcMeleeAttack(gameData, target, source, out var weaponDefinition, out var attributeRank, out var isCritical);
            target.ApplyDamage(damage, weaponDefinition.DamageType, EDamageSourceType.Weapon, 
                weaponDefinition.Identifier, source, isCritical);
        }

        private static int CalculateArmorRating(CtGameData gameData, CtEntity target)
        {
            var armorHitRoll = CtArmorSetDef.RollArmorHit();
            ulong armorData;
            switch (armorHitRoll)
            {
                case EArmorSlot.Head:
                    armorData = target.EntityDef.HeadSlot;
                    break;
                case EArmorSlot.Chest:
                    armorData = target.EntityDef.ChestSlot;
                    break;
                case EArmorSlot.Hands:
                    armorData = target.EntityDef.HandsSlot;
                    break;
                case EArmorSlot.Legs:
                    armorData = target.EntityDef.LegsSlot;
                    break;
                case EArmorSlot.Feet:
                    armorData = target.EntityDef.FeetSlot;
                    break;
                default:
                    return 0;
            }

            int armorRating = 0;
            if (CtDataBlock.IsValid(armorData))
            {
                ushort identifier = CtDataBlock.GetEquipmentIdentifier(armorData);
                CtArmorSetDef armorSetDef = gameData.GetArmorDef(identifier);
                if (armorSetDef)
                {
                    var armorSlot = armorSetDef.GetArmorSlot(armorHitRoll);
                    if (armorSlot)
                        armorRating = armorSlot.ArmorRating;
#if DEBUG_LOGS
                else
                {
                    Debug.LogWarning($"Failed to find armor slot (armorSlot={armorSlot}).");
                }
#endif
                }
#if DEBUG_LOGS
                else
                {
                    Debug.LogWarning($"Armor identifier was not found (identifier={identifier}).");
                }
#endif
            }

            ulong offHandWeaponData = target.EntityDef.OffHandWeapon;
            if (CtDataBlock.IsValid(offHandWeaponData))
            {
                ushort offHandIdentifier = CtDataBlock.GetOffHandIdentifier(offHandWeaponData);
                CtOffHandDef offHandDefinition = gameData.GetOffHandDef(offHandIdentifier);
                if (offHandDefinition.OffHandType == EOffHandType.Shield)
                {
                    int attributeRank = 
                        TryGetAttributeLevelByAttributeType(gameData, target.EntityDef, offHandDefinition.AttributeType);
                    int reqRank = CtDataBlock.GetOffHandRequirement(offHandWeaponData);

                    int additionalArmorRating = CtDataBlock.GetOffHandModifierStat(offHandWeaponData);
                    int armorRatingCap = 16;

                    // If source does not meet requirements to use the weapon.
                    if (reqRank <= attributeRank)
                    {
                        additionalArmorRating /= 2;
                        armorRatingCap /= 2;
                    }

                    additionalArmorRating += reqRank;
                    armorRating += Mathf.Min(additionalArmorRating, armorRatingCap);
                }
            }

            armorRating -= target.ArmorRatingReduction;
            armorRating = Mathf.Max(0, armorRating);

#if DEBUG_LOGS
            Debug.Log(
                "[Armor Rating] Additional armor rating " +
                $"(displayName=({target.DisplayName}), armorRating={armorRating}).");
#endif

            return armorRating;
        }

        private static int _CalcMeleeAttack(CtGameData gameData, CtEntity target, CtEntity source,
            out CtWeaponDef weaponDefinition, out int attributeRank, out bool isCritical)
        {
            ushort identifier = CtDataBlock.GetWeaponIdentifier(source.EntityDef.MainHandWeapon);
            weaponDefinition = gameData.GetWeaponDef(identifier);
#if DEBUG_LOGS
            if (!weaponDefinition)
                Debug.Log($"Weapon could not be found (identifier={identifier})");
#endif

            attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, weaponDefinition.AttributeType);

            int armorRating = CalculateArmorRating(gameData, target);

            int weaponAttributeLevel = CtDataBlock.GetWeaponRequirement(source.EntityDef.MainHandWeapon);
            return weaponDefinition.CalcDamage(weaponAttributeLevel, attributeRank, source.EntityDef.CharacterLevel,
                target.EntityDef.CharacterLevel, armorRating, out isCritical);
        }

        public static void MeleeSkill(CtGameData gameData, CtEntity target, CtEntity source, ushort skillId, 
            int damageBase, float damagePerAttribute, float armorPenetration = 0)
        {
            int armorRating = CalculateArmorRating(gameData, target);

            // Skill Weapon Damage
            int damage = _CalcMeleeAttack(
                gameData, target, source, out var weaponDefinition, out var attributeRank, out var isCritical);
            damage += CalcDamage(
                damageBase, damagePerAttribute, attributeRank, source.EntityDef.CharacterLevel, armorRating);
            target.ApplyDamage(
                damage, weaponDefinition.DamageType, EDamageSourceType.Skill, skillId, source, isCritical);
        }

        public static void SpellSkill(CtGameData gameData, CtEntity target, CtEntity source, 
            ushort attributeType, ushort skillId, EDamageType damageType, int damageBase, 
            float damagePerAttribute)
        {
            int armorRating = CalculateArmorRating(gameData, target);

            int attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, attributeType);
            int damage = CalcDamage(damageBase, damagePerAttribute, attributeRank, source.EntityDef.CharacterLevel,
                armorRating);
            target.ApplyDamage(damage, damageType, EDamageSourceType.Skill, skillId, source, false);
        }

        public static int CalcAttributePoints(int level)
        {
            int attributePoints = 0;

            // Calclute points by level.
            for (int i = 1; i < level + 1; ++i)
            {
                if (i > 20)
                    continue;

                if (i > 1)
                    attributePoints += 5;
                if (i > 10)
                    attributePoints += 5;
                if (i > 15)
                    attributePoints += 5;
            }

            // TODO: How did we want to access these 30 extra points? Maybe unlocked by doing something special?
            attributePoints += 30;

            return attributePoints;
        }

        public static int TryGetAttributeLevelByAttributeType(CtGameData gameData, CtEntityDef entityStats, ushort attributeType)
        {
            ushort profession = CtDataBlock.GetProfession(entityStats.AttributeData);
            CtProfessionDef professionDefinition = gameData.GetProfessionDef(profession);
            for (int i = 0; i < professionDefinition.Attributes.Length; ++i)
                if (professionDefinition.Attributes[i].Identifier == attributeType)
                    return CtDataBlock.GetAttributeRank(entityStats.AttributeData, i);

            return 0;
        }
    }
}