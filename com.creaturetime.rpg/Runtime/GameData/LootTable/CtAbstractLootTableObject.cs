
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractLootTableObject : CtAbstractSignal
    {
        [SerializeField] private float probability;
        [SerializeField] private bool unique;
        [SerializeField] private bool always;
        [SerializeField] private bool rollEnabled = true;

        public abstract bool IsNull { get; }
        public float Probability => probability;
        public bool Unique => unique;
        public bool Always => always;
        public bool RollEnabled => rollEnabled;

        public virtual ulong CreateInstance() => CtDataBlock.InvalidData;
    }
}