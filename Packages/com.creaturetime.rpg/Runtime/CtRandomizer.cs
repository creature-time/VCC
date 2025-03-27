
using UnityEngine;

namespace CreatureTime
{
    public static class CtRandomizer
    {
        private const double MultiplierOffset = 100000.0;

        public static double GetDoubleValue(double max)
        {
            return Random.Range(0, (int)(max * MultiplierOffset)) / MultiplierOffset;
        }

        public static double GetDoubleValue(double min, double max)
        {
            return Random.Range((int)(min * MultiplierOffset), (int)(max * MultiplierOffset)) / MultiplierOffset;
        }

        public static int GetIntValue(int max)
        {
            return Random.Range(0, max);
        }

        public static int GetIntValue(int min, int max)
        {
            return Random.Range(min, max);
        }

        public static int[] RollDice(int count, int sidesPerDice)
        {
            int[] results = new int[count];
            for (int i = 0; i < count; ++i)
                results[i] = GetIntValue(sidesPerDice);
            return results;
        }

        public static bool IsPercentHit(double percent)
        {
            return GetDoubleValue(1.0) <= percent;
        }
    }
}