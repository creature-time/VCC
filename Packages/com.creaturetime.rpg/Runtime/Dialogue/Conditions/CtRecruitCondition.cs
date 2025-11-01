
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRecruitCondition : CtResponseCondition
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtDialogueActor dialogueActor;
        [SerializeField] private bool recruitLeave;

        private bool IsMemberInParty(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; ++i)
            {
                var memberId = party.GetMemberId(i);
                if (memberId == CtConstants.InvalidId) continue;
                if (!rpgGame.EntityManager.TryGetEntity(memberId, out var entity))
                {
#if DEBUG_LOGS
                LogWarning($"Failed to find entity (identifier={memberId}).");
#endif
                    continue;
                }

                if (entity.IsPlayer) continue;

                if (entity.EntityId == dialogueActor.Identifier)
                    return true;
            }

            return false;
        }

        public override bool IsValid()
        {
            if (!rpgGame.PartyManager.TryGetEntityParty(rpgGame.LocalEntity, out var party))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find party for recruit (identifier={rpgGame.LocalEntity.Identifier}).");
#endif
                return false;
            }

            var isMemberInParty = IsMemberInParty(party);
            Debug.Log($"foo {recruitLeave} {isMemberInParty}");
            return recruitLeave == isMemberInParty;
        }
    }
}