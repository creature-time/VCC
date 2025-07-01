
using System;
using UdonSharp;

namespace CreatureTime
{
    enum EDamageBlockSignal
    {
        DamageSource,
        DamageApplied
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtDamageMessageBuilder : CtAbstractSignal
    {
        private const int MaxCount = 32;

        [UdonSynced] private long _timestamp;

        [UdonSynced] private ushort _sourceId = CtConstants.InvalidId;
        [UdonSynced] private ushort _targetId = CtConstants.InvalidId;
        [UdonSynced] private ushort _skillId = CtConstants.InvalidId;

        [UdonSynced] private ushort[] _damageSourceType = new ushort[MaxCount];
        [UdonSynced] private ushort[] _identifier = new ushort[MaxCount];
        [UdonSynced] private ushort[] _target = new ushort[MaxCount];
        [UdonSynced] private ushort[] _damageType = new ushort[MaxCount];
        [UdonSynced] private int[] _damage = new int[MaxCount];
        [UdonSynced] private bool[] _isCritical = new bool[MaxCount];

        [UdonSynced] private int _count = 0;

        public void SetHeader(ushort sourceId, ushort targetId, ushort skillId)
        {
#if DEBUG_LOGS
            LogDebug($"Setting header... (sourceId={sourceId}, targetId={targetId}, skillId={skillId})");
#endif

            _sourceId = sourceId;
            _targetId = targetId;
            _skillId = skillId;

            _count = 0;
        }

        public void AddDamageCommand(EDamageSourceType damageSourceType, ushort skillId,
            ushort targetId, EDamageType damageType, int damage, bool isCritical)
        {
            if (_count == MaxCount)
                return;

            var dst = (int)damageSourceType;
            var dt = (int)damageType;
            _damageSourceType[_count] = (ushort)dst;
            _identifier[_count] = skillId;
            _target[_count] = targetId;
            _damageType[_count] = (ushort)dt;
            _damage[_count] = damage;
            _isCritical[_count] = isCritical;

            _count++;
        }

        public void CommitDamage()
        {
#if DEBUG_LOGS
            LogDebug("Commiting damage block...");
#endif

            _timestamp = DateTime.Now.ToBinary();
            RequestSerialization();
            OnDeserialization();
        }

        public override void OnDeserialization()
        {
#if DEBUG_LOGS
            LogDebug("Sending damage block header... " +
                     $"(timestamp={_timestamp}, sourceId={_sourceId}, targetId={_targetId}, skillId={_skillId})");
#endif

            SetArgs.Add(_sourceId);
            SetArgs.Add(_targetId);
            SetArgs.Add(_skillId);
            this.Emit(EDamageBlockSignal.DamageSource);

#if DEBUG_LOGS
            LogDebug($"Sending damage blocks... (count={_count})");
#endif

            for (int i = 0; i < _count; ++i)
            {
#if DEBUG_LOGS
                LogDebug("Sending damage block...");
#endif

                SetArgs.Add(_damageSourceType[i]);
                SetArgs.Add(_identifier[i]);
                SetArgs.Add(_sourceId);
                SetArgs.Add(_target[i]);
                SetArgs.Add(_damageType[i]);
                SetArgs.Add(_damage[i]);
                SetArgs.Add(_isCritical[i]);
                this.Emit(EDamageBlockSignal.DamageApplied);
            }
        }
    }
}