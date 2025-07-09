
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcDef : CtEntityDef
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private CtNpcTypeDef npcType;
        [SerializeField] private CtNpcBehavior behavior;

        public ushort Identifier => identifier;
        public CtNpcTypeDef NpcType => npcType;
        public CtNpcBehavior Behavior => behavior;
    }
}