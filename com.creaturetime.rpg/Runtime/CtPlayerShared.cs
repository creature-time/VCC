
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Ui
{
    public enum EPlayerSharedSignal
    {
        EnemyTargetChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerShared : CtAbstractSignal
    {
        [SerializeField] private CtSelectionModel selectionModel;

        [UdonSynced, FieldChangeCallback(nameof(EnemyTargetChanged))]
        private ushort _enemyTarget;

        public ushort EnemyTargetChanged
        {
            get => _enemyTarget;
            set
            {
                _enemyTarget = value;

                LogDebug($"Player target changed (enemyTarget={_enemyTarget}");

                SetArgs.Add(_enemyTarget);
                this.Emit(EPlayerSharedSignal.EnemyTargetChanged);
            }
        }

        private ushort EnemyTarget
        {
            get => EnemyTargetChanged;
            set
            {
                EnemyTargetChanged = value;
                RequestSerialization();
            }
        }

        private void Start()
        {
            selectionModel.Connect(ESelectionModelSignal.SelectionChanged, this, nameof(_OnSelectionChanged));
        }

        public void _OnSelectionChanged()
        {
            // var prev = GetArgs[0].DataList;
            var curr = GetArgs[1].DataList;

            EnemyTarget = curr.Count > 0 ? curr[0].UShort : CtConstants.InvalidId;
        }

        public void Reset()
        {
            EnemyTarget = CtConstants.InvalidId;
        }
    }
}