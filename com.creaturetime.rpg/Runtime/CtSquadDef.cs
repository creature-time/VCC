
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSquadDef : CtAbstractDefinition
    {
        [SerializeField] private CtNpcDef[] npcDefs;
        [SerializeField] private CtLootTable chestLootTable;
        [SerializeField] private CtUserData userData;

        public CtNpcDef[] NpcDefs => npcDefs;
        public CtLootTable ChestLootTable => chestLootTable;
        public CtUserData UserData => userData;
    }
}