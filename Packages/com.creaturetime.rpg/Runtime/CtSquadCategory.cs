
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSquadCategory : UdonSharpBehaviour
    {
        [SerializeField] private CtSquadDef[] squadDefs;

        public CtSquadDef[] SquadDefs => squadDefs;
    }
}