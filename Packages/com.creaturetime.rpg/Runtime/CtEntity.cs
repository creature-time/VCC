
using System;
using UdonSharp;
using UnityEngine;

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
        IdentifierChanged,
        HealthChanged,
        EnergyChanged,
        SkillRechargeChanged,
        SkillAdrenalineChanged,
        EffectChanged,
        EntityStatsChanged,
        DamageApplied
    }

    public abstract class CtEntity : CtEntityBase
    {
        [Header("Global Variables")]
        [SerializeField] protected CtGameData gameData;

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

                SetArgs.Add(_health);
                this.Emit(EEntitySignal.HealthChanged);
            }
        }

        public int Health
        {
            get => HealthCallback;
            private set => HealthCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(EnergyCallback))]
        private int _energy = -1;

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
            private set => EnergyCallback = value;
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge0Callback))] private int _skillRecharge0;

        public int SkillRecharge0Callback
        {
            get => _skillRecharge0;
            set
            {
                _skillRecharge0 = value;
                SetArgs.Add(0);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge0
        {
            get => SkillRecharge0Callback;
            set
            {
                SkillRecharge0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge1Callback))] private int _skillRecharge1;

        public int SkillRecharge1Callback
        {
            get => _skillRecharge1;
            set
            {
                _skillRecharge1 = value;
                SetArgs.Add(1);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge1
        {
            get => SkillRecharge1Callback;
            set
            {
                SkillRecharge1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge2Callback))] private int _skillRecharge2;

        public int SkillRecharge2Callback
        {
            get => _skillRecharge2;
            set
            {
                _skillRecharge2 = value;
                SetArgs.Add(2);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge2
        {
            get => SkillRecharge2Callback;
            set
            {
                SkillRecharge2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge3Callback))] private int _skillRecharge3;

        public int SkillRecharge3Callback
        {
            get => _skillRecharge3;
            set
            {
                _skillRecharge3 = value;
                SetArgs.Add(3);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge3
        {
            get => SkillRecharge3Callback;
            set
            {
                SkillRecharge3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge4Callback))] private int _skillRecharge4;

        public int SkillRecharge4Callback
        {
            get => _skillRecharge4;
            set
            {
                _skillRecharge4 = value;
                SetArgs.Add(4);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge4
        {
            get => SkillRecharge4Callback;
            set
            {
                SkillRecharge4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge5Callback))] private int _skillRecharge5;

        public int SkillRecharge5Callback
        {
            get => _skillRecharge5;
            set
            {
                _skillRecharge5 = value;
                SetArgs.Add(5);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge5
        {
            get => SkillRecharge5Callback;
            set
            {
                SkillRecharge5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge6Callback))] private int _skillRecharge6;

        public int SkillRecharge6Callback
        {
            get => _skillRecharge6;
            set
            {
                _skillRecharge6 = value;
                SetArgs.Add(6);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge6
        {
            get => SkillRecharge6Callback;
            set
            {
                SkillRecharge6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge7Callback))] private int _skillRecharge7;

        public int SkillRecharge7Callback
        {
            get => _skillRecharge7;
            set
            {
                _skillRecharge0 = value;
                SetArgs.Add(7);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge7
        {
            get => SkillRecharge7Callback;
            set
            {
                SkillRecharge7Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge8Callback))] private int _skillRecharge8;

        public int SkillRecharge8Callback
        {
            get => _skillRecharge8;
            set
            {
                _skillRecharge8 = value;
                SetArgs.Add(8);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge8
        {
            get => SkillRecharge8Callback;
            set
            {
                SkillRecharge8Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge9Callback))] private int _skillRecharge9;

        public int SkillRecharge9Callback
        {
            get => _skillRecharge9;
            set
            {
                _skillRecharge9 = value;
                SetArgs.Add(9);
                this.Emit(EEntitySignal.SkillRechargeChanged);
            }
        }

        public int SkillRecharge9
        {
            get => SkillRecharge9Callback;
            set
            {
                SkillRecharge9Callback = value;
                RequestSerialization();
            }
        }

        public int GetRecharge(int index)
        {
            switch (index)
            {
                case 0: return SkillRecharge0;
                case 1: return SkillRecharge1;
                case 2: return SkillRecharge2;
                case 3: return SkillRecharge3;
                case 4: return SkillRecharge4;
                case 5: return SkillRecharge5;
                case 6: return SkillRecharge6;
                case 7: return SkillRecharge7;
                case 8: return SkillRecharge8;
                case 9: return SkillRecharge9;
                default: return CtConstants.InvalidId;
            }
        }

        private void _SetRecharge(int index, int value)
        {
            switch (index)
            {
                case 0: SkillRecharge0 = value; return;
                case 1: SkillRecharge1 = value; return;
                case 2: SkillRecharge2 = value; return;
                case 3: SkillRecharge3 = value; return;
                case 4: SkillRecharge4 = value; return;
                case 5: SkillRecharge5 = value; return;
                case 6: SkillRecharge6 = value; return;
                case 7: SkillRecharge7 = value; return;
                case 8: SkillRecharge8 = value; return;
                case 9: SkillRecharge9 = value; return;
                default: return;
            }
        }
        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline0Callback))] private int _skillAdrenaline0;

        public int SkillAdrenaline0Callback
        {
            get => _skillAdrenaline0;
            set
            {
                _skillAdrenaline0 = value;
                SetArgs.Add(0);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline0
        {
            get => SkillAdrenaline0Callback;
            set
            {
                SkillAdrenaline0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline1Callback))] private int _skillAdrenaline1;

        public int SkillAdrenaline1Callback
        {
            get => _skillAdrenaline1;
            set
            {
                _skillAdrenaline1 = value;
                SetArgs.Add(1);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline1
        {
            get => SkillAdrenaline1Callback;
            set
            {
                SkillAdrenaline1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline2Callback))] private int _skillAdrenaline2;

        public int SkillAdrenaline2Callback
        {
            get => _skillAdrenaline2;
            set
            {
                _skillAdrenaline2 = value;
                SetArgs.Add(2);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline2
        {
            get => SkillAdrenaline2Callback;
            set
            {
                SkillAdrenaline2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline3Callback))] private int _skillAdrenaline3;

        public int SkillAdrenaline3Callback
        {
            get => _skillAdrenaline3;
            set
            {
                _skillAdrenaline3 = value;
                SetArgs.Add(3);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline3
        {
            get => SkillAdrenaline3Callback;
            set
            {
                SkillAdrenaline3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline4Callback))] private int _skillAdrenaline4;

        public int SkillAdrenaline4Callback
        {
            get => _skillAdrenaline4;
            set
            {
                _skillAdrenaline4 = value;
                SetArgs.Add(4);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline4
        {
            get => SkillAdrenaline4Callback;
            set
            {
                SkillAdrenaline4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline5Callback))] private int _skillAdrenaline5;

        public int SkillAdrenaline5Callback
        {
            get => _skillAdrenaline5;
            set
            {
                _skillAdrenaline5 = value;
                SetArgs.Add(5);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline5
        {
            get => SkillAdrenaline5Callback;
            set
            {
                SkillAdrenaline5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline6Callback))] private int _skillAdrenaline6;

        public int SkillAdrenaline6Callback
        {
            get => _skillAdrenaline6;
            set
            {
                _skillAdrenaline6 = value;
                SetArgs.Add(6);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline6
        {
            get => SkillAdrenaline6Callback;
            set
            {
                SkillAdrenaline6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline7Callback))] private int _skillAdrenaline7;

        public int SkillAdrenaline7Callback
        {
            get => _skillAdrenaline7;
            set
            {
                _skillAdrenaline0 = value;
                SetArgs.Add(7);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline7
        {
            get => SkillAdrenaline7Callback;
            set
            {
                SkillAdrenaline7Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline8Callback))] private int _skillAdrenaline8;

        public int SkillAdrenaline8Callback
        {
            get => _skillAdrenaline8;
            set
            {
                _skillAdrenaline8 = value;
                SetArgs.Add(8);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline8
        {
            get => SkillAdrenaline8Callback;
            set
            {
                SkillAdrenaline8Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline9Callback))] private int _skillAdrenaline9;

        public int SkillAdrenaline9Callback
        {
            get => _skillAdrenaline9;
            set
            {
                _skillAdrenaline9 = value;
                SetArgs.Add(9);
                this.Emit(EEntitySignal.SkillAdrenalineChanged);
            }
        }

        public int SkillAdrenaline9
        {
            get => SkillAdrenaline9Callback;
            set
            {
                SkillAdrenaline9Callback = value;
                RequestSerialization();
            }
        }

        public int GetAdrenaline(int index)
        {
            switch (index)
            {
                case 0: return SkillAdrenaline0;
                case 1: return SkillAdrenaline1;
                case 2: return SkillAdrenaline2;
                case 3: return SkillAdrenaline3;
                case 4: return SkillAdrenaline4;
                case 5: return SkillAdrenaline5;
                case 6: return SkillAdrenaline6;
                case 7: return SkillAdrenaline7;
                case 8: return SkillAdrenaline8;
                case 9: return SkillAdrenaline9;
                default: return -1;
            }
        }

        private void _SetAdrenaline(int index, int value)
        {
            switch (index)
            {
                case 0: SkillAdrenaline0 = value; return;
                case 1: SkillAdrenaline1 = value; return;
                case 2: SkillAdrenaline2 = value; return;
                case 3: SkillAdrenaline3 = value; return;
                case 4: SkillAdrenaline4 = value; return;
                case 5: SkillAdrenaline5 = value; return;
                case 6: SkillAdrenaline6 = value; return;
                case 7: SkillAdrenaline7 = value; return;
                case 8: SkillAdrenaline8 = value; return;
                case 9: SkillAdrenaline9 = value; return;
                default: return;
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

        public CtSkillDef GetSkillDef(int index)
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

        // public int ArmorRating { get; set; }
        public int ArmorRatingReduction { get; set; }

        public float SlashReduction { get; set; }
        public float BluntReduction { get; set; }
        public float PierceReduction { get; set; }

        public float EarthReduction { get; set; }
        public float AirReduction { get; set; }
        public float FireReduction { get; set; }
        public float WaterReduction { get; set; }

        public bool IsDazed { get; set; }
        public bool IsBlind { get; set; }

        public float NormalizedHealth => Health / (float)_entityDef.MaxHealth;
        public float NormalizedEnergy => Energy / (float)_entityDef.MaxEnergy;
        public string DisplayName => _entityDef.DisplayName;
        public Texture Icon => _entityDef.Icon;

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
                    _entityDef.Disconnect(EEntityStatsSignal.SkillSlotChanged, this, nameof(_OnSkillSlotChanged));

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

                    Reset();
                }

                _entityDef = value;
                if (_entityDef)
                {
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

                    _entityDef.Connect(EEntityStatsSignal.SkillSlotChanged, this, nameof(_OnSkillSlotChanged));
                }

                SetArgs.Add(_entityDef);
                this.Emit(EEntitySignal.EntityStatsChanged);
            }
        }

        public void _OnSkillSlotChanged()
        {
            _OnSkillSlotChangedRaw(GetArgs[0].Int, _entityDef.SkillSlot0);
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

        public void Reset()
        {
            Health = -1;
            Energy = -1;
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

        private void _ResetSkillInstanceData()
        {
            SkillRecharge0 = 0;
            SkillRecharge1 = 0;
            SkillRecharge2 = 0;
            SkillRecharge3 = 0;
            SkillRecharge4 = 0;
            SkillRecharge5 = 0;
            SkillRecharge6 = 0;
            SkillRecharge7 = 0;
            SkillRecharge8 = 0;
            SkillRecharge9 = 0;

            SkillAdrenaline0 = 0;
            SkillAdrenaline1 = 0;
            SkillAdrenaline2 = 0;
            SkillAdrenaline3 = 0;
            SkillAdrenaline4 = 0;
            SkillAdrenaline5 = 0;
            SkillAdrenaline6 = 0;
            SkillAdrenaline7 = 0;
            SkillAdrenaline8 = 0;
            SkillAdrenaline9 = 0;
        }

        public override void ApplyDamage(int damage, EDamageType damageType, 
            EDamageSourceType damageSourceType, ushort identifier, CtEntity instigator, bool isCritical)
        {
            if (damage >= 0)
            {
                // Pre-damage calculations.
                GainAdrenalineOnHit(damage);

                // Check for resistances.
                int resistedDamage = 0;

                switch (damageType)
                {
                    case EDamageType.Slashing:
                        resistedDamage = (int)(damage * SlashReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Blunt:
                        resistedDamage = (int)(damage * BluntReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Piercing:
                        resistedDamage = (int)(damage * PierceReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Earth:
                        resistedDamage = (int)(damage * EarthReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Fire:
                        resistedDamage = (int)(damage * FireReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Air:
                        resistedDamage = (int)(damage * AirReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Water:
                        resistedDamage = (int)(damage * WaterReduction);
                        damage -= resistedDamage;
                        break;
                    case EDamageType.Smiting:
                    // resistedDamage = (int)(damage * SmiteReduction);
                    // damage -= resistedDamage;
                    // break;
                    case EDamageType.Bleeding:
                    case EDamageType.Burning:
                    // resistedDamage = (int)(damage * FireReduction);
                    // damage -= resistedDamage;
                    // break;
                    case EDamageType.Disease:
                    case EDamageType.Poison:
                        break;
                    default:
#if DEBUG_LOGS
                    CtLogger.LogCritical("Entity", $"Damage type not supported (damageType={damageType}.");
#endif
                        break;
                }

                damage = Mathf.Max(0, damage);

                // Calculate damage so we don't overkill.
                damage = Mathf.Min(Health, damage);

                // Update total damage resisted stats.
                DamageTakenResisted += resistedDamage;
                instigator.DamageResisted += resistedDamage;

                // Update total damage taken stats.
                DamageTaken += damage;
                instigator.DamageDealt += damage;

                // Apply damage.
                Health -= damage;

                if (Health <= 0)
                    Health = 0;

// #if DEBUG_LOGS
                LogDebug("Damage applied (" +
                         $"target={this}, damage={damage}, damageType={damageType}, " +
                         $"damageSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
                         $"isCritical={isCritical}" +
                         ").");
// #endif
            }
            else
            {
                var healing = -damage;

                // Calculate heal so we don't over heal.
                healing = Mathf.Min(healing, EntityDef.MaxHealth - Health);

                Health += healing;

// #if DEBUG_LOGS
                LogDebug("Healing applied (" +
                         $"target={this}, healing={healing}, healingType={damageType}, " +
                         $"healingSourceType={damageSourceType}, identifier={identifier}, instigator={instigator}, " +
                         $"isCritical={isCritical}" +
                         ").");
// #endif
            }

            // Request serialization.
            RequestSerialization();
            instigator.RequestSerialization();

            SetArgs.Add(Convert.ToInt32(damageSourceType));
            SetArgs.Add(identifier);
            SetArgs.Add(instigator);
            SetArgs.Add(this);
            SetArgs.Add(Convert.ToInt32(damageType));
            SetArgs.Add(damage);
            SetArgs.Add(isCritical);
            this.Emit(EEntitySignal.DamageApplied);
        }

        private void GainAdrenalineOnHit(int roll)
        {
            int adrenaline = (int)(roll / (float)EntityDef.MaxHealth * 100.0f);
            var skillDefs = new CtSkillDef[]
            {
                _skillDef0,
                _skillDef1,
                _skillDef2,
                _skillDef3,
                _skillDef4,
                _skillDef5,
                _skillDef6,
                _skillDef7,
                _skillDef8,
                _skillDef9,
            };

            for (int i = 0; i < skillDefs.Length; i++)
            {
                var skillDef = skillDefs[i];
                if (skillDef && skillDef.Type == ESkillType.Adrenaline)
                    _SetAdrenaline(i, Mathf.Min(GetAdrenaline(i) + adrenaline, skillDef.Value));
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect0Callback))] private ulong _statusEffect0 = CtDataBlock.InvalidData;

        public ulong StatusEffect0Callback
        {
            get => _statusEffect0;
            set
            {
                _statusEffect0 = value;
                SetArgs.Add(0);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect0
        {
            get => StatusEffect0Callback;
            set
            {
                StatusEffect0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect1Callback))] private ulong _statusEffect1 = CtDataBlock.InvalidData;

        public ulong StatusEffect1Callback
        {
            get => _statusEffect1;
            set
            {
                _statusEffect1 = value;
                SetArgs.Add(1);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect1
        {
            get => StatusEffect1Callback;
            set
            {
                StatusEffect1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect2Callback))] private ulong _statusEffect2 = CtDataBlock.InvalidData;

        public ulong StatusEffect2Callback
        {
            get => _statusEffect2;
            set
            {
                _statusEffect2 = value;
                SetArgs.Add(2);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect2
        {
            get => StatusEffect2Callback;
            set
            {
                StatusEffect2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect3Callback))] private ulong _statusEffect3 = CtDataBlock.InvalidData;

        public ulong StatusEffect3Callback
        {
            get => _statusEffect3;
            set
            {
                _statusEffect3 = value;
                SetArgs.Add(3);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect3
        {
            get => StatusEffect3Callback;
            set
            {
                StatusEffect3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect4Callback))] private ulong _statusEffect4 = CtDataBlock.InvalidData;

        public ulong StatusEffect4Callback
        {
            get => _statusEffect4;
            set
            {
                _statusEffect4 = value;
                SetArgs.Add(4);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect4
        {
            get => StatusEffect4Callback;
            set
            {
                StatusEffect4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect5Callback))] private ulong _statusEffect5 = CtDataBlock.InvalidData;

        public ulong StatusEffect5Callback
        {
            get => _statusEffect5;
            set
            {
                _statusEffect5 = value;
                SetArgs.Add(5);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect5
        {
            get => StatusEffect5Callback;
            set
            {
                StatusEffect5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect6Callback))] private ulong _statusEffect6 = CtDataBlock.InvalidData;

        public ulong StatusEffect6Callback
        {
            get => _statusEffect6;
            set
            {
                _statusEffect6 = value;
                SetArgs.Add(6);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect6
        {
            get => StatusEffect6Callback;
            set
            {
                StatusEffect6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect7Callback))] private ulong _statusEffect7 = CtDataBlock.InvalidData;

        public ulong StatusEffect7Callback
        {
            get => _statusEffect7;
            set
            {
                _statusEffect7 = value;
                SetArgs.Add(7);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect7
        {
            get => StatusEffect7Callback;
            set
            {
                StatusEffect7Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect8Callback))] private ulong _statusEffect8 = CtDataBlock.InvalidData;

        public ulong StatusEffect8Callback
        {
            get => _statusEffect8;
            set
            {
                _statusEffect8 = value;
                SetArgs.Add(8);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect8
        {
            get => StatusEffect8Callback;
            set
            {
                StatusEffect8Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect9Callback))] private ulong _statusEffect9 = CtDataBlock.InvalidData;

        public ulong StatusEffect9Callback
        {
            get => _statusEffect9;
            set
            {
                _statusEffect9 = value;
                SetArgs.Add(9);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect9
        {
            get => StatusEffect9Callback;
            set
            {
                StatusEffect9Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect10Callback))] private ulong _statusEffect10 = CtDataBlock.InvalidData;

        public ulong StatusEffect10Callback
        {
            get => _statusEffect10;
            set
            {
                _statusEffect10 = value;
                SetArgs.Add(10);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect10
        {
            get => StatusEffect10Callback;
            set
            {
                StatusEffect10Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect11Callback))] private ulong _statusEffect11 = CtDataBlock.InvalidData;

        public ulong StatusEffect11Callback
        {
            get => _statusEffect11;
            set
            {
                _statusEffect11 = value;
                SetArgs.Add(11);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect11
        {
            get => StatusEffect11Callback;
            set
            {
                StatusEffect11Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect12Callback))] private ulong _statusEffect12 = CtDataBlock.InvalidData;

        public ulong StatusEffect12Callback
        {
            get => _statusEffect12;
            set
            {
                _statusEffect12 = value;
                SetArgs.Add(12);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect12
        {
            get => StatusEffect12Callback;
            set
            {
                StatusEffect12Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect13Callback))] private ulong _statusEffect13 = CtDataBlock.InvalidData;

        public ulong StatusEffect13Callback
        {
            get => _statusEffect13;
            set
            {
                _statusEffect13 = value;
                SetArgs.Add(13);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect13
        {
            get => StatusEffect13Callback;
            set
            {
                StatusEffect13Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect14Callback))] private ulong _statusEffect14 = CtDataBlock.InvalidData;

        public ulong StatusEffect14Callback
        {
            get => _statusEffect14;
            set
            {
                _statusEffect14 = value;
                SetArgs.Add(14);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect14
        {
            get => StatusEffect14Callback;
            set
            {
                StatusEffect14Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect15Callback))] private ulong _statusEffect15 = CtDataBlock.InvalidData;

        public ulong StatusEffect15Callback
        {
            get => _statusEffect15;
            set
            {
                _statusEffect15 = value;
                SetArgs.Add(15);
                this.Emit(EEntitySignal.EffectChanged);
            }
        }

        public ulong StatusEffect15
        {
            get => StatusEffect15Callback;
            set
            {
                StatusEffect15Callback = value;
                RequestSerialization();
            }
        }

        public ulong GetStatusEffect(int index)
        {
            switch (index)
            {
                case 0: return StatusEffect0;
                case 1: return StatusEffect1;
                case 2: return StatusEffect2;
                case 3: return StatusEffect3;
                case 4: return StatusEffect4;
                case 5: return StatusEffect5;
                case 6: return StatusEffect6;
                case 7: return StatusEffect7;
                case 8: return StatusEffect8;
                case 9: return StatusEffect9;
                case 10: return StatusEffect10;
                case 11: return StatusEffect11;
                case 12: return StatusEffect12;
                case 13: return StatusEffect13;
                case 14: return StatusEffect14;
                case 15: return StatusEffect15;
                default: return CtDataBlock.InvalidData;
            }
        }

        public void SetStatusEffect(int index, ulong value)
        {
            switch (index)
            {
                case 0: StatusEffect0 = value; break;
                case 1: StatusEffect1 = value; break;
                case 2: StatusEffect2 = value; break;
                case 3: StatusEffect3 = value; break;
                case 4: StatusEffect4 = value; break;
                case 5: StatusEffect5 = value; break;
                case 6: StatusEffect6 = value; break;
                case 7: StatusEffect7 = value; break;
                case 8: StatusEffect8 = value; break;
                case 9: StatusEffect9 = value; break;
                case 10: StatusEffect10 = value; break;
                case 11: StatusEffect11 = value; break;
                case 12: StatusEffect12 = value; break;
                case 13: StatusEffect13 = value; break;
                case 14: StatusEffect14 = value; break;
                case 15: StatusEffect15 = value; break;
                default: return;
            }
        }

        public void ApplyStatus(CtSkillDef skillDef, CtEntity source, int turns)
        {
            for (int i = 0; i < 16; i++)
            {
                var statusEffect = GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    if (identifier == skillDef.Identifier)
                    {
                        var currentTurns = CtDataBlock.GetEffectTurns(statusEffect);
                        if (currentTurns >= turns)
                        {
                            LogDebug("Ignore applying effect if the current one is longer " +
                                     $"(skillDef={skillDef}, currentTurns={currentTurns}, turns={turns}).");

                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < 16; i++)
            {
                var statusEffect = GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData)
                {
                    LogDebug($"Applying Status (skillDef={skillDef}, source={source}, turns={turns})");
                    SetStatusEffect(i, CtDataBlock.CreateEffectData(skillDef.Identifier, source.Identifier, turns));
                    return;
                }
            }
        }

        public bool ProcessStatusTick()
        {
            CtEntity source;
            for (int i = 0; i < 16; i++)
            {
                var statusEffect = GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    var skillDef = gameData.GetSkillDef(identifier);
                    if (skillDef.HasTickEffect)
                    {
                        var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                        EntityManager.TryGetEntity(sourceId, out source);

                        var turns = CtDataBlock.GetEffectTurns(statusEffect);
                        LogDebug($"Processing Status Tick (skillDef={skillDef}, source={source}, turns={turns})");

                        skillDef.OnTickEffect(this, source);

                        if (State == ECombatState.Dead)
                            return true;
                    }
                }
            }

            return false;
        }

        public void RemoveExpiredStatusEffects(CtEntity source)
        {
            for (int i = 0; i < 16; i++)
            {
                var statusEffect = GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                    if (sourceId == source.Identifier)
                    {
                        var turns = CtDataBlock.GetEffectTurns(statusEffect) - 1;
                        if (turns > 0)
                        {
                            CtDataBlock.SetEffectTurns(turns, ref statusEffect);
                            SetStatusEffect(i, statusEffect);
                        }
                        else
                        {
                            var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                            LogDebug($"Removing expired status effect (effect={identifier}, source={sourceId}).");

                            SetStatusEffect(i, CtDataBlock.InvalidData);
                        }
                    }
                }
            }
        }

        public void UseWeapon(CtEntity target)
        {
            CtSkillDef.MeleeAttack(gameData, target, this);
        }

        public void UseSkill(ushort skillId, CtEntity target)
        {
            if (!EntityDef.TryGetSkillIndex(skillId, out int index))
            {
                LogCritical($"Failed to find skill identifier in entity skill set (skillId={skillId}, entity={this}).");
                return;
            }

            var usedSkillDef = gameData.GetSkillDef(skillId);
            usedSkillDef.OnUse(gameData, target, this);

            switch (usedSkillDef.Type)
            {
                case ESkillType.Energy:
                    Energy = Mathf.Max(0, Energy - usedSkillDef.Cost);
                    break;
                case ESkillType.Adrenaline:
                    _SetAdrenaline(index, 0);
                    break;
            }

            for (int i = 0; i < 16; i++)
            {
                var statusEffect = GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    var skillDef = gameData.GetSkillDef(identifier);
                    if (skillDef.HasSkillUsedEffect)
                    {
                        var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                        if (!EntityManager.TryGetEntity(sourceId, out var source))
                        {
                            LogCritical($"Failed to find entity for skill used effect (sourceId={sourceId}).");
                            continue;
                        }

                        skillDef.OnSkillUsed(gameData, this, source, usedSkillDef);
                    }
                }
            }

            _SetRecharge(index, usedSkillDef.RechargeTime);
        }

        public virtual void OnStartBattle()
        {
            Health = EntityDef.MaxHealth;
            Energy = EntityDef.MaxEnergy;
            _ResetSkillInstanceData();
            _ResetStats();

            RequestSerialization();
        }

        public void UpdateStatsAndSkills()
        {
            Energy += EntityDef.EnergyRegeneration;

            int rechargeSpeed = 1;
            for (int i = 0; i < 10; i++)
            {
                var recharge = GetRecharge(i);
                if (recharge > 0)
                    _SetRecharge(i, Mathf.Max(recharge - rechargeSpeed, 0));
            }
        }

        public virtual CtBattleState BattleState
        {
            set
            {
                // Do nothing?
            }
        }

        public virtual bool IsReady()
        {
            return true;
        }

        public virtual bool HasAttackReady() => false;

        public virtual bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            skillId = CtConstants.InvalidId;
            targetId = CtConstants.InvalidId;
            return false;
        }

        public virtual void ResetAttack() { }

        public virtual void OnEndBattle() {}
    }
}