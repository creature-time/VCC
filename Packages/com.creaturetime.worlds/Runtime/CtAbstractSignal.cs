
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public class CtAbstractSignal : CtLoggerUdonScript
    {
        private DataDictionary _callbacks = new DataDictionary();

        private bool _blocked;

        // TODO: Can we make these arguments stack in a DataList?
        private DataList _setArgs = new DataList();
        private DataList _getArgs = new DataList();
        private DataList _sender = new DataList();

        public CtAbstractSignal Sender => (CtAbstractSignal)_sender[0].Reference;
        public DataList SetArgs => _setArgs;
        public DataList GetArgs => _getArgs[0].DataList;

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
#if DEBUG_LOGS
            LogDebug($"Connected (typeId={typeId}, receiver={receiver}, method={method}).");
#endif
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
#if DEBUG_LOGS
            LogDebug($"Disconnected (typeId={typeId}, receiver={receiver}, method={method}).");
#endif
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

                    reference._sender.Insert(0, this);
                    reference._getArgs.Insert(0, _setArgs);

                    var methods = receivers[receiver].DataList;
                    for (int j = 0; j < methods.Count; ++j)
                    {
                        string method = methods[j].String;
#if DEBUG_LOGS
                        LogDebug(
                            "Emitting " +
                            $"(typeId={typeId}, sender={reference.Sender}, receiver={receiver}, method={method}).");
#endif
                        reference.SendCustomEvent(method);
                    }

                    reference._sender.RemoveAt(0);
                    reference._getArgs.RemoveAt(0);
                }
            }

            _blocked = false;

            _setArgs.Clear();
        }
    }
}