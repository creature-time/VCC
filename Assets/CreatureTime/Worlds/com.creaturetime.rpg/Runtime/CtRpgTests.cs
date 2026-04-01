
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
                Log($"{npcDef.DisplayName} (identifier={npcDef.Identifier})");
#endif
            }
        }

        public void _RunPartyManagerTest0()
        {
            rpgGame.RequestJoinParty(rpgGame.LocalEntity, null);
        }

        public void _RunPartyManagerTest1()
        {
            rpgGame.RequestLeaveParty(rpgGame.LocalEntity);
        }

        public void _RunPartyManagerTest2()
        {
            rpgGame.RequestRecruitNpc(rpgGame.LocalEntity, rpgGame.GameData.GetNpcDef(1));
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

            for (int i = 0; i < 4; ++i)
            {
                var entity = party.GetEntity(i);
                if (entity)
                {
                    if (!entity.IsPlayer)
                        rpgGame.RequestLeaveNpc(entity);
                }
            }
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

            // party.Quest = rpgGame.GameData.QuestDefinitions[0].Identifier;
            // rpgGame.RequestStartBattle(party);
        }
    }
}