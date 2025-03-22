
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    public abstract class CtNpcContext : CtBlackboard
    {
        [SerializeField] private CtNpcBrain brain;
        // [SerializeField] private CtEntity entity;
        [SerializeField] private NavMeshAgent agent;

        public CtNpcBrain Brain => brain;
        // public CtEntity Entity => entity;
        public NavMeshAgent Agent => agent;
        public Transform Transform => gameObject.transform;

        public abstract CtBehaviorTreeNodeBase[] GetNodes();
    }
}