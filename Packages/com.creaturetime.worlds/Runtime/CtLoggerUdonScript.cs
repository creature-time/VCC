
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public class CtLoggerUdonScript : UdonSharpBehaviour
    {
        [SerializeField] private CtLogger logger;

        protected void LogDebug(string message)
        {
            logger.LogDebug(gameObject.name, message);
        }

        protected void Log(string message)
        {
            logger.Log(gameObject.name, message);
        }

        protected void LogWarning(string message)
        {
            logger.LogWarning(gameObject.name, message);
        }

        protected void LogError(string message)
        {
            logger.LogError(gameObject.name, message);
        }

        protected void LogCritical(string message)
        {
            logger.LogCritical(gameObject.name, message);
        }
    }
}