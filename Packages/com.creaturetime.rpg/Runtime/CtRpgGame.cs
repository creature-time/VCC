
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgGame : CtSingleton
    {
        private const int MessagePartyStart = 100;
        private const int MessagePartyAcceptQuest = MessagePartyStart + 0;
        private const int MessagePartyJoin = MessagePartyStart + 1;
        private const int MessagePartyLeave = MessagePartyStart + 2;

        private const int MessageRecruitStart = 200;
        private const int MessageRecruitJoin = MessageRecruitStart + 0;
        private const int MessageRecruitLeave = MessageRecruitStart + 1;

        private const int MessageBattleStart = 300;
        private const int MessageStartBattle = MessageBattleStart + 0;
        private const int MessageDamageValues = MessageBattleStart + 1;

        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPlayerManager playerManager;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtDialogueManager dialogueManager;
        [SerializeField] private CtNetSocket netSocket;
        [SerializeField] private CtBattleStateManager battleStateManager;
        [SerializeField] private CtStateMachine stateMachine;

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

            playerManager.Connect(EPlayerManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerManager.Connect(EPlayerManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            entityManager.Connect(EEntityManagerSignal.NpcEntityChanged, this, nameof(_OnNpcEntityChanged));

            netSocket.Connect(ENetSocketSignal.PacketChanged, this, nameof(_OnPacketChanged));
        }

        public void _OnPlayerAdded()
        {
            var playerId = GetArgs[0].UShort;

            var playerDef = playerManager.GetPlayerDefById(playerId);
            entityManager.CreatePlayerEntity(playerDef, out var playerEntity);
            if (playerDef.IsLocal)
                LocalEntity = playerEntity;
        }

        public void _OnPlayerRemoved()
        {
            var playerId = GetArgs[0].UShort;

            if (!entityManager.TryGetEntity(playerId, out var entity))
            {
                LogError($"Failed to find player entity (identifier={entity.Identifier}).");
                return;
            }

            var playerDef = (CtPlayerDef)entity.EntityDef;
            if (playerDef.IsLocal)
                LocalEntity = null;

            // TODO: Make owner switch do a check and purge any invalid players, too.
            if (Networking.IsMaster)
            {
                if (partyManager.TryGetEntityParty(entity, out var party))
                    _LeaveParty(entity, party);
            }

            entityManager.ReleasePlayerEntity(playerDef);
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

        private void JoinParty(CtEntity playerEntity)
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

        private void JoinParty(CtEntity playerEntity, CtParty party)
        {
            party.Join(playerEntity);
        }

        private void _LeaveParty(CtEntity playerEntity, CtParty party)
        {
            party.Leave(playerEntity);

            if (!_HasPlayers(party))
            {
                foreach (var battleState in battleStateManager.BattleStates)
                    if (battleState.AllyParty == party)
                    {
                        EndBattle(battleState);
                        break;
                    }
                party.Clear();
            }
        }

        private void LeaveParty(CtEntity playerEntity)
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

        private void AcquireRecruitNpc(CtEntity playerEntity, CtNpcDef npcDef)
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

        private void ReleaseRecruitNpc(CtEntity recruit)
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

        private void _PopulateEnemyParty(CtParty party, CtNpcDef[] npcDefs)
        {
            foreach (var npcDef in npcDefs)
            {
                entityManager.TryCreateEnemy(npcDef, out var entity);
                entity.OnStartBattle();
                party.Join(entity);
            }
        }

        private void _StartBattle(CtParty party, CtNpcDef[] npcDefs)
        {
            if (!partyManager.TryGetAvailableEnemyParty(out var enemyParty))
            {
#if DEBUG_LOGS
                LogCritical("Failed to get available enemy party.");
#endif
                return;
            }

            _PopulateEnemyParty(enemyParty, npcDefs);

            for (int i = 0; i < party.MaxCount; ++i)
            {
                var identifer = party.GetMemberId(i);
                if (identifer == CtConstants.InvalidId)
                    continue;
                entityManager.TryGetEntity(identifer, out var entity);
                entity.OnStartBattle();
            }

            if (!battleStateManager.TryCreateBattleState(party, enemyParty, out var battleState))
            {
#if DEBUG_LOGS
                LogCritical("Could not find available battle state to start battle.");
#endif
                _ReleaseEnemyParty(battleState.EnemyParty);
                return;
            }

            stateMachine.Process(battleState.GetState());

#if DEBUG_LOGS
            LogDebug("Battle started.");
#endif
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
                entityManager.ReleaseEnemy(entity);
                party.Leave(entity);
            }

            if (party.Count > 0)
            {
#if DEBUG_LOGS
                LogWarning("Enemy party was not empty.");
#endif
            }
        }

        public void _Client_TestAttack()
        {
            entityManager.TryGetEntity(2000, out var target);
#if DEBUG_LOGS
            LogDebug($"Test Attack (target={target.name})");
#endif
            playerManager.LocalPlayerDef.WeaponAttack(target);
        }

        public void _Client_TestSkill()
        {
            entityManager.TryGetEntity(2000, out var target);
#if DEBUG_LOGS
            LogDebug($"Test Attack (target={target.name})");
#endif
            playerManager.LocalPlayerDef.UseSkill(0, target);
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

#if DEBUG_LOGS
            LogDebug("Battle ended.");
#endif
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            if (!newMaster.isLocal)
                return;

            foreach (var battleState in battleStateManager.BattleStates)
            {
                if (battleState.InProgress)
                    stateMachine.Process(battleState.GetState());
            }
        }

        public void _OnPacketChanged()
        {
            byte[] data = netSocket.Packet;
            if (data.Length < 4)
                return;

#if DEBUG_LOGS
            LogDebug($"(Data.Length={data.Length})");
#endif

            int offset = 0;
            int messageType = BitConverter.ToInt32(data, offset);
            offset += 4;

#if DEBUG_LOGS
            LogDebug($"(MessageType={messageType})");
#endif

            ushort playerId = CtConstants.InvalidId;
            ushort identifier = CtConstants.InvalidId;

            switch (messageType)
            {
                case MessagePartyAcceptQuest:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandlePartyAcceptQuest(playerId, identifier);

                    return;
                case MessagePartyJoin:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleJoinParty(playerId, identifier);

                    return;
                case MessagePartyLeave:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleLeaveParty(playerId);
                    return;
                case MessageRecruitJoin:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleRecruitNpc(playerId, identifier);

                    return;
                case MessageRecruitLeave:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleLeaveNpc(identifier);

                    return;
                case MessageStartBattle:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleStartBattle(identifier);

                    return;
                case MessageDamageValues:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleStartBattle(identifier);

                    return;
                default:
                    return;
            }
        }

        public void RequestPartyAcceptQuest(CtEntity playerEntity, CtAbstractQuest quest)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessagePartyAcceptQuest);
            size += messageId.Length;

            byte[] playerIdBytes = BitConverter.GetBytes(playerEntity.Identifier);
            size += playerIdBytes.Length;

            byte[] questIdBytes = BitConverter.GetBytes(quest.Identifier);
            size += questIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(playerIdBytes, 0, data, offset, playerIdBytes.Length);
            offset += playerIdBytes.Length;

            Buffer.BlockCopy(questIdBytes, 0, data, offset, questIdBytes.Length);
            offset += questIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandlePartyAcceptQuest(ushort playerId, ushort questId)
        {
            if (!entityManager.TryGetEntity(playerId, out var playerEntity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find player entity (playerId={playerId}).");
#endif
                return;
            }

            JoinParty(playerEntity);

            if (!partyManager.TryGetEntityParty(playerEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for entity (identifier={playerEntity.Identifier})");
#endif
                return;
            }

            party.Quest = questId;
        }

        public void RequestJoinParty(CtEntity playerEntity, CtParty party)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessagePartyJoin);
            size += messageId.Length;

            byte[] playerIdBytes = BitConverter.GetBytes(playerEntity.Identifier);
            size += playerIdBytes.Length;

            var partyId = party ? party.Identifier : CtConstants.InvalidId;
            byte[] partyIdBytes = BitConverter.GetBytes(partyId);
            size += partyIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(playerIdBytes, 0, data, offset, playerIdBytes.Length);
            offset += playerIdBytes.Length;

            Buffer.BlockCopy(partyIdBytes, 0, data, offset, partyIdBytes.Length);
            offset += partyIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleJoinParty(ushort playerId, ushort partyId)
        {
            entityManager.TryGetEntity(playerId, out var playerEntity);
            if (partyId == CtConstants.InvalidId)
            {
                JoinParty(playerEntity);
            }
            else
            {
                partyManager.TryGetParty(partyId, out var party);
                JoinParty(playerEntity, party);
            }
        }

        public void RequestLeaveParty(CtEntity playerEntity)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessagePartyLeave);
            size += messageId.Length;

            byte[] playerIdBytes = BitConverter.GetBytes(playerEntity.Identifier);
            size += playerIdBytes.Length;
            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(playerIdBytes, 0, data, offset, playerIdBytes.Length);
            offset += playerIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleLeaveParty(ushort playerId)
        {
            entityManager.TryGetEntity(playerId, out var entity);
            LeaveParty(entity);
        }

        public void RequestRecruitNpc(CtEntity playerEntity, CtNpcDef npcDef)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessageRecruitJoin);
            size += messageId.Length;

            byte[] playerIdBytes = BitConverter.GetBytes(playerEntity.Identifier);
            size += playerIdBytes.Length;

            byte[] npcIdBytes = BitConverter.GetBytes(npcDef.Identifier);
            size += npcIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(playerIdBytes, 0, data, offset, playerIdBytes.Length);
            offset += playerIdBytes.Length;

            Buffer.BlockCopy(npcIdBytes, 0, data, offset, npcIdBytes.Length);
            offset += npcIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleRecruitNpc(ushort playerId, ushort npcId)
        {
            entityManager.TryGetEntity(playerId, out var playerEntity);
            AcquireRecruitNpc(playerEntity, gameData.GetNpcDef(npcId));
        }

        public void RequestLeaveNpc(CtEntity npcEntity)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessageRecruitLeave);
            size += messageId.Length;

            byte[] npcIdBytes = BitConverter.GetBytes(npcEntity.Identifier);
            size += npcIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(npcIdBytes, 0, data, offset, npcIdBytes.Length);
            offset += npcIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleLeaveNpc(ushort npcId)
        {
            entityManager.TryGetEntity(npcId, out var entity);
            ReleaseRecruitNpc(entity);
        }

        public void _RequestStartBattleTest()
        {
            if (!partyManager.TryGetEntityParty(LocalEntity, out var party))
            {
                LogWarning($"Local entity was not in a party (identifier={LocalEntity.Identifier}).");
                return;
            }

            var quest = gameData.GetQuestDef(party.Quest);
            if (!quest)
            {
                LogWarning($"Failed to get quest definition (identifier={party.Quest}).");
                return;
            }

            RequestStartBattle(party);
        }

        public void StartBattle(CtParty party, CtNpcDef[] npcDefs)
        {
            if (npcDefs.Length == 0)
            {
                LogError("No npc defs to battle against.");
                return;
            }

            _StartBattle(party, npcDefs);
        }

        public void RequestStartBattle(CtParty party)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessageStartBattle);
            size += messageId.Length;

            byte[] partyIdBytes = BitConverter.GetBytes(party.Identifier);
            size += partyIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(partyIdBytes, 0, data, offset, partyIdBytes.Length);
            offset += partyIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleStartBattle(ushort partyId)
        {
            if (!partyManager.TryGetParty(partyId, out var party))
            {
                return;
            }

            var quest = gameData.GetQuestDef(party.Quest);
            if (!quest)
            {
                LogWarning($"Failed to get quest definition (identifier={party.Quest}).");
                return;
            }

            quest.Execute(party);
        }
    }
}