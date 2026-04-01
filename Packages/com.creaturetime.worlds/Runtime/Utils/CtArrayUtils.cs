
using System;

namespace CreatureTime
{
    public static class CtArrayUtils
    {
        public static void Add<T>(ref T[] array, T value)
        {
            Insert(ref array, value, -1);
        }

        public static void Insert<T>(ref T[] array, T value, int index)
        {
            var size = array.Length;
            var temp = array;
            array = new T[size + 1];
            if (0 > index || index >= size)
            {
                Array.Copy(temp, array, size);
                array[size] = value;
            }
            else
            {
                // if (index > 0)
                Array.Copy(temp, array, index);
                array[index] = value;
                Array.Copy(temp, index, array, index + 1, size - index);
            }
        }

        public static T Pop<T>(ref T[] array, int index)
        {
            var size = array.Length - 1;
            var temp = array;
            array = new T[size];

            Array.Copy(temp, array, index);
            T result = temp[index];
            Array.Copy(temp, index + 1, array, index, size - index);

            return result;
        }

        public static void Resize<T>(ref T[] array, int size)
        {
            T t = default;
            Resize(ref array, size, t);
        }

        public static void Resize<T>(ref T[] array, int size, T defaultValue)
        {
            var result = new T[size];
            if (result.Length > 0)
            {
                Array.Copy(array, 0, result, 0, array.Length);
                for (var i = array.Length; i < result.Length; i++)
                    result[i] = defaultValue;
            }

            array = result;
        }

        public static T[] Reverse<T>(T[] array)
        {
            var result = new T[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                result[i] = array[array.Length - 1 - i];
            }

            return result;
        }

        public static string DebugToString<T>(T[] array)
        {
            var text = string.Empty;
            if (array.Length == 0) return text;

            text += array[0].ToString();
            for (var i = 1; i < array.Length; i++)
            {
                text += ", ";
                text += array[i].ToString();
            }

            return text;
        }
    }
}