
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

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
        EntityDefChanged,
        DamageApplied
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

        public float NormalizedHealth => _entityDef ? Health / (float)_entityDef.MaxHealth : 0;
        public float NormalizedEnergy => _entityDef ? Energy / (float)_entityDef.MaxEnergy : 0;
        public string DisplayName => _entityDef ? _entityDef.DisplayName : "Disconnected";
        public Texture Icon => _entityDef ? _entityDef.Icon : null;
        public int Level => _entityDef ? _entityDef.CharacterLevel : 0;

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
                this.Emit(EEntitySignal.EntityDefChanged);
            }
        }

        public void _OnSkillSlotChanged()
        {
            var index = GetArgs[0].Int;
            _OnSkillSlotChangedRaw(index, _entityDef.GetSkill(index));
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
            skillInstances.SkillRecharge0 = (char)0;
            skillInstances.SkillRecharge1 = (char)0;
            skillInstances.SkillRecharge2 = (char)0;
            skillInstances.SkillRecharge3 = (char)0;
            skillInstances.SkillRecharge4 = (char)0;
            skillInstances.SkillRecharge5 = (char)0;
            skillInstances.SkillRecharge6 = (char)0;
            skillInstances.SkillRecharge7 = (char)0;
            skillInstances.SkillRecharge8 = (char)0;
            skillInstances.SkillRecharge9 = (char)0;

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
                    LogCritical($"Damage type not supported (damageType={damageType}.");
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
                var healing = -damage;

                // Calculate heal so we don't over heal.
                healing = Mathf.Min(healing, EntityDef.MaxHealth - Health);

                Health += healing;

#if DEBUG_LOGS
                LogDebug("Healing applied (" +
                         $"target={this}, healing={healing}, healingType={damageType}, " +
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

        private void GainAdrenalineOnHit(int roll)
        {
            int adrenaline = (int)(roll / (float)EntityDef.MaxHealth * 100.0f);
#if DEBUG_LOGS
            LogDebug($"Adrenaline gained on hit (adrenaline={adrenaline}).");
#endif
            GainAdrenaline(adrenaline);
        }

        public void GainAdrenaline(int adrenaline)
        {
            for (int i = 0; i < 10; i++)
            {
                var skillDef = GetSkillDef(i);
                if (!skillDef) continue;
                if (skillDef.Type != ESkillType.Adrenaline) continue;

                var clampedAdrenaline = Mathf.Min(skillInstances.GetAdrenaline(i) + adrenaline, skillDef.Value);
                skillInstances.SetAdrenaline(i, clampedAdrenaline);
            }
        }

        public void ApplyStatus(CtSkillDef skillDef, CtEntity source, int turns)
        {
            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                    if (identifier == skillDef.Identifier)
                    {
                        var currentTurns = CtDataBlock.GetEffectTurns(statusEffect);
                        if (currentTurns >= turns)
                        {
#if DEBUG_LOGS
                            LogDebug("Ignore applying effect if the current one is longer " +
                                     $"(skillDef={skillDef}, currentTurns={currentTurns}, turns={turns}).");
#endif

                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < statusEffectInstances.Count; i++)
            {
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect == CtDataBlock.InvalidData)
                {
#if DEBUG_LOGS
                    LogDebug($"Applying Status (skillDef={skillDef}, source={source}, turns={turns})");
#endif
                    statusEffectInstances.SetStatusEffect(i, CtDataBlock.CreateEffectData(skillDef.Identifier, source.Identifier, turns));
                    return;
                }
            }

            UpdatePersistantEffects();
        }

        private void UpdatePersistantEffects()
        {
            // TODO: Can this be done while iterating over the list the first time?

            ArmorRatingReduction = 0;
            SlashReduction = 0;
            BluntReduction = 0;
            PierceReduction = 0;
            EarthReduction = 0;
            AirReduction = 0;
            FireReduction = 0;
            WaterReduction = 0;
            IsDazed = false;
            IsBlind = false;

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

                skillDef.OnPersistentEffect(this, source);
            }

#if DEBUG_LOGS
            LogDebug($"Persistant effects updated (armorRatingReduction={ArmorRatingReduction}, slashReduction={SlashReduction}, bluntReduction={BluntReduction}, pierceReduction={PierceReduction}, earthReduction={EarthReduction}, airReduction={AirReduction}, fireReduction={FireReduction}, waterReduction={WaterReduction}, isDazed={IsDazed}, isBlind={IsBlind}).");
#endif
        }

        public bool ProcessStatusTick()
        {
            for (int i = 0; i < 16; i++)
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
                var statusEffect = statusEffectInstances.GetStatusEffect(i);
                if (statusEffect != CtDataBlock.InvalidData)
                {
                    var sourceId = CtDataBlock.GetEffectSource(statusEffect);
                    if (sourceId == source.Identifier)
                    {
                        var turns = CtDataBlock.GetEffectTurns(statusEffect) - 1;
                        if (turns > 0)
                        {
                            CtDataBlock.SetEffectTurns(turns, ref statusEffect);
                            statusEffectInstances.SetStatusEffect(i, statusEffect);
                        }
                        else
                        {
                            var identifier = CtDataBlock.GetEffectIdentifier(statusEffect);
                            LogDebug($"Removing expired status effect (effect={identifier}, source={sourceId}).");

                            statusEffectInstances.SetStatusEffect(i, CtDataBlock.InvalidData);
                        }
                    }
                }
            }

            UpdatePersistantEffects();
        }

        public void UseWeapon(CtEntity target)
        {
            CtSkillDef.MeleeAttack(gameData, target, this);
        }

        public void UseSkill(ushort skillId, CtEntity target, DataList adjacentTargets)
        {
            if (!EntityDef.TryGetSkillIndex(skillId, out int index))
            {
                LogCritical($"Failed to find skill identifier in entity skill set (skillId={skillId}, entity={this}).");
                return;
            }

            var usedSkillDef = gameData.GetSkillDef(skillId);
            usedSkillDef.OnUse(gameData, this, target, adjacentTargets);

            switch (usedSkillDef.Type)
            {
                case ESkillType.Energy:
                    Energy = Mathf.Max(0, Energy - usedSkillDef.Cost);
                    break;
                case ESkillType.Adrenaline:
                    skillInstances.SetAdrenaline(index, 0);
                    break;
            }

            for (int i = 0; i < 16; i++)
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

                        skillDef.OnSkillUsed(gameData, this, source, usedSkillDef);
                    }
                }
            }

            skillInstances.SetRecharge(index, usedSkillDef.RechargeTime);
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
                var recharge = skillInstances.GetRecharge(i);
                if (recharge > 0)
                    skillInstances.SetRecharge(i, Mathf.Max(recharge - rechargeSpeed, 0));
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