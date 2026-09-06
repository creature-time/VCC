
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum ELoggerType
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public enum ELoggerSignal
    {
        MessageChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLogger : CtSingleton
    {
        public const string DebugColor = "#808080";
        public const string InfoColor = "#6495ED";
        public const string WarningColor = "#FFFF00";
        public const string ErrorColor = "#FF0000";
        public const string CriticalColor = "#BA3F38";

        private string[] _colors = 
        {
            DebugColor,
            InfoColor,
            WarningColor,
            ErrorColor,
            CriticalColor
        };

        private string _message;

        private string GetColor(ELoggerType loggerType) => _colors[(int)loggerType];

        public void LogDebug(string topic, string message)
        {
            Log(topic, message, ELoggerType.Debug);
        }

        public void Log(string topic, string message)
        {
            Log(topic, message, ELoggerType.Info);
        }

        public void LogWarning(string topic, string message)
        {
            Log(topic, message, ELoggerType.Warning);
        }

        public void LogError(string topic, string message)
        {
            Log(topic, message, ELoggerType.Error);
        }

        public void LogCritical(string topic, string message)
        {
            Log(topic, message, ELoggerType.Critical);
        }

        private static string _AsString(ELoggerType loggerType)
        {
            switch (loggerType)
            {
                case ELoggerType.Debug:
                    return "Debug";
                case ELoggerType.Info:
                    return "Info";
                case ELoggerType.Warning:
                    return "Warning";
                case ELoggerType.Error:
                    return "Error";
                case ELoggerType.Critical:
                    return "Critical";
            }

            return "None";
        }

        private bool blocked = false;
        private void Log(string topic, string message, ELoggerType loggerType)
        {
            DateTime now = DateTime.Now;

            if (!string.IsNullOrEmpty(topic))
            {
                var hexColor = CtColorUtils.RandomStringColorHex(topic);
                topic = $"[<color={hexColor}>{topic}</color>] ";
            }

            _message =
                $"<color={GetColor(loggerType)}>" +
                $"[{now:HH:mm-ss}] " +
                topic +
                $"[{_AsString(loggerType)}] " +
                message + 
                "</color>";

            switch (loggerType)
            {
                case ELoggerType.Debug:
                    Debug.Log(_message);
                    break;
                case ELoggerType.Info:
                    Debug.Log(_message);
                    break;
                case ELoggerType.Warning:
                    Debug.LogWarning(_message);
                    break;
                case ELoggerType.Error:
                    Debug.LogError(_message);
                    break;
                case ELoggerType.Critical:
                    Debug.LogError(_message);
                    break;
            }

            SetArgs.Add(_message);
            this.Emit(ELoggerSignal.MessageChanged);
        }
    }
}