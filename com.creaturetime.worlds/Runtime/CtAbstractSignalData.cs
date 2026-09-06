using System;
using UnityEngine;

namespace CreatureTime
{
    public static class CtAbstractSignalExtensions
    {
        public static void Connect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
#if DEBUG_SIGNALS
            signal.LogDebug($"Connecting signal [{receiver} -> {signal} for {typeId} and {method}]");
#endif
            signal.Connect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Disconnect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
#if DEBUG_SIGNALS
            signal.LogDebug($"Disconnecting signal [{receiver} -//> {signal}] for {typeId} and {method}");
#endif
            signal.Disconnect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Emit<T>(this CtAbstractSignal signal, T typeId)
            where T : Enum
        {
#if DEBUG_SIGNALS
            signal.LogDebug($"Emitting signal [{signal} for {typeId}]");
#endif
            signal.Emit(Convert.ToInt32(typeId));
        }
    }
}