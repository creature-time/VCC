
using System;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace CreatureTime
{
    public enum EBattleStateSignal
    {
        StateChanged,
        InProgressChanged,
        SquadIdChanged,
        InitiativesChanged,
        TurnIndexChanged,
        // IsLocalChanged,
        AllyPartyChanged,
        EnemyPartyChanged,
        DamageSource,
        // DamageApplied,
        TickApplied,
        ChestOpened
    }

    public enum EBattleState
    {
        None,
        Start,
        Wait,
        Attack,
        NextTurn,
        Loot,
        Results,
        End,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleState : CtAbstractSignal
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtQuestSystem questSystem;
        [SerializeField] private CtDropDatabase dropDatabase;

        [SerializeField] private CtDamageMessageBuilder damageMessageBuilder;
        [SerializeField] private CtStatusEffectsMessageBuilder statusEffectsMessageBuilder;

        [SerializeField] private CtBattleStartState startState;
        [SerializeField] private CtBattleWaitState waitState;
        [SerializeField] private CtBattleAttackState attackState;
        [SerializeField] private CtBattleNextTurnState nextTurnState;
        [SerializeField] private CtBattleLootState lootState;
        [SerializeField] private CtBattleResultsState resultState;
        [SerializeField] private CtBattleEndState endState;

        // [SerializeField] private CtBattleLoot loot;

        // public CtBattleLoot Loot => loot;

        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public int DamageBlockCount => damageMessageBuilder.Count;

        public void GetDamage(int index, out EDamageSourceType damageSourceType, out ushort skillId,
            out ushort sourceId, out ushort targetId, out EDamageType damageType, out int damage, out bool isCritical)
        {
            damageMessageBuilder.GetDamage(index, out damageSourceType, out skillId, out sourceId, out targetId,
                out damageType, out damage, out isCritical);
        }

        #region Synced Variables

        [UdonSynced, FieldChangeCallback(nameof(_Callback_ChestOpened))]
        private bool _chestOpened;

        public bool _Callback_ChestOpened
        {
            get => _chestOpened;
            set
            {
                _chestOpened = value;
                this.Emit(EBattleStateSignal.ChestOpened);
            }
        }

        public bool ChestOpened
        {
            get => _Callback_ChestOpened;
            set
            {
                _Callback_ChestOpened = value;
                RequestSerialization();
            }
        }

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
                    }

                    _allyParty = null;
                }

#if DEBUG_LOGS
                LogDebug($"Battle state ally party has changed (prev={_allyId}, allyId={value}).");
