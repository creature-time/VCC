﻿
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBehaviorTreeTest : CtBehaviorTreeNode
    {
        [SerializeField] private int testOutput;
        [SerializeField] private float timer = 2f;

        private float _timer;

        public override void OnEnter(CtNpcContext context)
        {
            _timer = 0;
        }

        public override ENodeStatus Process(CtNpcContext context)
        {
            if (_timer < timer)
            {
                _timer += Time.deltaTime;
                if (_timer >= timer)
                {
                    _timer = timer;

                    context.TryGetInt("Test", out var value);
#if DEBUG_LOGS
                    Log($"Testing behavior tree node (testOutput={testOutput}, blackboardValue={value}).");
#endif
                    return ENodeStatus.Success;
                }
            }
            else
            {
                return ENodeStatus.Success;
            }

            return ENodeStatus.Running;
        }
    }
}