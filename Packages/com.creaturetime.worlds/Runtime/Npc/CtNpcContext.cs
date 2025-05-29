
using UnityEngine;
using UnityEngine.AI;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public abstract class CtNpcContext : CtBlackboard
    {
        // [SerializeField] private CtNpcBrain brain;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        // public CtNpcBrain Brain => brain;
        public NavMeshAgent Agent => agent;
        public Animator Animator => animator;
        public Transform Transform => agent.transform;
    }
}