#endif
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
                        for (int i = 0; i < _allyParty.MaxCount; i++)
                        {
                            var entity = _allyParty.GetEntity(i);
                            if (!entity) continue;
                            _OnAllyPartyAddedRaw(_allyParty, i);
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

#if DEBUG_LOGS
                LogDebug($"Battle state enemy party has changed (prev={_enemyId}, allyId={value}).");
#endif
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

        public bool IsHardMode => false;

        private void Start()
        {
            // rpgGame.Connect(ERpgGameSignal.LocalPlayerChanged, this, nameof(_OnLocalPlayerChanged));
            damageMessageBuilder.Connect(EDamageBlockSignal.DamageSource, this, nameof(_OnDamageSourceChanged));
            // damageMessageBuilder.Connect(EDamageBlockSignal.DamageApplied, this, nameof(_OnDamageBlockChanged));
            statusEffectsMessageBuilder.Connect(EStatusEffectBlockSignal.DamageApplied, this, nameof(_OnStatusEffectDamageApplied));
        }

        // public void _OnLocalPlayerChanged()
        // {
        //     if (!_allyParty) return;
        //
        //     for (int i = 0; i < _allyParty.MaxCount; i++)
        //     {
        //         var entity = _allyParty.GetEntity(i);
        //         if (!entity) continue;
        //         _OnLocalPlayerChangedRaw(entity, true);
        //     }
        // }

        // private void _OnLocalPlayerChangedRaw(CtEntity entity, bool value)
        // {
        //     if (entity == rpgGame.LocalEntity)
        //         IsLocal = value;
        // }

        public void _OnDamageSourceChanged()
        {
// #if DEBUG_LOGS
//             LogDebug("Damage source forwarded.");
// #endif
//
        //     SetArgs.Add(GetArgs[0].UShort);
        //     SetArgs.Add(GetArgs[1].UShort);
        //     SetArgs.Add(GetArgs[2].UShort);
        //     this.Emit(EBattleStateSignal.DamageSource);
        //     
        //     
        // }
        //
        // public void _OnDamageSource()
        // {
            var sourceId = GetArgs[0].UShort;
            var targetId = GetArgs[1].UShort;
            var skillId = GetArgs[2].UShort;

#if DEBUG_LOGS
            LogDebug($"{sourceId} attacked {targetId} with {skillId}.");
#endif

            if (!entityManager.TryGetEntity(sourceId, out var damageSource))
            {
#if DEBUG_LOGS
                Debug.LogError($"Failed to find damage source (identifier={sourceId}).");
#endif
                return;
            }

            damageSource.ResetAttack();

            // if (damageSource.IsPlayer)
            // {
            //     damageSource.ResetAttack();
            //     // _OnDamageTrigger();
            // }
            // else
            // {
            //     var sourceNpc = (CtNpcUserData)_generatedNpcs[sourceId].Reference;
            //     if (skillId == CtConstants.InvalidId)
            //     {
            //         sourceNpc.Controller.MeleeAttack();
            //         _ApplyDamage();
            //     }
            //     else
            //     {
            //         var skillDef = gameData.GetSkillDef(skillId);
            //         var animationTime = sourceNpc.Controller.UseSkill(skillDef);
            //         SendCustomEventDelayedSeconds(nameof(_ApplyDamage), animationTime);
            //     }
            // }

            SetArgs.Add(sourceId);
            SetArgs.Add(targetId);
            SetArgs.Add(skillId);
            this.Emit(EBattleStateSignal.DamageSource);
        }

        // public void _ApplyDamage()
        // {
        //     damageSource.SendCustomEventDelayedSeconds(nameof(damageSource.ResetAttack), 2);
        //     damageSource = null;
        // }

//         public void _OnDamageBlockChanged()
//         {
// #if DEBUG_LOGS
//             LogDebug("Damage block forwarded.");
// #endif
//
//             SetArgs.Add(GetArgs[0].UShort);
//             SetArgs.Add(GetArgs[1].UShort);
//             SetArgs.Add(GetArgs[2].UShort);
//             SetArgs.Add(GetArgs[3].UShort);
//             SetArgs.Add(GetArgs[4].UShort);
//             SetArgs.Add(GetArgs[5].Int);
//             SetArgs.Add(GetArgs[6].Boolean);
//             this.Emit(EBattleStateSignal.DamageApplied);
//         }

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

            // _OnLocalPlayerChangedRaw(entity, true);
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

            // _OnLocalPlayerChangedRaw(entity, false);
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
            entity.Connect(EEntitySignal.Death, this, nameof(_OnDeathTrigger));
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

            entity.Disconnect(EEntitySignal.Death, this, nameof(_OnDeathTrigger));
            entity.Disconnect(EEntitySignal.DamageApplied, this, nameof(_HandleAppliedDamage));
            entity.BattleState = null;
        }

        public void _OnDeathTrigger()
        {
            var npcEntity = (CtNpcEntity)Sender;

            var localPlayer = rpgGame.LocalEntity;

            var eventData = CtKillObjective.CreateEventData(npcEntity.NpcId, 1);
            questSystem.UpdateQuests(localPlayer.PrimaryQuestProgression, eventData);
            questSystem.UpdateQuests(localPlayer.SecondaryQuestProgression, eventData);

            var exp = CtRpgFormulas.CalcExperience(localPlayer.Level, npcEntity.Level);

            // Split exp between party members
            exp /= _allyParty.Count;

            // Boss bonus!
            if (localPlayer.IsBoss)
            {
                exp *= 2;
            }

            // Hard mode bonus!
            if (IsHardMode)
            {
                exp += exp / 2;
            }

#if DEBUG_LOGS
            Log($"{localPlayer.DisplayName} gains {exp} experience from {npcEntity.DisplayName}.");
#endif
            localPlayer.GainExperience(exp);
        }

        public bool TryGetEntity(ushort identifier, out CtEntity entity)
        {
            return entityManager.TryGetEntity(identifier, out entity);
        }

        public void ResetTurns()
        {
            _NextTurn(true);
            ChestOpened = false;
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

        public bool IsReadyToLeave()
        {
            for (var i = 0; i < _allyParty.MaxCount; i++)
            {
                var entity = _allyParty.GetEntity(i);
                if (!entity) continue;
                if (!entity.IsReadyToLeave())
                    return false;
            }

            return true;
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

        [UdonSynced, FieldChangeCallback(nameof(_Callback_EndTime))] private long _endTime;

        public long _Callback_EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                EndTime = DateTime.FromBinary(_endTime);
            }
        }

        public DateTime EndTime { get; private set; }
        public float TimeLeft => (float)(EndTime - DateTime.UtcNow).TotalSeconds;

        public void UpdateEndTime(float delayDuration)
        {
            var endTime = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, (int)(delayDuration * 1000));
            _Callback_EndTime = endTime.ToBinary();
            RequestSerialization();
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

        [SerializeField] private Vector2 dropDistance = new Vector2(1.5f, 3f);

        public void OpenChest()
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Event_OpenChest));
        }

        public void Event_OpenChest()
        {
            if (_squadId == CtConstants.InvalidId) return;

            var squadDef = gameData.GetSquadDef(_squadId);
            if (!squadDef.ChestLootTable)
            {
#if DEBUG_LOGS
                LogWarning("There was no loot table to produce loot.");
#endif
                return;
            }

            CtLootTable.GetItemsFromLootTable(squadDef.ChestLootTable, 4, out var items);

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var randomPoint = CtDropDatabase.RandomSpawnLocation(dropDistance.x, dropDistance.y);
                dropDatabase.AddDrop(
                    item, CtConstants.InvalidId, Vector3.up * 1f + randomPoint, _allyParty.Identifier, CtConstants.InvalidId);
            }

            ChestOpened = true;
        }
    }
}
