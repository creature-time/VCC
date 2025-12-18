
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EBattleStateSignal
    {
        StateChanged,
        InProgressChanged,
        SquadIdChanged,
        InitiativesChanged,
        TurnIndexChanged,
        IsLocalChanged,
        AllyPartyChanged,
        EnemyPartyChanged,
        DamageSource,
        DamageApplied,
        TickApplied
    }

    public enum EBattleState
    {
        None,
        Start,
        Wait,
        Attack,
        NextTurn,
        End,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleState : CtAbstractSignal
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        [SerializeField] private CtDamageMessageBuilder damageMessageBuilder;
        [SerializeField] private CtStatusEffectsMessageBuilder statusEffectsMessageBuilder;

        [SerializeField] private CtBattleStartState startState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleAttackState attackState;
        [SerializeField] private CtBattleNextTurnState nextTurnState;
        [SerializeField] private CtBattleEndState endState;

        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public int DamageBlockCount => damageMessageBuilder.Count;

        public void GetDamage(int index, out EDamageSourceType damageSourceType, out ushort skillId,
            out ushort sourceId, out ushort targetId, out EDamageType damageType, out int damage, out bool isCritical)
        {
            damageMessageBuilder.GetDamage(index, out damageSourceType, out skillId, out sourceId, out targetId,
                out damageType, out damage, out isCritical);
        }

        #region Synced Variables

        [UdonSynced, FieldChangeCallback(nameof(StateCallback))]
        private EBattleState _state = EBattleState.Start;

        public EBattleState StateCallback
        {
            get => _state;
            set
            {
                _state = value;
                this.Emit(EBattleStateSignal.StateChanged);
            }
        }

        public EBattleState State
        {
            get => StateCallback;
            set
            {
                StateCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(InProgressCallback))]
        private bool _inProgress;

        public bool InProgressCallback
        {
            get => _inProgress;
            set
            {
                _inProgress = value;
                this.Emit(EBattleStateSignal.InProgressChanged);
            }
        }

        public bool InProgress
        {
            get => InProgressCallback;
            set
            {
                InProgressCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SquadIdCallback))]
        private ushort _squadId;

        public ushort SquadIdCallback
        {
            get => _squadId;
            set
            {
                _squadId = value;
                this.Emit(EBattleStateSignal.SquadIdChanged);
            }
        }

        public ushort SquadId
        {
            get => SquadIdCallback;
            set
            {
                SquadIdCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(InitiativesCallback))]
        private ushort[] _initiatives = { };

        public ushort[] InitiativesCallback
        {
            get { return _initiatives; }
            set
            {
                _initiatives = value;
                this.Emit(EBattleStateSignal.InitiativesChanged);
            }
        }

        public ushort[] Initiatives
        {
            get { return InitiativesCallback; }
            set
            {
                InitiativesCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(TurnIndexCallback))]
        private int _turnIndex = -1;

        public int TurnIndexCallback
        {
            get => _turnIndex;
            set
            {
                _turnIndex = value;

                SetArgs.Add(_turnIndex);
                this.Emit(EBattleStateSignal.TurnIndexChanged);
            }
        }

        public int TurnIndex
        {
            get => TurnIndexCallback;
            private set
            {
                TurnIndexCallback = value;
                RequestSerialization();
            }
        }

        private bool _isLocal;
        public bool IsLocal
        {
            get => _isLocal;
            private set
            {
                _isLocal = value;
                this.Emit(EBattleStateSignal.IsLocalChanged);
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(AllyIdCallback))]
        private ushort _allyId = CtConstants.InvalidId;

        public ushort AllyIdCallback
        {
            get => _allyId;
            set
            {
                if (_allyParty)
                {
                    _allyParty.Disconnect(EPartySignal.MemberAdded, this, nameof(_OnAllyPartyAdded));
                    _allyParty.Disconnect(EPartySignal.MemberRemoved, this, nameof(_OnAllyPartyRemoved));

                    for (int i = 0; i < _allyParty.MaxCount; i++)
                    {
                        var entity = _allyParty.GetEntity(i);
                        if (!entity) continue;
                        _OnAllyPartyRemovedRaw(_allyParty, i);
                        if (entity == rpgGame.LocalEntity)
                            IsLocal = false;
                    }

                    _allyParty = null;
                }

                _allyId = value;

                if (_allyId != CtConstants.InvalidId)
                {
                    if (!partyManager.TryGetParty(_allyId, out _allyParty))
                    {
#if DEBUG_LOGS
                        LogCritical($"Failed to find valid party by identifier (partyId={_allyId}).");
#endif
                        return;
                    }

                    if (_allyParty)
                    {
                        _AssignBattleStateToParty(_allyParty);
    
                        for (int i = 0; i < _allyParty.MaxCount; i++)
                        {
                            var entity = _allyParty.GetEntity(i);
                            if (!entity) continue;
                            _OnAllyPartyAddedRaw(_allyParty, i);
                            if (entity == rpgGame.LocalEntity)
                                IsLocal = true;
                        }

                        _allyParty.Connect(EPartySignal.MemberAdded, this, nameof(_OnAllyPartyAdded));
                        _allyParty.Connect(EPartySignal.MemberRemoved, this, nameof(_OnAllyPartyRemoved));
                    }
                }

                this.Emit(EBattleStateSignal.AllyPartyChanged);
            }
        }

        public ushort AllyId
        {
            get => AllyIdCallback;
            set
            {
                AllyIdCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(EnemyIdCallback))]
        private ushort _enemyId = CtConstants.InvalidId;

        public ushort EnemyIdCallback
        {
            get => _enemyId;
            set
            {
                if (_enemyParty)
                {
                    _enemyParty.Disconnect(EPartySignal.MemberAdded, this, nameof(_OnEnemyPartyAdded));
                    _enemyParty.Disconnect(EPartySignal.MemberRemoved, this, nameof(_OnEnemyPartyRemoved));

                    for (int i = 0; i < _enemyParty.MaxCount; i++)
                    {
                        var entity = _enemyParty.GetEntity(i);
                        if (!entity) continue;
                        _OnEnemyPartyRemovedRaw(_enemyParty, i);
                    }

                    _enemyParty = null;
                }

                _enemyId = value;

                if (_enemyId != CtConstants.InvalidId)
                {
                    if (!partyManager.TryGetParty(_enemyId, out _enemyParty))
                    {
#if DEBUG_LOGS
                        LogCritical($"Failed to find valid party by identifier (partyId={_enemyId}).");
#endif
                        return;
                    }

                    for (int i = 0; i < _enemyParty.MaxCount; i++)
                    {
                        var entity = _enemyParty.GetEntity(i);
                        if (!entity) continue;
                        _OnEnemyPartyAddedRaw(_enemyParty, i);
                    }

                    _AssignBattleStateToParty(_enemyParty);

                    if (_enemyParty)
                    {
                        _enemyParty.Connect(EPartySignal.MemberAdded, this, nameof(_OnEnemyPartyAdded));
                        _enemyParty.Connect(EPartySignal.MemberRemoved, this, nameof(_OnEnemyPartyRemoved));
                    }

                    this.Emit(EBattleStateSignal.EnemyPartyChanged);
                }
            }
        }

        public ushort EnemyId
        {
            get { return EnemyIdCallback; }
            set
            {
                EnemyIdCallback = value;
                RequestSerialization();
            }
        }

        #endregion

        private CtParty _allyParty;
        private CtParty _enemyParty;

        public CtParty AllyParty => _allyParty;
        public CtParty EnemyParty => _enemyParty;

        private void Start()
        {
            damageMessageBuilder.Connect(EDamageBlockSignal.DamageSource, this, nameof(_OnDamageSourceChanged));
            damageMessageBuilder.Connect(EDamageBlockSignal.DamageApplied, this, nameof(_OnDamageBlockChanged));
            statusEffectsMessageBuilder.Connect(EStatusEffectBlockSignal.DamageApplied, this, nameof(_OnStatusEffectDamageApplied));
        }

        public void _OnDamageSourceChanged()
        {
#if DEBUG_LOGS
            LogDebug("Damage source forwarded.");
#endif

            SetArgs.Add(GetArgs[0].UShort);
            SetArgs.Add(GetArgs[1].UShort);
            SetArgs.Add(GetArgs[2].UShort);
            this.Emit(EBattleStateSignal.DamageSource);
        }

        public void _OnDamageBlockChanged()
        {
#if DEBUG_LOGS
            LogDebug("Damage block forwarded.");
#endif

            SetArgs.Add(GetArgs[0].UShort);
            SetArgs.Add(GetArgs[1].UShort);
            SetArgs.Add(GetArgs[2].UShort);
            SetArgs.Add(GetArgs[3].UShort);
            SetArgs.Add(GetArgs[4].UShort);
            SetArgs.Add(GetArgs[5].Int);
            SetArgs.Add(GetArgs[6].Boolean);
            this.Emit(EBattleStateSignal.DamageApplied);
        }

        public void _OnStatusEffectDamageApplied()
        {
#if DEBUG_LOGS
            LogDebug("Tick block forwarded.");
#endif

            SetArgs.Add(GetArgs[0].UShort);
            SetArgs.Add(GetArgs[1].UShort);
            SetArgs.Add(GetArgs[2].UShort);
            SetArgs.Add(GetArgs[3].UShort);
            SetArgs.Add(GetArgs[4].UShort);
            SetArgs.Add(GetArgs[5].Int);
            SetArgs.Add(GetArgs[6].Boolean);
            this.Emit(EBattleStateSignal.TickApplied);
        }

        public CtStateBase GetState()
        {
            switch (_state)
            {
                case EBattleState.Start:
                    return startState;
                case EBattleState.Wait:
                    return waitState;
                case EBattleState.Attack:
                    return attackState;
                case EBattleState.NextTurn:
                    return nextTurnState;
                case EBattleState.End:
                    return endState;
                default:
                    return null;
            }
        }

        private void _AssignBattleStateToParty(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; i++)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                entity.BattleState = this;
            }
        }

        public void _OnAllyPartyAdded()
        {
            _OnAllyPartyAddedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnAllyPartyAddedRaw(CtParty party, int index)
        {
            var entity = party.GetEntity(index);
            if (!entity)
            {
#if DEBUG_LOGS
                LogCritical($"Ally party did not have entity (index={index}).");
#endif
                return;
            }
            entity.BattleState = this;
            entity.Connect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
        }

        public void _OnAllyPartyRemoved()
        {
            _OnAllyPartyRemovedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnAllyPartyRemovedRaw(CtParty party, int index)
        {
            var entity = party.GetEntity(index);
            if (!entity)
            {
#if DEBUG_LOGS
                LogCritical($"Ally party did not have entity (index={index}).");
#endif
                return;
            }
            entity.Disconnect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = null;
        }

        public void _OnEnemyPartyAdded()
        {
            _OnEnemyPartyAddedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnEnemyPartyAddedRaw(CtParty party, int index)
        {
            var entity = party.GetEntity(index);
            if (!entity)
            {
#if DEBUG_LOGS
                LogCritical($"Enemy party did not have entity (index={index}).");
#endif
                return;
            }
            entity.Connect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = this;
        }

        public void _OnEnemyPartyRemoved()
        {
            _OnEnemyPartyRemovedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnEnemyPartyRemovedRaw(CtParty party, int index)
        {
            var entity = party.GetEntity(index);
            if (!entity)
            {
#if DEBUG_LOGS
                LogCritical($"Enemy party did not have entity (index={index}).");
#endif
                return;
            }
            entity.Disconnect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = null;
        }

        public bool TryGetEntity(ushort identifier, out CtEntity entity)
        {
            return entityManager.TryGetEntity(identifier, out entity);
        }

        public void ResetTurns()
        {
            _NextTurn(true);
        }

        public void NextTurn()
        {
            _NextTurn(false);
        }

        private void _NextTurn(bool reset)
        {
            int turn = reset ? -1 : TurnIndex;

            CtEntity entity;
            for (int i = 0; i < _initiatives.Length; ++i)
            {
                turn = (turn + 1) % _initiatives.Length;
                if (!entityManager.TryGetEntity(_initiatives[turn], out entity))
                {
                    continue;
                }

                if (entity.State == ECombatState.Alive)
                {
                    break;
                }
            }

            TurnIndex = turn;
        }

        public bool ArePlayersLoaded()
        {
            for (int i = 0; i < _allyParty.MaxCount; i++)
            {
                var entity = _allyParty.GetEntity(i);
                if (!entity) continue;
                if (!entity.IsReady())
                    return false;
            }

            return true;
        }

        private bool _IsPartyDead(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; i++)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (entity.State == ECombatState.Alive)
                    return false;
            }

            return true;
        }

        public bool IsAllyTeamDead()
        {
            return _IsPartyDead(_allyParty);
        }

        public bool IsEnemyTeamDead()
        {
            return _IsPartyDead(_enemyParty);
        }

        public void BeginTickBlock()
        {
            statusEffectsMessageBuilder.SetHeader();
        }

        public void EndTickBlock()
        {
            statusEffectsMessageBuilder.CommitDamage();
        }

        public void BeginDamageBlock(CtEntity sourceEntity, CtEntity targetEntity, 
            ushort skillId)
        {
            damageMessageBuilder.SetHeader(sourceEntity.Identifier, targetEntity.Identifier, skillId);
        }

        public void EndDamageBlock()
        {
            damageMessageBuilder.CommitDamage();
        }

        public void _HandleAppliedDamage()
        {
#if DEBUG_LOGS
            LogDebug($"Handling applied damage from entity (sender={Sender}).");
#endif

            var target = (CtEntity)Sender;
            var damageSourceTypeValue = GetArgs[0].Int;
            var skillId = GetArgs[1].UShort;
            var instigator = (CtEntity)GetArgs[2].Reference;
            var damageTypeValue = GetArgs[3].Int;
            var damage = GetArgs[4].Int;
            var isCritical = GetArgs[5].Boolean;

            var damageSourceType = (EDamageSourceType)damageSourceTypeValue;
            var damageType = (EDamageType)damageTypeValue;

            switch (State)
            {
                case EBattleState.Attack:
                    damageMessageBuilder.AddDamageCommand(
                        damageSourceType, skillId, target.Identifier, damageType, damage, isCritical);
                    break;
                case EBattleState.NextTurn:
                    statusEffectsMessageBuilder.AddDamageCommand(
                        damageSourceType, skillId, target.Identifier, instigator.Identifier, damageType, damage, 
                        isCritical);
                    break;
            }
        }

        // public void Reset()
        // {
        //     damageMessageBuilder.Reset();
        // }
    }
}
