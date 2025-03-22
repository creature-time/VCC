
using UnityEngine;
using UdonSharp;
using Random = System.Random;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public static class CtColorUtils
    {
        private static float HashString(string value)
        {
            return (float)new Random(CtEncodingUtils.Encode(value)).NextDouble();
        }

        public static Color RandomColor(float h, float saturation = 0.8f)
        {
            const float goldenRatioConjugate = 0.618034f;

            float value = 0.8f;
            float hh = goldenRatioConjugate * h % 1.0f;
            return Color.HSVToRGB(hh, saturation, value, false);
        }

        public static Color RandomStringColor(string value, float saturation = 8.0f)
        {
            return RandomColor(HashString(value), saturation);
        }

        public static string RandomStringColorHex(string value, float saturation = 8)
        {
            Color color = RandomStringColor(value, saturation);
            return $"#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(int)(f * 255.0f);
        }

        public static Color Darker(Color color, float amount = 0.1f)
        {
            color.r = Mathf.Clamp01(color.r - amount);
            color.g = Mathf.Clamp01(color.g - amount);
            color.b = Mathf.Clamp01(color.b - amount);
            return color;
        }
    }
}