

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPathFindingNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private Transform[] wayPoints = {};
        [SerializeField] private float nextWayPointDistance = 0.5f;

        [UdonSynced] private int _currentIndex;

        private int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                if (Networking.IsMaster)
                    RequestSerialization();
            }
        }

        public override void OnEnter(CtNpcContext context)
        {
            CurrentIndex = 0;
        }

        public override ENodeStatus Process(CtNpcContext context)
        {
            if (wayPoints.Length == 0)
                return ENodeStatus.Success;

            var agent = context.Agent;
            if (!Networking.IsOwner(agent.gameObject))
                return ENodeStatus.Running;

            var targetPosition = wayPoints[CurrentIndex].position;
            agent.SetDestination(targetPosition);
            float distance = Vector3.Distance(targetPosition, context.transform.position);
            if (distance < nextWayPointDistance + agent.radius)
                CurrentIndex = (CurrentIndex + 1) % wayPoints.Length;
            return CurrentIndex == wayPoints.Length ? ENodeStatus.Success : ENodeStatus.Running;
        }
    }
}