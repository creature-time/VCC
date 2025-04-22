
using UdonSharp;
using UnityEngine;

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

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleState : CtAbstractSignal
    {
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        #region Synced Variables

        [SerializeField]
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

        [SerializeField]
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
            get { return TurnIndexCallback; }
            private set
            {
                TurnIndexCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField]
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
                        LogCritical($"Failed to find valid party by identifier (partyId={_allyId}).");
                        return;
                    }
        
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

        [SerializeField]
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
                        LogCritical($"Failed to find valid party by identifier (partyId={_enemyId}).");
                        return;
                    }

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
    }
}
