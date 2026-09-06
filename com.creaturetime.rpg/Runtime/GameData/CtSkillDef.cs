
using UnityEngine;
using VRC.SDK3.Data;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    public abstract class CtSkillDef : CtAbstractDefinition
    {
        [SerializeField, HideInInspector] protected CtGameData gameData;

        protected const string ValueColor = "#008000";

        protected const float PreCalcOneThirds = 1.0f / 3.0f;
        protected const float PreCalcTwoThirds = 1.0f / 3.0f;

        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private Texture2D icon;
        [SerializeField] private bool isWeaponSkill;
        [SerializeField] private ushort attributeType = CtConstants.InvalidId;
        [SerializeField] private ECombatEffectFlags flags;
        [SerializeField] private ETargetType targetType;
        [SerializeField] private ESkillSubType subType;
        [SerializeField] private bool isBeneficial;
        [SerializeField] private ESkillType skillType;
        [SerializeField] private int cost;
        [SerializeField] private int rechargeTime;

        public string DisplayName => displayName;
        public Texture2D Icon => icon;
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
        public bool HasBlockEffect => ((int)flags & (int)ECombatEffectFlags.BlockEffect) != 0;

        public int Value => (SkillType == ESkillType.Adrenaline) ? Cost * 25 : Cost;

        public virtual void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets) {}
        public virtual void OnEntryEffect(CtEntity source, CtEntity target) {}
        public virtual void OnPersistentEffect(CtEntity source, CtEntity target) {}
        public virtual void OnSkillUsed(CtEntity source, CtEntity target, CtSkillDef usedSkill) {}
        public virtual void OnTickEffect(CtEntity source, CtEntity target) {}
        public virtual void OnLeaveEffect(CtEntity source, CtEntity target) {}
        public virtual bool TryBlock(CtEntity source, CtEntity target, int damage) => false;

        public virtual void OnPreRollDamage(CtEntity source, CtEntity target, EDamageType damageType) { }
        public virtual void OnPostRollDamage(CtEntity source, CtEntity target, EDamageType damageType, int damage, bool isCritical) { }

        public virtual string GetDescription(int attributeRank) => "<Invalid Description>";

        public string GetDebugDescription() => GetDescription(12);

