
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConstantConsideration : CtBehaviorConsiderationBase
    {
        [SerializeField] private float value;

        public override float Evaluate(CtNpcContext context) => value;
    }
}