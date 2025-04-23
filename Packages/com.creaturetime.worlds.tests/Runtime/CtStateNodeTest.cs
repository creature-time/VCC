using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtStateNodeTest : CtStateBase
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
            Log($"Testing state (testOutput={testOutput}, blackboardValue={value}).");
            return ENodeStatus.Success;
        }
    }
}