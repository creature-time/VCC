
using UnityEngine;
using UnityEngine.AI;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public abstract class CtNpcContext : CtBlackboard
    {
        // [SerializeField] private CtNpcBrain brain;
        [SerializeField] private NavMeshAgent agent;

        // public CtNpcBrain Brain => brain;
        public NavMeshAgent Agent => agent;
        public Transform Transform => agent.transform;

        // TODO: Change node to actions that modify the blackboard with given information from the expert.
        public abstract CtBehaviorTreeNodeBase[] GetActions();
    }
}