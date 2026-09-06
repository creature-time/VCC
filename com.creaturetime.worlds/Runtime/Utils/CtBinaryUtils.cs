
using System;
using System.Text;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public static class CtBinaryUtils
    {
        public const int SizeOfBoolean = 1;
        public const int SizeOfSByte = 1;
        public const int SizeOfByte = 1;
        public const int SizeOfShort = 2;
        public const int SizeOfUShort = 2;
        public const int SizeOfInt = 4;
        public const int SizeOfUInt = 4;
        public const int SizeOfLong = 8;
        public const int SizeOfULong = 8;

        public const int SizeOfFloat = 4;
        public const int SizeOfDouble = 8;

        #region Reading

        public static bool AsBoolean(byte[] data, ref int offset)
        {
            var result = BitConverter.ToBoolean(data, offset);
            offset += SizeOfBoolean;
            return result;
        }

        public static sbyte AsSByte(byte[] data, ref int offset) => (sbyte)data[offset++];

        public static byte AsByte(byte[] data, ref int offset) => data[offset++];

        public static short AsShort(byte[] data, ref int offset)
        {
            var result = BitConverter.ToInt16(data, offset);
            offset += SizeOfShort;
            return result;
        }

        public static ushort AsUShort(byte[] data, ref int offset)
        {
            var result = BitConverter.ToUInt16(data, offset);
            offset += SizeOfUShort;
            return result;
        }

        public static int AsInt(byte[] data, ref int offset)
        {
            var result = BitConverter.ToInt32(data, offset);
            offset += SizeOfInt;
            return result;
        }

        public static uint AsUInt(byte[] data, ref int offset)
        {
            var result = BitConverter.ToUInt32(data, offset);
            offset += SizeOfUInt;
            return result;
        }

        public static long AsLong(byte[] data, ref int offset)
        {
            var result = BitConverter.ToInt64(data, offset);
            offset += SizeOfLong;
            return result;
        }

        public static ulong AsULong(byte[] data, ref int offset)
        {
            var result = BitConverter.ToUInt64(data, offset);
            offset += SizeOfULong;
            return result;
        } 

        public static float AsFloat(byte[] data, ref int offset)
        {
            var result = BitConverter.ToSingle(data, offset);
            offset += SizeOfFloat;
            return result;
        }

        public static double AsDouble(byte[] data, ref int offset)
        {
            var result = BitConverter.ToDouble(data, offset);
            offset += SizeOfDouble;
            return result;
        } 

        public static string AsString(byte[] data, ref int offset)
        {
            var length = AsInt(data, ref offset);
            var str = string.Empty;

            for (int i = 0; i < length; i++)
                str += (char)data[offset++];
            return str;
        }

        public static T AsEnum<T>(byte[] data, ref int offset)
        {
            return (T)Enum.ToObject(typeof(T), AsInt(data, ref offset));
        } 

        #endregion

        #region Writing

        private static int _CalculateDataSize(DataList dataList)
        {
            int dataSize = 0;
            for (int i = 0; i < dataList.Count; i++)
            {
                var token = dataList[i];
                switch (token.TokenType)
                {
                    case TokenType.Boolean:
                        dataSize += SizeOfBoolean;
                        break;
                    case TokenType.SByte:
                        dataSize += SizeOfSByte;
                        break;
                    case TokenType.Byte:
                        dataSize += SizeOfByte;
                        break;
                    case TokenType.Short:
                        dataSize += SizeOfShort;
                        break;
                    case TokenType.UShort:
                        dataSize += SizeOfUShort;
                        break;
                    case TokenType.Int:
                        dataSize += SizeOfInt;
                        break;
                    case TokenType.UInt:
                        dataSize += SizeOfUInt;
                        break;
                    case TokenType.Long:
                        dataSize += SizeOfLong;
                        break;
                    case TokenType.ULong:
                        dataSize += SizeOfULong;
                        break;
                    case TokenType.Float:
                        dataSize += SizeOfFloat;
                        break;
                    case TokenType.Double:
                        dataSize += SizeOfDouble;
                        break;
                    case TokenType.String:
                        dataSize += SizeOfInt + token.String.Length;
                        break;
                    case TokenType.DataList:
                        _CalculateDataSize(token.DataList);
                        break;
                    // case TokenType.DataDictionary:
                    //     break;
                    // case TokenType.Reference:
                    //     break;
                    // case TokenType.Error:
                    //     break;
                    default:
                        Debug.LogWarning($"CtBinaryReader does not support type (type={token.TokenType}");
                        break;
                }
            }

            return dataSize;
        }

        public static void ToBytes(bool value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfBoolean;
        }

        public static void ToBytes(sbyte value, ref byte[] data, ref int offset)
        {
            byte[] bytes = { (byte)value };
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfSByte;
        }

        public static void ToBytes(byte value, ref byte[] data, ref int offset)
        {
            byte[] bytes = { value };
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfByte;
        }

        public static void ToBytes(short value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfShort;
        }

        public static void ToBytes(ushort value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfUShort;
        }

        public static void ToBytes(int value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfInt;
        }

        public static void ToBytes(uint value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfUInt;
        }

        public static void ToBytes(long value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfLong;
        }

        public static void ToBytes(ulong value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfULong;
        }

        public static void ToBytes(float value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfFloat;
        }

        public static void ToBytes(double value, ref byte[] data, ref int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += SizeOfDouble;
        }

        public static void ToBytes(string value, ref byte[] data, ref int offset)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            ToBytes(bytes.Length, ref data, ref offset);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
        }

        public static void ToBytes<T>(T value, ref byte[] data, ref int offset)
            where T : Enum
        {
            var tmpValue = Convert.ToInt32(value);
            ToBytes(tmpValue, ref data, ref offset);
        }

        private static void _WriteData(DataList dataList, ref byte[] data, ref int offset)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                var token = dataList[i];
                switch (token.TokenType)
                {
                    case TokenType.Boolean:
                        ToBytes(token.Boolean, ref data, ref offset);
                        break;
                    case TokenType.SByte:
                        ToBytes(token.SByte, ref data, ref offset);
                        break;
                    case TokenType.Byte:
                        ToBytes(token.Byte, ref data, ref offset);
                        break;
                    case TokenType.Short:
                        ToBytes(token.Short, ref data, ref offset);
                        break;
                    case TokenType.UShort:
                        ToBytes(token.UShort, ref data, ref offset);
                        break;
                    case TokenType.Int:
                        ToBytes(token.Int, ref data, ref offset);
                        break;
                    case TokenType.UInt:
                        ToBytes(token.UInt, ref data, ref offset);
                        break;
                    case TokenType.Long:
                        ToBytes(token.Long, ref data, ref offset);
                        break;
                    case TokenType.ULong:
                        ToBytes(token.ULong, ref data, ref offset);
                        break;
                    // case TokenType.Float:
                    //     data[offset++] = (byte)((token.Float >> 24) & 0xff);
                    //     data[offset++] = (byte)((token.Float >> 16) & 0xff);
                    //     data[offset++] = (byte)((token.Float >> 8) & 0xff);
                    //     data[offset++] = (byte)(token.Float & 0xff);
                    //     break;
                    // case TokenType.Double:
                    //     data[offset++] = (byte)((token.Double >> 56) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 48) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 40) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 32) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 24) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 16) & 0xff);
                    //     data[offset++] = (byte)((token.Double >> 8) & 0xff);
                    //     data[offset++] = (byte)(token.Double & 0xff);
                    //     break;
                    case TokenType.String:
                        ToBytes(token.String, ref data, ref offset);
                        break;
                    case TokenType.DataList:
                        _WriteData(token.DataList, ref data, ref offset);
                        break;
                    // case TokenType.DataDictionary:
                    //     break;
                    // case TokenType.Reference:
                    //     break;
                    // case TokenType.Error:
                    //     break;
                    default:
                        Debug.LogWarning($"CtBinaryReader does not support type (type={token.TokenType}");
                        break;
                }
            }
        }

        public static byte[] ToData(DataList dl)
        {
            int dataSize = _CalculateDataSize(dl);
            byte[] data = new byte[dataSize];
            int offset = 0;
            _WriteData(dl, ref data, ref offset);
            return data;
        }

        #endregion
    }
}
