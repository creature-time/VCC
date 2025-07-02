
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSquadDef : CtAbstractDefinition
    {
        [SerializeField] private CtNpcDef[] npcDefs;
        [SerializeField] private CtUserData userData;

        public CtNpcDef[] NpcDefs => npcDefs;
        public CtUserData UserData => userData;
    }
}