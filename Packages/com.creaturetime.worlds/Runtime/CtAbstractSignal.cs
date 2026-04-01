
using VRC.SDK3.Data;

namespace CreatureTime
{
    public abstract class CtAbstractSignal : CtLoggerUdonScript
    {
        private DataDictionary _callbacks = new DataDictionary();

        private bool _blocked;

        // TODO: Can we make these arguments stack in a DataList?
        private DataList _setArgs = new DataList();
        private DataList _getArgs = new DataList();
        private DataList _sender = new DataList();

        public CtAbstractSignal Sender => (CtAbstractSignal)_sender[0].Reference;

        public DataList SetArgs => _setArgs;

        public DataList GetArgs
        {
            get
            {
#if DEBUG_LOGS
                if (_getArgs.Count == 0)
                    LogCritical("Getting arguments failed due to no arguments available. " +
                                "Make sure you are not calling this from a non-signal called method.");
#endif

                return _getArgs[0].DataList;
            }
        }

        public void Connect(int typeId, CtAbstractSignal receiver, string method)
        {
            if (!_callbacks.ContainsKey(typeId))
            {
#if DEBUG_SIGNALS
                LogDebug($"Adding typeId to callbacks (signal={this}, typeId={typeId}).");
#endif
                _callbacks.Add(typeId, new DataDictionary());
            }

            if (!_callbacks[typeId].DataDictionary.ContainsKey(receiver))
            {
#if DEBUG_SIGNALS
                LogDebug($"Adding receiver to receivers (signal={this}, typeId={typeId}, receiver={receiver}).");
#endif
                _callbacks[typeId].DataDictionary.Add(receiver, new DataList());
            }

#if DEBUG_SIGNALS
            LogDebug($"Connected (signal={this}, typeId={typeId}, receiver={receiver}, method={method}).");
#endif
            _callbacks[typeId].DataDictionary[receiver].DataList.Add(method);
        }

        public void Disconnect(int typeId, CtAbstractSignal receiver, string method)
        {
            if (!_callbacks.ContainsKey(typeId))
            {
                return;
            }

            DataDictionary receivers = _callbacks[typeId].DataDictionary;
            if (!receivers.ContainsKey(receiver))
            {
                return;
            }

            DataList methods = receivers[receiver].DataList;
            methods.Remove(method);

#if DEBUG_SIGNALS
            LogDebug($"Disconnected (signal={this}, typeId={typeId}, receiver={receiver}, method={method}).");
#endif

            if (methods.Count > 0)
                return;
#if DEBUG_SIGNALS
            LogDebug($"Removing receiver from receivers (signal={this}, typeId={typeId}, receiver={receiver}).");
#endif
            receivers.Remove(receiver);

            if (receivers.Count > 0)
                return;
#if DEBUG_SIGNALS
            LogDebug($"Removing typeId from callbacks (signal={this}, typeId={typeId}).");
#endif
            _callbacks.Remove(typeId);
        }

        public void Emit(int typeId)
        {
            if (!_blocked)
            {
                _blocked = true;

#if DEBUG_SIGNALS
                LogDebug($"Begin emitting (signal={this}, typeId={typeId}).");
#endif

                if (_callbacks.TryGetValue(typeId, TokenType.DataDictionary, out DataToken token))
                {
                    var receivers = token.DataDictionary;
                    var keys = receivers.GetKeys();
                    DataToken[] tokens = keys.ToArray();
                    for (int i = 0; i < tokens.Length; ++i)
                    {
                        var receiver = tokens[i];
                        if (!receivers.ContainsKey(receiver))
                        {
#if DEBUG_SIGNALS
                            LogWarning($"Receiver was invalid (signal={this}, typeId={typeId}, receiver={receiver}).");
#endif
                            continue;
                        }

                        var reference = (CtAbstractSignal)receiver.Reference;

                        reference._sender.Insert(0, this);
                        reference._getArgs.Insert(0, _setArgs);

                        var methods = receivers[receiver].DataList;
                        for (int j = 0; j < methods.Count; ++j)
                        {
                            string method = methods[j].String;

#if DEBUG_SIGNALS
                            LogDebug(
                                $"Emitting (signal={this}, typeId={typeId}, receiver={receiver}, method={method}).");
#endif

                            reference.SendCustomEvent(method);
                        }

                        reference._sender.RemoveAt(0);
                        reference._getArgs.RemoveAt(0);
                    }
                }

                _blocked = false;
            }

            _setArgs.Clear();
        }
    }
}