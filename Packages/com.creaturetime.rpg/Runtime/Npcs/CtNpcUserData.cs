
using UdonSharp;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcUserData : CtUserData
    {
        public ushort TargetId { get; set; } = CtConstants.InvalidId;
    }
}