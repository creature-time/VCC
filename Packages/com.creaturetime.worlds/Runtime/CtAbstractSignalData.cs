
using System;
using UnityEngine;

namespace CreatureTime
{
    public static class CtAbstractSignalExtensions
    {
        public static void Connect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
            Debug.LogWarning($"Attempting to connect to CtAbstractSignal {receiver} {method}");
            signal.Connect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Disconnect<T>(this CtAbstractSignal signal, T typeId, CtAbstractSignal receiver, string method)
            where T : Enum
        {
            signal.Disconnect(Convert.ToInt32(typeId), receiver, method);
        }

        public static void Emit<T>(this CtAbstractSignal signal, T typeId)
            where T : Enum
        {
            signal.Emit(Convert.ToInt32(typeId));
        }
    }
}