
using UdonSharp;

namespace CreatureTime
{
    public enum CTBattleInteractType
    {
        Waiting = 1,
        Attack = 2,
        Leave = 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerTurn : UdonSharpBehaviour
    {
        [UdonSynced] private CTBattleInteractType _interactType = CTBattleInteractType.Waiting;
        [UdonSynced] private int _skillIndex = -1;
        [UdonSynced] private int _targetIndex = -1;

        public CTBattleInteractType InteractType => _interactType;
        // public int SkillIndex => _skillIndex;
        // public int TargetIndex => _targetIndex;

        public void Submit(CTBattleInteractType interactType, int skillIndex, int targetIndex)
        {
            CtLogger.LogDebug("Player Turn", 
                $"(interactType={interactType}, skillIndex={skillIndex}, targetIndex={targetIndex})");

            _interactType = interactType;
            _skillIndex = skillIndex;
            _targetIndex = targetIndex;
            RequestSerialization();

            OnDeserialization();
        }

        public void Reset()
        {
            CtLogger.LogDebug("Player Turn", "Reset");

            _interactType = CTBattleInteractType.Waiting;
            _skillIndex = -1;
            _targetIndex = -1;
            RequestSerialization();

            OnDeserialization();
        }

        public override void OnDeserialization()
        {
            CtLogger.LogDebug("Player Turn",
                "OnDeserialization " +
                $"(interactType={InteractType}, skillIndex={_skillIndex}, targetIndex={_targetIndex}).");
        }

        public bool TryGetAttack(out int skillIndex, out int targetId)
        {
            skillIndex = -1;
            targetId = -1;
            if (InteractType == CTBattleInteractType.Waiting)
                return false;

            skillIndex = _skillIndex;
            targetId = _targetIndex;
            return true;
        }
    }
}