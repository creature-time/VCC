
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgTests : CtLoggerUdonScript
    {
        [SerializeField] private CtRpgGame rpgGame;

        public void _RunGameDataTest0()
        {
            for (ushort i = 1; i <= 3; ++i)
            {
                var npcDef = rpgGame.GameData.GetNpcDef(i);
#if DEBUG_LOGS
                Log($"{npcDef.DisplayName} (identifier={npcDef.Identifier}, level={npcDef.CharacterLevel})");
#endif
            }
        }

        public void _RunPartyManagerTest0()
        {
            rpgGame.JoinParty(rpgGame.LocalEntity);
        }

        public void _RunPartyManagerTest1()
        {
            rpgGame.LeaveParty(rpgGame.LocalEntity);
        }

        public void _RunPartyManagerTest2()
        {
            rpgGame.AcquireRecruitNpc(rpgGame.LocalEntity, rpgGame.GameData.GetNpcDef(1));
        }

        public void _RunPartyManagerTest3()
        {
            if (!rpgGame.PartyManager.TryGetEntityParty(rpgGame.LocalEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for recruit (identifier={rpgGame.LocalEntity.Identifier}).");
#endif
                return;
            }

            CtEntity recruit = null;
            for (int i = 0; i < 4; ++i)
            {
                var identifier = party.GetMemberId(i);
                if (identifier != CtConstants.InvalidId)
                {
                    if (!rpgGame.EntityManager.TryGetEntity(identifier, out var entity))
                    {
#if DEBUG_LOGS
                        LogCritical($"[_RunPartyMemberTest3] Failed to find entity (identifier={identifier}).");
#endif
                        continue;
                    }

                    if (!entity.IsPlayer)
                        recruit = entity;
                }
            }

            rpgGame.ReleaseRecruitNpc(recruit);
        }

        public void _RunDialogueTest0()
        {
            rpgGame.DialogueManager.StartConversation(1);
        }

        public void _RunChatterTest0()
        {
            rpgGame.DialogueManager.StartChatter(1);
        }

        public void _RunBattleStateTest0()
        {
            if (!rpgGame.PartyManager.TryGetEntityParty(rpgGame.LocalEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for recruit (identifier={rpgGame.LocalEntity.Identifier}).");
#endif
                return;
            }

            rpgGame.StartBattle(party);
        }
    }
}