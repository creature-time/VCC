
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreatureTime
{
    public enum EBattleStateSignal
    {
        InitiativesChanged,
        TurnIndexChanged,
        AllyAdded,
        AllyRemoved,
        EnemyAdded,
        EnemyRemoved
    }

    public enum EBattleState
    {
        Start,
        Wait,
        Attack,
        NextTurn,
        End,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleState : CtAbstractSignal
    {
        [SerializeField] private ushort identifier;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        [SerializeField] private CtBattleStartState startState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleAttackState attackState;
        [SerializeField] private CtBattleNextTurnState nextTurnState;
        [SerializeField] private CtBattleEndState endState;

        public ushort Identifier => identifier;

        #region Synced Variables

        [UdonSynced, FieldChangeCallback(nameof(StateCallback))]
        private EBattleState _state = EBattleState.Start;

        public EBattleState StateCallback
        {
            get => _state;
            set => _state = value;
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
            set => _inProgress = value;
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
            get { return _turnIndex; }
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

                    _AssignBattleStateToParty(_allyParty);

                    if (_allyParty)
                    {
                        _allyParty.Connect(EPartySignal.MemberAdded, this, nameof(_OnAllyPartyAdded));
                        _allyParty.Connect(EPartySignal.MemberRemoved, this, nameof(_OnAllyPartyRemoved));
                    }
                }
            }
        }

        public ushort AllyId
        {
            get { return AllyIdCallback; }
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

                    _AssignBattleStateToParty(_enemyParty);

                    if (_enemyParty)
                    {
                        _enemyParty.Connect(EPartySignal.MemberAdded, this, nameof(_OnEnemyPartyAdded));
                        _enemyParty.Connect(EPartySignal.MemberRemoved, this, nameof(_OnEnemyPartyRemoved));
                    }
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
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.BattleState = this;
            
            SetArgs.Add(party.Identifier);
            SetArgs.Add(index);
            this.Emit(EBattleStateSignal.AllyAdded);
        }

        public void _OnAllyPartyRemoved()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.BattleState = null;

            SetArgs.Add(party.Identifier);
            SetArgs.Add(index);
            this.Emit(EBattleStateSignal.AllyRemoved);
        }

        public void _OnEnemyPartyAdded()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.BattleState = this;
            
            SetArgs.Add(party.Identifier);
            SetArgs.Add(index);
            this.Emit(EBattleStateSignal.EnemyAdded);
        }

        public void _OnEnemyPartyRemoved()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            TryGetEntity(party.GetMemberId(index), out var entity);
            entity.BattleState = null;
            
            SetArgs.Add(party.Identifier);
            SetArgs.Add(index);
            this.Emit(EBattleStateSignal.EnemyRemoved);
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

            ushort identifier;
            CtEntity entity;
            for (int i = 0; i < _initiatives.Length; ++i)
            {
                turn = (turn + 1) % _initiatives.Length;
                identifier = _initiatives[turn];
                if (!entityManager.TryGetEntity(identifier, out entity))
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
    }
}
