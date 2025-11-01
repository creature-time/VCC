
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRecruitConsequence : CtResponseConsequence
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtDialogueActor dialogueActor;
        [SerializeField] private bool recruitLeave;

        public override void Execute()
        {
            if (recruitLeave)
                rpgGame.RequestRecruitNpc(rpgGame.LocalEntity, rpgGame.GameData.GetNpcDef(dialogueActor.Identifier));
            else
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

                        if (!entity.IsPlayer && entity.EntityId == dialogueActor.Identifier)
                            rpgGame.RequestLeaveNpc(entity);
                    }
                }
            }
        }
    }
}