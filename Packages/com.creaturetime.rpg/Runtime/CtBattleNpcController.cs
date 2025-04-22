
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNpcController : CtAbstractSignal
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        [Header("Characteristics")]
        [SerializeField] private CtNpcBrain brain;
        [SerializeField] private CtNpcFeature[] features = {};

        [Header("Skeleton References")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

        public Transform HeadBone => headBone;
        public Transform EyeBoneL => eyeBoneL;
        public Transform EyeBoneR => eyeBoneR;

        private void Update()
        {
            // for (int i = 0; i < features.Length; i++)
            //     features[i].ExecuteUpdate(this);
        }

        private void LateUpdate()
        {
            // for (int i = 0; i < features.Length; i++)
            //     features[i].ExecuteLateUpdate(this);
        }
    }
}