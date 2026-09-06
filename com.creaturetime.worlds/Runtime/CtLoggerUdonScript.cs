
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public class CtLoggerUdonScript : UdonSharpBehaviour
    {
        [SerializeField] private CtLogger logger;

        private string _ConstructTopic() => $"{gameObject.name}-{GetUdonTypeName()}";

        public void LogDebug(string message)
        {
            logger.LogDebug(_ConstructTopic(), message);
        }

        public void Log(string message)
        {
            logger.Log(_ConstructTopic(), message);
        }

        public void LogWarning(string message)
        {
            logger.LogWarning(_ConstructTopic(), message);
        }

        public void LogError(string message)
        {
            logger.LogError(_ConstructTopic(), message);
        }

        public void LogCritical(string message)
        {
            logger.LogCritical(_ConstructTopic(), message);
        }
    }
}