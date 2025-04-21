
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
        None,
        OneHanded,
        TwoHanded,
        // OffHand,
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
        None,
        Shield,
        Focus
    }

    public enum EArmorSlot
    {
        None,
        Head,
        Chest,
        Hands,
        Legs,
        Feet
    }

    public enum EBonusType
    {
        None,
        EnergyRecovery,
        EnergyIncrease,
        ArmorIncrease,
        HealthIncrease,
    }

    public enum EAttributeType
    {
        None,

        AxeMastery,
        HammerMastery,
        Swordsmanship,
        Tactics,

        Earth,
        Fire,
        Air,
        Water,

        Healing,
        Smiting,
        Protection,

        Marksmanship,
        WildernessSurvival,
        Beastmastery
    }

    public enum EDamageSourceType
    {
        Weapon,
        Skill,
        Condition
    }

    public enum EDamageType
    {
        Slashing,
        Blunt,
        Piercing,

        Earth,
        Fire,
        Air,
        Water,

        Healing,
        Protection,
        Smiting,
        
        Bleeding,
        Burning,
        Disease,
        Poison,

        Dazed,
        Blind,

        Exhausted,
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
    }

    public enum ETargetType
    {
        None = 0,

        EnemyOnly = 1,
        AllEnemies = 2,
        AllyOnly = 3,
        SelfOnly = 4
    }
}