
using System;
using UnityEngine;
using VRC.SDK3.Data;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    public abstract class CtSkillDef : CtInventoryItemDef
    {
        protected const string ValueColor = "#008000";

        protected const float PreCalcOneThirds = 1.0f / 3.0f;
        protected const float PreCalcTwoThirds = 1.0f / 3.0f;

        [SerializeField] private bool isWeaponSkill;
        [SerializeField] private ushort attributeType = CtConstants.InvalidId;
        [SerializeField] private ECombatEffectFlags flags;
        [SerializeField] private ETargetType targetType;
        [SerializeField] private ESkillSubType subType;
        [SerializeField] private bool isBeneficial;
        [SerializeField] private ESkillType skillType;
        [SerializeField] private int cost;
        [SerializeField] private int rechargeTime;

        public ETargetType TargetType => targetType;
        public ESkillSubType SubType => subType;
        public ushort AttributeType => attributeType;
        public ECombatEffectFlags Flags => flags;
        public bool IsWeaponSkill => isWeaponSkill;
        public bool IsBeneficial => isBeneficial;
        public ESkillType SkillType => skillType;
        public int Cost => cost;
        public int RechargeTime => rechargeTime;

        public bool IsTargetType(ETargetType value) => ((int)targetType & (int)value) != 0;

        public bool HasUse => ((int)flags & (int)ECombatEffectFlags.Use) != 0;
        public bool HasPersistentEffect => ((int)flags & (int)ECombatEffectFlags.PersistentEffect) != 0;
        public bool HasSkillUsedEffect => ((int)flags & (int)ECombatEffectFlags.SkillUsedEffect) != 0;
        public bool HasTickEffect => ((int)flags & (int)ECombatEffectFlags.TickEffect) != 0;

        public int Value => (SkillType == ESkillType.Adrenaline) ? Cost * 25 : Cost;

        public virtual void OnUse(CtGameData gameData, CtEntity source, CtEntity target, DataList adjacentTargets) {}
        public virtual void OnEntryEffect(CtEntity target, CtEntity source) {}
        public virtual void OnPersistentEffect(CtEntity target, CtEntity source) {}
        public virtual void OnSkillUsed(CtGameData gameData, CtEntity target, CtEntity source, CtSkillDef usedSkill) {}
        public virtual void OnTickEffect(CtEntity target, CtEntity source) {}
        public virtual void OnLeaveEffect(CtEntity target, CtEntity source) {}
        public virtual void OnTakeDamage(CtGameData gameData, CtEntity target, CtEntity source) {}

        public virtual string GetDescription(int attributeRank) => "<Invalid Description>";

        public static int CalcSkillValue(float baseValue, float valuePerAttribute, int attributeRank)
        {
            return Mathf.RoundToInt(baseValue + valuePerAttribute * attributeRank);
        }

        // private static void HandleTakeDamage(CtGameData gameData, CtEntity target, CtEntity source)
        // {
        //     for (int i = 0; i < target.StatusEffectInstances.Count; i++)
        //     {
        //         var statusEffect = target.StatusEffectInstances.GetStatusEffect(i);
        //         if (statusEffect == CtDataBlock.InvalidData) continue;
        //
        //         var effectIdentifier = CtDataBlock.GetEffectIdentifier(statusEffect);
        //         var skillDef = gameData.GetSkillDef(effectIdentifier);
        //         if (!skillDef.Flags.HasFlag(ECombatEffectFlags.TakeDamage)) continue;
        //
        //         skillDef.OnTakeDamage(gameData, target, source);
        //     }
        // }

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

            _ApplyDamage(skillValue, target, source, identifier, EDamageSourceType.Skill, EDamageType.None, false);
        }

        private static bool DoesAttackMiss(CtEntity target, CtEntity source, CtWeaponDef weaponDef)
        {
            bool missed = source.IsBlind && Random.Range(0.0f, 1.0f) <= 0.9f;
#if DEBUG_LOGS
            Debug.LogWarning($"Does attack miss? (missed={missed}).");
#endif

            if (missed)
                _ApplyDamage(0, target, source, weaponDef.Identifier, EDamageSourceType.Weapon, EDamageType.Missed, false);
            return missed;
        }

        private static bool IsAttackBlocked(CtEntity target, CtEntity source, CtWeaponDef weaponDef)
        {
            bool blocked = target.Block > 0 && Random.Range(0.0f, 1.0f) <= target.Block;
#if DEBUG_LOGS
            Debug.LogWarning($"Is attack blocked? (blocked={blocked}).");
#endif

            if (blocked)
                _ApplyDamage(0, target, source, weaponDef.Identifier, EDamageSourceType.Weapon, EDamageType.Blocked, false);
            return blocked;
        }

        public static void MeleeAttack(CtGameData gameData, CtEntity target, CtEntity source, float armorPenetration = 0)
        {
            int damage = _CalcMeleeAttack(gameData, target, source, armorPenetration, out var weaponDefinition, out var attributeRank, out var isCritical);
            if (DoesAttackMiss(target, source, weaponDefinition)) return;
            if (IsAttackBlocked(target, source, weaponDefinition)) return;

            source.GainAdrenaline(25);
            _MeleeApplyDamage(
                target, damage, weaponDefinition.DamageType, EDamageSourceType.Weapon, weaponDefinition.Identifier, source, isCritical);
        }

        private static int CalculateArmorRating(CtGameData gameData, CtEntity target, EDamageType damageType)
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
                    if (armorSetDef.IsAllowedProfession(target.Profession))
                    {
                        var armorSlot = armorSetDef.GetArmorSlot(armorHitRoll);
                        if (armorSlot)
                            armorSlot.CalcArmorRating(damageType);
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
                        Debug.LogWarning($"Target is wearing armor not valid for their profession (armorSetDef={armorSetDef}, profession={target.Profession}).");
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

                    // If target does not meet requirements to block.
                    if (attributeRank < reqRank)
                    {
                        additionalArmorRating /= 2;
                        armorRatingCap /= 2;
                    }

                    additionalArmorRating += reqRank;
                    armorRating += Mathf.Min(additionalArmorRating, armorRatingCap);
                }
            }

            switch (damageType)
            {
                case EDamageType.Slashing:
                    armorRating += target.SlashArmorIncrease - target.SlashArmorReduction;
                    break;
                case EDamageType.Blunt:
                    armorRating += target.BluntArmorIncrease - target.BluntArmorReduction;
                    break;
                case EDamageType.Piercing:
                    armorRating += target.PierceArmorIncrease - target.PierceArmorReduction;
                    break;
                case EDamageType.Earth:
                    armorRating += target.EarthArmorIncrease - target.EarthArmorReduction;
                    break;
                case EDamageType.Fire:
                    armorRating += target.FireArmorIncrease - target.FireArmorReduction;
                    break;
                case EDamageType.Air:
                    armorRating += target.AirArmorIncrease - target.AirArmorReduction;
                    break;
                case EDamageType.Water:
                    armorRating += target.WaterArmorIncrease - target.WaterArmorReduction;
                    break;
                case EDamageType.Holy:
                    armorRating += target.HolyArmorIncrease - target.HolyArmorReduction;
                    break;
                case EDamageType.Bleeding:
                case EDamageType.Burning:
                case EDamageType.Disease:
                case EDamageType.Poison:
                    break;
                default:
#if DEBUG_LOGS
                    // LogCritical($"Damage type not supported (damageType={damageType}.");
#endif
                    break;
            }

            armorRating += target.ArmorRatingIncrease - target.ArmorRatingReduction;
            armorRating = Mathf.Max(0, armorRating);

