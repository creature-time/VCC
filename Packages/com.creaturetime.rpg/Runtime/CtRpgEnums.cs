
using System;

namespace CreatureTime
{
    public enum EItemRarity
    {
        None = 0, // Nothing happens and no stats are changed.
        Common = 1, // Potentially a benefit, but still pretty common.
        Magical = 2, // Comes with magical items, but still weak.
        Uncommon = 3, // Higher end magical items.
        Rare = 4 // All slots are filled and potential to be "perfect" items.
    }

    public enum EWeaponPrefix
    {
        None = 0,
        Barbed = 1,
        Ebon = 2,
        Fiery = 3,
        Shocking = 4,
        Icy = 5,
    }

    public enum EWeaponSuffix
    {
        None = 0,
        Defense = 1,
        Shelter = 2,
        Warding = 3,
        Enchanting = 4,
    }

    public enum EWeaponType
    {
        None = 0,
        OneHanded = 1,
        TwoHanded = 2
    }

    public enum EWeaponAttackType
    {
        None,
        Melee,
        Magic,
        Ranged
    }

    public enum EOffHandPrefix
    {
        None = 0,
        Barbed = 1,
        Ebon = 2,
        Fiery = 3,
        Shocking = 4,
        Icy = 5,
    }

    public enum EOffHandSuffix
    {
        None = 0,
        Defense = 1,
        Shelter = 2,
        Warding = 3,
        Enchanting = 4,
    }

    public enum EOffHandType
    {
        Shield,
        Focus
    }

    public enum EArmorSlot
    {
        Head = 0,
        Chest = 1,
        Hands = 2,
        Legs = 3,
        Feet = 4
    }

    public enum EArmorBonusType
    {
        None,
        EnergyRecovery,
        EnergyIncrease,
        HealthIncrease,
    }

    public enum EArmorRatingBonusType
    {
        None,
        PhysicalDamage,
        ElementalDamage
    }

    public enum EDamageSourceType
    {
        Weapon,
        Skill,
        Condition
    }

    public enum EDamageType
    {
        None,

        Slashing,
        Blunt,
        Piercing,

        Earth,
        Fire,
        Air,
        Water,

        Holy,
        Shadow,

        Bleeding,
        Burning,
        Disease,
        Poison,

        Missed,
        Blocked,
    }

    public enum ESkillType
    {
        None,
        Energy,
        Adrenaline
    }

    [Flags]
    public enum ECombatEffectFlags
    {
        None = 0,
        Use = 1 << 0,
        PersistentEffect = 1 << 1,
        SkillUsedEffect = 1 << 2,
        TickEffect = 1 << 3,
        TakeDamage = 1 << 4,
    }

    [Flags]
    public enum ETargetType
    {
        Self                = 1 << 0, // 00001
        SingleAlly          = 1 << 1, // 00010
        AllAllies           = 1 << 2, // 00100
        SingleEnemy         = 1 << 3, // 01000
        AllEnemies          = 1 << 4, // 10000
    }

    public enum ESkillSubType
    {
        None                = 0,
        Enchantment         = 1,
        Hex                 = 2,
        Unused0             = 3,
        ShadowBound         = 4,
        Unused1             = 5
    }
}