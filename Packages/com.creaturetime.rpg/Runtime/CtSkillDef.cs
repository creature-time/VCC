
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSkillDef : CtAbstractDefinition
    {
        public const string ValueColor = "#008000";

        public const float PreCalcOneThirds = 1.0f / 3.0f;
        public const float PreCalcTwoThirds = 1.0f / 3.0f;

        [SerializeField] private Texture2D icon;
        [SerializeField] [HideInInspector] private ECombatEffectFlags flags;

        public string DebugDisplayName => $"{DisplayName} [{Identifier}]";

        public Texture2D Icon => icon;
        public ECombatEffectFlags Flags => flags;

        public bool IsUse => ((int)Flags & (int)ECombatEffectFlags.Use) != 0;
        public bool IsPersistentEffect => ((int)Flags & (int)ECombatEffectFlags.PersistentEffect) != 0;
        public bool IsSkillUsedEffect => ((int)Flags & (int)ECombatEffectFlags.SkillUsedEffect) != 0;
        public bool IsTickEffect => ((int)Flags & (int)ECombatEffectFlags.TickEffect) != 0;

        public virtual bool IsBeneficial => false;
        public virtual string DisplayName => "<Invalid>";
        public virtual EAttributeType AttributeType => EAttributeType.None;
        public virtual ESkillType Type => ESkillType.Energy;
        public virtual int Cost => 0;
        public virtual int RechargeTime => 0;
        public virtual ETargetType TargetType => ETargetType.None;

        public int Value => (Type == ESkillType.Adrenaline) ? Cost * 25 : Cost;

        public virtual void OnUse(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source) {}
        public virtual void OnEntryEffect(CtEntity target, CtEntity source) {}
        public virtual void OnPersistentEffect(ushort instanceId, CtEntity target, CtEntity source) {}
        public virtual void OnSkillUsed(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source,
            CtSkillDef usedSkill) {}
        public virtual void OnTickEffect(ushort instanceId, CtEntity target, CtEntity source) {}
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

        public static void HealingSkill(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source, EAttributeType attributeType,
            ushort identifier, int healingBase, float healingPerAttribute)
        {
            int attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityStats, attributeType);
            int skillValue = CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            target.ApplyHeal(instanceId, skillValue, identifier, source);

        }

        public static void MeleeAttack(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source)
        {
            int damage = _CalcMeleeAttack(gameData, target, source, out var weaponDefinition, out var attributeRank, out var isCritical);
            target.ApplyDamage(instanceId, damage, weaponDefinition.DamageType, EDamageSourceType.Weapon, 
                weaponDefinition.Identifier, source, isCritical);
        }

        private static int CalculateArmorRating(CtGameData gameData, CtEntity target)
        {
            int armorHit = CtArmorDef.GetArmorIndex(CtArmorDef.RollArmorHit());
            ulong armorData = target.EntityStats.EquipmentData[armorHit];
            int armorRating = 0;
            if (CtDataBlock.IsValid(armorData))
            {
                ushort armorIdentifier = CtDataBlock.GetEquipmentIdentifier(armorData);
                CtArmorDef armorDefinition = gameData.GetArmorDef(armorIdentifier);
                if (armorDefinition)
                {
                    armorRating = armorDefinition.ArmorRating;
                }
                else
                {
                    CtLogger.LogCritical("Skill Definition", 
                        $"Armor identifier was not found (identifier={armorIdentifier}).");
                }
            }

            ulong offHandWeaponData = target.EntityStats.OffHandWeapon;
            if (CtDataBlock.IsValid(offHandWeaponData))
            {
                ushort offHandIdentifier = CtDataBlock.GetOffHandIdentifier(offHandWeaponData);
                CtOffHandDef offHandDefinition = gameData.GetOffHandDef(offHandIdentifier);
                if (offHandDefinition.OffHandType == EOffHandType.Shield)
                {
                    int attributeRank = 
                        TryGetAttributeLevelByAttributeType(gameData, target.EntityStats, offHandDefinition.AttributeType);
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

            CtLogger.LogDebug("Skill Definition", 
                "[Armor Rating] Additional armor rating " +
                $"(displayName=({target.DisplayName}), armorRating={armorRating}).");

            return armorRating;
        }

        private static int _CalcMeleeAttack(CtGameData gameData, CtEntity target, CtEntity source,
            out CtWeaponDef weaponDefinition, out int attributeRank, out bool isCritical)
        {
            ushort identifier = CtDataBlock.GetWeaponIdentifier(source.EntityStats.MainHandWeapon);
            weaponDefinition = gameData.GetWeaponDef(identifier);
            if (!weaponDefinition)
                CtLogger.LogCritical("Skill Definition", $"Weapon could not be found (identifier={identifier})");

            attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityStats, weaponDefinition.AttributeType);

            int armorRating = CalculateArmorRating(gameData, target);

            int weaponAttributeLevel = CtDataBlock.GetWeaponRequirement(source.EntityStats.MainHandWeapon);
            return weaponDefinition.CalcDamage(weaponAttributeLevel, attributeRank, source.EntityStats.CharacterLevel,
                target.EntityStats.CharacterLevel, armorRating, out isCritical);
        }

        public static void MeleeSkill(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source, int identifier, int damageBase, 
            float damagePerAttribute, float armorPenetration = 0)
        {
            int armorLevel = target.EntityStats.armorLevel - 
                             Mathf.RoundToInt(target.EntityStats.armorLevel * armorPenetration);

            // Skill Weapon Damage
            int damage = _CalcMeleeAttack(gameData, target, source, out var weaponDefinition, out var attributeRank, out var isCritical);
            damage += CalcDamage(damageBase, damagePerAttribute, attributeRank, source.EntityStats.CharacterLevel,
                armorLevel);
            target.ApplyDamage(instanceId, damage, weaponDefinition.DamageType, EDamageSourceType.Skill, identifier, source,
                isCritical);
        }

        public static void SpellSkill(CtGameData gameData, ushort instanceId, CtEntity target, CtEntity source, EAttributeType attributeType,
            int identifier, EDamageType damageType, int damageBase, float damagePerAttribute)
        {
            int attributeRank =
                TryGetAttributeLevelByAttributeType(gameData, source.EntityStats, attributeType);
            int damage = CalcDamage(damageBase, damagePerAttribute, attributeRank, source.EntityStats.CharacterLevel,
                target.EntityStats.armorLevel);
            target.ApplyDamage(instanceId, damage, damageType, EDamageSourceType.Skill, identifier,
                source, false);
        }

        public static void ApplyStatus(ushort instanceId, CtEntity target, CtEntity source, int identifier, int turns)
        {
            // bool skipApply = false;
            // int count = target.CombatEffectData.EffectCount;
            // for (int i = 0; i < count; i++)
            // {
            //     if (!target.CombatEffectData.IsValid(i))
            //         continue;
            //
            //     int id = target.CombatEffectData.GetIdentifier(i);
            //     if (id == identifier)
            //     {
            //         int currentTurns = target.CombatEffectData.GetTurns(i);
            //         if (turns > currentTurns)
            //         {
            //             target.CombatEffectData.RemoveEffect(i);
            //         }
            //         else
            //         {
            //             skipApply = true;
            //         }
            //
            //         break;
            //     }
            // }
            //
            // if (skipApply)
            //     return;
            //
            // target.CombatEffectData.AddEffect(
            //     instanceId,
            //     source.Identifier,
            //     turns,
            //     0,
            //     (ushort)identifier);
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

        public static int TryGetAttributeLevelByAttributeType(CtGameData gameData, CtEntityDef entityStats, EAttributeType attributeType)
        {
            ushort profession = CtDataBlock.GetProfession(entityStats.AttributeData);
            CtProfessionDef professionDefinition = gameData.GetProfessionDef(profession);
            for (int i = 0; i < professionDefinition.Attributes.Length; ++i)
                if (professionDefinition.Attributes[i].AttributeType == attributeType)
                    return CtDataBlock.GetAttributeRank(entityStats.AttributeData, i);

            return 0;
        }
    }
}