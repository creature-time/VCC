
using UdonSharp;

namespace CreatureTime
{
    public enum EStatusEffectInstancesSignal
    {
        EffectChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtStatusEffectInstances : CtAbstractSignal
    {
        public int Count => 8;

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect0Callback))] private ulong _statusEffect0 = CtDataBlock.InvalidData;

        public ulong StatusEffect0Callback
        {
            get => _statusEffect0;
            set
            {
                _statusEffect0 = value;
                SetArgs.Add(0);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect0
        {
            get => StatusEffect0Callback;
            set
            {
                StatusEffect0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect1Callback))] private ulong _statusEffect1 = CtDataBlock.InvalidData;

        public ulong StatusEffect1Callback
        {
            get => _statusEffect1;
            set
            {
                _statusEffect1 = value;
                SetArgs.Add(1);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect1
        {
            get => StatusEffect1Callback;
            set
            {
                StatusEffect1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect2Callback))] private ulong _statusEffect2 = CtDataBlock.InvalidData;

        public ulong StatusEffect2Callback
        {
            get => _statusEffect2;
            set
            {
                _statusEffect2 = value;
                SetArgs.Add(2);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect2
        {
            get => StatusEffect2Callback;
            set
            {
                StatusEffect2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect3Callback))] private ulong _statusEffect3 = CtDataBlock.InvalidData;

        public ulong StatusEffect3Callback
        {
            get => _statusEffect3;
            set
            {
                _statusEffect3 = value;
                SetArgs.Add(3);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect3
        {
            get => StatusEffect3Callback;
            set
            {
                StatusEffect3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect4Callback))] private ulong _statusEffect4 = CtDataBlock.InvalidData;

        public ulong StatusEffect4Callback
        {
            get => _statusEffect4;
            set
            {
                _statusEffect4 = value;
                SetArgs.Add(4);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect4
        {
            get => StatusEffect4Callback;
            set
            {
                StatusEffect4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect5Callback))] private ulong _statusEffect5 = CtDataBlock.InvalidData;

        public ulong StatusEffect5Callback
        {
            get => _statusEffect5;
            set
            {
                _statusEffect5 = value;
                SetArgs.Add(5);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect5
        {
            get => StatusEffect5Callback;
            set
            {
                StatusEffect5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect6Callback))] private ulong _statusEffect6 = CtDataBlock.InvalidData;

        public ulong StatusEffect6Callback
        {
            get => _statusEffect6;
            set
            {
                _statusEffect6 = value;
                SetArgs.Add(6);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect6
        {
            get => StatusEffect6Callback;
            set
            {
                StatusEffect6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(StatusEffect7Callback))] private ulong _statusEffect7 = CtDataBlock.InvalidData;

        public ulong StatusEffect7Callback
        {
            get => _statusEffect7;
            set
            {
                _statusEffect7 = value;
                SetArgs.Add(7);
                this.Emit(EStatusEffectInstancesSignal.EffectChanged);
            }
        }

        public ulong StatusEffect7
        {
            get => StatusEffect7Callback;
            set
            {
                StatusEffect7Callback = value;
                RequestSerialization();
            }
        }

        // NOTE: If we want to add more status effects to manage, we should probably make a manager that moderates each data block of 8.

        public ulong GetStatusEffect(int index)
        {
            switch (index)
            {
                case 0: return StatusEffect0;
                case 1: return StatusEffect1;
                case 2: return StatusEffect2;
                case 3: return StatusEffect3;
                case 4: return StatusEffect4;
                case 5: return StatusEffect5;
                case 6: return StatusEffect6;
                case 7: return StatusEffect7;
                default: return CtDataBlock.InvalidData;
            }
        }

        public void SetStatusEffect(int index, ulong value)
        {
            switch (index)
            {
                case 0: StatusEffect0 = value; break;
                case 1: StatusEffect1 = value; break;
                case 2: StatusEffect2 = value; break;
                case 3: StatusEffect3 = value; break;
                case 4: StatusEffect4 = value; break;
                case 5: StatusEffect5 = value; break;
                case 6: StatusEffect6 = value; break;
                case 7: StatusEffect7 = value; break;
                default: return;
            }
        }
    }
}