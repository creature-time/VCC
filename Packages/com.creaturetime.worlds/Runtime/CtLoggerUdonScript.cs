
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public class CtLoggerUdonScript : UdonSharpBehaviour
    {
        [SerializeField] private CtLogger logger;

        public void LogDebug(string message)
        {
            logger.LogDebug(gameObject.name, message);
        }

        public void Log(string message)
        {
            logger.Log(gameObject.name, message);
        }

        public void LogWarning(string message)
        {
            logger.LogWarning(gameObject.name, message);
        }

        public void LogError(string message)
        {
            logger.LogError(gameObject.name, message);
        }

        public void LogCritical(string message)
        {
            logger.LogCritical(gameObject.name, message);
        }
    }
}