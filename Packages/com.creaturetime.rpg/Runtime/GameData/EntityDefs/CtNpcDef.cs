
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcDef : CtEntityDef
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private bool isBoss;
        [SerializeField] private CtNpcTypeDef npcType;
        [SerializeField] private CtNpcBehavior behavior;
        [SerializeField] private CtLootTable lootTable;

        public override bool IsBoss => isBoss;

        public ushort Identifier => identifier;
        public CtNpcTypeDef NpcType => npcType;
        public CtNpcBehavior Behavior => behavior;
        public CtLootTable LootTable => lootTable;
    }
}