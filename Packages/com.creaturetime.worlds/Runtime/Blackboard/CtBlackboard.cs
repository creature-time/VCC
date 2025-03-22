
using UdonSharp;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtBlackboard : UdonSharpBehaviour
    {
        private DataDictionary _data = new DataDictionary();

        public void Clear()
        {
            _data.Clear();
        }

        public void SetValue(string key, DataToken value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public int GetOrCreateKey(string key)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (!_data.TryGetValue(encodedKey, out var token))
            {
                token = default;
                _data.SetValue(encodedKey, token);
            }

            return encodedKey;
        }

        public void SetBool(string key, bool value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetBool(string key, out bool value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetBool(token);
                return true;
            }

            value = default;
            return false;
        }

        private static bool _GetBool(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return token.Boolean;
                case TokenType.Short:
                    return token.Short != 0;
                case TokenType.UShort:
                    return token.UShort != 0;
                case TokenType.Int:
                    return token.Int != 0;
                case TokenType.UInt:
                    return token.UInt != 0;
                case TokenType.Long:
                    return token.Long != 0;
                case TokenType.ULong:
                    return token.ULong != 0;
                case TokenType.Float:
                    return token.Float != 0;
                default:
                    return false;
            }
        }

        public void SetShort(string key, short value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetShort(string key, out short value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetShort(token);
                return true;
            }

            value = 0;
            return false;
        }

        private static short _GetShort(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return (short)(token.Boolean ? 1 : 0);
                case TokenType.Short:
                    return token.Short;
                case TokenType.UShort:
                    return (short)token.UShort;
                case TokenType.Int:
                    return (short)token.Int;
                case TokenType.UInt:
                    return (short)token.UInt;
                case TokenType.Long:
                    return (short)token.Long;
                case TokenType.ULong:
                    return (short)token.ULong;
                case TokenType.Float:
                    return (short)token.Float;
                default:
                    return 0;
            }
        }

        public void SetUShort(string key, ushort value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetUShort(string key, out ushort value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetUShort(token);
                return true;
            }

            value = 0;
            return false;
        }

        private static ushort _GetUShort(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return (ushort)(token.Boolean ? 1 : 0);
                case TokenType.Short:
                    return (ushort)token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return (ushort)token.Int;
                case TokenType.UInt:
                    return (ushort)token.UInt;
                case TokenType.Long:
                    return (ushort)token.Long;
                case TokenType.ULong:
                    return (ushort)token.ULong;
                case TokenType.Float:
                    return (ushort)token.Float;
                default:
                    return 0;
            }
        }

        public void SetInt(string key, int value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetInt(string key, out int value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetInt(token);
                return true;
            }

            value = default;
            return false;
        }

        private static int _GetInt(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return token.Boolean ? 1 : 0;
                case TokenType.Short:
                    return token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return token.Int;
                case TokenType.UInt:
                    return (int)token.UInt;
                case TokenType.Long:
                    return (int)token.Long;
                case TokenType.ULong:
                    return (int)token.ULong;
                case TokenType.Float:
                    return (int)token.Float;
                default:
                    return 0;
            }
        }

        public void SetUInt(string key, int value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetUInt(string key, out uint value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetUInt(token);
                return true;
            }

            value = 0;
            return false;
        }

        private static uint _GetUInt(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return (uint)(token.Boolean ? 1 : 0);
                case TokenType.Short:
                    return (uint)token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return (uint)token.Int;
                case TokenType.UInt:
                    return token.UInt;
                case TokenType.Long:
                    return (uint)token.Long;
                case TokenType.ULong:
                    return (uint)token.ULong;
                case TokenType.Float:
                    return (uint)token.Float;
                default:
                    return 0;
            }
        }

        public void SetLong(string key, long value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetLong(string key, out long value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetLong(token);
                return true;
            }

            value = 0;
            return false;
        }

        private static long _GetLong(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return token.Boolean ? 1 : 0;
                case TokenType.Short:
                    return token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return token.Int;
                case TokenType.UInt:
                    return token.UInt;
                case TokenType.Long:
                    return token.Long;
                case TokenType.ULong:
                    return (long)token.ULong;
                case TokenType.Float:
                    return (long)token.Float;
                default:
                    return 0;
            }
        }

        public void SetULong(string key, ulong value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetULong(string key, out ulong value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetULong(token);
                return true;
            }

            value = 0;
            return false;
        }

        private static ulong _GetULong(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return (ulong)(token.Boolean ? 1 : 0);
                case TokenType.Short:
                    return (ulong)token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return (ulong)token.Int;
                case TokenType.UInt:
                    return token.UInt;
                case TokenType.Long:
                    return (ulong)token.Long;
                case TokenType.ULong:
                    return token.ULong;
                case TokenType.Float:
                    return (ulong)token.Float;
                default:
                    return 0;
            }
        }

        public void SetFloat(string key, float value)
        {
            _data.SetValue(CtEncodingUtils.Encode(key), value);
        }

        public bool TryGetFloat(string key, out float value)
        {
            var encodedKey = CtEncodingUtils.Encode(key);
            if (_data.TryGetValue(encodedKey, out var token))
            {
                value = _GetFloat(token);
                return true;
            }

            value = default;
            return false;
        }

        private static float _GetFloat(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return token.Boolean ? 1 : 0;
                case TokenType.Int:
                    return token.Int;
                case TokenType.Float:
                    return token.Float;
                default:
                    return default;
            }
        }
    }
}