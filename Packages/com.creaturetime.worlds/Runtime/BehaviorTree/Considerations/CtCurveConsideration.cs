
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtCurveConsideration : CtBehaviorConsiderationBase
    {
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private string key;

        public override float Evaluate(CtNpcContext context)
        {
            if (context.TryGetFloat(key, out var value))
                return curve.Evaluate(value);
            return 0;
        }
    }
}