//         private static bool DoesAttackMiss(CtEntity source, CtEntity target)
//         {
//             bool missed = source.IsBlind && Random.Range(0.0f, 1.0f) <= 0.9f;
// #if DEBUG_LOGS
//             Debug.LogWarning($"Does attack miss? (missed={missed}).");
// #endif
//
//             if (missed)
//             {
//                 _ApplyDamage(0, source, target, source.MainHand.Identifier, EDamageSourceType.Weapon, EDamageType.Missed, false);
//                 return true;
//             }
//
//             missed = target.Evasion > 0 && Random.Range(0.0f, 1.0f) <= target.Evasion;
// #if DEBUG_LOGS
//             Debug.LogWarning($"Does attack get evaded? (missed={missed}).");
// #endif
//
//             if (missed)
//             {
//                 _ApplyDamage(0, source, target, source.MainHand.Identifier, EDamageSourceType.Weapon, EDamageType.Missed, false);
//                 return true;
//             }
//
//             return false;
//         }

        private static bool IsAttackBlocked(CtEntity source, CtEntity target, int damage)
        {
            if (target.TryBlockWithEffects(source, damage)) return false;

            bool blocked = target.Block > 0 && Random.Range(0.0f, 1.0f) <= target.Block;
#if DEBUG_LOGS
            Debug.LogWarning($"Is attack blocked? (blocked={blocked}).");
#endif

            if (blocked)
                _ApplyDamage(0, source, target, source.MainHand.Identifier, EDamageSourceType.Weapon, EDamageType.Blocked, false);
            return blocked;
        }

        public void HealingSkill(CtEntity source, CtEntity target, int healingBase, float healingPerAttribute, 
            ushort attributeType)
        {
            int attributeRank =
                source.TryGetAttributeLevelByAttributeType(attributeType);
            int skillValue = -CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);

            _ApplyDamage(skillValue, source, target, Identifier, EDamageSourceType.Skill, EDamageType.None, false);
        }

        // private static bool _TryCalcMeleeAttack(CtGameData gameData, CtEntity source, CtEntity target, 
        //     EDamageType damageType, out int damage, out bool isCritical)
        // {
        //     if (DoesAttackMiss(source, target))
        //     {
        //         damage = 0;
        //         isCritical = false;
        //         return false;
        //     }
        //
        //     var weaponDefinition = source.MainHand;
        //     var attributeRank = source.TryGetAttributeLevelByAttributeType(weaponDefinition.AttributeType);
        //
        //     isCritical = CtRpgFormulas.IsCritical(source.Level, target.Level, attributeRank, source.CriticalChanceMod);
        //
        //     int armorRating = CalculateArmorRating(gameData, target, damageType);
        //     armorRating = (int)(armorRating * (1.0f - source.ArmorPenetrationMod));
        //
        //     int weaponAttributeLevel = CtDataBlock.GetWeaponRequirement(source.EntityDef.MainHandWeapon);
        //     damage = weaponDefinition.CalcDamage(weaponAttributeLevel, attributeRank, source.Level,
        //         target.Level, armorRating, isCritical);
        //
        //     source.GainAdrenaline(25);
        //
        //     return true;
        // }

        public static void MeleeAttack(CtGameData gameData, CtEntity source, CtEntity target)
        {
            source.UpdatePersistantEffects();
            target.UpdatePersistantEffects();

            var damageType = source.ConvertDamageType(source.MainHand.DamageType);
            var attributeRank = source.TryGetAttributeLevelByAttributeType(source.MainHand.AttributeType);
            var isCritical = CtRpgFormulas.IsCritical(source.Level, target.Level, attributeRank, source.CriticalChanceMod);

            if (!source.MainHand.TryCalcMeleeAttack(source, target, isCritical, out var damage)) return;

            damage += source.DamageMod;

            if (IsAttackBlocked(source, target, damage)) return;
            MeleeApplyDamage(target, damage, damageType, EDamageSourceType.Weapon,
                source.MainHand.Identifier, source, isCritical);
        }

        public void MeleeSkill(CtEntity source, CtEntity target, int damageBase, float damagePerAttribute, 
            ushort attributeType)
        {
            source.UpdatePersistantEffects();
            target.UpdatePersistantEffects();

            var damageType = source.ConvertDamageType(source.MainHand.DamageType);
            var attributeRank = source.TryGetAttributeLevelByAttributeType(source.MainHand.AttributeType);
            var isCritical = CtRpgFormulas.IsCritical(source.Level, target.Level, attributeRank, source.CriticalChanceMod);

            OnPreRollDamage(source, target, damageType);

            if (!source.MainHand.TryCalcMeleeAttack(source, target, isCritical, out var damage)) return;
            OnPostRollDamage(source, target, damageType, damage, isCritical);

            damage += source.DamageMod;

            if (IsAttackBlocked(source, target, damage)) return;

            var skillAttributeRank = source.TryGetAttributeLevelByAttributeType(attributeType);
            damage += CtRpgFormulas.CalcDamage(
                damageBase, damagePerAttribute, skillAttributeRank, source.Level, 0);
            MeleeApplyDamage(target, damage, damageType, EDamageSourceType.Skill,
                Identifier, source, isCritical);
        }

        public static void MeleeApplyDamage(CtEntity target, int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, ushort identifier, CtEntity source, bool isCritical)
        {
            int adrenaline = 25;
#if DEBUG_LOGS
            Debug.Log($"Adrenaline gained due to weapon used (adrenaline={adrenaline}).");
#endif
            source.GainAdrenaline(adrenaline);

            _ApplyDamage(damage, source, target, identifier, damageSourceType, damageType, isCritical);
        }

        public void SpellSkill(CtEntity source, CtEntity target, EDamageType damageType, 
            int damageBase, float damagePerAttribute, ushort attributeType)
        {
            source.UpdatePersistantEffects();
            target.UpdatePersistantEffects();

            damageType = source.ConvertDamageType(damageType);
            OnPreRollDamage(source, target, damageType);

            var armorRating = CalculateArmorRating(gameData, target, damageType);
            var attributeRank = source.TryGetAttributeLevelByAttributeType(attributeType);
            var damage = CtRpgFormulas.CalcDamage(damageBase, damagePerAttribute, attributeRank, source.Level,
                armorRating);

            OnPostRollDamage(source, target, damageType, damage, false);

            damage += source.DamageMod;

            _ApplyDamage(damage, source, target, Identifier, EDamageSourceType.Skill, damageType, false);
        }

        public static int CalculateArmorRating(CtGameData gameData, CtEntity target, EDamageType damageType)
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
                    if (armorSetDef.IsAllowedProfession(target.ProfessionDef))
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
                        Debug.LogWarning($"Target is wearing armor not valid for their profession (armorSetDef={armorSetDef}, profession={target.ProfessionDef}).");
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

            ulong offHandWeaponData = target.OffHandData;
            if (CtDataBlock.IsValid(offHandWeaponData))
            {
                var offHandIdentifier = CtDataBlock.GetOffHandIdentifier(offHandWeaponData);
                if (offHandIdentifier != CtConstants.InvalidId)
                {
                    CtOffHandDef offHandDefinition = gameData.GetOffHandDef(offHandIdentifier);
                    if (offHandDefinition.OffHandType == EOffHandType.Shield)
                    {
                        int attributeRank =
                            target.TryGetAttributeLevelByAttributeType(offHandDefinition.AttributeType);
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
            }

            switch (damageType)
            {
                case EDamageType.Slashing:
                    armorRating += target.SlashArmorMod;
                    break;
                case EDamageType.Blunt:
                    armorRating += target.BluntArmorMod;
                    break;
                case EDamageType.Piercing:
                    armorRating += target.PiercingArmorMod;
                    break;
                case EDamageType.Earth:
                    armorRating += target.EarthArmorMod;
                    break;
                case EDamageType.Fire:
                    armorRating += target.FireArmorMod;
                    break;
                case EDamageType.Air:
                    armorRating += target.AirArmorMod;
                    break;
                case EDamageType.Water:
                    armorRating += target.WaterArmorMod;
                    break;
                case EDamageType.Holy:
                    armorRating += target.HolyArmorMod;
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

            armorRating += target.ArmorRatingMod;
            armorRating = Mathf.Max(0, armorRating);

#if DEBUG_LOGS
            Debug.Log(
                "[Armor Rating] Additional armor rating " +
                $"(displayName=({target.DisplayName}), armorRating={armorRating}).");
#endif

            return armorRating;
        }

        public static void _ApplyDamage(int damage, CtEntity source, CtEntity target,
            ushort skillId, EDamageSourceType damageSourceType, EDamageType damageType, bool isCritical)
        {
            if (damage > 0)
            {
                if (target.AbsorbShield > 0)
                {
                    damage = Mathf.Max(damage - target.AbsorbShield, 0);
#if DEBUG_LOGS
                    Debug.Log($"Absorbed {Mathf.Min(damage, target.AbsorbShield)} damage (damage={damage}).");
#endif
                }

                // Pre-damage calculations.
                target.GainAdrenalineOnHit(damage);

                // Check for resistances.
                int damageMod = 0;
                switch (damageType)
                {
                    case EDamageType.Slashing:
                        damageMod += target.SlashDamageMod;
                        break;
                    case EDamageType.Blunt:
                        damageMod += target.BluntDamageMod;
                        break;
                    case EDamageType.Piercing:
                        damageMod += target.PiercingDamageMod;
                        break;
                    case EDamageType.Earth:
                        damageMod += target.EarthDamageMod;
                        break;
                    case EDamageType.Fire:
                        damageMod += target.FireDamageMod;
                        break;
                    case EDamageType.Air:
                        damageMod += target.AirDamageMod;
                        break;
                    case EDamageType.Water:
                        damageMod += target.WaterDamageMod;
                        break;
                    case EDamageType.Holy:
                        damageMod += target.HolyDamageMod;
                        break;
                    case EDamageType.Bleeding:
                    case EDamageType.Burning:
                    case EDamageType.Disease:
                    case EDamageType.Poison:
                        break;
                }

                damage += damageMod;

                // Make sure that we do not resist more than the damage, so we do not heal on accident.
                damage = Mathf.Max(damage, 0);
            }

            target.ApplyDamage(damage, damageType, damageSourceType, skillId, source, isCritical);
        }
    }
}