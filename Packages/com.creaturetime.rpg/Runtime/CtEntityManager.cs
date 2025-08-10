
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    enum EEntityManagerSignal
    {
        DamageApplied,
        NpcEntityChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEntityManager : CtSingleton
    {
        [SerializeField, HideInInspector] private CtPlayerEntity[] playerEntities;
        [SerializeField, HideInInspector] private CtNpcEntity[] recruitEntities;
        [SerializeField, HideInInspector] private CtNpcEntity[] enemyEntities;

        private DataDictionary _entityLookup = new DataDictionary();

        public void Init()
        {
            foreach (var entity in playerEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
            }

            foreach (var entity in recruitEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
                entity.Connect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));
            }

            foreach (var entity in enemyEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
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

        public void CreatePlayerEntity(int playerId, CtPlayerDef playerDef, out CtEntity entity)
        {
            var playerEntity = playerEntities[playerId];
            playerEntity.PlayerDef = playerDef;
            // _entityLookup.Add(playerEntity.Identifier, playerEntity);
            entity = playerEntity;

#if DEBUG_LOGS
                Log($"Setup player entity (identifier={playerEntity.Identifier}).");
#endif
        }

        public void ReleasePlayerEntity(ushort playerId)
        {
#if DEBUG_LOGS
            Log($"Releasing player entity (playerId={playerId}).");
#endif

            var playerEntity = playerEntities[playerId];
            playerEntity.PlayerDef = null;
            // _entityLookup.Remove(playerEntity.Identifier);
        }

        public bool TryAcquireRecruit(CtNpcDef npcDef, out CtEntity entity)
        {
            entity = null;
            foreach (var other in recruitEntities)
            {
                if (other.EntityId == CtConstants.InvalidId)
                {
                    other.NpcId = npcDef.Identifier;
                    entity = other;
                    return true;
                }
            }

            return false;
        }

        public void ReleaseRecruitEntity(CtEntity entity)
        {
            var npcEntity = (CtNpcEntity)entity;
            npcEntity.NpcId = CtConstants.InvalidId;
        }

        public bool TryCreateEnemy(CtNpcDef npcDef, out CtEntity entity)
        {
            entity = null;
            foreach (var other in enemyEntities)
            {
                if (other.EntityId == CtConstants.InvalidId)
                {
                    other.NpcId = npcDef.Identifier;
                    entity = other;
                    return true;
                }
            }

            return false;
        }

        public void ReleaseEnemy(CtEntity entity)
        {
            var npcEntity = (CtNpcEntity)entity;
            npcEntity.NpcId = CtConstants.InvalidId;
        }
    }
}