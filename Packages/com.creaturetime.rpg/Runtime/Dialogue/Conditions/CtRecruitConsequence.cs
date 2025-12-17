
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

        private bool IsMemberInParty(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; ++i)
            {
                var entity = party.GetEntity(i);
                if (entity.IsPlayer) continue;
                if (entity.EntityId == dialogueActor.Identifier)
                    return true;
            }

            return false;
        }

        public override void Execute()
        {
            if (!rpgGame.PartyManager.TryGetEntityParty(rpgGame.LocalEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for recruit (identifier={rpgGame.LocalEntity.Identifier}).");
#endif
                return;
            }

            var isMemberInParty = IsMemberInParty(party);
            if (recruitLeave)
            {
                if (isMemberInParty)
                {
#if DEBUG_LOGS
                LogWarning($"Npc already in party (partyId={party.Identifier}, recruitId={dialogueActor.Identifier}).");
#endif
                    return;
                }

                rpgGame.RequestRecruitNpc(rpgGame.LocalEntity, rpgGame.GameData.GetNpcDef(dialogueActor.Identifier));
            }
            else
            {
                for (int i = 0; i < party.MaxCount; ++i)
                {
                    var entity = party.GetEntity(i);
                    if (!entity) continue;
                    if (!entity.IsPlayer && entity.EntityId == dialogueActor.Identifier)
                        rpgGame.RequestLeaveNpc(entity);
                }
            }
        }
    }
}