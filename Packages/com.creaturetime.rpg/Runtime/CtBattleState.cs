
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
        DamageApplied
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
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        [SerializeField] private CtDamageMessageBuilder damageMessageBuilder;

        [SerializeField] private CtBattleStartState startState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleAttackState attackState;
        [SerializeField] private CtBattleNextTurnState nextTurnState;
        [SerializeField] private CtBattleEndState endState;

        // public ushort Identifier => identifier;

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
                        var memberId = _allyParty.GetMemberId(i);
                        if (memberId == CtConstants.InvalidId)
                            continue;
                        _OnAllyPartyRemovedRaw(_allyParty, i);
                        if (memberId == rpgGame.LocalEntity.Identifier)
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
                            var memberId = _allyParty.GetMemberId(i);
                            if (memberId == CtConstants.InvalidId)
                                continue;
                            _OnAllyPartyAddedRaw(_allyParty, i);
                            if (memberId == rpgGame.LocalEntity.Identifier)
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
                        var memberId = _enemyParty.GetMemberId(i);
                        if (memberId != CtConstants.InvalidId)
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
                        var memberId = _enemyParty.GetMemberId(i);
                        if (memberId != CtConstants.InvalidId)
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
                var id = party.GetMemberId(i);
                if (id == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(id, out var entity);
                entity.BattleState = this;
            }
        }

        public void _OnAllyPartyAdded()
        {
            _OnAllyPartyAddedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnAllyPartyAddedRaw(CtParty party, int index)
        {
            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.BattleState = this;
            entity.Connect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
        }

        public void _OnAllyPartyRemoved()
        {
            _OnAllyPartyRemovedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnAllyPartyRemovedRaw(CtParty party, int index)
        {
            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.Disconnect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = null;
        }

        public void _OnEnemyPartyAdded()
        {
            _OnEnemyPartyAddedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnEnemyPartyAddedRaw(CtParty party, int index)
        {
            entityManager.TryGetEntity(party.GetMemberId(index), out var entity);
            entity.Connect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = this;
        }

        public void _OnEnemyPartyRemoved()
        {
            _OnEnemyPartyRemovedRaw((CtParty)Sender, GetArgs[0].Int);
        }

        private void _OnEnemyPartyRemovedRaw(CtParty party, int index)
        {
            TryGetEntity(party.GetMemberId(index), out var entity);
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
                var memberId = _allyParty.GetMemberId(i);
                if (memberId == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(memberId, out var entity);
                if (!entity.IsReady())
                    return false;
            }

            return true;
        }

        private bool _IsPartyDead(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; i++)
            {
                var memberId = party.GetMemberId(i);
                if (memberId == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(memberId, out var entity);
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

        public void BeginDamageBlock(CtBattleState battleState, CtEntity sourceEntity, CtEntity targetEntity, 
            ushort skillId)
        {
            damageMessageBuilder.SetHeader(sourceEntity.Identifier, targetEntity.Identifier, skillId);
        }

        public void _HandleAppliedDamage()
        {
#if DEBUG_LOGS
            LogDebug($"Handling applied damage from entity (sender={Sender}).");
#endif

            var damageSourceTypeValue = GetArgs[0].Int;
            var skillId = GetArgs[1].UShort;
            // var instigator = (CtEntity)GetArgs[2].Reference;
            var target = (CtEntity)GetArgs[3].Reference;
            var damageTypeValue = GetArgs[4].Int;
            var damage = GetArgs[5].Int;
            var isCritical = GetArgs[6].Boolean;

            var damageSourceType = (EDamageSourceType)damageSourceTypeValue;
            var damageType = (EDamageType)damageTypeValue;
            damageMessageBuilder.AddDamageCommand(
                damageSourceType, skillId, target.Identifier, damageType, damage, isCritical);
        }

        public void EndDamageBlock()
        {
            damageMessageBuilder.CommitDamage();
        }

        // public void Reset()
        // {
        //     damageMessageBuilder.Reset();
        // }
    }
}
