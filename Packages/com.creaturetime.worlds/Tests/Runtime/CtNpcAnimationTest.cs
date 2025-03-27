
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcAnimationTest : CtLoggerUdonScript
    {
        [SerializeField] private CtNpcManager npcManager;
        [SerializeField] private NavMeshAgent npcAgent;
        [SerializeField] private Animator npcAnimator;
        [SerializeField] private string forwardVelocityAnimParam = "ForwardVelocity";
        [SerializeField] private string rightVelocityAnimParam = "RightVelocity";

        [SerializeField] private float packetsPerSecond = 10f;
        [SerializeField] private int maxSnapshotCount = 32;
        [SerializeField] private float oldestSnapshotTimeInSeconds = 1f;

        private int _forwardVelocity;
        private int _rightVelocity;
        private Transform _npcTransform;

        private void Start()
        {
            _forwardVelocity = Animator.StringToHash(forwardVelocityAnimParam);
            _rightVelocity = Animator.StringToHash(rightVelocityAnimParam);
            _npcTransform = npcAgent.transform;
        }

        private void Update()
        {
            var direction = _npcTransform.InverseTransformDirection(npcAgent.velocity / npcAgent.speed);
            npcAnimator.SetFloat(_forwardVelocity, direction.z);
            npcAnimator.SetFloat(_rightVelocity, direction.x);
        }
    }
}