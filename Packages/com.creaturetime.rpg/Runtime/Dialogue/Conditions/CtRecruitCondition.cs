
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRecruitCondition : CtResponseCondition
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private CtNpcDef npc;
        [SerializeField] private ERecruitResponseNodeType action;

        private bool IsMemberInParty(CtParty party)
        {
            for (int i = 0; i < party.MaxCount; ++i)
            {
                var entity = party.GetEntity(i);
                if (!entity) continue;
                if (entity.IsPlayer) continue;
                if (entity.EntityId == npc.Identifier)
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
            switch (action)
            {
                case ERecruitResponseNodeType.Join:
                    if (!isMemberInParty)
                        return true;
                    break;
                case ERecruitResponseNodeType.Leave:
                    if (isMemberInParty)
                        return true;
                    break;
            }

            return false;
        }
    }
}