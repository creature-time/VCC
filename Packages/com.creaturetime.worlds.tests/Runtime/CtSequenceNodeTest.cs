
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSequenceNodeTest : CtSequenceNodeBase
    {
        [SerializeField] private int testOutput;
        [SerializeField] private float timer = 2f;

        private float _timer;

        public override void OnEnter(CtBlackboard context)
        {
            _timer = 0;
        }

        public override ENodeStatus Process(CtBlackboard context)
        {
            _timer += Time.deltaTime;

            if (_timer < timer)
                return ENodeStatus.Running;

            context.TryGetInt("Test", out var value);
#if DEBUG_LOGS
            LogDebug($"Testing sequence node (testOutput={testOutput}, blackboardValue={value}).");
#endif
            return ENodeStatus.Success;
        }
    }
}