
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    [Flags]
    enum EPlayerEntityTransformFlags
    {
        Root = 1 << 0,
        Head = 1 << 1,
        LeftHand = 1 << 2,
        RightHand = 1 << 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtEntityManager : CtSingleton
    {
        [SerializeField] private CtPlayerEntity[] playerEntities;
        [SerializeField, HideInInspector] private CtNpcEntity[] recruitEntities;
        [SerializeField, HideInInspector] private CtNpcEntity[] enemyEntities;
        [SerializeField, EnumFlag] private EPlayerEntityTransformFlags playerEntityTransformFlags;

        private DataDictionary _entityLookup = new DataDictionary();

        public override void Init()
        {
            foreach (var entity in playerEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
            }

            foreach (var entity in recruitEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
            }

            foreach (var entity in enemyEntities)
            {
                _entityLookup.Add(entity.Identifier, entity);
            }
        }

        public bool TryGetEntity(ushort identifer, out CtEntity entity)
        {
            if (identifer == CtConstants.InvalidId)
            {
                entity = null;
                return true;
            }

            entity = null;
            if (_entityLookup.TryGetValue(identifer, out var token))
            {
                entity = (CtEntity)token.Reference;
                return true;
            }

#if DEBUG_LOGS
            LogCritical($"Failed to find entity (identifier={identifer}).");
#endif

            return false;
        }

//         public void CreatePlayerEntity(int playerId, CtPlayerDef playerDef, out CtEntity entity)
//         {
//             var playerEntity = playerEntities[playerId];
//             playerEntity.PlayerDef = playerDef;
//             // _entityLookup.Add(playerEntity.Identifier, playerEntity);
//             entity = playerEntity;
//
// #if DEBUG_LOGS
//                 Log($"Setup player entity (identifier={playerEntity.Identifier}).");
// #endif
//         }

//         public void ReleasePlayerEntity(ushort playerId)
//         {
// #if DEBUG_LOGS
//             Log($"Releasing player entity (playerId={playerId}).");
// #endif
//
//             var playerEntity = playerEntities[playerId];
//             playerEntity.PlayerDef = null;
//             // _entityLookup.Remove(playerEntity.Identifier);
//         }

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

        void Update()
        {
            if (playerEntityTransformFlags == 0) return;

            foreach (var playerEntity in playerEntities)
            {
                if (!playerEntity.EntityDef) continue;

                var player = Networking.GetOwner(playerEntity.EntityDef.gameObject);
                if (player == null) continue;

                if (((int)playerEntityTransformFlags & (int)EPlayerEntityTransformFlags.Root) != 0)
                {
                    var playerTransform = playerEntity.RootTransform;
                    playerTransform.position = player.GetPosition();
                    playerTransform.rotation = player.GetRotation();
                }

                if (((int)playerEntityTransformFlags & (int)EPlayerEntityTransformFlags.Head) != 0)
                {
                    var headTransform = playerEntity.HeadTransform;
                    var trackingData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                    headTransform.position = trackingData.position;
                    headTransform.rotation = trackingData.rotation;
                }

                if (((int)playerEntityTransformFlags & (int)EPlayerEntityTransformFlags.LeftHand) != 0)
                {
                    var leftHandTransform = playerEntity.LeftHandTransform;
                    var trackingData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                    leftHandTransform.position = trackingData.position;
                    leftHandTransform.rotation = trackingData.rotation;
                }

                if (((int)playerEntityTransformFlags & (int)EPlayerEntityTransformFlags.RightHand) != 0)
                {
                    var rightHandTransform = playerEntity.RightHandTransform;
                    var trackingData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                    rightHandTransform.position = trackingData.position;
                    rightHandTransform.rotation = trackingData.rotation;
                }
            }
        }
    }
}