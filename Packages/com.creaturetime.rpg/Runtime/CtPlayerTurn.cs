
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
    public class CtPlayerTurn : CtLoggerUdonScript
    {
        [UdonSynced] private CTBattleInteractType _interactType = CTBattleInteractType.None;
        [UdonSynced] private int _skillIndex = -1;
        [UdonSynced] private ushort _targetIndex = CtConstants.InvalidId;

        public CTBattleInteractType InteractType => _interactType;
        // public int SkillIndex => _skillIndex;
        // public int TargetIndex => _targetIndex;

        public void Submit(CTBattleInteractType interactType, int skillIndex, ushort targetIndex)
        {
#if DEBUG_LOGS
            LogDebug($"(interactType={interactType}, skillIndex={skillIndex}, targetIndex={targetIndex})");
#endif

            _interactType = interactType;
            _skillIndex = skillIndex;
            _targetIndex = targetIndex;
            RequestSerialization();

            OnDeserialization();
        }

        public void ResetToWait()
        {
#if DEBUG_LOGS
            LogDebug("Reset");
#endif

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
#if DEBUG_LOGS
            LogDebug("OnDeserialization " +
                $"(interactType={InteractType}, skillIndex={_skillIndex}, targetIndex={_targetIndex}).");
#endif
        }

        public bool TryGetAttack(out int skillIndex, out ushort targetId)
        {
            skillIndex = -1;
            targetId = CtConstants.InvalidId;;
            if (InteractType == CTBattleInteractType.Waiting)
                return false;

            if (InteractType == CTBattleInteractType.None)
                return false;

            skillIndex = _skillIndex;
            targetId = _targetIndex;
            return true;
        }
    }
}