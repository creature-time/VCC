
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgTests : CtLoggerUdonScript
    {
        [Header("Game Data Tests")]
        [SerializeField] private CtGameData gameData;

        public void _RunGameDataTest0()
        {
            for (ushort i = 1; i <= 3; ++i)
            {
                var npcDef = gameData.GetNpcDef(i);
                Log($"{npcDef.DisplayName} (identifier={npcDef.Identifier}, level={npcDef.CharacterLevel})");
            }
        }

        [Header("Party Manager Tests")]
        [SerializeField] private CtPlayerManager playerManager;
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        private bool _HasPlayers(CtParty party)
        {
            for (int i = 0; i < 4; ++i)
            {
                var identifier = party.GetMemberId(i);
                Debug.LogError($"identifier {identifier}");
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

        public void _RunPartyManagerTest0()
        {
            if (partyManager.TryGetParty(entityManager.LocalEntity, out var party))
            {
                LogWarning($"Entity already joined party  (identifier={party.Identifier})");
                return;
            }

            if (partyManager.TryGetAvailableParty(out party))
            {
                LogDebug($"Empty party found (identifier={party.Identifier})");
                party.Join(entityManager.LocalEntity);
            }
            else
            {
                LogDebug($"Failed to find empty party (identifier={party.Identifier})");
            }
        }

        public void _RunPartyManagerTest1()
        {
            if (!partyManager.TryGetParty(entityManager.LocalEntity, out var party))
            {
                LogWarning("Entity was not in a party)");
                return;
            }

            party.Leave(entityManager.LocalEntity);

            if (!_HasPlayers(party))
                party.Clear();
        }

        public void _RunPartyManagerTest2()
        {
            if (!partyManager.TryGetParty(entityManager.LocalEntity, out var party))
            {
                LogWarning("Entity was not in a party)");
                return;
            }

            if (!entityManager.TryCreateRecruit(gameData.GetNpcDef(1), out var recruit))
            {
                LogWarning("No recruit available.");
                return;
            }

            party.Join(recruit);
        }

        public void _RunPartyManagerTest3()
        {
            if (!partyManager.TryGetParty(entityManager.LocalEntity, out var party))
            {
                LogWarning("Entity was not in a party)");
                return;
            }

            CtEntity recruit = null;
            for (int i = 0; i < 4; ++i)
            {
                var identifier = party.GetMemberId(i);
                if (identifier != CtConstants.InvalidId)
                {
                    if (!entityManager.TryGetEntity(identifier, out var entity))
                    {
                        LogCritical($"[_RunPartyMemberTest3] Failed to find entity (identifier={identifier}).");
                        continue;
                    }

                    if (!entity.IsPlayer)
                        recruit = entity;
                }
            }

            if (!recruit)
            {
                LogWarning("No recruit found");
                return;
            }

            party.Leave(recruit);

            recruit.EntityId = CtConstants.InvalidId;

            if (!_HasPlayers(party))
                party.Clear();
        }
    }
}