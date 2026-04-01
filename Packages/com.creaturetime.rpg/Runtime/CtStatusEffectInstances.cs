
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EStatusEffectInstancesSignal
    {
        EffectChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtStatusEffectInstances : CtAbstractSignal
    {
        [SerializeField] private CtEntity entity;
        [SerializeField] private CtStatusEffectInstance[] instances;

        public CtEntity Entity => entity;

        public int Count => instances.Length * 8;

        public void OnStatusEffectChanged(CtStatusEffectInstance instance, int index)
        {
            index = Array.IndexOf(instances, instance) * 8 + index;

            SetArgs.Add(index);
            this.Emit(EStatusEffectInstancesSignal.EffectChanged);
        }

        public ulong GetStatusEffect(int index) => instances[index / 8].GetStatusEffect(index % 8);

        public void SetStatusEffect(int index, ulong value)
        {
#if DEBUG_LOGS
            var identifier = CtDataBlock.GetEffectIdentifier(value);
            var turns = CtDataBlock.GetEffectTurns(value);
            LogDebug($"Settings status effect (index={index}, identifier={identifier}, turns={turns}).");
#endif

            if (index >= Count)
            {
#if DEBUG_LOGS
                LogCritical($"Out of range for status effects (index={index}, maxCount={Count}).");
#endif
                return;
            }

            instances[index / 8].SetStatusEffect(index % 8, value);
        }
    }
}