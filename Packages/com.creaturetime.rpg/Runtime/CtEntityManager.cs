
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEntityManager : CtAbstractSignal
    {
        [SerializeField, HideInInspector] private CtEntity[] playerEntities;
        [SerializeField, HideInInspector] private CtEntity[] recruitEntities;
        [SerializeField, HideInInspector] private CtEntity[] enemyEntities;

        [SerializeField] private CtPlayerManager playerManager;
        [SerializeField] private CtGameData gameData;

        public CtEntity LocalEntity { get; private set; }

        private DataDictionary _entityLookup = new DataDictionary();

        private void Start()
        {
            for (int i = 0; i < playerEntities.Length; i++)
            {
                ushort id = (ushort)i;
                var entity = playerEntities[i];
                entity.Init(id);
                _entityLookup.Add(id, entity);
            }

            playerManager.Connect(EPlayerManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerManager.Connect(EPlayerManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            for (int i = 0; i < recruitEntities.Length; i++)
            {
                ushort id = (ushort)(i + 1000);
                var entity = recruitEntities[i];
                entity.Init(id);
                _entityLookup.Add(id, entity);

                entity.Connect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));
            }

            for (int i = 0; i < enemyEntities.Length; i++)
            {
                ushort id = (ushort)(i + 2000);
                var entity = enemyEntities[i];
                entity.Init(id);
                _entityLookup.Add(id, entity);

                entity.Connect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));
            }
        }

        public void _OnPlayerAdded()
        {
            var index = GetArgs[0].Int;

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            TryCreatePlayer(index, playerDef, out var entity);

            if (playerDef.IsLocal)
                LocalEntity = entity;

            entity.PlayerDef = playerDef;
            entity.SetupEntityForBattle();
        }

        public void _OnPlayerRemoved()
        {
            var index = GetArgs[0].Int;

            var entity = playerEntities[index];
            entity.EntityId = CtConstants.InvalidId;
            entity.PlayerDef = null;
            entity.Reset();
        }

        public void _OnIdentifierChanged()
        {
            var entity = (CtEntity)Sender;

            var memberId = entity.EntityId;
            if (memberId != CtConstants.InvalidId)
            {
                ushort npcId = (ushort)((memberId & 0xFF00) >> 8);
                entity.NpcDef = gameData.GetNpcDef(npcId);

                entity.SetupEntityForBattle();
            }
            else
            {
                entity.RemoveEntityDef();
                entity.Reset();
            }
        }

        public bool TryGetEntity(ushort identifer, out CtEntity entity)
        {
            entity = null;
            if (_entityLookup.TryGetValue(identifer, out var token))
            {
                entity = (CtEntity)token.Reference;
                return true;
            }

            return false;
        }

        public void TryCreatePlayer(int index, CtPlayerDef playerDef, out CtEntity entity)
        {
            entity = playerEntities[index];
            entity.EntityId = GeneratePartyId(playerDef);
        }

        public bool TryCreateRecruit(CtNpcDef npcDef, out CtEntity entity)
        {
            entity = null;
            foreach (var other in recruitEntities)
            {
                if (other.EntityId == CtConstants.InvalidId)
                {
                    other.EntityId = GeneratePartyId(npcDef);
                    entity = other;
                    return true;
                }
            }

            return false;
        }

        public bool TryCreateEnemy(CtNpcDef npcDef, out CtEntity entity)
        {
            entity = null;
            foreach (var other in enemyEntities)
            {
                if (other.EntityId == CtConstants.InvalidId)
                {
                    other.EntityId = GeneratePartyId(npcDef);
                    entity = other;
                    return true;
                }
            }

            return false;
        }

        public static bool IsPlayer(ushort memberId)
        {
            return (memberId & 0x00FF) != 0;
        }

        public static ushort GeneratePartyId(CtPlayerDef playerDef)
        {
            ushort partyId = 1 & 0xFF;
            ushort idx = playerDef.PlayerId;
            partyId |= (ushort)((idx & 0x00FF) << 8);
            return partyId;
        }

        public static ushort GeneratePartyId(CtNpcDef npcDef)
        {
            ushort partyId = 0 & 0xFF;
            ushort id = npcDef.Identifier;
            partyId |= (ushort)((id & 0x00FF) << 8);
            return partyId;
        }
    }
}