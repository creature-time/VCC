
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcUserData : CtUserData
    {
        [FormerlySerializedAs("npcController")] [SerializeField] private CtBattleController controller;

        public CtBattleController Controller => controller;
        public ushort TargetId { get; set; } = CtConstants.InvalidId;

        public void _DamageTrigger()
        {
            controller.Emit(ENpcBattleControllerSignal.DamageTrigger);
        }
    }
}