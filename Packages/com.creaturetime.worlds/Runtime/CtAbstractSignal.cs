
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public class CtAbstractSignal<T> : CtAbstractSignalData
        where T : Enum
    {
        // private DataDictionary _changedCallbacks = new DataDictionary();
        // private DataDictionary _addCallbacks = new DataDictionary();
        // private DataDictionary _removeCallbacks = new DataDictionary();
        //
        // private bool _blocked;

        public int SignalIndex => _signalIndex;

        public void Connect(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            if (!_changedCallbacks.ContainsKey(typeId))
            {
                _changedCallbacks.Add(typeId, new DataDictionary());
            }

            if (!_changedCallbacks[typeId].DataDictionary.ContainsKey(receiver))
            {
                _changedCallbacks[typeId].DataDictionary.Add(receiver, new DataList());
            }

            _changedCallbacks[typeId].DataDictionary[receiver].DataList.Add(method);
            Debug.Log($"Connected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void Disconnect(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            DataDictionary receivers;
            if (!_changedCallbacks.ContainsKey(typeId))
            {
                return;
            }

            receivers = _changedCallbacks[typeId].DataDictionary;
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

            _changedCallbacks.Remove(receiver);
            Debug.Log($"Disconnected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void Emit(T identifier)
        {
            if (_blocked)
                return;

            _blocked = true;

            int typeId = Convert.ToInt32(identifier);

            if (_changedCallbacks.TryGetValue(typeId, TokenType.DataDictionary, out DataToken token))
            {
                var receivers = token.DataDictionary;
                var keys = receivers.GetKeys();
                DataToken[] tokens = keys.ToArray();
                for (int i = 0; i < tokens.Length; ++i)
                {
                    var receiver = tokens[i];
                    var reference = (UdonSharpBehaviour)receiver.Reference;

                    var methods = receivers[receiver].DataList;
                    for (int j = 0; j < methods.Count; ++j)
                    {
                        string method = methods[j].String;
                        reference.SendCustomEvent(method);
                    }
                }
            }

            _blocked = false;
        }

        public void ConnectAdd(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            if (!_addCallbacks.ContainsKey(typeId))
            {
                _addCallbacks.Add(typeId, new DataDictionary());
            }

            if (!_addCallbacks[typeId].DataDictionary.ContainsKey(receiver))
            {
                _addCallbacks[typeId].DataDictionary.Add(receiver, new DataList());
            }

            _addCallbacks[typeId].DataDictionary[receiver].DataList.Add(method);
            Debug.Log($"Connected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void DisconnectAdd(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            DataDictionary receivers;
            if (!_addCallbacks.ContainsKey(typeId))
            {
                Debug.LogWarning($"[DisconnectAdd] Could not find type id in callbacks (typeId={typeId}.");
                return;
            }

            receivers = _addCallbacks[typeId].DataDictionary;
            if (!receivers.ContainsKey(receiver))
            {
                Debug.LogWarning($"[DisconnectAdd] Could not find methodName in callbacks (methodName={method}.");
                return;
            }

            DataList methods = receivers[receiver].DataList;
            methods.Remove(method);

            Debug.Log($"Disconnected (typeId={typeId}, receiver={receiver}, method={method}).");

            if (methods.Count > 0)
                return;

            receivers.Remove(method);
            if (receivers.Count > 0)
                return;

            _addCallbacks.Remove(receiver);
        }

        protected void EmitAdd(T identifier, int index)
        {
            if (_blocked)
                return;

            _blocked = true;

            _signalIndex = index;

            int typeId = Convert.ToInt32(identifier);

            if (_addCallbacks.TryGetValue(typeId, TokenType.DataDictionary, out DataToken token))
            {
                var receivers = token.DataDictionary;
                var keys = receivers.GetKeys();
                DataToken[] tokens = keys.ToArray();
                for (int i = 0; i < tokens.Length; ++i)
                {
                    var receiver = tokens[i];
                    var reference = (UdonSharpBehaviour)receiver.Reference;

                    var methods = receivers[receiver].DataList;
                    for (int j = 0; j < methods.Count; ++j)
                    {
                        string method = methods[j].String;
                        Debug.Log($"Calling callback (typeId={typeId}, receiver={receiver}, index={j}, method={method}).");
                        reference.SendCustomEvent(method);
                    }
                }
            }

            _signalIndex = -1;

            _blocked = false;
        }

        public void ConnectRemove(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            if (!_removeCallbacks.ContainsKey(typeId))
            {
                _removeCallbacks.Add(typeId, new DataDictionary());
            }

            if (!_removeCallbacks[typeId].DataDictionary.ContainsKey(receiver))
            {
                _removeCallbacks[typeId].DataDictionary.Add(receiver, new DataList());
            }

            _removeCallbacks[typeId].DataDictionary[receiver].DataList.Add(method);
            Debug.Log($"Connected (typeId={typeId}, receiver={receiver}, method={method}).");
        }

        public void DisconnectRemove(T identifier, UdonSharpBehaviour receiver, string method)
        {
            int typeId = Convert.ToInt32(identifier);

            DataDictionary receivers;
            if (!_removeCallbacks.ContainsKey(typeId))
            {
                Debug.LogWarning($"[DisconnectRemove] Could not find type id in callbacks (typeId={typeId}.");
                return;
            }

            receivers = _removeCallbacks[typeId].DataDictionary;
            if (!receivers.ContainsKey(receiver))
            {
                Debug.LogWarning($"[DisconnectRemove] Could not find methodName in callbacks (methodName={method}.");
                return;
            }

            DataList methods = receivers[receiver].DataList;
            methods.Remove(method);

            Debug.Log($"Disconnected (typeId={typeId}, receiver={receiver}, method={method}).");

            if (methods.Count > 0)
                return;

            receivers.Remove(method);
            if (receivers.Count > 0)
                return;

            _removeCallbacks.Remove(receiver);
        }

        protected void EmitRemove(T identifier, int index)
        {
            if (_blocked)
                return;

            _blocked = true;

            _signalIndex = index;

            int typeId = Convert.ToInt32(identifier);

            if (_removeCallbacks.TryGetValue(typeId, TokenType.DataDictionary, out DataToken token))
            {
                var receivers = token.DataDictionary;
                var keys = receivers.GetKeys();
                DataToken[] tokens = keys.ToArray();
                for (int i = 0; i < tokens.Length; ++i)
                {
                    var receiver = tokens[i];
                    var reference = (UdonSharpBehaviour)receiver.Reference;

                    var methods = receivers[receiver].DataList;
                    for (int j = 0; j < methods.Count; ++j)
                    {
                        string method = methods[j].String;
                        Debug.Log($"Calling callback (typeId={typeId}, receiver={receiver}, index={j}, method={method}).");
                        reference.SendCustomEvent(method);
                    }
                }
            }

            _signalIndex = -1;

            _blocked = false;
        }
    }
}