#if DEBUG_LOGS
            Debug.Log(
                "[Armor Rating] Additional armor rating " +
                $"(displayName=({target.DisplayName}), armorRating={armorRating}).");
#endif

            return armorRating;
        }

        private static int _CalcMeleeAttack(CtGameData gameData, CtEntity target, CtEntity source,
            float armorPenetration, out CtWeaponDef weaponDefinition, out int attributeRank, out bool isCritical)
        {
            ushort identifier = CtDataBlock.GetWeaponIdentifier(source.EntityDef.MainHandWeapon);
            weaponDefinition = gameData.GetWeaponDef(identifier);
#if DEBUG_LOGS
            if (!weaponDefinition)
                Debug.LogError($"Weapon could not be found (identifier={identifier})");
#endif

            attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, weaponDefinition.AttributeType);

            int armorRating = CalculateArmorRating(gameData, target, weaponDefinition.DamageType);
            armorRating = (int)(armorRating * (1.0f - armorPenetration));

            int weaponAttributeLevel = CtDataBlock.GetWeaponRequirement(source.EntityDef.MainHandWeapon);
            return weaponDefinition.CalcDamage(weaponAttributeLevel, attributeRank, source.EntityDef.CharacterLevel,
                target.EntityDef.CharacterLevel, armorRating, out isCritical);
        }

        public static void MeleeSkill(CtGameData gameData, CtEntity target, CtEntity source, ushort skillId, 
            int damageBase, float damagePerAttribute, float armorPenetration = 0)
        {
            // Skill Weapon Damage
            int damage = _CalcMeleeAttack(
                gameData, target, source, armorPenetration, out var weaponDefinition, out var attributeRank, out var isCritical);
            if (DoesAttackMiss(target, source, weaponDefinition)) return;
            if (IsAttackBlocked(target, source, weaponDefinition)) return;

            // int armorRating = CalculateArmorRating(gameData, target, weaponDefinition.DamageType);
            damage += CalcDamage(
                damageBase, damagePerAttribute, attributeRank, source.EntityDef.CharacterLevel, 0);
            _MeleeApplyDamage(
                target, damage, weaponDefinition.DamageType, EDamageSourceType.Skill, skillId, source, isCritical);
        }

        private static void _MeleeApplyDamage(CtEntity target, int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, ushort identifier, CtEntity source, bool isCritical)
        {
            int adrenaline = 25;
#if DEBUG_LOGS
            Debug.Log($"Adrenaline gained due to weapon used (adrenaline={adrenaline}).");
#endif
            source.GainAdrenaline(adrenaline);

            _ApplyDamage(damage, target, source, identifier, damageSourceType, damageType, isCritical);
        }

        public static void SpellSkill(CtGameData gameData, CtEntity target, CtEntity source, 
            ushort attributeType, ushort skillId, EDamageType damageType, int damageBase, 
            float damagePerAttribute)
        {
            int armorRating = CalculateArmorRating(gameData, target, damageType);

            int attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, attributeType);
            int damage = CalcDamage(damageBase, damagePerAttribute, attributeRank, source.EntityDef.CharacterLevel,
                armorRating);
            _ApplyDamage(damage, target, source, skillId, EDamageSourceType.Skill, damageType, false);
        }

        private static void _ApplyDamage(int damage, CtEntity target, CtEntity source,
            ushort skillId, EDamageSourceType damageSourceType, EDamageType damageType, bool isCritical)
        {
            if (damage >= 0)
            {
                // Pre-damage calculations.
                target.GainAdrenalineOnHit(damage);

                // Check for resistances.
                int damageMod = 0;

                // Ignore condition damage.
                switch (damageType)
                {
                    case EDamageType.Bleeding:
                    case EDamageType.Burning:
                    case EDamageType.Disease:
                    case EDamageType.Poison:
                        break;
                    default:
                        damageMod += target.DamageIncrease - target.DamageReduction;
                        break;
                }

                switch (damageType)
                {
                    case EDamageType.Slashing:
                        damageMod += target.SlashDamageReduction - target.SlashDamageReduction;
                        break;
                    case EDamageType.Blunt:
                        damageMod += target.BluntDamageReduction - target.BluntDamageReduction;
                        break;
                    case EDamageType.Piercing:
                        damageMod += target.PierceDamageReduction - target.PierceDamageReduction;
                        break;
                    case EDamageType.Earth:
                        damageMod += target.EarthDamageReduction - target.EarthDamageReduction;
                        break;
                    case EDamageType.Fire:
                        damageMod += target.FireDamageReduction - target.FireDamageReduction;
                        break;
                    case EDamageType.Air:
                        damageMod += target.AirDamageReduction - target.AirDamageReduction;
                        break;
                    case EDamageType.Water:
                        damageMod += target.WaterDamageIncrease - target.WaterDamageReduction;
                        break;
                    case EDamageType.Holy:
                        damageMod += target.HolyDamageReduction - target.HolyDamageReduction;
                        break;
                    case EDamageType.Bleeding:
                    case EDamageType.Burning:
                    case EDamageType.Disease:
                    case EDamageType.Poison:
                        break;
                    default:
#if DEBUG_LOGS
                        // LogCritical($"Damage type not supported (damageType={damageType}.");
#endif
                        break;
                }

                damage += damageMod;

                // Make sure that we do not resist more than the damage, so we do not heal on accident.
                damage = Mathf.Max(damage, 0);

                // // Calculate damage so we don't overkill.
                // damage = Mathf.Min(target.Health, damage);

                // // Update total damage resisted stats.
                // DamageTakenResisted += resistedDamage;
                // instigator.DamageResisted += resistedDamage;

                // // Update total damage taken stats.
                // DamageTaken += damage;
                // instigator.DamageDealt += damage;
            }

            target.ApplyDamage(damage, damageType, damageSourceType, skillId, source, isCritical);
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