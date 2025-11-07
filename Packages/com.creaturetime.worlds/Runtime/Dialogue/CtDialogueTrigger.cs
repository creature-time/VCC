
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueTrigger : CtLoggerUdonScript
    {
        [SerializeField] public UdonSharpBehaviour target;
        [SerializeField] public string eventTrigger;
    }
}