
using System;

#if DEBUG_SIGNALS
using UnityEngine;
#endif

namespace CreatureTime
{
    public static class CtAbstractSignalExtensions
    {
        public static void Connect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
#if DEBUG_SIGNALS
            Debug.Log($"Connecting signal [{receiver} -> {signal} for {typeId} and {method}]");
#endif
            signal.Connect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Disconnect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
#if DEBUG_SIGNALS
            Debug.Log($"Disconnecting signal [{receiver} -//> {signal}] for {typeId} and {method}");
#endif
            signal.Disconnect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Emit<T>(this CtAbstractSignal signal, T typeId)
            where T : Enum
        {
#if DEBUG_SIGNALS
            Debug.Log($"Emitting signal [{signal} for {typeId}]");
#endif
            signal.Emit(Convert.ToInt32(typeId));
        }
    }
}