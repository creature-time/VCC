
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum ECombatState
    {
        None = 0,
        Spectator = 1,
        Alive = 2,
        Dead = 3
    }

    public enum EEntitySignal
    {
        EntityDefChanged,
        AttributesChanged,
        MainHandChanged,
        OffHandChanged,
        HeadSlotChanged,
        ChestSlotChanged,
        HandsSlotChanged,
        LegsSlotChanged,
        FeetSlotChanged,
        SkillSlotChanged,
        HealthChanged,
        MaxHealthChanged,
        HealthRegenChanged,
        EnergyChanged,
        MaxEnergyChanged,
        EnergyRegenChanged,
        ExpChanged,
        LevelChanged,
        DamageApplied,
        Death,

        Extension
    }

    public abstract class CtEntity : CtEntityBase
    {
        [Header("Global Variables")]

        [SerializeField] protected CtGameData gameData;
        [SerializeField] protected CtSkillInstances skillInstances;
        [SerializeField] protected CtStatusEffectInstances statusEffectInstances;

        public override bool IsValid => EntityDef;

        public CtSkillInstances SkillInstances => skillInstances;
        public CtStatusEffectInstances StatusEffectInstances => statusEffectInstances;

        public ECombatState State
        {
            get
            {
                if (Health > 0)
                {
                    return ECombatState.Alive;
                }

                if (Health == 0)
                {
                    return ECombatState.Dead;
                }

                return ECombatState.None;
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(HealthCallback))]
        private int _health = -1;

        public int HealthCallback
        {
            get => _health;
            set
            {
                _health = value;

                if (_health == 0)
                {
                    OnDeath();

                    this.Emit(EEntitySignal.Death);
                }

                SetArgs.Add(_health);
                this.Emit(EEntitySignal.HealthChanged);
            }
        }

        protected virtual void OnDeath()
        {
            Energy = 0;
            _ResetStatusEffectsInstanceData();
        }

        public int Health
        {
            get => HealthCallback;
            private set => HealthCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(EnergyCallback))]
        private int _energy = 0;

        public int EnergyCallback
        {
            get => _energy;
            set
            {
                _energy = value;

                SetArgs.Add(_energy);
                this.Emit(EEntitySignal.EnergyChanged);
            }
        }

        public int Energy
        {
            get => EnergyCallback;
            private set
            {
                EnergyCallback = value;
                RequestSerialization();
            }
        }

        # region Offensive

        [UdonSynced, FieldChangeCallback(nameof(DamageDealtCallback))]
        private int _damageDealt = 0;

        public int DamageDealtCallback
        {
            get => _damageDealt;
            set => _damageDealt = value;
        }

        public int DamageDealt
        {
            get => DamageDealtCallback;
            set => DamageDealtCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(DamageDealtResistedCallback))]
        private int _damageDealtResisted = 0;

        public int DamageDealtResistedCallback
        {
            get => _damageDealtResisted;
            set => _damageDealtResisted = value;
        }

        public int DamageResisted
        {
            get => DamageDealtResistedCallback;
            set => DamageDealtResistedCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(HealingDealtCallback))]
        private int _healingDealt = 0;

        public int HealingDealtCallback
        {
            get => _healingDealt;
            set => _healingDealt = value;
        }

        public int HealingDealt
        {
            get => HealingDealtCallback;
            set => HealingDealtCallback = value;
        }

        # endregion

        #region Defensive
        
        [UdonSynced, FieldChangeCallback(nameof(DamageTakenCallback))]
        private int _damageTaken = 0;

        public int DamageTakenCallback
        {
            get => _damageTaken;
            set => _damageTaken = value;
        }

        public int DamageTaken
        {
            get => DamageTakenCallback;
            set => DamageTakenCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(DamageTakenResistedCallback))]
        private int _damageTakenResisted = 0;

        public int DamageTakenResistedCallback
        {
            get => _damageTakenResisted;
            set => _damageTakenResisted = value;
        }

        public int DamageTakenResisted
        {
            get => DamageTakenResistedCallback;
            set => DamageTakenResistedCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(HealingTakenCallback))]
        private int _healingTaken = 0;

        public int HealingTakenCallback
        {
            get => _healingTaken;
            set => _healingTaken = value;
        }

        public int HealingTaken
        {
            get => HealingTakenCallback;
            set => HealingTakenCallback = value;
        }

        # endregion

        private CtSkillDef _skillDef0;
        private CtSkillDef _skillDef1;
        private CtSkillDef _skillDef2;
        private CtSkillDef _skillDef3;
        private CtSkillDef _skillDef4;
        private CtSkillDef _skillDef5;
        private CtSkillDef _skillDef6;
        private CtSkillDef _skillDef7;
        private CtSkillDef _skillDef8;
        private CtSkillDef _skillDef9;

        public CtSkillDef GetSkill(int index)
        {
            switch (index)
            {
                case 0: return _skillDef0;
                case 1: return _skillDef1;
                case 2: return _skillDef2;
                case 3: return _skillDef3;
                case 4: return _skillDef4;
                case 5: return _skillDef5;
                case 6: return _skillDef6;
                case 7: return _skillDef7;
                case 8: return _skillDef8;
                case 9: return _skillDef9;
                default: return null;
            }

        }

        private float _rechargeIncrease;
        public float RechargeIncrease
        {
            private get => _rechargeIncrease;
            set => _rechargeIncrease = Mathf.Max(_rechargeIncrease, value);
        }
        
        private float _rechargeReduction;
        public float RechargeReduction
        {
            private get => _rechargeReduction;
            set => _rechargeReduction = Mathf.Max(_rechargeReduction, value);
        }
        
        public float RechargeMod => RechargeIncrease - RechargeReduction;

        private int _absorbShield;
        public int AbsorbShield
        {
            get => _absorbShield;
            set => _absorbShield += value;
        }

        private float _armorPenetrationIncrease;
        public float ArmorPenetrationIncrease
        {
            private get => _armorPenetrationIncrease;
            set => _armorPenetrationIncrease += value;
        }

        private float _armorPenetrationReduction;
        public float ArmorPenetrationReduction
        {
            private get => _armorPenetrationReduction;
            set => _armorPenetrationReduction += value;
        }
        
        public float ArmorPenetrationMod => ArmorPenetrationIncrease - ArmorPenetrationReduction;
        
        private float _criticalChanceIncrease;
        public float CriticalChanceIncrease
        {
            private get => _criticalChanceIncrease;
            set => _criticalChanceIncrease += value;
        }
        
        private float _criticalChanceReduction;
        public float CriticalChanceReduction
        {
            private get => _criticalChanceReduction;
            set => _criticalChanceReduction += value;
        }
        
        public float CriticalChanceMod => CriticalChanceIncrease - CriticalChanceReduction;
        
        private int _damageIncrease;
        public int DamageIncrease
        {
            private get => _damageIncrease;
            set => _damageIncrease += value;
        }

        private int _damageReduction;
        public int DamageReduction
        {
            private get => _damageReduction;
            set => _damageReduction += value;
        }

        public int DamageMod => DamageIncrease - DamageReduction;

        private int _armorRatingIncrease;
        public int ArmorRatingIncrease
        {
            private get => _armorRatingIncrease;
            set => _armorRatingIncrease += value;
        }
        
        private int _armorRatingReduction;
        public int ArmorRatingReduction
        {
            private get => _armorRatingReduction;
            set => _armorRatingReduction += value;
        }
        
        public int ArmorRatingMod => ArmorRatingIncrease - ArmorRatingReduction;
        
        private int _slashDamageIncrease;
        public int SlashDamageIncrease
        {
            private get => _slashDamageIncrease;
            set => _slashDamageIncrease = Mathf.Max(_slashDamageIncrease, value);
        }

        private int _slashDamageReduction;
        public int SlashDamageReduction
        {
            private get => _slashDamageReduction;
            set => _slashDamageReduction = Mathf.Max(_slashDamageReduction, value);
        }
        
        public int SlashDamageMod => SlashDamageIncrease - SlashDamageReduction;
        
        private int _slashArmorIncrease;
        public int SlashArmorIncrease
        {
            private get => _slashArmorIncrease;
            set => _slashArmorIncrease = Mathf.Max(_slashArmorIncrease, value);
        }
        
        private int _slashArmorReduction;
        public int SlashArmorReduction
        {
            private get => _slashArmorReduction;
            set => _slashArmorReduction = Mathf.Max(_slashArmorReduction, value);
        }
        
        public int SlashArmorMod => SlashArmorIncrease - SlashArmorReduction;
        
        private int _bluntDamageIncrease;
        public int BluntDamageIncrease
        {
            private get => _bluntDamageIncrease;
            set => _bluntDamageIncrease = Mathf.Max(_bluntDamageIncrease, value);
        }
        
        private int _bluntDamageReduction;
        public int BluntDamageReduction
        {
            private get => _bluntDamageReduction;
            set => _bluntDamageReduction = Mathf.Max(_bluntDamageReduction, value);
        }
        
        public int BluntDamageMod => BluntDamageIncrease - BluntDamageReduction;
        
        private int _bluntArmorIncrease;
        public int BluntArmorIncrease
        {
            private get => _bluntArmorIncrease;
            set => _bluntArmorIncrease = Mathf.Max(_bluntArmorIncrease, value);
        }
        
        private int _bluntArmorReduction;
        public int BluntArmorReduction
        {
            private get => _bluntArmorReduction;
            set => _bluntArmorReduction = Mathf.Max(_bluntArmorReduction, value);
        }
        
        public int BluntArmorMod => BluntArmorIncrease - BluntArmorReduction;
        
        private int _piercingDamageIncrease;
        public int PiercingDamageIncrease
        {
            private get => _piercingDamageIncrease;
            set => _piercingDamageIncrease = Mathf.Max(_piercingDamageIncrease, value);
        }
        
        private int _piercingDamageReduction;
        public int PiercingDamageReduction
        {
            private get => _piercingDamageReduction;
            set => _piercingDamageReduction = Mathf.Max(_piercingDamageReduction, value);
        }
        
        public int PiercingDamageMod => PiercingDamageIncrease - PiercingDamageReduction;
        
        private int _piercingArmorIncrease;
        public int PiercingArmorIncrease
        {
            private get => _piercingArmorIncrease;
            set => _piercingArmorIncrease = Mathf.Max(_piercingArmorIncrease, value);
        }
        
        private int _piercingArmorReduction;
        public int PiercingArmorReduction
        {
            private get => _piercingArmorReduction;
            set => _piercingArmorReduction = Mathf.Max(_piercingArmorReduction, value);
        }
        
        public int PiercingArmorMod => PiercingArmorIncrease - PiercingArmorReduction;
        
        private int _earthDamageIncrease;
        public int EarthDamageIncrease
        {
            private get => _earthDamageIncrease;
            set => _earthDamageIncrease = Mathf.Max(_earthDamageIncrease, value);
        }
        
        private int _earthDamageReduction;
        public int EarthDamageReduction
        {
            private get => _earthDamageReduction;
            set => _earthDamageReduction = Mathf.Max(_earthDamageReduction, value);
        }
        
        public int EarthDamageMod => EarthDamageIncrease - EarthDamageReduction;
        
        private int _earthArmorIncrease;
        public int EarthArmorIncrease
        {
            private get => _earthArmorIncrease;
            set => _earthArmorIncrease = Mathf.Max(_earthArmorIncrease, value);
        }
        
        private int _earthArmorReduction;
        public int EarthArmorReduction
        {
            private get => _earthArmorReduction;
            set => _earthArmorReduction = Mathf.Max(_earthArmorReduction, value);
        }
        
        public int EarthArmorMod => EarthArmorIncrease - EarthArmorReduction;
        
        private int _fireDamageIncrease;
        public int FireDamageIncrease
        {
            private get => _fireDamageIncrease;
            set => _fireDamageIncrease = Mathf.Max(_fireDamageIncrease, value);
        }
        
        private int _fireDamageReduction;
        public int FireDamageReduction
        {
            private get => _fireDamageReduction;
            set => _fireDamageReduction = Mathf.Max(_fireDamageReduction, value);
        }
        
        public int FireDamageMod => FireDamageIncrease - FireDamageReduction;
        
        private int _fireArmorIncrease;
        public int FireArmorIncrease
        {
            private get => _fireArmorIncrease;
            set => _fireArmorIncrease = Mathf.Max(_fireArmorIncrease, value);
        }
        
        private int _fireArmorReduction;
        public int FireArmorReduction
        {
            private get => _fireArmorReduction;
            set => _fireArmorReduction = Mathf.Max(_fireArmorReduction, value);
        }
        
        public int FireArmorMod => FireArmorIncrease - FireArmorReduction;
        
        private int _airDamageIncrease;
        public int AirDamageIncrease
        {
            private get => _airDamageIncrease;
            set => _airDamageIncrease = Mathf.Max(_airDamageIncrease, value);
        }
        
        private int _airDamageReduction;
        public int AirDamageReduction
        {
            private get => _airDamageReduction;
            set => _airDamageReduction = Mathf.Max(_airDamageReduction, value);
        }
        
        public int AirDamageMod => AirDamageIncrease - AirDamageReduction;
        
        private int _airArmorIncrease;
        public int AirArmorIncrease
        {
            private get => _airArmorIncrease;
            set => _airArmorIncrease = Mathf.Max(_airArmorIncrease, value);
        }
        
        private int _airArmorReduction;
        public int AirArmorReduction
        {
            private get => _airArmorReduction;
            set => _airArmorReduction = Mathf.Max(_airArmorReduction, value);
        }
        
        public int AirArmorMod => AirArmorIncrease - AirArmorReduction;
        
        private int _waterDamageIncrease;
        public int WaterDamageIncrease
        {
            private get => _waterDamageIncrease;
            set => _waterDamageIncrease = Mathf.Max(_waterDamageIncrease, value);
        }
        
        private int _waterDamageReduction;
        public int WaterDamageReduction
        {
            private get => _waterDamageReduction;
            set => _waterDamageReduction = Mathf.Max(_waterDamageReduction, value);
        }
        
        public int WaterDamageMod => WaterDamageIncrease - WaterDamageReduction;
        
        private int _waterArmorIncrease;
        public int WaterArmorIncrease
        {
            private get => _waterArmorIncrease;
            set => _waterArmorIncrease = Mathf.Max(_waterArmorIncrease, value);
        }
        
        private int _waterArmorReduction;
        public int WaterArmorReduction
        {
            private get => _waterArmorReduction;
            set => _waterArmorReduction = Mathf.Max(_waterArmorReduction, value);
        }
        
        public int WaterArmorMod => WaterArmorIncrease - WaterArmorReduction;
        
        private int _holyDamageIncrease;
        public int HolyDamageIncrease
        {
            private get => _holyDamageIncrease;
            set => _holyDamageIncrease = Mathf.Max(_holyDamageIncrease, value);
        }
        
        private int _holyDamageReduction;
        public int HolyDamageReduction
        {
            private get => _holyDamageReduction;
            set => _holyDamageReduction = Mathf.Max(_holyDamageReduction, value);
        }
        
        public int HolyDamageMod => HolyDamageIncrease - HolyDamageReduction;
        
        private int _holyArmorIncrease;
        public int HolyArmorIncrease
        {
            private get => _holyArmorIncrease;
            set => _holyArmorIncrease = Mathf.Max(_holyArmorIncrease, value);
        }
        
        private int _holyArmorReduction;
        public int HolyArmorReduction
        {
            private get => _holyArmorReduction;
            set => _holyArmorReduction = Mathf.Max(_holyArmorReduction, value);
        }
        
        public int HolyArmorMod => HolyArmorIncrease - HolyArmorReduction;

        private float _evasion;
        public float Evasion
        {
            get => _evasion;
            set => _evasion += value * (1 - _evasion);
        }
        
        private float _block;
        public float Block
        {
            get => _block;
            set => _block += value * (1 - _block);
        }

        // private int[] _intMods = new int[Convert.ToInt32(EIntModType.Max)];
        //
        // public int GetIntMod(EIntModType modType)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     return _intMods[index] - _intMods[index + 1];
        // }
        //
        // public void AddIntMod(EIntModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType);
        //     _intMods[index] += Mathf.Max(value,  _intMods[index]);
        // }
        //
        // private int[] _tempIntMods = new int[Convert.ToInt32(EIntModType.Max) * 2];
        //
        // public int GetTempIntMod(EIntModType modType)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     return _tempIntMods[index] - _tempIntMods[index + 1];
        // }
        //
        // public void SetTempIntIncreaseMod(EIntModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     _tempIntMods[index] = Mathf.Max(value,  _tempIntMods[index]);
        // }
        //
        // public void SetTempReductionMod(EIntModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2 + 1;
        //     _tempIntMods[index] = Mathf.Max(value,  _tempIntMods[index]);
        // }
        //
        // private float[] _floatMods = new float[Convert.ToInt32(EFloatModType.Max) * 2];
        //
        // public float GetFloatMod(EFloatModType modType)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     return _floatMods[index] - _floatMods[index + 1];
        // }
        //
        // public void SetFloatIncreaseMod(EFloatModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     _floatMods[index] = Mathf.Max(value,  _floatMods[index]);
        // }
        //
        // public void SetFloatReductionMod(EFloatModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2 + 1;
        //     _floatMods[index] = Mathf.Max(value,  _floatMods[index]);
        // }
        //
        // private float[] _tempFloatMods = new float[Convert.ToInt32(EFloatModType.Max) * 2];
        //
        // public float GetTempFloatMod(EFloatModType modType)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     return _tempFloatMods[index] - _tempFloatMods[index + 1];
        // }
        //
        // public void SetTempFloatIncreaseMod(EFloatModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2;
        //     _tempFloatMods[index] = Mathf.Max(value, _tempFloatMods[index]);
        // }
        //
        // public void SetTempFloatReductionMod(EFloatModType modType, int value)
        // {
        //     var index = Convert.ToInt32(modType) * 2 + 1;
        //     _tempFloatMods[index] = Mathf.Max(value, _tempFloatMods[index]);
        // }

        // public int ArmorRatingIncrease { get; set; }
        // public int ArmorRatingReduction { get; set; }
        // public float SlashDamageIncrease { get; set; }
        // public float SlashDamageReduction { get; set; }
        // public int SlashArmorIncrease { get; set; }
        // public int SlashArmorReduction { get; set; }
        // public float BluntDamageIncrease { get; set; }
        // public float BluntDamageReduction { get; set; }
        // public int BluntArmorIncrease { get; set; }
        // public int BluntArmorReduction { get; set; }
        // public float PierceDamageIncrease { get; set; }
        // public float PierceDamageReduction { get; set; }
        // public int PierceArmorIncrease { get; set; }
        // public int PierceArmorReduction { get; set; }
        // public float EarthDamageIncrease { get; set; }
        // public float EarthDamageReduction { get; set; }
        // public int EarthArmorIncrease { get; set; }
        // public int EarthArmorReduction { get; set; }
        // public float AirDamageIncrease { get; set; }
        // public float AirDamageReduction { get; set; }
        // public int AirArmorIncrease { get; set; }
        // public int AirArmorReduction { get; set; }
        // public float FireDamageIncrease { get; set; }
        // public float FireDamageReduction { get; set; }
        // public int FireArmorIncrease { get; set; }
        // public int FireArmorReduction { get; set; }
        // public float WaterDamageIncrease { get; set; }
        // public float WaterDamageReduction { get; set; }
        // public int WaterArmorIncrease { get; set; }
        // public int WaterArmorReduction { get; set; }
        //
        // public float Evasion { get; set; }

        public bool HasStatusEffect(CtSkillDef skillDef)
        {
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData) continue;

                var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                if (identifier == skillDef.Identifier)
                    return true;
            }

            return false;
        }

        public bool IsDazed { get; set; }
        public bool IsBlind { get; set; }
        public bool IsKnockedDown { get; set; }
        public bool IsFreezing { get; set; }

        public string DisplayName => _entityDef ? _entityDef.DisplayName : "Disconnected";
        public Texture Icon => _entityDef ? _entityDef.Icon : null;
        public bool IsBoss => _entityDef && _entityDef.IsBoss;

        private int _maxHealth;

        public int MaxHealth
        {
            get => _maxHealth;
            private set
            {
                _maxHealth = value;
                this.Emit(EEntitySignal.MaxHealthChanged);
            }
        }

        private int _healthRegen;

        public int HealthRegen
        {
            get => _healthRegen;
            private set
            {
                _healthRegen = value;
                this.Emit(EEntitySignal.HealthRegenChanged);
            }
        }

        private int _maxEnergy;

        public int MaxEnergy
        {
            get => _maxEnergy;
            private set
            {
                Energy += value - _maxEnergy;
                _maxEnergy = value;
                this.Emit(EEntitySignal.MaxEnergyChanged);
            }
        }

        private int _energyRegen;

        public int EnergyRegen
        {
            get => _energyRegen;
            private set
            {
                _energyRegen = value;
                this.Emit(EEntitySignal.EnergyRegenChanged);
            }
        }

        private int _level = -1;

        public int Level
        {
            get => _level;
            private set
            {
                if (_level == value) return;
                _level = value;
                this.Emit(EEntitySignal.LevelChanged);
            }
        }

        public int Exp => _entityDef ? _entityDef.Exp : 0;
        public int ExpStart { get; private set; }
        public int ExpEnd { get; private set; }

        private int _baseHealth;

        public void _OnExpChanged()
        {
            CtRpgFormulas.ConvertExpToLevel(_entityDef.Exp, out var level, out var start, out var end,
                out var baseHealth);
            Level = level;
            ExpStart = start;
            ExpEnd = end;
            _baseHealth = baseHealth;

            _UpdateStats();

            this.Emit(EEntitySignal.ExpChanged);
        }

        public void _UpdateStats()
        {
            var maxHealth = _baseHealth;
            var maxEnergy = 20;
            var energyRegen = 2;

            ulong[] equipment =
            {
                EntityDef.HeadSlot,
                EntityDef.ChestSlot,
                EntityDef.HandsSlot,
                EntityDef.LegsSlot,
                EntityDef.FeetSlot
            };

            for (int i = 0; i < equipment.Length; i++)
            {
                var slotData = equipment[i];
                if (!CtDataBlock.IsValid(slotData)) continue;

                var slot = (EArmorSlot)i;
                var identifier = CtDataBlock.GetEquipmentIdentifier(slotData);
                var armorDef = gameData.GetArmorDef(identifier);
                if (!armorDef)
                {
#if DEBUG_LOGS
                    LogWarning($"Failed to find armor definition for equipment in armor slot (slot={slot}).");
#endif
                    continue;
                }

                if (!armorDef.IsAllowedProfession(ProfessionDef)) continue;

                var armorSlotDef = armorDef.GetArmorSlot(slot);
                if (!armorSlotDef)
                {
#if DEBUG_LOGS
                    LogWarning(
                        $"Failed to find slot for equipment in armor slot (armorDef={armorDef}, slot={slot}).");
#endif
                    continue;
                }

                maxHealth += armorSlotDef.HealthIncreaseBonus;
                maxEnergy += armorSlotDef.EnergyIncreaseBonus;
                energyRegen += armorSlotDef.EnergyRegenerationBonus;
            }

            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            EnergyRegen = energyRegen;
        }

        // public float MaxHealth => _entityDef ? _entityDef.MaxHealth : 0;
        public float NormalizedHealth => _entityDef ? Health / (float)MaxHealth : 0;
        // public float MaxEnergy => _entityDef ? _entityDef.MaxEnergy : 0;
        public float NormalizedEnergy => _entityDef ? Energy / (float)MaxEnergy : 0;
        public float NormalizedExp => _entityDef ? (_entityDef.Exp - ExpStart) / (float)(ExpEnd - ExpStart) : 0;

        // TODO: Maybe make these flags? IsPlayer, IsNpc, IsLocal, IsRemote?
        public abstract bool IsPlayer
        {
            get;
        }

        private CtEntityDef _entityDef;

        public CtEntityDef EntityDef
        {
            get => _entityDef;
            protected set
            {
                if (_entityDef)
                {
                    _entityDef.Disconnect(EEntityDefSignal.AttributesChanged, this, nameof(_OnAttributesChanged));
                    _entityDef.Disconnect(EEntityDefSignal.MainHandChanged, this, nameof(_OnMainHandChanged));
                    _entityDef.Disconnect(EEntityDefSignal.OffHandChanged, this, nameof(_OnOffHandChanged));
                    _entityDef.Disconnect(EEntityDefSignal.HeadSlotChanged, this, nameof(_UpdateStats));
                    _entityDef.Disconnect(EEntityDefSignal.ChestSlotChanged, this, nameof(_UpdateStats));
                    _entityDef.Disconnect(EEntityDefSignal.HandsSlotChanged, this, nameof(_UpdateStats));
                    _entityDef.Disconnect(EEntityDefSignal.LegsSlotChanged, this, nameof(_UpdateStats));
                    _entityDef.Disconnect(EEntityDefSignal.FeetSlotChanged, this, nameof(_UpdateStats));
                    _entityDef.Disconnect(EEntityDefSignal.SkillSlotChanged, this, nameof(_OnSkillSlotChanged));
                    _entityDef.Disconnect(EEntityDefSignal.ExpChanged, this, nameof(_OnExpChanged));

                    Level = 0;
                    ExpStart = 0;
                    ExpEnd = 0;
                    _baseHealth = 0;
                    ProfessionDef = null;
                    _attributeRanks = new int[] { };
                    MainHand = null;
                    OffHand = null;
                    _OnSkillSlotChangedRaw(0, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(1, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(2, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(3, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(4, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(5, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(6, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(7, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(8, CtConstants.InvalidId);
                    _OnSkillSlotChangedRaw(9, CtConstants.InvalidId);
                }

                _entityDef = value;

                if (_entityDef)
                {
                    _OnExpChanged();
                    OnAttributesChangedRaw();
                    OnMainHandChangedRaw();
                    _OnOffHandChanged();
                    _UpdateStats();
                    _OnSkillSlotChangedRaw(0, _entityDef.SkillSlot0);
                    _OnSkillSlotChangedRaw(1, _entityDef.SkillSlot1);
                    _OnSkillSlotChangedRaw(2, _entityDef.SkillSlot2);
                    _OnSkillSlotChangedRaw(3, _entityDef.SkillSlot3);
                    _OnSkillSlotChangedRaw(4, _entityDef.SkillSlot4);
                    _OnSkillSlotChangedRaw(5, _entityDef.SkillSlot5);
                    _OnSkillSlotChangedRaw(6, _entityDef.SkillSlot6);
                    _OnSkillSlotChangedRaw(7, _entityDef.SkillSlot7);
                    _OnSkillSlotChangedRaw(8, _entityDef.SkillSlot8);
                    _OnSkillSlotChangedRaw(9, _entityDef.SkillSlot9);

                    _entityDef.Connect(EEntityDefSignal.AttributesChanged, this, nameof(_OnAttributesChanged));
                    _entityDef.Connect(EEntityDefSignal.MainHandChanged, this, nameof(_OnMainHandChanged));
                    _entityDef.Connect(EEntityDefSignal.OffHandChanged, this, nameof(_OnOffHandChanged));
                    _entityDef.Connect(EEntityDefSignal.HeadSlotChanged, this, nameof(_OnHeadSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.ChestSlotChanged, this, nameof(_OnChestSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.HandsSlotChanged, this, nameof(_OnHandsSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.LegsSlotChanged, this, nameof(_OnLegsSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.FeetSlotChanged, this, nameof(_OnFeetSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.SkillSlotChanged, this, nameof(_OnSkillSlotChanged));
                    _entityDef.Connect(EEntityDefSignal.ExpChanged, this, nameof(_OnExpChanged));
                }

                SetArgs.Add(_entityDef);
                this.Emit(EEntitySignal.EntityDefChanged);
            }
        }

        public CtWeaponDef MainHand { get; private set; }

        public ulong MainHandData
        {
            get => _entityDef.MainHandWeapon;
            set => _entityDef.MainHandWeapon = value;
        }

        public CtOffHandDef OffHand { get; private set; }

        public ulong OffHandData
        {
            get => _entityDef.OffHandWeapon;
            set => _entityDef.OffHandWeapon = value;
        }

        public ulong HeadSlotData
        {
            get => _entityDef.HeadSlot;
            set => _entityDef.HeadSlot = value;
        }

        public ulong ChestSlotData
        {
            get => _entityDef.ChestSlot;
            set => _entityDef.ChestSlot = value;
        }

        public ulong HandsSlotData
        {
            get => _entityDef.HandsSlot;
            set => _entityDef.HandsSlot = value;
        }

        public ulong LegsSlotData
        {
            get => _entityDef.LegsSlot;
            set => _entityDef.LegsSlot = value;
        }

        public ulong FeetSlotData
        {
            get => _entityDef.FeetSlot;
            set => _entityDef.FeetSlot = value;
        }

        private int[] _attributeRanks = { };

        public CtProfessionDef ProfessionDef { get; private set; }
        public int AttrCount => _attributeRanks.Length;
        public int GetAttrRank(int attrIndex) => _attributeRanks[attrIndex];

        public void _OnSkillSlotChanged()
        {
            var index = GetArgs[0].Int;
            _OnSkillSlotChangedRaw(index, _entityDef.GetSkill(index));

            SetArgs.Add(index);
            this.Emit(EEntitySignal.SkillSlotChanged);
        }

        protected virtual void OnAttributesChangedRaw()
        {
            ProfessionDef = null;
            if (CtDataBlock.IsValid(EntityDef.AttributeData))
            {
                var professionId = CtDataBlock.GetProfession(EntityDef.AttributeData);
                ProfessionDef = gameData.GetProfessionDef(professionId);
                int attrCount = CtDataBlock.GetAttributeCount(EntityDef.AttributeData);
                _attributeRanks = new int[ProfessionDef.Attributes.Length];
                for (int i = 0; i < attrCount; i++)
                {
                    _attributeRanks[i] = CtDataBlock.GetAttributeRank(EntityDef.AttributeData, i);
                }
            }
        }

        public void _OnAttributesChanged()
        {
            OnAttributesChangedRaw();
            _UpdateStats();
            this.Emit(EEntitySignal.AttributesChanged);
        }

        protected virtual void OnMainHandChangedRaw()
        {
            MainHand = null;
            if (CtDataBlock.IsValid(_entityDef.MainHandWeapon))
            {
                var identifier = CtDataBlock.GetWeaponIdentifier(_entityDef.MainHandWeapon);
                MainHand = gameData.GetWeaponDef(identifier);
            }
        }

        public void _OnMainHandChanged()
        {
            OnMainHandChangedRaw();
            _UpdateStats();
            this.Emit(EEntitySignal.MainHandChanged);
        }

        protected virtual void OnOffHandChangedRaw()
        {
            OffHand = null;
            if (CtDataBlock.IsValid(_entityDef.OffHandWeapon))
            {
                var identifier = CtDataBlock.GetOffHandIdentifier(_entityDef.OffHandWeapon);
                OffHand = gameData.GetOffHandDef(identifier);
            }
        }

        public void _OnOffHandChanged()
        {
            OnOffHandChangedRaw();
            _UpdateStats();
            this.Emit(EEntitySignal.OffHandChanged);
        }

        public void _OnHeadSlotChanged()
        {
            _UpdateStats();
            this.Emit(EEntitySignal.HeadSlotChanged);
        }

        public void _OnChestSlotChanged()
        {
            _UpdateStats();
            this.Emit(EEntitySignal.ChestSlotChanged);
        }

        public void _OnHandsSlotChanged()
        {
            _UpdateStats();
            this.Emit(EEntitySignal.HandsSlotChanged);
        }

        public void _OnLegsSlotChanged()
        {
            _UpdateStats();
            this.Emit(EEntitySignal.LegsSlotChanged);
        }

        public void _OnFeetSlotChanged()
        {
            _UpdateStats();
            this.Emit(EEntitySignal.FeetSlotChanged);
        }

        private void _OnSkillSlotChangedRaw(int index, ushort skillId)
        {
            var skillDef = skillId != CtConstants.InvalidId ? gameData.GetSkillDef(skillId) : null;
            switch (index)
            {
                case 0: _skillDef0 = skillDef; break;
                case 1: _skillDef1 = skillDef; break;
                case 2: _skillDef2 = skillDef; break;
                case 3: _skillDef3 = skillDef; break;
                case 4: _skillDef4 = skillDef; break;
                case 5: _skillDef5 = skillDef; break;
                case 6: _skillDef6 = skillDef; break;
                case 7: _skillDef7 = skillDef; break;
                case 8: _skillDef8 = skillDef; break;
                case 9: _skillDef9 = skillDef; break;
                default: return;
            }
        }

        protected void Reset()
        {
            if (!Networking.IsMaster) return;

            Health = MaxHealth;
            Energy = MaxEnergy;
            _ResetStatusEffectsInstanceData();
            _ResetSkillInstanceData();
            _ResetStats();

            RequestSerialization();
        }

        private void _ResetStats()
        {
            DamageDealt = 0;
            DamageTaken = 0;
            HealingDealt = 0;
            HealingTaken = 0;
            DamageResisted = 0;
            DamageTakenResisted = 0;
        }

        private void _ResetStatusEffectsInstanceData()
        {
            for (int i = 0; i < statusEffectInstances.Count; i++)
                statusEffectInstances.SetStatusEffect(i, CtDataBlock.InvalidData);
            UpdatePersistantEffects();
        }

        private void _ResetSkillInstanceData()
        {
            skillInstances.SkillRecharge0 = 0;
            skillInstances.SkillRecharge1 = 0;
            skillInstances.SkillRecharge2 = 0;
            skillInstances.SkillRecharge3 = 0;
            skillInstances.SkillRecharge4 = 0;
            skillInstances.SkillRecharge5 = 0;
            skillInstances.SkillRecharge6 = 0;
            skillInstances.SkillRecharge7 = 0;
            skillInstances.SkillRecharge8 = 0;
            skillInstances.SkillRecharge9 = 0;

            skillInstances.SkillAdrenaline0 = 0;
            skillInstances.SkillAdrenaline1 = 0;
            skillInstances.SkillAdrenaline2 = 0;
            skillInstances.SkillAdrenaline3 = 0;
            skillInstances.SkillAdrenaline4 = 0;
            skillInstances.SkillAdrenaline5 = 0;
            skillInstances.SkillAdrenaline6 = 0;
            skillInstances.SkillAdrenaline7 = 0;
            skillInstances.SkillAdrenaline8 = 0;
            skillInstances.SkillAdrenaline9 = 0;
        }

        public override void ApplyDamage(int damage, EDamageType damageType, 
            EDamageSourceType damageSourceType, ushort identifier, CtEntity instigator, bool isCritical)
        {
//             if (damage >= 0)
//             {
//                 // Pre-damage calculations.
//                 GainAdrenalineOnHit(damage);
//
//                 // Ignore condition damage.
//                 switch (damageType)
//                 {
//                     case EDamageType.Bleeding:
//                     case EDamageType.Burning:
//                     case EDamageType.Disease:
//                     case EDamageType.Poison:
//                         break;
//                     default:
//                         damage -= DamageReduction;
//                         break;
//                 }
//
//                 // Check for resistances.
//                 int resistedDamage = 0;
//
//                 switch (damageType)
//                 {
//                     case EDamageType.Slashing:
//                         resistedDamage = damage - SlashDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Blunt:
//                         resistedDamage = damage * BluntDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Piercing:
//                         resistedDamage = damage * PierceDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Earth:
//                         resistedDamage = damage * EarthDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Fire:
//                         resistedDamage = damage * FireDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Air:
//                         resistedDamage = damage * AirDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Water:
//                         resistedDamage = damage * WaterDamageReduction;
//                         damage -= resistedDamage;
//                         break;
//                     case EDamageType.Holy:
//                     // resistedDamage = (int)(damage * SmiteReduction);
//                     // damage -= resistedDamage;
//                     // break;
//                     case EDamageType.Bleeding:
//                     case EDamageType.Burning:
//                     // resistedDamage = (int)(damage * FireReduction);
//                     // damage -= resistedDamage;
//                     // break;
//                     case EDamageType.Disease:
//                     case EDamageType.Poison:
//                         break;
//                     default:
// #if DEBUG_LOGS
//                     LogCritical($"Damage type not supported (damageType={damageType}.");
// #endif
//                         break;
//                 }
//
//                 damage = Mathf.Max(0, damage);
//
//                 // Calculate damage so we don't overkill.
//                 damage = Mathf.Min(Health, damage);
//
//                 // Update total damage resisted stats.
//                 DamageTakenResisted += resistedDamage;
//                 instigator.DamageResisted += resistedDamage;
//
//                 // Update total damage taken stats.
//                 DamageTaken += damage;
//                 instigator.DamageDealt += damage;
//
//                 // Apply damage.
//                 Health -= damage;
//
//                 if (Health <= 0)
//                     Health = 0;
//
// #if DEBUG_LOGS
//                 LogDebug("Damage applied (" +
//                          $"target={this}, damage={damage}, damageType={damageType}, " +
//                          $"damageSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
//                          $"isCritical={isCritical}" +
//                          ").");
// #endif
//             }
//             else
//             {
//                 var healing = -damage;
//
//                 // Calculate heal so we don't over heal.
//                 healing = Mathf.Min(healing, EntityDef.MaxHealth - Health);
//
//                 Health += healing;
//
// #if DEBUG_LOGS
//                 LogDebug("Healing applied (" +
//                          $"target={this}, healing={healing}, healingType={damageType}, " +
//                          $"healingSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
//                          $"isCritical={isCritical}" +
//                          ").");
// #endif
//             }

            if (damage >= 0)
            {
                // Apply damage.
                damage = Mathf.Min(Health, damage);
                Health -= damage;

#if DEBUG_LOGS
                LogDebug("Damage applied (" +
                         $"target={this}, damage={damage}, damageType={damageType}, " +
                         $"damageSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
                         $"isCritical={isCritical}" +
                         ").");
#endif
            }
            else
            {
                // Calculate heal so we don't over heal.
                damage = Mathf.Max(damage, Health - MaxHealth);
                Health -= damage;

#if DEBUG_LOGS
                LogDebug("Healing applied (" +
                         $"target={this}, damage={damage}, healingType={damageType}, " +
                         $"healingSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
                         $"isCritical={isCritical}" +
                         ").");
#endif
            }

            // Request serialization.
            RequestSerialization();
            instigator.RequestSerialization();

            SetArgs.Add(Convert.ToInt32(damageSourceType));
            SetArgs.Add(identifier);
            SetArgs.Add(instigator);
            SetArgs.Add(Convert.ToInt32(damageType));
            SetArgs.Add(damage);
            SetArgs.Add(isCritical);
            this.Emit(EEntitySignal.DamageApplied);
        }

        public void GainExperience(int exp)
        {
            EntityDef.Exp += exp;
        }

        public void GainAdrenalineOnHit(int roll)
        {
            int adrenaline = (int)(roll / (float)MaxHealth * 100.0f);
#if DEBUG_LOGS
            LogDebug($"Adrenaline gained on hit (adrenaline={adrenaline}).");
#endif
            GainAdrenaline(adrenaline);
        }

        public void GainAdrenaline(int adrenaline)
        {
            for (int i = 0; i < 10; i++)
            {
                var skillDef = GetSkill(i);
                if (!skillDef) continue;
                if (skillDef.SkillType != ESkillType.Adrenaline) continue;

                var clampedAdrenaline = Mathf.Min(skillInstances.GetAdrenaline(i) + adrenaline, skillDef.Value);
                skillInstances.SetAdrenaline(i, clampedAdrenaline);
            }
        }

        public void ApplyStatus(CtSkillDef skillDef, CtEntity source, int turns)
        {
            var emptyIndex = -1;
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData)
                {
                    if (emptyIndex == -1)
                        emptyIndex = i;
                    continue;
                }

                var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                if (identifier != skillDef.Identifier) continue;

                var currentTurns = CtDataBlock.GetEffectTurns(statusEffect);
                if (turns > currentTurns)
                {
#if DEBUG_LOGS
                    LogDebug("Ignore applying effect if the current one is longer " +
                             $"(skillDef={skillDef}, currentTurns={currentTurns}, turns={turns}).");
#endif
                    UpdatePersistantEffects();
                }

                return;
            }

#if DEBUG_LOGS
            if (emptyIndex == -1)
            {
                LogCritical($"Failed to apply status (skillDef={skillDef}).");
                return;
            }
#endif

#if DEBUG_LOGS
            LogDebug($"Applying Status (skillDef={skillDef}, source={source}, turns={turns})");
#endif
            statusEffectInstances.SetStatusEffect(emptyIndex, CtDataBlock.CreateEffectData(skillDef.Identifier, source.Identifier, turns));
            UpdatePersistantEffects();
        }

        public int TryGetAttributeLevelByAttributeType(ushort attributeType)
        {
            for (int i = 0; i < ProfessionDef.Attributes.Length; ++i)
                if (ProfessionDef.Attributes[i].Identifier == attributeType)
                    return GetAttrRank(i);

            return 0;
        }

        private EDamageType _convertSlashing = EDamageType.None;
        
        public EDamageType ConvertSlashing
        {
            get => _convertSlashing;
            set
            {
                if (_convertSlashing != EDamageType.None) return;
                _convertSlashing = value;
            }
        }
        
        private EDamageType _convertBlunt = EDamageType.None;
        
        public EDamageType ConvertBlunt
        {
            get => _convertBlunt;
            set
            {
                if (_convertBlunt != EDamageType.None) return;
                _convertBlunt = value;
            }
        }
        
        private EDamageType _convertPiercing = EDamageType.None;
        
        public EDamageType ConvertPiercing
        {
            get => _convertPiercing;
            set
            {
                if (_convertPiercing != EDamageType.None) return;
                _convertPiercing = value;
            }
        }
        
        private EDamageType _convertEarth = EDamageType.None;
        
        public EDamageType ConvertEarth
        {
            get => _convertEarth;
            set
            {
                if (_convertEarth != EDamageType.None) return;
                _convertEarth = value;
            }
        }
        
        private EDamageType _convertFire = EDamageType.None;
        
        public EDamageType ConvertFire
        {
            get => _convertFire;
            set
            {
                if (_convertFire != EDamageType.None) return;
                _convertFire = value;
            }
        }
        
        private EDamageType _convertAir = EDamageType.None;
        
        public EDamageType ConvertAir
        {
            get => _convertAir;
            set
            {
                if (_convertAir != EDamageType.None) return;
                _convertAir = value;
            }
        }
        
        private EDamageType _convertWater = EDamageType.None;
        
        public EDamageType ConvertWater
        {
            get => _convertWater;
            set
            {
                if (_convertWater != EDamageType.None) return;
                _convertWater = value;
            }
        }
        
        private EDamageType _convertPhysical = EDamageType.None;
        
        public EDamageType ConvertPhysical
        {
            set
            {
                ConvertSlashing = value;
                ConvertBlunt = value;
                ConvertPiercing = value;
            }
        }
        
        private EDamageType _convertElemental = EDamageType.None;
        
        public EDamageType ConvertElemental
        {
            set
            {
                ConvertEarth = value;
                ConvertFire = value;
                ConvertAir = value;
                ConvertWater = value;
            }
        }

        public void UpdatePersistantEffects()
        {
            // TODO: Can this be done while iterating over the list the first time?

            _convertPhysical = EDamageType.None;
            _convertElemental = EDamageType.None;

            _convertEarth = EDamageType.None;
            _convertFire = EDamageType.None;
            _convertAir = EDamageType.None;
            _convertWater = EDamageType.None;

            // _intMods = new int[Convert.ToInt32(EIntModType.Max) * 2];
            // _floatMods = new float[Convert.ToInt32(EIntModType.Max) * 2];

            _absorbShield = 0;
            _block = 0;

            _rechargeIncrease = 0;
            _rechargeReduction = 0;
            _criticalChanceIncrease = 0;
            _criticalChanceReduction = 0;
            _armorPenetrationIncrease = 0;
            _armorPenetrationReduction = 0;
            _damageIncrease = 0;
            _damageReduction = 0;
            _armorRatingIncrease = 0;
            _armorRatingReduction = 0;
            _slashDamageIncrease = 0;
            _slashDamageReduction = 0;
            _slashArmorIncrease = 0;
            _slashArmorReduction = 0;
            _bluntDamageIncrease = 0;
            _bluntDamageReduction = 0;
            _bluntArmorIncrease = 0;
            _bluntArmorReduction = 0;
            _piercingDamageIncrease = 0;
            _piercingDamageReduction = 0;
            _piercingArmorIncrease = 0;
            _piercingArmorReduction = 0;
            _earthDamageIncrease = 0;
            _earthDamageReduction = 0;
            _earthArmorIncrease = 0;
            _earthArmorReduction = 0;
            _airDamageIncrease = 0;
            _airDamageReduction = 0;
            _airArmorIncrease = 0;
            _airArmorReduction = 0;
            _fireDamageIncrease = 0;
            _fireDamageReduction = 0;
            _fireArmorIncrease = 0;
            _fireArmorReduction = 0;
            _waterDamageIncrease = 0;
            _waterDamageReduction = 0;
            _waterArmorIncrease = 0;
            _waterArmorReduction = 0;
            _holyDamageIncrease = 0;
            _holyDamageReduction = 0;
            _holyArmorIncrease = 0;
            _holyArmorReduction = 0;

            IsDazed = false;
            IsBlind = false;
            IsKnockedDown = false;
            IsFreezing = false;

            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData) continue;

                var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                if (identifier == CtConstants.InvalidId) continue;

                var skillDef = gameData.GetSkillDef(identifier);
                if (!skillDef)
                {
#if DEBUG_LOGS
                    LogCritical($"Failed to find skill definition (skillId={identifier}).");
#endif
                    continue;
                }

                var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                if (!entityManager.TryGetEntity(sourceId, out var source))
                {
#if DEBUG_LOGS
                    LogCritical($"Failed to find source entity (sourceId={sourceId}).");
#endif
                    continue;
                }

                Debug.Log($"Persistant effects updated skillDef.OnPersistentEffect {skillDef}");
                skillDef.OnPersistentEffect(source, this);
            }

// #if DEBUG_LOGS
//             LogDebug($"Persistant effects updated (armorRatingReduction={GetIntMod(EIntModType.ArmorRating)}, slashReduction={GetIntMod(EIntModType.SlashArmor)}, bluntReduction={GetIntMod(EIntModType.BluntArmor)}, pierceReduction={GetIntMod(EIntModType.PiercingArmor)}, earthReduction={GetIntMod(EIntModType.EarthArmor)}, airReduction={GetIntMod(EIntModType.AirArmor)}, fireReduction={GetIntMod(EIntModType.FireArmor)}, waterReduction={GetIntMod(EIntModType.WaterArmor)}, isDazed={IsDazed}, isBlind={IsBlind}).");
// #endif
        }

        public bool ProcessStatusTick()
        {
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    var skillDef = gameData.GetSkillDef(identifier);
                    if (skillDef.HasTickEffect)
                    {
                        var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                        if (!entityManager.TryGetEntity(sourceId, out var source))
                        {
                            LogCritical($"Failed to find entity source (sourceId={sourceId}).");
                            continue;
                        }

                        var turns = CtDataBlock.GetEffectTurns(statusEffect);
                        LogDebug($"Processing Status Tick (skillDef={skillDef}, source={source}, turns={turns})");

                        skillDef.OnTickEffect(source, this);

                        if (State == ECombatState.Dead)
                            return true;
                    }
                }
            }

            return false;
        }

        public void RemoveExpiredStatusEffects(CtEntity source)
        {
            var expired = false;
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                    if (sourceId == source.Identifier)
                    {
                        var turns = CtDataBlock.GetEffectTurns(statusEffect) - 1;
                        if (turns > 0)
                        {
#if DEBUG_LOGS
                            var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                            LogDebug($"Turns left for status effect (effect={identifier}, turns={turns}).");
#endif

                            CtDataBlock.SetEffectTurns(turns, ref statusEffect);
                            statusEffectInstances.SetStatusEffect(i, statusEffect);
                        }
                        else
                        {
                            var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
#if DEBUG_LOGS
                            LogDebug($"Removing expired status effect (effect={identifier}, source={sourceId}).");
#endif

                            statusEffectInstances.SetStatusEffect(i, CtDataBlock.InvalidData);
                            expired = true;
                        }
                    }
                }
            }

            if (expired)
                UpdatePersistantEffects();
        }

        public bool TryRemoveStatusEffect(DataList toRemove, int count)
        {
            bool result = false;
            // Remove a condition.
            for (int i = 0; i < StatusEffectInstances.Count; i++)
            {
                var statusEffect = StatusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData) continue;

                var effectIdentifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                var skillDef = gameData.GetSkillDef(effectIdentifier);
                if (toRemove.Contains(skillDef))
                {
                    StatusEffectInstances.SetStatusEffect(i, CtDataBlock.InvalidData);
                    result = true;

                    count--;
                    if (count == 0)
                        break;
                }
            }

            return result;
        }

        public bool TryBlockWithEffects(CtEntity source, int damage)
        {
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData) continue;

                var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                var skillDef = gameData.GetSkillDef(identifier);
                if (!skillDef.HasBlockEffect) continue;

                if (skillDef.TryBlock(this, source, damage))
                    return true;
            }

            return false;
        }

        public void UseWeapon(CtEntity target)
        {
            CtSkillDef.MeleeAttack(gameData, this, target);
        }

        public void UseSkill(ushort skillId, CtEntity target, DataList adjacentTargets)
        {
            if (!EntityDef.TryGetSkillIndex(skillId, out var index))
            {
                LogCritical($"Failed to find skill identifier in entity skill set (skillId={skillId}, entity={this}).");
                return;
            }

#if DEBUG_LOGS
            var recharge = skillInstances.GetRecharge(index);
            if (recharge > 0)
            {
                LogWarning($"Skill was not fully recharge, but still used (index={index}, entity={this})?");
            }
#endif

            var usedSkillDef = gameData.GetSkillDef(skillId);
            usedSkillDef.OnUse(this, target, adjacentTargets);

            switch (usedSkillDef.SkillType)
            {
                case ESkillType.Energy:
                    Energy = Mathf.Max(0, Energy - usedSkillDef.Cost);
                    break;
                case ESkillType.Adrenaline:
                    skillInstances.SetAdrenaline(index, 0);
                    break;
            }

            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    var skillDef = gameData.GetSkillDef(identifier);
                    if (skillDef.HasSkillUsedEffect)
                    {
                        var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                        if (!entityManager.TryGetEntity(sourceId, out var source))
                        {
                            LogCritical($"Failed to find entity for skill used effect (sourceId={sourceId}).");
                            continue;
                        }

                        skillDef.OnSkillUsed(this, source, usedSkillDef);
                    }
                }
            }

            skillInstances.SetRecharge(index, usedSkillDef.RechargeTime);
        }

        private void _ResetBattleStates()
        {
            Reset();
            _ResetSkillInstanceData();
            _ResetStats();
            RequestSerialization();
        }

        public virtual void OnStartBattle()
        {
            _ResetBattleStates();
        }

        public void UpdateStatsAndSkills()
        {
            Health = Mathf.Min(Health + HealthRegen, MaxHealth);
            Energy = Mathf.Min(Energy + EnergyRegen, MaxEnergy);

            var rechargeSpeed = 1 - RechargeMod;
#if DEBUG_LOGS
            if (rechargeSpeed <= 0)
            {
                LogWarning("Recharge speed should never be less than or 0 " +
                           $"(rechargeSpeed={rechargeSpeed}, RechargeMod={RechargeMod}). " +
                           "Reverting to default 1 recharge speed.");
                rechargeSpeed = 1;
            }
#endif

#if DEBUG_LOGS
            LogDebug($"Recharge speed is {rechargeSpeed} turns.");
#endif

            for (int i = 0; i < 10; i++)
            {
                var recharge = skillInstances.GetRecharge(i);
                if (recharge > 0)
                    skillInstances.SetRecharge(i, Mathf.Max(recharge - rechargeSpeed, 0));
            }
        }

        public virtual CtBattleState BattleState
        {
            get => null;
            set
            {
                // Do nothing?
            }
        }

        public virtual bool IsReady()
        {
            return true;
        }

        public virtual bool IsReadyToLeave() => true;

        public virtual bool HasAttackReady() => false;

        public virtual bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            skillId = CtConstants.InvalidId;
            targetId = CtConstants.InvalidId;
            return false;
        }

        public virtual void ResetAttack() { }

        public virtual void OnEndBattle()
        {
            _ResetBattleStates();
        }

        public EDamageType ConvertDamageType(EDamageType damageType)
        {
            switch (damageType)
            {
                case EDamageType.Slashing:
                    return _convertSlashing;
                case EDamageType.Blunt:
                    return _convertBlunt;
                case EDamageType.Piercing:
                    return _convertPiercing;
                case EDamageType.Earth:
                    return _convertEarth;
                case EDamageType.Fire:
                    return _convertFire;
                case EDamageType.Air:
                    return _convertAir;
                case EDamageType.Water:
                    return _convertWater;
            }

            return damageType;
        }

        
    }
}