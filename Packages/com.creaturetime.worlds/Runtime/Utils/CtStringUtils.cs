
using System;
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtStringUtils : UdonSharpBehaviour
    {
        public static string GenerateNameWithDateTime(string topic, DateTime dt)
        {
            // TODO: Optimize later?
            topic = topic.Replace(' ', '_');
            topic = topic.Replace('-', '_');
            topic = topic.ToUpper();

            string suffix = GenerateTimeStamp(dt);
            return $"__GENERATED_{topic}_{suffix}";
        }

        public static long GenerateTimeStampId(DateTime dt)
        {
            return dt.ToBinary();
        }

        public static string GenerateTimeStamp(DateTime dt)
        {
            return dt.ToString("yyyy'_'MM'_'dd'T'HH'_'mm'_'ss");
        }
    }
}