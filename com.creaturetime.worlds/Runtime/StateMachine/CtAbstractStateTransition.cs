
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.Worlds.CtRpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public abstract class CtAbstractStateTransition : UdonSharpBehaviour
    {
        // public abstract CtStateBase GetNext();
    }
}