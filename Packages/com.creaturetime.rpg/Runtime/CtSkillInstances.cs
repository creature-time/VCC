
using UdonSharp;

namespace CreatureTime
{
    public enum ESkillInstancesSignal
    {
        SkillRechargeChanged,
        SkillAdrenalineChanged,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtSkillInstances : CtAbstractSignal
    {
        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge0Callback))] private char _skillRecharge0;

        public char SkillRecharge0Callback
        {
            get => _skillRecharge0;
            set
            {
                _skillRecharge0 = value;
                SetArgs.Add(0);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge0
        {
            get => SkillRecharge0Callback;
            set
            {
                SkillRecharge0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge1Callback))] private char _skillRecharge1;

        public char SkillRecharge1Callback
        {
            get => _skillRecharge1;
            set
            {
                _skillRecharge1 = value;
                SetArgs.Add(1);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge1
        {
            get => SkillRecharge1Callback;
            set
            {
                SkillRecharge1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge2Callback))] private char _skillRecharge2;

        public char SkillRecharge2Callback
        {
            get => _skillRecharge2;
            set
            {
                _skillRecharge2 = value;
                SetArgs.Add(2);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge2
        {
            get => SkillRecharge2Callback;
            set
            {
                SkillRecharge2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge3Callback))] private char _skillRecharge3;

        public char SkillRecharge3Callback
        {
            get => _skillRecharge3;
            set
            {
                _skillRecharge3 = value;
                SetArgs.Add(3);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge3
        {
            get => SkillRecharge3Callback;
            set
            {
                SkillRecharge3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge4Callback))] private char _skillRecharge4;

        public char SkillRecharge4Callback
        {
            get => _skillRecharge4;
            set
            {
                _skillRecharge4 = value;
                SetArgs.Add(4);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge4
        {
            get => SkillRecharge4Callback;
            set
            {
                SkillRecharge4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge5Callback))] private char _skillRecharge5;

        public char SkillRecharge5Callback
        {
            get => _skillRecharge5;
            set
            {
                _skillRecharge5 = value;
                SetArgs.Add(5);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge5
        {
            get => SkillRecharge5Callback;
            set
            {
                SkillRecharge5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge6Callback))] private char _skillRecharge6;

        public char SkillRecharge6Callback
        {
            get => _skillRecharge6;
            set
            {
                _skillRecharge6 = value;
                SetArgs.Add(6);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge6
        {
            get => SkillRecharge6Callback;
            set
            {
                SkillRecharge6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge7Callback))] private char _skillRecharge7;

        public char SkillRecharge7Callback
        {
            get => _skillRecharge7;
            set
            {
                _skillRecharge0 = value;
                SetArgs.Add(7);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge7
        {
            get => SkillRecharge7Callback;
            set
            {
                SkillRecharge7Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge8Callback))] private char _skillRecharge8;

        public char SkillRecharge8Callback
        {
            get => _skillRecharge8;
            set
            {
                _skillRecharge8 = value;
                SetArgs.Add(8);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge8
        {
            get => SkillRecharge8Callback;
            set
            {
                SkillRecharge8Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillRecharge9Callback))] private char _skillRecharge9;

        public char SkillRecharge9Callback
        {
            get => _skillRecharge9;
            set
            {
                _skillRecharge9 = value;
                SetArgs.Add(9);
                this.Emit(ESkillInstancesSignal.SkillRechargeChanged);
            }
        }

        public char SkillRecharge9
        {
            get => SkillRecharge9Callback;
            set
            {
                SkillRecharge9Callback = value;
                RequestSerialization();
            }
        }

        public int GetRecharge(int index)
        {
            switch (index)
            {
                case 0: return SkillRecharge0;
                case 1: return SkillRecharge1;
                case 2: return SkillRecharge2;
                case 3: return SkillRecharge3;
                case 4: return SkillRecharge4;
                case 5: return SkillRecharge5;
                case 6: return SkillRecharge6;
                case 7: return SkillRecharge7;
                case 8: return SkillRecharge8;
                case 9: return SkillRecharge9;
                default: return CtConstants.InvalidId;
            }
        }

        public void SetRecharge(int index, int value)
        {
            switch (index)
            {
                case 0: SkillRecharge0 = (char)value; return;
                case 1: SkillRecharge1 = (char)value; return;
                case 2: SkillRecharge2 = (char)value; return;
                case 3: SkillRecharge3 = (char)value; return;
                case 4: SkillRecharge4 = (char)value; return;
                case 5: SkillRecharge5 = (char)value; return;
                case 6: SkillRecharge6 = (char)value; return;
                case 7: SkillRecharge7 = (char)value; return;
                case 8: SkillRecharge8 = (char)value; return;
                case 9: SkillRecharge9 = (char)value; return;
                default: return;
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline0Callback))] private short _skillAdrenaline0;

        public short SkillAdrenaline0Callback
        {
            get => _skillAdrenaline0;
            set
            {
                _skillAdrenaline0 = value;
                SetArgs.Add(0);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline0
        {
            get => SkillAdrenaline0Callback;
            set
            {
                SkillAdrenaline0Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline1Callback))] private short _skillAdrenaline1;

        public short SkillAdrenaline1Callback
        {
            get => _skillAdrenaline1;
            set
            {
                _skillAdrenaline1 = value;
                SetArgs.Add(1);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline1
        {
            get => SkillAdrenaline1Callback;
            set
            {
                SkillAdrenaline1Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline2Callback))] private short _skillAdrenaline2;

        public short SkillAdrenaline2Callback
        {
            get => _skillAdrenaline2;
            set
            {
                _skillAdrenaline2 = value;
                SetArgs.Add(2);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline2
        {
            get => SkillAdrenaline2Callback;
            set
            {
                SkillAdrenaline2Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline3Callback))] private short _skillAdrenaline3;

        public short SkillAdrenaline3Callback
        {
            get => _skillAdrenaline3;
            set
            {
                _skillAdrenaline3 = value;
                SetArgs.Add(3);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline3
        {
            get => SkillAdrenaline3Callback;
            set
            {
                SkillAdrenaline3Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline4Callback))] private short _skillAdrenaline4;

        public short SkillAdrenaline4Callback
        {
            get => _skillAdrenaline4;
            set
            {
                _skillAdrenaline4 = value;
                SetArgs.Add(4);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline4
        {
            get => SkillAdrenaline4Callback;
            set
            {
                SkillAdrenaline4Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline5Callback))] private short _skillAdrenaline5;

        public short SkillAdrenaline5Callback
        {
            get => _skillAdrenaline5;
            set
            {
                _skillAdrenaline5 = value;
                SetArgs.Add(5);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline5
        {
            get => SkillAdrenaline5Callback;
            set
            {
                SkillAdrenaline5Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline6Callback))] private short _skillAdrenaline6;

        public short SkillAdrenaline6Callback
        {
            get => _skillAdrenaline6;
            set
            {
                _skillAdrenaline6 = value;
                SetArgs.Add(6);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline6
        {
            get => SkillAdrenaline6Callback;
            set
            {
                SkillAdrenaline6Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline7Callback))] private short _skillAdrenaline7;

        public short SkillAdrenaline7Callback
        {
            get => _skillAdrenaline7;
            set
            {
                _skillAdrenaline0 = value;
                SetArgs.Add(7);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline7
        {
            get => SkillAdrenaline7Callback;
            set
            {
                SkillAdrenaline7Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline8Callback))] private short _skillAdrenaline8;

        public short SkillAdrenaline8Callback
        {
            get => _skillAdrenaline8;
            set
            {
                _skillAdrenaline8 = value;
                SetArgs.Add(8);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline8
        {
            get => SkillAdrenaline8Callback;
            set
            {
                SkillAdrenaline8Callback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SkillAdrenaline9Callback))] private short _skillAdrenaline9;

        public short SkillAdrenaline9Callback
        {
            get => _skillAdrenaline9;
            set
            {
                _skillAdrenaline9 = value;
                SetArgs.Add(9);
                this.Emit(ESkillInstancesSignal.SkillAdrenalineChanged);
            }
        }

        public short SkillAdrenaline9
        {
            get => SkillAdrenaline9Callback;
            set
            {
                SkillAdrenaline9Callback = value;
                RequestSerialization();
            }
        }

        public int GetAdrenaline(int index)
        {
            switch (index)
            {
                case 0: return SkillAdrenaline0;
                case 1: return SkillAdrenaline1;
                case 2: return SkillAdrenaline2;
                case 3: return SkillAdrenaline3;
                case 4: return SkillAdrenaline4;
                case 5: return SkillAdrenaline5;
                case 6: return SkillAdrenaline6;
                case 7: return SkillAdrenaline7;
                case 8: return SkillAdrenaline8;
                case 9: return SkillAdrenaline9;
                default: return -1;
            }
        }

        public void SetAdrenaline(int index, int value)
        {
            switch (index)
            {
                case 0: SkillAdrenaline0 = (short)value; return;
                case 1: SkillAdrenaline1 = (short)value; return;
                case 2: SkillAdrenaline2 = (short)value; return;
                case 3: SkillAdrenaline3 = (short)value; return;
                case 4: SkillAdrenaline4 = (short)value; return;
                case 5: SkillAdrenaline5 = (short)value; return;
                case 6: SkillAdrenaline6 = (short)value; return;
                case 7: SkillAdrenaline7 = (short)value; return;
                case 8: SkillAdrenaline8 = (short)value; return;
                case 9: SkillAdrenaline9 = (short)value; return;
                default: return;
            }
        }
    }
}