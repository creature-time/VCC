
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtNpcSync : UdonSharpBehaviour
    {
        [SerializeField] private NavMeshAgent npcAgent;
        [SerializeField] private Animator npcAnimator;
        [SerializeField] private string forwardVelocityAnimParam = "ForwardVelocity";
        [SerializeField] private string rightVelocityAnimParam = "RightVelocity";

        private int _forwardVelocity;
        private int _rightVelocity;
        private Transform _npcTransform;
        private Vector3 _lastPosition;
        private Vector3 _velocity;

        private void Start()
        {
            _forwardVelocity = Animator.StringToHash(forwardVelocityAnimParam);
            _rightVelocity = Animator.StringToHash(rightVelocityAnimParam);
            _npcTransform = npcAgent.transform;
        }

        private void OnEnable()
        {
            _lastPosition = npcAgent.transform.position;
        }

        private void Update()
        {
            _velocity = Vector3.Lerp(_velocity, (_npcTransform.position - _lastPosition) / Time.deltaTime, 0.95f);
            _lastPosition = _npcTransform.position;

            var direction = _npcTransform.TransformDirection(_velocity / npcAgent.speed);
            npcAnimator.SetFloat(_forwardVelocity, direction.z);
            npcAnimator.SetFloat(_rightVelocity, direction.x);
        }
    }
}