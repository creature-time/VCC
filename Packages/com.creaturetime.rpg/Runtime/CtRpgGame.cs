
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgGame : CtAbstractSignal
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtPlayerManager playerManager;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        [SerializeField] private CtAbstractQuest[] quests;

        private DataDictionary _quests = new DataDictionary();

        public CtGameData GameData => gameData;
        public CtPlayerManager PlayerManager => playerManager;
        public CtPartyManager PartyManager => partyManager;
        public CtEntityManager EntityManager => entityManager;

        public CtEntity LocalEntity { get; private set; }

        private void Start()
        {
            foreach (var quest in quests)
                _quests.Add(quest.Identifier, quest);

            playerManager.Connect(EPlayerManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerManager.Connect(EPlayerManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            entityManager.Connect(EEntityManagerSignal.NpcEntityChanged, this, nameof(_OnNpcEntityChanged));
        }

        public void _OnPlayerAdded()
        {
            var index = GetArgs[0].Int;

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            entityManager.AcquirePlayerEntity(index, playerDef, out var entity);

            if (playerDef.IsLocal)
                LocalEntity = entity;

            entity.PlayerDef = playerDef;
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
        }

        public void _OnNpcEntityChanged()
        {
            var entity = (CtEntity)GetArgs[0].Reference;
            var previousId = GetArgs[1].UShort;
            var entityId = GetArgs[2].UShort;

            if (previousId != CtConstants.InvalidId)
            {
                entity.NpcDef = null;
                entity.Reset();
            }

            if (entityId != CtConstants.InvalidId)
            {
                ushort npcId = (ushort)((entityId & 0xFF00) >> 8);
                entity.NpcDef = gameData.GetNpcDef(npcId);

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
                LogWarning("Entity was not in a party)");
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
            // TODO
        }

        public void LeaveQuest(CtParty party)
        {
            // TODO
        }
    }
}