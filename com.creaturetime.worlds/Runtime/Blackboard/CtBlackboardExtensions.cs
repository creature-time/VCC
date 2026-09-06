
using System;

namespace CreatureTime
{
    public static class CtBlackboardExtensions
    {
        public static void SetEnum<T>(this CtBlackboard blackboard, string key, T value)
            where T : Enum
        {
            blackboard.SetInt(key, Convert.ToInt32(value));
        }

        public static bool TryGetEnum<T>(this CtBlackboard blackboard, string key, out T value)
            where T : Enum
        {
            value = default;
            if (blackboard.TryGetInt(key, out var enumValueIndex))
            {
                value = (T)(object)enumValueIndex;
                return true;
            }

            return false;
        }
    }
}