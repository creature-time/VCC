
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcPushedState : CtBehaviorTreeNodeBase
    {
        public override ENodeStatus Process(CtNpcContext context)
        {
            var animatorState = context.Animator.GetCurrentAnimatorStateInfo(0);
            if (animatorState.IsName("Shoved"))
            {
                var agent = context.Agent;
                agent.velocity = Vector3.zero;
                agent.SetDestination(agent.transform.position);
                return ENodeStatus.Running;
            }

            return ENodeStatus.Success;
        }
    }
}