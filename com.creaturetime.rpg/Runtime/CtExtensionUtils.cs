
using UdonSharp;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public static class CtExtensionUtils
    {
        public static bool TryGetDefinition<Obj, T>(this Obj obj, DataDictionary dict, ushort identifier, out T definition)
            where Obj : CtLoggerUdonScript
            where T : UdonSharpBehaviour
        {
            definition = null;

            if (!dict.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                obj.LogWarning($"Failed to find {obj.GetType().Name} by identifier (identifier={identifier}).");
#endif
                return false;
            }

            definition = (T)dataToken.Reference;
            return true;
        }
    }
}