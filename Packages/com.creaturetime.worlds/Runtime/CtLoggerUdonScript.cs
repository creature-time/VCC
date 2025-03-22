
using UdonSharp;

namespace CreatureTime
{
    public class CtLoggerUdonScript : UdonSharpBehaviour
    {
        protected void LogDebug(string message)
        {
            CtLogger.LogDebug(gameObject.name, message);
        }

        protected void Log(string message)
        {
            CtLogger.Log(gameObject.name, message);
        }

        protected void LogWarning(string message)
        {
            CtLogger.LogWarning(gameObject.name, message);
        }

        protected void LogError(string message)
        {
            CtLogger.LogError(gameObject.name, message);
        }

        protected void LogCritical(string message)
        {
            CtLogger.LogCritical(gameObject.name, message);
        }
    }
}