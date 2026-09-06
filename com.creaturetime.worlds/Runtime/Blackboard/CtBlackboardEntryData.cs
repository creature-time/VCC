
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBlackboardEntryData : UdonSharpBehaviour
    {
        public CtBlackboardEntryDataType[] entries;

        public void SetValueOnBlackboard(CtBlackboardData blackboard)
        {
            foreach (var blackboardEntry in entries)
                blackboardEntry.SetValueOnBlackboard(blackboard);
        }
    }

    public static class CtBlackboardEntryDataExtensions
    {
        public static void SetupBlackboard(this CtBlackboard blackboard, CtBlackboardEntryData entryData)
        {
            foreach (var entry in entryData.entries)
            {
                switch (entry.Type)
                {
                    case EBlackboardEntryDataType.Bool:
                        blackboard.SetValue(entry.Key, entry.Bool);
                        break;
                    case EBlackboardEntryDataType.Short:
                        blackboard.SetValue(entry.Key, entry.Short);
                        break;
                    case EBlackboardEntryDataType.UShort:
                        blackboard.SetValue(entry.Key, entry.UShort);
                        break;
                    case EBlackboardEntryDataType.Int:
                        blackboard.SetValue(entry.Key, entry.Int);
                        break;
                    case EBlackboardEntryDataType.UInt:
                        blackboard.SetValue(entry.Key, entry.UInt);
                        break;
                    case EBlackboardEntryDataType.Long:
                        blackboard.SetValue(entry.Key, entry.Long);
                        break;
                    case EBlackboardEntryDataType.ULong:
                        blackboard.SetValue(entry.Key, entry.ULong);
                        break;
                    case EBlackboardEntryDataType.Float:
                        blackboard.SetValue(entry.Key, entry.Float);
                        break;
                }
            }
        }
    }
}