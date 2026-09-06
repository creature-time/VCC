
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    public enum ERpgGameSignal
    {
        LocalPlayerChanged,
        LocalPartyAdded,
        LocalPartyRemoved,
        PlayerProfessionChanged
    }

    public enum EGameState
    {
        OpenWorld,
        TransitionToOpenWorld,
        CampSite,
        TransitionToCampSite,
        Battle,
        TransitionToBattle
    }

    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgGame : CtSingleton
    {
        private const int MessagePartyStart = 100;
        private const int MessagePartyAcceptBattle = MessagePartyStart + 0;
        private const int MessagePartyStartAdventure = MessagePartyStart + 1;
        private const int MessagePartyJoin = MessagePartyStart + 2;
        private const int MessagePartyLeave = MessagePartyStart + 3;

        private const int MessageRecruitStart = 200;
        private const int MessageRecruitJoin = MessageRecruitStart + 0;
        private const int MessageRecruitLeave = MessageRecruitStart + 1;

        private const int MessageBattleStart = 300;
        private const int MessageStartBattle = MessageBattleStart + 0;
        // private const int MessageDamageValues = MessageBattleStart + 1;

        private const int MessageLootRollStart = 400;
        private const int MessageLootPickUpItem = MessageLootRollStart + 0;
        private const int MessageLootGiveItem = MessageLootRollStart + 1;
        private const int MessageLootChoice = MessageLootRollStart + 2;
        private const int MessageLootRoll = MessageLootRollStart + 3;

        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPlayerPersistenceManager playerPersistenceManager;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private CtDialogueManager dialogueManager;
        [SerializeField] private CtNetSocket netSocket;
        [SerializeField] private CtBattleStateManager battleStateManager;
        [SerializeField] private CtStateMachine stateMachine;
        [SerializeField] private CtQuestSystem questSystem;
        [SerializeField] private CtShopSystem shopSystem;
        [SerializeField] private CtSoundManager soundSystem;
        [SerializeField] private CtOpenWorldInput openWorldInput;
        [SerializeField] private CtDropDatabase dropSystem;

        [SerializeField] private CtStateMachine gameStateMachine;
        [SerializeField] private CtStateBase openWorldGameState;

        public CtGameData GameData => gameData;
        public CtPlayerPersistenceManager PlayerPersistenceManager => playerPersistenceManager;
        public CtPartyManager PartyManager => partyManager;
        public CtEntityManager EntityManager => entityManager;
        public CtDialogueManager DialogueManager => dialogueManager;

        public EGameState GameState { get; set; }
        public ushort LocationId => LocalEntity.LocationId;

        public void _Test_TransitionToWorld()
        {
            LocalEntity.LocationId = gameData.LocationDefinitions[Random.Range(0, gameData.LocationDefinitions.Length)].Identifier;
            GameState = EGameState.TransitionToOpenWorld;
        }

        public void _Test_TransitionToCampSite()
        {
            GameState = EGameState.TransitionToCampSite;
        }

        private CtPlayerEntity _localEntity;

        public CtPlayerEntity LocalEntity
        {
            get => _localEntity;
            private set
            {
                _localEntity = value;
                this.Emit(ERpgGameSignal.LocalPlayerChanged);
            }
        }

        private CtParty _localParty;

        public CtParty LocalParty
        {
            get => _localParty;
            private set
            {
                if (_localParty)
                    this.Emit(ERpgGameSignal.LocalPartyRemoved);
                _localParty = value;
                if (_localParty)
                    this.Emit(ERpgGameSignal.LocalPartyAdded);
            }
        }

        private void Start()
        {
            Init();
        }

        public override void Init()
        {
#if DEBUG_LOGS
            LogDebug("Initializing Rpg Game...");
#endif

            gameData.Init();
            playerPersistenceManager.Init();
            partyManager.Init();
            entityManager.Init();
            questSystem.Init();
            shopSystem.Init();
            dialogueManager.Init();
            soundSystem.Init();
            openWorldInput.Init();

            stateMachine.Init();
            gameStateMachine.Init();

            playerPersistenceManager.Connect(EPlayerPersistenceManagerSignal.LocalPlayerChanged, this, nameof(_OnLocalPlayerChanged));
            playerPersistenceManager.Connect(EPlayerPersistenceManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerPersistenceManager.Connect(EPlayerPersistenceManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            partyManager.Connect(EPartyManagerSignal.PartyStarted, this, nameof(_Signal_OnPartyStarted));
            partyManager.Connect(EPartyManagerSignal.PartyDisbanded, this, nameof(_Signal_OnPartyDisbanded));

            netSocket.Connect(ENetSocketSignal.PacketChanged, this, nameof(_OnPacketChanged));

            gameStateMachine.Process(openWorldGameState);
        }

        public void _OnLocalPlayerChanged()
        {
            var playerWorldPersistenceData = (CtPlayerWorldPersistenceData)GetArgs[0].Reference;
            if (playerWorldPersistenceData)
            {
                LocalEntity = playerWorldPersistenceData.GetComponent<CtPlayerEntity>();
            }
            else
            {
                LocalEntity = null;
            }

            openWorldInput.LocalEntity = LocalEntity;
        }

        [SerializeField] private CtAvatarSnapshot avatarSnapshot;
        public void _OnPlayerAdded()
        {
            var playerWorldPersistenceData = (CtPlayerWorldPersistenceData)GetArgs[0].Reference;
            var playerPersistenceData = playerWorldPersistenceData.PlayerPersistenceData;
#if DEBUG_LOGS
            LogDebug("Player added " +
                     $"(worldData={playerWorldPersistenceData}, worldDataGuid={playerWorldPersistenceData.PlayerGuid}, " +
                     $"playerData={playerPersistenceData}, playerDataGuid={playerPersistenceData.PlayerGuid}).");
#endif

            avatarSnapshot.Register(playerPersistenceData.PlayerId, out var renderTexture);
            var playerDef = (CtPlayerDef)playerPersistenceData.Extension;
            playerDef.Setup(renderTexture);

            var playerEntity = playerWorldPersistenceData.GetComponent<CtPlayerEntity>();
            playerEntity.PlayerDef = playerDef;

            LogWarning($"playerDef.LocationId {playerDef.LocationId}");
            if (playerDef.LocationId != 0)
                GameState = EGameState.TransitionToOpenWorld;
        }

        public void _OnPlayerRemoved()
        {
            var playerWorldPersistenceData = (CtPlayerWorldPersistenceData)GetArgs[0].Reference;
            var playerPersistenceData = playerWorldPersistenceData.PlayerPersistenceData;
#if DEBUG_LOGS
            LogDebug("Player removed " +
                     $"(worldData={playerWorldPersistenceData}, worldDataGuid={playerWorldPersistenceData.PlayerGuid}, " +
                     $"playerData={playerPersistenceData}, playerDataGuid={playerPersistenceData.PlayerGuid}).");
#endif

            var playerEntity = playerWorldPersistenceData.GetComponent<CtPlayerEntity>();
            playerEntity.PlayerDef = null;

            avatarSnapshot.Unregister(playerPersistenceData.PlayerId);
            var playerDef = (CtPlayerDef)playerPersistenceData.Extension;
            playerDef.TearDown();
        }

        public void _Signal_OnPartyStarted()
        {
            var party = (CtParty)GetArgs[0].Reference;
            party.Connect(EPartySignal.MemberAdded, this, nameof(_Signal_OnMemberAdded));
            party.Connect(EPartySignal.MemberRemoved, this, nameof(_Signal_OnMemberRemoved));
        }

        public void _Signal_OnPartyDisbanded()
        {
            var party = (CtParty)GetArgs[0].Reference;
            party.Disconnect(EPartySignal.MemberAdded, this, nameof(_Signal_OnMemberAdded));
            party.Disconnect(EPartySignal.MemberRemoved, this, nameof(_Signal_OnMemberRemoved));
        }

        public void _Signal_OnMemberAdded()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;
            var entity = party.GetEntity(index);
            if (entity != _localEntity) return;

            LocalParty = party;
        }

        public void _Signal_OnMemberRemoved()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;
            var entity = party.GetEntity(index);
            if (entity != _localEntity) return;

            LocalParty = null;
        }

        // public void _OnNpcEntityChanged()
        // {
        //     var entity = (CtEntity)GetArgs[0].Reference;
        //     var previousId = GetArgs[1].UShort;
        //     var entityId = GetArgs[2].UShort;
        //
        //     if (previousId != CtConstants.InvalidId)
        //     {
        //         entity.Reset();
        //     }
        //
        //     if (entityId != CtConstants.InvalidId)
        //     {
        //         entity.OnStartBattle();
        //     }
        // }

        private void JoinParty(CtEntity playerEntity)
        {
            if (partyManager.TryGetEntityParty(playerEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Entity already joined party (identifier={party.Identifier})");
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
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (entity.IsPlayer)
                    return true;
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
                LogWarning("Cannot add anymore members to party.");
#endif
                return;
            }

            for (int i = 0; i < party.MaxCount; i++)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (entity.IsPlayer) continue;
                if (entity.EntityId == npcDef.Identifier)
                {
#if DEBUG_LOGS
                    LogWarning($"Recruit already added to party (partyId={party.Identifier}, recruitId={npcDef.Identifier}).");
#endif
                    return;
                }
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
        }

        // public void JoinQuest(CtParty party, CtAbstractQuest quest)
        // {
        //     party.Battle = quest.Identifier;
        // }
        //
        // public void LeaveQuest(CtParty party)
        // {
        //     party.Battle = CtConstants.InvalidId;
        // }

        private void _PopulateEnemyParty(CtParty party, CtNpcDef[] npcDefs)
        {
            foreach (var npcDef in npcDefs)
            {
                entityManager.TryCreateEnemy(npcDef, out var entity);
                entity.OnStartBattle();
                party.Join(entity);
            }
        }

        private void _StartBattle(CtParty party, CtSquadDef squadDef)
        {
#if DEBUG_LOGS
            LogDebug($"Starting battle (party={party}, squadDef={squadDef.Identifier}).");
#endif

            if (!partyManager.TryGetAvailableEnemyParty(out var enemyParty))
            {
#if DEBUG_LOGS
                LogCritical("Failed to get available enemy party.");
#endif
                return;
            }

            _PopulateEnemyParty(enemyParty, squadDef.NpcDefs);

            for (var i = 0; i < party.MaxCount; ++i)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                entity.OnStartBattle();
            }

            if (!battleStateManager.TryCreateBattleState(squadDef, party, enemyParty, out var battleState))
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
                var entity = party.GetEntity(i);
                if (!entity) continue;
                entity.OnEndBattle();
                party.Leave(entity);
                entityManager.ReleaseEnemy(entity);
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
#if DEBUG_LOGS
            LogDebug($"Ending battle (battleState={battleState}).");
#endif

            for (int i = 0; i < battleState.AllyParty.MaxCount; ++i)
            {
                var entity = battleState.AllyParty.GetEntity(i);
                if (!entity)
                    continue;
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

            var playerId = CtConstants.InvalidId;
            var identifier = CtConstants.InvalidId;

            switch (messageType)
            {
                case MessagePartyAcceptBattle:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    // identifier = BitConverter.ToUInt16(data, offset);
                    _HandlePartyAcceptBattle(playerId);

                    break;
                case MessagePartyStartAdventure:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    var locationId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleStartAdventure(identifier, locationId);

                    break;
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

                    break;
                case MessageRecruitJoin:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleRecruitNpc(playerId, identifier);

                    break;
                case MessageRecruitLeave:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleLeaveNpc(identifier);

                    break;
                case MessageStartBattle:
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    var nodeId = BitConverter.ToInt32(data, offset);
                    offset += 4;
                    _HandleStartBattle(identifier, nodeId);

                    break;
                case MessageLootPickUpItem:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    _HandleTakeItem(playerId, identifier);

                    break;
                case MessageLootGiveItem:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    var item = BitConverter.ToUInt64(data, offset);
                    offset += 8;
                    _HandleGiveItem(playerId, item);

                    break;
                case MessageLootChoice:
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    identifier = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    var rollType = (ERollType)BitConverter.ToInt32(data, offset);
                    offset += 4;
                    _HandleRollChoice(playerId, identifier, rollType);

                    break;
                case MessageLootRoll:
                    item = BitConverter.ToUInt64(data, offset);
                    offset += 4;
                    playerId = BitConverter.ToUInt16(data, offset);
                    offset += 2;
                    var roll = BitConverter.ToInt32(data, offset);
                    offset += 4;
                    _HandleNotifyRoll(item, playerId, roll);

                    break;
                default:
                    return;
            }

#if DEBUG_LOGS
            if (offset != data.Length)
            {
                LogWarning("Failed to parse network message? Please ensure the network message is correctly parsed!).");
            }
#endif
        }

        public void RequestProfession(CtProfessionDef professionDef)
        {
            var localPlayerDef = LocalEntity.EntityDef;
            for (int i = 0; i < 10; ++i)
                localPlayerDef.SetSkill(i, CtConstants.InvalidId);

            var data = CtDataBlock.SetProfession(professionDef.Identifier, professionDef.Attributes.Length);
            for (int i = 0; i < professionDef.Attributes.Length; ++i)
            {
                data = CtDataBlock.SetAttributeRank(i, 0, data);
            }

            localPlayerDef.AttributeData = data;

            this.Emit(ERpgGameSignal.PlayerProfessionChanged);
        }

        public void RequestUpdatePlayerAttribute(int attributeIndex, int value)
        {
            var localPlayerDef = LocalEntity.EntityDef;
            localPlayerDef.AttributeData = CtDataBlock.SetAttributeRank(attributeIndex, value, localPlayerDef.AttributeData);
        }

        public void RequestUpdatePlayerSkillSlot(int skillIndex, CtSkillDef skillDef)
        {
            var localPlayerDef = LocalEntity.EntityDef;
            if (localPlayerDef.GetSkill(skillIndex) == skillDef.Identifier) return;

            for (int i = 0; i < 10; ++i)
                if (localPlayerDef.GetSkill(i) == skillDef.Identifier)
                    localPlayerDef.SetSkill(i, CtConstants.InvalidId);

            localPlayerDef.SetSkill(skillIndex, skillDef.Identifier);
        }

        public void RequestPartyAcceptQuest(CtEntity playerEntity)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessagePartyAcceptBattle);
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

        private void _HandlePartyAcceptBattle(ushort playerId)
        {
            if (!entityManager.TryGetEntity(playerId, out var playerEntity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find player entity (playerId={playerId}).");
#endif
                return;
            }

            JoinParty(playerEntity);

//             if (!partyManager.TryGetEntityParty(playerEntity, out var party))
//             {
// #if DEBUG_LOGS
//                 LogCritical($"Failed to find party for entity (identifier={playerEntity.Identifier})");
// #endif
//                 return;
//             }
        }

        public void _Test_RequestStartAdventure()
        {
            if (gameData.TryGetLocationDef(1, out var locationDef))
                RequestStartAdventure(LocalEntity, locationDef);
        }

        public void RequestStartAdventure(CtEntity playerEntity, CtLocationDef locationDef)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessagePartyStartAdventure);
            size += messageId.Length;

            byte[] playerIdBytes = BitConverter.GetBytes(playerEntity.Identifier);
            size += playerIdBytes.Length;

            byte[] locationIdBytes = BitConverter.GetBytes(locationDef.Identifier);
            size += locationIdBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(playerIdBytes, 0, data, offset, playerIdBytes.Length);
            offset += playerIdBytes.Length;

            Buffer.BlockCopy(locationIdBytes, 0, data, offset, locationIdBytes.Length);
            offset += locationIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleStartAdventure(ushort playerId, ushort locationId)
        {
            if (!entityManager.TryGetEntity(playerId, out var playerEntity))
            {
#if DEBUG_LOGS
                LogError($"Failed to find player entity (playerId={playerId}).");
#endif
                return;
            }

            if (!partyManager.TryGetEntityParty(playerEntity, out var party))
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find party for entity (identifier={playerEntity.Identifier})");
#endif
                return;
            }

            if (!gameData.TryGetLocationDef(locationId, out var locationDef))
                return;

            party.GenerateMap(locationDef);
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
            if (!entityManager.TryGetEntity(playerId, out var playerEntity)) return;

            if (partyId == CtConstants.InvalidId)
            {
                JoinParty(playerEntity);
                return;
            }

            if (!partyManager.TryGetParty(partyId, out var party)) return;

            JoinParty(playerEntity, party);
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
            if (!entityManager.TryGetEntity(playerId, out var playerEntity))
            {
#if DEBUG_LOGS
                LogCritical($"Failed to find player entity (identifier={playerId})");
#endif
                return;
            }

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

//         public void _RequestStartBattleTest()
//         {
//             if (!partyManager.TryGetEntityParty(LocalEntity, out var party))
//             {
// #if DEBUG_LOGS
//                 LogWarning($"Local entity was not in a party (identifier={LocalEntity.Identifier}).");
// #endif
//                 return;
//             }
//
//             var battleDef = gameData.GetBattleDef(party.Battle);
//             if (!battleDef)
//             {
// #if DEBUG_LOGS
//                 LogWarning($"Failed to get battle definition (identifier={party.Battle}).");
// #endif
//                 return;
//             }
//
//             RequestStartBattle(party);
//         }

        public void StartBattle(CtParty party, CtSquadDef squadDef)
        {
            if (squadDef.NpcDefs.Length == 0)
            {
#if DEBUG_LOGS
                LogError("No npc defs to battle against.");
#endif
                return;
            }

            _StartBattle(party, squadDef);
        }

        public void RequestStartBattle(CtParty party, int nodeId)
        {
            int size = 0;

            byte[] messageId = BitConverter.GetBytes(MessageStartBattle);
            size += messageId.Length;

            byte[] partyIdBytes = BitConverter.GetBytes(party.Identifier);
            size += partyIdBytes.Length;

            byte[] mapPoiTypeBytes = BitConverter.GetBytes(nodeId);
            size += mapPoiTypeBytes.Length;

            byte[] data = new byte[size];
            int offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(partyIdBytes, 0, data, offset, partyIdBytes.Length);
            offset += partyIdBytes.Length;

            Buffer.BlockCopy(mapPoiTypeBytes, 0, data, offset, mapPoiTypeBytes.Length);
            offset += mapPoiTypeBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleStartBattle(ushort partyId, int nodeId)
        {
            if (!partyManager.TryGetParty(partyId, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party (partyId={partyId}).");
#endif
                return;
            }

            var map = party.Map;
            if (!map.TryGoToNext(nodeId))
                return;

            var destinationId = map.DestinationId;
            if (!gameData.TryGetLocationDef(destinationId, out var destinationDef))
                return;

            CtSquadDef squadDef;
            if (map.IsLastNode())
            {
                if (destinationDef.HasEndBoss)
                {
                    squadDef = destinationDef.RandomEndBossSquad;
                    StartBattle(party, squadDef);
                }
                else
                {
                    foreach (var player in party.Players)
                    {
                        player.LocationId = map.DestinationId;
                    }

                    map.Clear();
                }

                return;
            }

            var poiType = map.PoiType[map.Current];

            switch (poiType)
            {
                case EMapPoiType.Easy:
                    squadDef = destinationDef.RandomEasySquad;
                    StartBattle(party, squadDef);
                    break;
                case EMapPoiType.Medium:
                    squadDef = destinationDef.RandomMediumSquad;
                    StartBattle(party, squadDef);
                    break;
                case EMapPoiType.Hard:
                    squadDef = destinationDef.RandomHardSquad;
                    StartBattle(party, squadDef);
                    break;
                case EMapPoiType.Boss:
                    squadDef = destinationDef.RandomBossSquad;
                    StartBattle(party, squadDef);
                    break;
                case EMapPoiType.Rest:
                    map.SetCompleted();
                    break;
                default:

                    return;
            }
        }

        // private void SendUpdateLocation(ushort[] playerIds, ushort locationId)
        // {
        //     var size = 0;
        //
        //     var messageId = BitConverter.GetBytes(MessageLootChoice);
        //     size += messageId.Length;
        //
        //     var playerIdCountBytes = BitConverter.GetBytes(playerIds.Length);
        //     size += playerIdCountBytes.Length;
        //
        //     var playerIdsBytes = new byte[playerIds.Length][];
        //     for (var i = 0; i < playerIds.Length; i++)
        //     {
        //         playerIdsBytes[i] = BitConverter.GetBytes(playerIds[i]);
        //         size += playerIdsBytes[i].Length;
        //     }
        //
        //     var locationIdBytes = BitConverter.GetBytes(locationId);
        //     size += locationIdBytes.Length;
        //
        //     var data = new byte[size];
        //     var offset = 0;
        //
        //     Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
        //     offset += messageId.Length;
        //
        //     Buffer.BlockCopy(playerIdCountBytes, 0, data, offset, playerIdCountBytes.Length);
        //     offset += playerIdCountBytes.Length;
        //
        //     foreach (var playerIdsByteData in playerIdsBytes)
        //     {
        //         Buffer.BlockCopy(playerIdsByteData, 0, data, offset, playerIdsByteData.Length);
        //         offset += playerIdsByteData.Length;
        //     }
        //
        //     Buffer.BlockCopy(locationIdBytes, 0, data, offset, locationIdBytes.Length);
        //     offset += locationIdBytes.Length;
        //
        //     netSocket.SendAll(data);
        // }
        //
        // private void _HandleUpdateLocation(ushort[] playerIds, ushort locationId)
        // {
        //     if (Array.IndexOf(playerIds, ) != -1)
        // }

        public void RequestRollChoice(ushort itemId, ERollType choice)
        {
#if DEBUG_LOGS
            LogDebug($"Requesting roll choice (itemId={itemId}, choice={choice}).");
#endif

            var size = 0;

            var messageId = BitConverter.GetBytes(MessageLootChoice);
            size += messageId.Length;

            var entityIdBytes = BitConverter.GetBytes(_localEntity.Identifier);
            size += entityIdBytes.Length;

            var itemIdBytes = BitConverter.GetBytes(itemId);
            size += itemIdBytes.Length;

            var choiceBytes = BitConverter.GetBytes(Convert.ToInt32(choice));
            size += choiceBytes.Length;

            var data = new byte[size];
            var offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(entityIdBytes, 0, data, offset, entityIdBytes.Length);
            offset += entityIdBytes.Length;

            Buffer.BlockCopy(itemIdBytes, 0, data, offset, itemIdBytes.Length);
            offset += itemIdBytes.Length;

            Buffer.BlockCopy(choiceBytes, 0, data, offset, choiceBytes.Length);
            offset += choiceBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleRollChoice(ushort entityId, ushort itemId, ERollType choice)
        {
            if (!entityManager.TryGetEntity(entityId, out var entity))
                return;

            if (!partyManager.TryGetEntityParty(entity, out var party))
                return;

            // if (!battleStateManager.TryGetBattleState(party, out var battleState))
            //     return;

            if (!party.TryGetRollSession(itemId, out var rollSession))
                return;

            rollSession.SubmitChoice((CtPlayerEntity)entity, choice);
        }

        public void RequestTakeItem(ushort itemId)
        {
            var size = 0;

            var messageId = BitConverter.GetBytes(MessageLootPickUpItem);
            size += messageId.Length;

            var entityIdBytes = BitConverter.GetBytes(_localEntity.Identifier);
            size += entityIdBytes.Length;

            var itemIdBytes = BitConverter.GetBytes(itemId);
            size += itemIdBytes.Length;

            var data = new byte[size];
            var offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(entityIdBytes, 0, data, offset, entityIdBytes.Length);
            offset += entityIdBytes.Length;

            Buffer.BlockCopy(itemIdBytes, 0, data, offset, itemIdBytes.Length);
            offset += itemIdBytes.Length;

            netSocket.SendToMasterOnly(data);
        }

        private void _HandleTakeItem(ushort entityId, ushort itemId)
        {
            dropSystem.TryTakeItem(itemId, entityId);

            // if (!entityManager.TryGetEntity(entityId, out var entity))
            //     return;
            //
            // if (!dropSystem.TakeDrop(itemId, out var drop)) return;
            //
            // var playerEntity = (CtPlayerEntity)entity;
            // RequestGiveItem(playerEntity, drop);
        }

        public void RequestGiveItem(ushort entityId, ulong item)
        {
            var size = 0;

            var messageId = BitConverter.GetBytes(MessageLootGiveItem);
            size += messageId.Length;

            var entityIdBytes = BitConverter.GetBytes(entityId);
            size += entityIdBytes.Length;

            var itemIdBytes = BitConverter.GetBytes(item);
            size += itemIdBytes.Length;

            var data = new byte[size];
            var offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(entityIdBytes, 0, data, offset, entityIdBytes.Length);
            offset += entityIdBytes.Length;

            Buffer.BlockCopy(itemIdBytes, 0, data, offset, itemIdBytes.Length);
            offset += itemIdBytes.Length;

            netSocket.SendAll(data);
        }

        private void _HandleGiveItem(ushort entityId, ulong item)
        {
            if (entityId != _localEntity.Identifier) return;
            _localEntity.PlayerInventory.TryGiveItem(item);
        }

        public void NotifyRoll(ulong itemData, ushort entityId, int roll)
        {
            var size = 0;

            var messageId = BitConverter.GetBytes(MessageLootRoll);
            size += messageId.Length;

            var itemDataBytes = BitConverter.GetBytes(itemData);
            size += itemDataBytes.Length;

            var entityIdBytes = BitConverter.GetBytes(entityId);
            size += entityIdBytes.Length;

            var rollBytes = BitConverter.GetBytes(roll);
            size += rollBytes.Length;

            var data = new byte[size];
            var offset = 0;

            Buffer.BlockCopy(messageId, 0, data, offset, messageId.Length);
            offset += messageId.Length;

            Buffer.BlockCopy(itemDataBytes, 0, data, offset, itemDataBytes.Length);
            offset += itemDataBytes.Length;

            Buffer.BlockCopy(entityIdBytes, 0, data, offset, entityIdBytes.Length);
            offset += entityIdBytes.Length;

            Buffer.BlockCopy(rollBytes, 0, data, offset, rollBytes.Length);
            offset += rollBytes.Length;

            netSocket.SendAll(data);
        }

        private void _HandleNotifyRoll(ulong data, ushort entityId, int roll)
        {
            if (!_localParty) return;

            if (!entityManager.TryGetEntity(entityId, out var entity)) return;
            if (!partyManager.TryGetEntityParty(entity, out var party)) return;
            if (_localParty.Identifier != party.Identifier) return;

            LogDebug($"{entity.DisplayName} rolls {roll}.");
        }
    }
}