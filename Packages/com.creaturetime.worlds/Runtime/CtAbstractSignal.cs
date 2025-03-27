
using VRC.SDK3.Data;

namespace CreatureTime
{
    public class CtAbstractSignal : CtLoggerUdonScript
    {
        private DataDictionary _callbacks = new DataDictionary();

        private bool _blocked;

        // TODO: Can we make these arguments stack in a DataList?
        private DataList _setArgs = new DataList();
        private DataList _getArgs;

        public DataList SetArgs => _setArgs;
        public DataList GetArgs => _getArgs;

        public void Connect(int typeId, CtAbstractSignal receiver, string method)
        {
            if (!_callbacks.ContainsKey(typeId))
            {
                _callbacks.Add(typeId, new DataDictionary());
            }

            if (!_callbacks[typeId].DataDictionary.ContainsKey(receiver))
            {
                _callbacks[typeId].DataDictionary.Add(receiver, new DataList());
            }

            _callbacks[typeId].DataDictionary[receiver].DataList.Add(method);
            LogDebug($"Connected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void Disconnect(int typeId, CtAbstractSignal receiver, string method)
        {
            DataDictionary receivers;
            if (!_callbacks.ContainsKey(typeId))
            {
                return;
            }

            receivers = _callbacks[typeId].DataDictionary;
            if (!receivers.ContainsKey(receiver))
            {
                return;
            }

            DataList methods = receivers[receiver].DataList;
            methods.Remove(method);

            if (methods.Count > 0)
                return;

            receivers.Remove(method);
            if (receivers.Count > 0)
                return;

            _callbacks.Remove(receiver);
            LogDebug($"Disconnected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void Emit(int typeId)
        {
            if (_blocked)
                return;

            _blocked = true;

            if (_callbacks.TryGetValue(typeId, TokenType.DataDictionary, out DataToken token))
            {
                var receivers = token.DataDictionary;
                var keys = receivers.GetKeys();
                DataToken[] tokens = keys.ToArray();
                for (int i = 0; i < tokens.Length; ++i)
                {
                    var receiver = tokens[i];
                    var reference = (CtAbstractSignal)receiver.Reference;

                    reference._getArgs = _setArgs;

                    var methods = receivers[receiver].DataList;
                    for (int j = 0; j < methods.Count; ++j)
                    {
                        string method = methods[j].String;
                        reference.SendCustomEvent(method);
                    }

                    reference._getArgs = null;
                }
            }

            _blocked = false;

            _setArgs.Clear();
        }
    }
}