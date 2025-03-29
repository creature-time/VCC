
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        IdentifierChanged,
        HealthChanged,
        EnergyChanged,
        EffectChanged,
        EntityStatsChanged,
        ApplyDamage
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtEntity : CtAbstractSignal
    {
        private const int MaxSkillCount = 10;

        [Header("Global Variables")]
        [SerializeField] private CtGameData gameData;

        public ushort Identifier { get; private set; } = CtConstants.InvalidId;

        [UdonSynced, FieldChangeCallback(nameof(EntityIdCallback))]
        ushort _entityId = CtConstants.InvalidId;
        
        public ushort EntityIdCallback
        {
            get => _entityId;
            set
            {
                if (_entityId == value)
                    return;

                var previousId = _entityId;
                _entityId = value;

                SetArgs.Add(previousId);
                SetArgs.Add(_entityId);
                this.Emit(EEntitySignal.IdentifierChanged);
            }
        }

        public ushort EntityId
        {
            get => EntityIdCallback;
            set
            {
                EntityIdCallback = value;
                RequestSerialization();
            }
        }

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

        [UdonSynced]
        private int[] _recharge = new int[MaxSkillCount]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };
        private int[] _rechargeCmp = new int[MaxSkillCount]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        [UdonSynced]
        private int[] _adrenaline = new int[MaxSkillCount]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };
        private int[] _adrenalineCmp = new int[MaxSkillCount]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

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

        private CtSkillDef[] _skillDefs = new CtSkillDef[MaxSkillCount];

        // public int ArmorRating { get; set; }
        public int ArmorRatingReduction { get; set; }
        public float DamageReduction { get; set; }
        public bool IsDazed { get; set; }
        public bool IsBlind { get; set; }

        public float NormalizedHealth => Health / (float)_entityStats.MaxHealth;
        public string DisplayName => _entityStats.DisplayName;
        public Texture Icon => _entityStats.Icon;
        public bool IsPlayer { get; private set; }

        private CtEntityDef _entityStats;

        public CtEntityDef EntityStats
        {
            get => _entityStats;
            private set
            {
                if (_entityStats)
                {
                    _entityStats.Disconnect(EEntityStatsSignal.SkillChanged, this, nameof(_OnSkillChanged));

                    for (int i = 0; i < MaxSkillCount; ++i)
                        _skillDefs[i] = null;

                    Reset();
                }

                _entityStats = value;
                if (_entityStats)
                {
                    _OnSkillChanged();

                    _entityStats.Connect(EEntityStatsSignal.SkillChanged, this, nameof(_OnSkillChanged));
                }

                SetArgs.Add(_entityStats);
                this.Emit(EEntitySignal.EntityStatsChanged);
            }
        }

        public CtPlayerDef PlayerDef
        {
            set
            {
                if (EntityStats)
                    IsPlayer = false;
                EntityStats = value;
                if (EntityStats)
                    IsPlayer = true;
            }
        }

        public CtNpcDef NpcDef
        {
            set => EntityStats = value;
        }

        public void Init(ushort identifier)
        {
            Identifier = identifier;
        }

        public void _OnSkillChanged()
        {
            for (int i = 0; i < MaxSkillCount; ++i)
            {
                ushort skillId = _entityStats.GetSkill(i);
                _skillDefs[i] = skillId != CtConstants.InvalidId ? gameData.GetSkillDef(skillId) : null;
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
            for (int i = 0; i < _recharge.Length; ++i)
                _recharge[i] = 0;
            for (int i = 0; i < _adrenaline.Length; ++i)
                _adrenaline[i] = 0;
        }

        public void ApplyHeal(ushort instanceId, int heal, ushort identifier, CtEntity instigator)
        {
            // Calculate max heal so we don't over heal.
            heal = Mathf.Min(EntityStats.MaxHealth - Health, heal);

            // Update total healing stats.
            instigator.HealingDealt += heal;
            HealingTaken += heal;

            // Apply heal.
            Health += heal;

            // Request serialization.
            RequestSerialization();
            instigator.RequestSerialization();

            // TODO: Do same as damage notifications.
        }

        public void ApplyDamage(ushort instanceId, int damage, EDamageType damageType, EDamageSourceType damageSourceType, int identifier, 
            CtEntity instigator, bool isCritical)
        {
            // Pre-damage calculations.
            GainAdrenalineOnHit(this, damage);

            // Check for resistances.
            int resistedDamage = 0;

            switch (damageType)
            {
                case EDamageType.Slashing:
                case EDamageType.Blunt:
                case EDamageType.Piercing:
                case EDamageType.Earth:
                case EDamageType.Fire:
                case EDamageType.Air:
                case EDamageType.Water:
                case EDamageType.Smiting:
                    // CtLogger.LogDebug("Entity", $"Damage reduction was {DamageReduction}.");
                    // resistedDamage = (int)(damage * DamageReduction);
                    // damage -= resistedDamage;
                    // break;
                case EDamageType.Bleeding:
                case EDamageType.Burning:
                case EDamageType.Disease:
                case EDamageType.Poison:
                    break;
                default:
                    CtLogger.LogCritical("Entity", $"Damage type not supported (damageType={damageType}.");
                    break;
            }

            // Calculate damage so we don't over kill.
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
            this.Emit(EEntitySignal.ApplyDamage);
        }

        private void GainAdrenalineOnHit(CtEntity target, int roll)
        {
            int adrenaline = (int)(roll / (float)target.EntityStats.MaxHealth * 100.0f);

            for (int i = 0; i < MaxSkillCount; ++i)
            {
                CtSkillDef skill = _skillDefs[i];
                if (skill && skill.Type == ESkillType.Adrenaline)
                {
                    if (target.EntityStats.GetSkill(i) != CtConstants.InvalidId)
                    {
                        target._adrenaline[i] = Mathf.Min(target._adrenaline[i] + adrenaline, skill.Value);
                        RequestSerialization();
                    }
                }
            }
        }

        public void SetupEntityForBattle()
        {
            Health = EntityStats.MaxHealth;
            Energy = EntityStats.MaxEnergy;
            _ResetSkillInstanceData();
            _ResetStats();

            RequestSerialization();
        }

        public void UpdateStatsAndSkills()
        {
            Energy += EntityStats.EnergyRegeneration;

            for (int i = 0; i < MaxSkillCount; ++i)
            {
                if (_recharge[i] > 0)
                {
                    _recharge[i] = Mathf.Max(_recharge[i] - 1, 0);
                    RequestSerialization();
                }
            }
        }

        public void TransferOwnership(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(player, gameObject))
                Networking.SetOwner(player, gameObject);
        }
    }
}