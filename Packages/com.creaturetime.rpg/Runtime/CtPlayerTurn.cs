
using UdonSharp;

namespace CreatureTime
{
    public enum CTBattleInteractType
    {
        None = 0,
        Waiting = 1,
        Attack = 2,
        Leave = 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerTurn : UdonSharpBehaviour
    {
        [UdonSynced] private CTBattleInteractType _interactType = CTBattleInteractType.None;
        [UdonSynced] private int _skillIndex = -1;
        [UdonSynced] private ushort _targetIndex = CtConstants.InvalidId;

        public CTBattleInteractType InteractType => _interactType;
        // public int SkillIndex => _skillIndex;
        // public int TargetIndex => _targetIndex;

        public void Submit(CTBattleInteractType interactType, int skillIndex, ushort targetIndex)
        {
            CtLogger.LogDebug("Player Turn", 
                $"(interactType={interactType}, skillIndex={skillIndex}, targetIndex={targetIndex})");

            _interactType = interactType;
            _skillIndex = skillIndex;
            _targetIndex = targetIndex;
            RequestSerialization();

            OnDeserialization();
        }

        public void ResetToWait()
        {
            CtLogger.LogDebug("Player Turn", "Reset");

            _interactType = CTBattleInteractType.Waiting;
            _skillIndex = -1;
            _targetIndex = CtConstants.InvalidId;
            RequestSerialization();

            OnDeserialization();
        }

        public void Reset()
        {
            _interactType = CTBattleInteractType.None;
            _skillIndex = -1;
            _targetIndex = CtConstants.InvalidId;
            RequestSerialization();

            OnDeserialization();
        }

        public override void OnDeserialization()
        {
            CtLogger.LogDebug("Player Turn",
                "OnDeserialization " +
                $"(interactType={InteractType}, skillIndex={_skillIndex}, targetIndex={_targetIndex}).");
        }

        public bool TryGetAttack(out int skillIndex, out ushort targetId)
        {
            skillIndex = -1;
            targetId = CtConstants.InvalidId;;
            if (InteractType == CTBattleInteractType.Waiting)
                return false;

            skillIndex = _skillIndex;
            targetId = _targetIndex;
            return true;
        }
    }
}