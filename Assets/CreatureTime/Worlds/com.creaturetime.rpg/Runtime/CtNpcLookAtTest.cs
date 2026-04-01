
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcLookAtTest : UdonSharpBehaviour
    {
        [SerializeField] private CtNpcController npcController;

        public override void OnPickupUseDown()
        {
            npcController.LookTarget = npcController.LookTarget ? null : transform;
        }

        public override void OnDrop()
        {
            npcController.LookTarget = null;
        }
    }
}