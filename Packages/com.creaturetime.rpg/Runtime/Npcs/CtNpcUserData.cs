
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcUserData : CtUserData
    {
        [SerializeField] private CtNpcController npcController;

        public CtNpcController NpcController => npcController;
        public ushort TargetId { get; set; } = CtConstants.InvalidId;

        public void _DamageTrigger()
        {
            npcController.Emit(ECharacterSignal.DamageTrigger);
        }
    }
}