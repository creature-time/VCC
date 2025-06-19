
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgGame : CtAbstractSignal
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPlayerManager playerManager;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtDialogueManager dialogueManager;

        [SerializeField] private CtAbstractQuest[] quests;
        [SerializeField] private CtBattleStateManager battleStateManager;
        [SerializeField] private CtStateMachine stateMachine;

        private DataDictionary _quests = new DataDictionary();

        public CtGameData GameData => gameData;
        public CtPlayerManager PlayerManager => playerManager;
        public CtPartyManager PartyManager => partyManager;
        public CtEntityManager EntityManager => entityManager;
        public CtDialogueManager DialogueManager => dialogueManager;

        public CtEntity LocalEntity { get; private set; }

        private void Start()
        {
#if DEBUG_LOGS
            LogDebug("Initializing Rpg Game...");
#endif

            gameData.Init();
            playerManager.Init();
            partyManager.Init();
            entityManager.Init();

            foreach (var quest in quests)
                _quests.Add(quest.Identifier, quest);

            playerManager.Connect(EPlayerManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerManager.Connect(EPlayerManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            entityManager.Connect(EEntityManagerSignal.NpcEntityChanged, this, nameof(_OnNpcEntityChanged));

            // battleState.Connect(EBattleStateSignal.AllyAdded, this, nameof(_OnBattleAllyAdded));
            // battleState.Connect(EBattleStateSignal.AllyRemoved, this, nameof(_OnBattleAllyRemoved));
            //
            // battleState.Connect(EBattleStateSignal.EnemyAdded, this, nameof(_OnBattleEnemyAdded));
            // battleState.Connect(EBattleStateSignal.EnemyRemoved, this, nameof(_OnBattleEnemyRemoved));
        }

        // public void _OnBattleAllyAdded()
        // {
        //     
        // }
        //
        // public void _OnBattleAllyRemoved()
        // {
        //     
        // }
        //
        // public void _OnBattleEnemyAdded()
        // {
        //     
        // }
        //
        // public void _OnBattleEnemyRemoved()
        // {
        //     
        // }

        public void _OnPlayerAdded()
        {
            var index = GetArgs[0].Int;

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            entityManager.AcquirePlayerEntity(index, playerDef, out var entity);

            if (playerDef.IsLocal)
                LocalEntity = entity;

            entity.OnStartBattle();
        }

        public void _OnPlayerRemoved()
        {
            var index = GetArgs[0].Int;

            if (!entityManager.TryGetPlayerEntity(index, out var entity))
                return;

            if (partyManager.TryGetEntityParty(entity, out var party))
                _LeaveParty(entity, party);

            entityManager.ReleasePlayerEntity(index);

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            if (playerDef.IsLocal)
                LocalEntity = null;
        }

        public void _OnNpcEntityChanged()
        {
            var entity = (CtEntity)GetArgs[0].Reference;
            var previousId = GetArgs[1].UShort;
            var entityId = GetArgs[2].UShort;

            if (previousId != CtConstants.InvalidId)
            {
                entity.Reset();
            }

            if (entityId != CtConstants.InvalidId)
            {
                entity.OnStartBattle();
            }
        }

        public void JoinParty(CtEntity playerEntity)
        {
            if (partyManager.TryGetEntityParty(playerEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Entity already joined party  (identifier={party.Identifier})");
#endif
                return;
            }

            if (!partyManager.TryGetAvailablePlayerParty(out party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find empty party (identifier={party.Identifier})");
#endif
                return;
            }

            JoinParty(playerEntity, party);
        }

        public void JoinParty(CtEntity playerEntity, CtParty party)
        {
            party.Join(playerEntity);
        }

        public void _LeaveParty(CtEntity playerEntity, CtParty party)
        {
            party.Leave(playerEntity);

            if (!_HasPlayers(party))
                party.Clear();
        }

        public void LeaveParty(CtEntity playerEntity)
        {
            if (!partyManager.TryGetEntityParty(playerEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning("Entity was not in a party)");
#endif
                return;
            }

            _LeaveParty(playerEntity, party);
        }

        private bool _HasPlayers(CtParty party)
        {
            for (int i = 0; i < 4; ++i)
            {
                var identifier = party.GetMemberId(i);
                if (identifier != CtConstants.InvalidId)
                {
                    if (!entityManager.TryGetEntity(identifier, out var entity))
                    {
#if DEBUG_LOGS
                        LogCritical($"[_HasPlayers] Failed to find entity (identifier={identifier}).");
#endif
                        continue;
                    }

                    if (entity.IsPlayer)
                        return true;
                }
            }

            return false;
        }

        public void AcquireRecruitNpc(CtEntity playerEntity, CtNpcDef npcDef)
        {
            if (!partyManager.TryGetEntityParty(playerEntity, out var party))
            {
                JoinParty(playerEntity);
                if (!partyManager.TryGetEntityParty(playerEntity, out party))
                {
#if DEBUG_LOGS
                    LogCritical($"Failed to find party for entity (identifier={playerEntity.Identifier}).");
#endif
                    return;
                }
            }

            if (party.IsFull)
            {
#if DEBUG_LOGS
                LogCritical("Cannot add anymore members to party.");
#endif
                return;
            }

            if (!entityManager.TryAcquireRecruit(npcDef, out var recruit))
            {
#if DEBUG_LOGS
                LogWarning("No recruit available.");
#endif
                return;
            }

            party.Join(recruit);
        }

        public void ReleaseRecruitNpc(CtEntity recruit)
        {
            if (!recruit)
            {
#if DEBUG_LOGS
                LogWarning("No recruit found");
#endif
                return;
            }

            if (!partyManager.TryGetEntityParty(recruit, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for recruit (identifier={recruit.Identifier}).");
#endif
                return;
            }

            party.Leave(recruit);

            entityManager.ReleaseRecruitEntity(recruit);

            if (!_HasPlayers(party))
                party.Clear();
        }

        public void JoinQuest(CtParty party, CtAbstractQuest quest)
        {
            party.Quest = quest.Identifier;
        }

        public void LeaveQuest(CtParty party)
        {
            party.Quest = CtConstants.InvalidId;
        }

        private void _PopulateEnemyParty(CtParty party)
        {
            entityManager.TryCreateEnemy(gameData.GetNpcDef(1), out var entity);
            entity.OnStartBattle();
            party.Join(entity);
            entityManager.TryCreateEnemy(gameData.GetNpcDef(2), out entity);
            entity.OnStartBattle();
            party.Join(entity);
            entityManager.TryCreateEnemy(gameData.GetNpcDef(3), out entity);
            entity.OnStartBattle();
            party.Join(entity);
        }

        public void StartBattle(CtParty party)
        {
            if (!partyManager.TryGetAvailableEnemyParty(out var enemyParty))
            {
#if DEBUG_LOGS
                LogCritical("Failed to get available enemy party.");
#endif
                return;
            }

            _PopulateEnemyParty(enemyParty);

            if (!battleStateManager.TryCreateBattleState(party, enemyParty, out var battleState))
            {
#if DEBUG_LOGS
                LogCritical("Could not find available battle state to start battle.");
#endif
                _ReleaseEnemyParty(battleState.EnemyParty);
                return;
            }

            for (int i = 0; i < party.MaxCount; ++i)
            {
                var identifer = party.GetMemberId(i);
                if (identifer == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(identifer, out var entity);
                entity.OnStartBattle();
            }

            stateMachine.Process(battleState.GetState());
        }

        private void _ReleaseEnemyParty(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; ++i)
            {
                var identifier = party.GetMemberId(i);
                if (identifier == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(identifier, out var entity);
                entity.OnEndBattle();
                entityManager.ReleaseRecruitEntity(entity);
                party.Leave(entity);
            }

            if (party.Count > 0)
            {
#if DEBUG_LOGS
                LogWarning("Enemy party was not empty.");
#endif
            }
        }

        public void EndBattle(CtBattleState battleState)
        {
            for (int i = 0; i < battleState.AllyParty.MaxCount; ++i)
            {
                var identifer = battleState.AllyParty.GetMemberId(i);
                if (identifer == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(identifer, out var entity);
                entity.OnEndBattle();
            }

            _ReleaseEnemyParty(battleState.EnemyParty);
            battleStateManager.ReleaseBattleState(battleState);
        }
    }
}