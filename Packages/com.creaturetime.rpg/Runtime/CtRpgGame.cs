
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
        [SerializeField] private CtBattleState battleState;

        private DataDictionary _quests = new DataDictionary();

        public CtGameData GameData => gameData;
        public CtPlayerManager PlayerManager => playerManager;
        public CtPartyManager PartyManager => partyManager;
        public CtEntityManager EntityManager => entityManager;
        public CtDialogueManager DialogueManager => dialogueManager;

        public CtEntity LocalEntity { get; private set; }

        private void Start()
        {
            LogDebug("Initializing Rpg Game...");

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

        public void _OnBattleAllyAdded()
        {
            
        }

        public void _OnBattleAllyRemoved()
        {
            
        }

        public void _OnBattleEnemyAdded()
        {
            
        }

        public void _OnBattleEnemyRemoved()
        {
            
        }

        public void _OnPlayerAdded()
        {
            var index = GetArgs[0].Int;

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            entityManager.AcquirePlayerEntity(index, playerDef, out var entity);

            if (playerDef.IsLocal)
                LocalEntity = entity;

            entity.SetupEntityForBattle();
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
                entity.SetupEntityForBattle();
            }
        }

        public void JoinParty(CtEntity playerEntity)
        {
            if (partyManager.TryGetEntityParty(playerEntity, out var party))
            {
                LogWarning($"Entity already joined party  (identifier={party.Identifier})");
                return;
            }

            if (!partyManager.TryGetAvailablePlayerParty(out party))
            {
                LogWarning($"Failed to find empty party (identifier={party.Identifier})");
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
                LogWarning("Entity was not in a party)");
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
                        LogCritical($"[_HasPlayers] Failed to find entity (identifier={identifier}).");
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
                    LogCritical($"Failed to find party for entity (identifier={playerEntity.Identifier}).");
                    return;
                }
            }

            if (party.IsFull)
            {
                LogCritical("Cannot add anymore members to party.");
                return;
            }

            if (!entityManager.TryAcquireRecruit(npcDef, out var recruit))
            {
                LogWarning("No recruit available.");
                return;
            }

            party.Join(recruit);
        }

        public void ReleaseRecruitNpc(CtEntity recruit)
        {
            if (!recruit)
            {
                LogWarning("No recruit found");
                return;
            }

            if (!partyManager.TryGetEntityParty(recruit, out var party))
            {
                LogWarning($"Failed to find party for recruit (identifier={recruit.Identifier}).");
                return;
            }

            party.Leave(recruit);

            recruit.EntityId = CtConstants.InvalidId;

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

        public void StartBattle(CtParty party)
        {
            battleState.AllyId = party.Identifier;

            partyManager.TryGetAvailableEnemyParty(out var enemyParty);
            battleState.EnemyId = enemyParty.Identifier;

            entityManager.TryCreateEnemy(gameData.GetNpcDef(1), out var entity);
            enemyParty.Join(entity);

            int index = 0;
            int count = party.Count + enemyParty.Count;
            ushort[] temp = new ushort[count];

            for (int i = 0; i < party.MaxCount; i++)
            {
                var identifier = party.GetMemberId(i);
                if (identifier == CtConstants.InvalidId)
                    continue;

                temp[index++] = identifier;
                entityManager.TryGetEntity(identifier, out var ent);
                ent.BattleState = battleState;
            }

            for (int i = 0; i < enemyParty.MaxCount; i++)
            {
                var identifier = enemyParty.GetMemberId(i);
                if (identifier == CtConstants.InvalidId)
                    continue;

                temp[index++] = identifier;
                entityManager.TryGetEntity(identifier, out var ent);
                ent.BattleState = battleState;
            }

            if (index != count)
                LogCritical($"Index did not match count (index={index}, count={count}).");

            battleState.Initiatives = temp;

            battleState.ResetTurns();

            for (int i = 0; i < battleState.Initiatives.Length * 2; ++i)
            {
                var entityIdentifier = battleState.Initiatives[battleState.TurnIndex];
                entityManager.TryGetEntity(entityIdentifier, out entity);
                if (entity.IsPlayer)
                    entity.EntityDef.GetComponent<CtPlayerTurn>().Submit(CTBattleInteractType.Attack, -1, 255);
                entity.TryGetAttack(out var skillIndex, out var targetId);
                LogDebug($"Testing {i} (turnIndex={battleState.TurnIndex}, skillIndex={skillIndex}, targetId={targetId})");
                battleState.NextTurn();
            }
        }
    }
}