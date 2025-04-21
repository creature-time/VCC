
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    enum EEntityManagerSignal
    {
        NpcEntityChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEntityManager : CtAbstractSignal
    {
        [SerializeField, HideInInspector] private CtEntity[] playerEntities;
        [SerializeField, HideInInspector] private CtEntity[] recruitEntities;
        [SerializeField, HideInInspector] private CtEntity[] enemyEntities;

        private DataDictionary _entityLookup = new DataDictionary();

        public void Init()
        {
            for (int i = 0; i < playerEntities.Length; i++)
            {
                ushort id = (ushort)i;
                var entity = playerEntities[i];
                entity.Init(this, id);
                _entityLookup.Add(id, entity);
            }

            for (int i = 0; i < recruitEntities.Length; i++)
            {
                ushort id = (ushort)(i + 1000);
                var entity = recruitEntities[i];
                entity.Init(this, id);
                _entityLookup.Add(id, entity);

                entity.Connect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));
            }

            for (int i = 0; i < enemyEntities.Length; i++)
            {
                ushort id = (ushort)(i + 2000);
                var entity = enemyEntities[i];
                entity.Init(this, id);
                _entityLookup.Add(id, entity);

                entity.Connect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));
            }
        }

        public void _OnIdentifierChanged()
        {
            var entity = (CtEntity)Sender;
            var previousId = GetArgs[0].UShort;
            var entityId = GetArgs[1].UShort;

            SetArgs.Add(entity);
            SetArgs.Add(previousId);
            SetArgs.Add(entityId);
            this.Emit(EEntityManagerSignal.NpcEntityChanged);
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

        public void AcquirePlayerEntity(int index, CtPlayerDef playerDef, out CtEntity entity)
        {
            entity = playerEntities[index];
            entity.EntityId = GeneratePartyId(playerDef);
        }

        public bool TryGetPlayerEntity(int index, out CtEntity entity)
        {
            entity = playerEntities[index];
            if (entity.EntityId == CtConstants.InvalidId)
            {
                entity = null;
                return false;
            }

            return true;
        }

        public void ReleasePlayerEntity(int index)
        {
            var entity = playerEntities[index];
            entity.EntityId = CtConstants.InvalidId;
        }

        public bool TryAcquireRecruit(CtNpcDef npcDef, out CtEntity entity)
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

        public void ReleaseRecruitEntity(int index)
        {
            var entity = recruitEntities[index];
            entity.EntityId = CtConstants.InvalidId;
            entity.Reset();
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

        public static ushort GetIdentifier(ushort memberId)
        {
            return (ushort)((memberId & 0xFF00) >> 8);
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