
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRecruitConsequence : CtResponseConsequence
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
            switch (action)
            {
                case ERecruitResponseNodeType.Join:
                    if (isMemberInParty)
                    {
#if DEBUG_LOGS
                        LogWarning($"Npc already in party (partyId={party.Identifier}, recruitId={npc.Identifier}).");
#endif
                        return;
                    }

                    rpgGame.RequestRecruitNpc(rpgGame.LocalEntity, rpgGame.GameData.GetNpcDef(npc.Identifier));

                    break;
                case ERecruitResponseNodeType.Leave:
                    for (int i = 0; i < party.MaxCount; ++i)
                    {
                        var entity = party.GetEntity(i);
                        if (!entity) continue;
                        if (!entity.IsPlayer && entity.EntityId == npc.Identifier)
                            rpgGame.RequestLeaveNpc(entity);
                    }
                    break;
            }
        }
    }
}