
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBehaviorTreePathFindingTest : CtBehaviorTreeNodeBase
    {
        [SerializeField] private Transform[] wayPoints;
        [SerializeField] private float nextWayPointDistance = 0.5f;

        private int _currentIndex;

        public override void OnEnter(CtNpcContext context)
        {
            _currentIndex = 0;
        }

        public override ENodeStatus Process(CtNpcContext context)
        {
            var agent = context.Agent;
            if (!Networking.IsOwner(agent.gameObject))
                return ENodeStatus.Running;

            var targetPosition = wayPoints[_currentIndex].position;
            agent.SetDestination(targetPosition);
            float distance = Vector3.Distance(targetPosition, context.transform.position);
            if (distance < nextWayPointDistance + agent.radius)
                _currentIndex = (_currentIndex + 1) % wayPoints.Length;
            return ENodeStatus.Running; //_currentIndex == wayPoints.Length ? ENodeStatus.Success : ENodeStatus.Running;
        }
    }
}