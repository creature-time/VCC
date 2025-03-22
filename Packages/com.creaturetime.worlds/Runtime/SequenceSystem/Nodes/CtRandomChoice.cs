
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRandomChoice : CtSequenceNodeBase
    {
        public CtSequenceNodeBase[] choices;

        public override CtSequenceNodeBase GetNext(CtBlackboard context) => choices[Random.Range(0, choices.Length)];
    }
}