
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public static class CtBinaryUtils
    {
        #region Reading

        public static bool AsBoolean(byte[] data, ref int offset) => data[offset++] != 0;

        public static sbyte As(byte[] data, ref int offset) => (sbyte)data[offset++];

        public static byte AsByte(byte[] data, ref int offset) => data[offset++];

        public static short AsShort(byte[] data, ref int offset) => (short)((data[offset++] << 8) | data[offset++]);

        public static ushort AsUShort(byte[] data, ref int offset) => (ushort)((data[offset++] << 8) | data[offset++]);

        public static int AsInt(byte[] data, ref int offset) => 
            (data[offset++] << 24) | (data[offset++] << 16) | (data[offset++] << 8) | data[offset++];

        public static uint AsUInt(byte[] data, ref int offset) => 
            (uint)((data[offset++] << 24) | (data[offset++] << 16) | (data[offset++] << 8) | data[offset++]);

        public static long AsLong(byte[] data, ref int offset) =>
            ((long)data[offset++] << 56) | ((long)data[offset++] << 48) | ((long)data[offset++] << 40) | (long)data[offset++] << 32 | 
            ((long)data[offset++] << 24) | ((long)data[offset++] << 16) | ((long)data[offset++] << 8) | data[offset++];

        public static ulong AsULong(byte[] data, ref int offset) => 
            ((ulong)data[offset++] << 56) | ((ulong)data[offset++] << 48) | ((ulong)data[offset++] << 40) | (ulong)data[offset++] << 32 | 
            ((ulong)data[offset++] << 24) | ((ulong)data[offset++] << 16) | ((ulong)data[offset++] << 8) | data[offset++];

        public static string AsString(byte[] data, ref int offset)
        {
            var length = AsInt(data, ref offset);
            var str = string.Empty;

            for (int i = 0; i < length; i++)
                str += (char)data[offset++];
            return str;
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
                        dataSize += 1;
                        break;
                    case TokenType.SByte:
                        dataSize += 1;
                        break;
                    case TokenType.Byte:
                        dataSize += 1;
                        break;
                    case TokenType.Short:
                        dataSize += 2;
                        break;
                    case TokenType.UShort:
                        dataSize += 2;
                        break;
                    case TokenType.Int:
                        dataSize += 4;
                        break;
                    case TokenType.UInt:
                        dataSize += 4;
                        break;
                    case TokenType.Long:
                        dataSize += 8;
                        break;
                    case TokenType.ULong:
                        dataSize += 8;
                        break;
                    // case TokenType.Float:
                    //     dataSize += 4;
                    //     break;
                    // case TokenType.Double:
                    //     dataSize += 8;
                    //     break;
                    case TokenType.String:
                        dataSize += token.String.Length + 4;
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
            data[offset++] = (byte)(value ? 1 : 0);
        }

        public static void ToBytes(sbyte value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)value;
        }

        public static void ToBytes(byte value, ref byte[] data, ref int offset)
        {
            data[offset++] = value;
        }

        public static void ToBytes(short value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(ushort value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(int value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 24) & 0xff);
            data[offset++] = (byte)((value >> 16) & 0xff);
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(uint value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 24) & 0xff);
            data[offset++] = (byte)((value >> 16) & 0xff);
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(long value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 56) & 0xff);
            data[offset++] = (byte)((value >> 48) & 0xff);
            data[offset++] = (byte)((value >> 40) & 0xff);
            data[offset++] = (byte)((value >> 32) & 0xff);
            data[offset++] = (byte)((value >> 24) & 0xff);
            data[offset++] = (byte)((value >> 16) & 0xff);
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(ulong value, ref byte[] data, ref int offset)
        {
            data[offset++] = (byte)((value >> 56) & 0xff);
            data[offset++] = (byte)((value >> 48) & 0xff);
            data[offset++] = (byte)((value >> 40) & 0xff);
            data[offset++] = (byte)((value >> 32) & 0xff);
            data[offset++] = (byte)((value >> 24) & 0xff);
            data[offset++] = (byte)((value >> 16) & 0xff);
            data[offset++] = (byte)((value >> 8) & 0xff);
            data[offset++] = (byte)(value & 0xff);
        }

        public static void ToBytes(string value, ref byte[] data, ref int offset)
        {
            var strLen = value.Length;
            data[offset++] = (byte)((strLen >> 24) & 0xff);
            data[offset++] = (byte)((strLen >> 16) & 0xff);
            data[offset++] = (byte)((strLen >> 8) & 0xff);
            data[offset++] = (byte)(strLen & 0xff);
            for (int j = 0; j < value.Length; j++)
                data[offset++] = (byte)value[j];
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
