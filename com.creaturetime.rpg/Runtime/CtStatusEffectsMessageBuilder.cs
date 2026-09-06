
using UdonSharp;

namespace CreatureTime
{
    enum EStatusEffectBlockSignal
    {
        DamageApplied
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtStatusEffectsMessageBuilder : CtAbstractSignal
    {
        private const int MaxCount = 32;

        [UdonSynced] private ushort[] _damageSourceType = new ushort[MaxCount];
        [UdonSynced] private ushort[] _identifier = new ushort[MaxCount];
        [UdonSynced] private ushort[] _target = new ushort[MaxCount];
        [UdonSynced] private ushort[] _source = new ushort[MaxCount];
        [UdonSynced] private ushort[] _damageType = new ushort[MaxCount];
        [UdonSynced] private int[] _damage = new int[MaxCount];
        [UdonSynced] private bool[] _isCritical = new bool[MaxCount];

        [UdonSynced] private int _count = 0;

        public void SetHeader()
        {
#if DEBUG_LOGS
            LogDebug($"Setting header...");
#endif

            _count = 0;
        }

        public void AddDamageCommand(EDamageSourceType damageSourceType, ushort skillId, ushort targetId,
            ushort sourceId, EDamageType damageType, int damage, bool isCritical)
        {
            if (_count == MaxCount)
                return;

            var dst = (int)damageSourceType;
            var dt = (int)damageType;
            _damageSourceType[_count] = (ushort)dst;
            _identifier[_count] = skillId;
            _target[_count] = targetId;
            _source[_count] = sourceId;
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

            RequestSerialization();
            OnDeserialization();
        }

        public override void OnDeserialization()
        {
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
                SetArgs.Add(_source[i]);
                SetArgs.Add(_target[i]);
                SetArgs.Add(_damageType[i]);
                SetArgs.Add(_damage[i]);
                SetArgs.Add(_isCritical[i]);
                this.Emit(EStatusEffectBlockSignal.DamageApplied);
            }
        }
    }
}