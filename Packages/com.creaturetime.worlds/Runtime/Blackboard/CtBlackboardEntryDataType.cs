
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EBlackboardEntryDataType
    {
        Bool,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBlackboardEntryDataType : CtLoggerUdonScript
    {
        [SerializeField] private string key;
        [SerializeField] private EBlackboardEntryDataType type;

        [SerializeField] private bool valueBool;
        [SerializeField] private short valueShort;
        [SerializeField] private ushort valueUShort;
        [SerializeField] private int valueInt;
        [SerializeField] private uint valueUInt;
        [SerializeField] private long valueLong;
        [SerializeField] private ulong valueULong;
        [SerializeField] private float valueFloat;

        public string Key => key;
        public EBlackboardEntryDataType Type => type;

        public bool Bool
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return valueBool;
                    case EBlackboardEntryDataType.Short:
                        return valueShort != 0;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort != 0;
                    case EBlackboardEntryDataType.Int:
                        return valueInt != 0;
                    case EBlackboardEntryDataType.UInt:
                        return valueUInt != 0;
                    case EBlackboardEntryDataType.Long:
                        return valueLong != 0;
                    case EBlackboardEntryDataType.ULong:
                        return valueULong != 0;
                    case EBlackboardEntryDataType.Float:
                        return valueFloat != 0;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return false;
                }
            }
            set
            {
                valueBool = value;
                type = EBlackboardEntryDataType.Bool;
            }
        }

        public short Short
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return (short)(valueBool ? 1 : 0);
                    case EBlackboardEntryDataType.Short:
                        return valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return (short)valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return (short)valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return (short)valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return (short)valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return (short)valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (short)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueShort = (short)Mathf.Clamp(value, short.MinValue, short.MaxValue);
                type = EBlackboardEntryDataType.Int;
            }
        }

        public ushort UShort
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return (ushort)(valueBool ? 1 : 0);
                    case EBlackboardEntryDataType.Short:
                        return (ushort)valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return (ushort)valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return (ushort)valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return (ushort)valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return (ushort)valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (ushort)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueUShort = (ushort)Mathf.Clamp(value, ushort.MinValue, ushort.MaxValue);
                type = EBlackboardEntryDataType.UInt;
            }
        }

        public int Int
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return valueBool ? 1 : 0;
                    case EBlackboardEntryDataType.Short:
                        return valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return (int)valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return (int)valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return (int)valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (int)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueInt = value;
                type = EBlackboardEntryDataType.Int;
            }
        }

        public uint UInt
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return (uint)(valueBool ? 1 : 0);
                    case EBlackboardEntryDataType.Short:
                        return (uint)valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return (uint)valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return (uint)valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return (uint)valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (uint)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueUInt = value;
                type = EBlackboardEntryDataType.UInt;
            }
        }

        public long Long
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return valueBool ? 1 : 0;
                    case EBlackboardEntryDataType.Short:
                        return valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return (long)valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (long)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueLong = value;
                type = EBlackboardEntryDataType.Int;
            }
        }

        public ulong ULong
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return (ulong)(valueBool ? 1 : 0);
                    case EBlackboardEntryDataType.Short:
                        return (ulong)valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return (ulong)valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return (ulong)valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return valueULong;
                    case EBlackboardEntryDataType.Float:
                        return (ulong)valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueULong = value;
                type = EBlackboardEntryDataType.UInt;
            }
        }

        public float Float
        {
            get
            {
                switch (type)
                {
                    case EBlackboardEntryDataType.Bool:
                        return valueBool ? 1 : 0;
                    case EBlackboardEntryDataType.Short:
                        return valueShort;
                    case EBlackboardEntryDataType.UShort:
                        return valueUShort;
                    case EBlackboardEntryDataType.Int:
                        return valueInt;
                    case EBlackboardEntryDataType.UInt:
                        return valueUInt;
                    case EBlackboardEntryDataType.Long:
                        return valueLong;
                    case EBlackboardEntryDataType.ULong:
                        return valueULong;
                    case EBlackboardEntryDataType.Float:
                        return valueFloat;
                    default:
#if DEBUG_LOGS
                        LogCritical("Blackboard Entry Date Type",
                            $"Unknown blackboard entry data type (type={type}).");
#endif
                        return 0;
                }
            }
            set
            {
                valueFloat = value;
                type = EBlackboardEntryDataType.Float;
            }
        }

        public void SetValueOnBlackboard(CtBlackboardData blackboard)
        {
            switch (Type)
            {
                case EBlackboardEntryDataType.Bool:
                    blackboard.SetValue(Key, Bool);
                    break;
                case EBlackboardEntryDataType.Int:
                    blackboard.SetValue(Key, Int);
                    break;
                case EBlackboardEntryDataType.UInt:
                    blackboard.SetValue(Key, UInt);
                    break;
                case EBlackboardEntryDataType.Float:
                    blackboard.SetValue(Key, Float);
                    break;
                default:
#if DEBUG_LOGS
                    LogCritical("Blackboard Entry Date Type",
                        $"Unknown blackboard entry data type (type={type}).");
#endif
                    break;
            }
        }
    }
}