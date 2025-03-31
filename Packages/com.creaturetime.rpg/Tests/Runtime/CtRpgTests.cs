
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
                Log($"{npcDef.DisplayName} (identifier={npcDef.Identifier}, level={npcDef.CharacterLevel})");
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
                LogWarning($"Failed to find party for recruit (identifier={rpgGame.LocalEntity.Identifier}).");
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
                        LogCritical($"[_RunPartyMemberTest3] Failed to find entity (identifier={identifier}).");
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
    }
}