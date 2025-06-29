
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
        [UdonSynced] private ushort _skillId = CtConstants.InvalidId;
        [UdonSynced] private ushort _targetIndex = CtConstants.InvalidId;

        public CTBattleInteractType InteractType => _interactType;

        public void Submit(CTBattleInteractType interactType, ushort skillId, ushort targetIndex)
        {
#if DEBUG_LOGS
            LogDebug($"(interactType={interactType}, skillIndex={skillIndex}, targetIndex={targetIndex})");
#endif

            _interactType = interactType;
            _skillId = skillId;
            _targetIndex = targetIndex;
            RequestSerialization();

            OnDeserialization();
        }

        public void ResetToWait()
        {
#if DEBUG_LOGS
            LogDebug("ResetToWait");
#endif

            _interactType = CTBattleInteractType.Waiting;
            _skillId = CtConstants.InvalidId;
            _targetIndex = CtConstants.InvalidId;
            RequestSerialization();

            OnDeserialization();
        }

        public void Reset()
        {
            _interactType = CTBattleInteractType.None;
            _skillId = CtConstants.InvalidId;
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

        public bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            skillId = CtConstants.InvalidId;
            targetId = CtConstants.InvalidId;;

            switch (InteractType)
            {
                case CTBattleInteractType.None:
                case CTBattleInteractType.Waiting:
                    return false;
            }

            skillId = _skillId;
            targetId = _targetIndex;
            return true;
        }
    }
}