
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
    public class CtLogger : CtAbstractSignal
    {
        private const string InstanceName = "__Logger";

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

        public static CtLogger Logger()
        {
            GameObject obj = GameObject.Find(InstanceName);
            if (!obj)
                return null;
            return obj.GetComponent<CtLogger>();
        }

        public static void ConnectMessageChanged(CtAbstractSignal receiver, string method)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Connect(ELoggerSignal.MessageChanged, receiver, method);
        }

        public static void DisconnectMessageChanged(CtAbstractSignal receiver, string method)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Disconnect(ELoggerSignal.MessageChanged, receiver, method);
        }

        public static string GetMessage()
        {
            CtLogger logger = Logger();
            if (!logger)
                return null;
            return logger._message;
        }

        public static void LogDebug(string topic, string message)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Log(topic, message, ELoggerType.Debug);
        }

        public static void Log(string topic, string message)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Log(topic, message, ELoggerType.Info);
        }

        public static void LogWarning(string topic, string message)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Log(topic, message, ELoggerType.Warning);
        }

        public static void LogError(string topic, string message)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Log(topic, message, ELoggerType.Error);
        }

        public static void LogCritical(string topic, string message)
        {
            CtLogger logger = Logger();
            if (!logger)
                return;
            logger.Log(topic, message, ELoggerType.Critical);
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

        private void Log(string domain, string message, ELoggerType loggerType)
        {
            DateTime now = DateTime.Now;

            if (!string.IsNullOrEmpty(domain))
            {
                string hexColor = CtColorUtils.RandomStringColorHex(domain);
                domain = $"[<color={hexColor}>{domain}</color>] ";
            }

            _message =
                $"<color={GetColor(loggerType)}>" +
                $"[{now:HH:mm-ss}] " +
                domain +
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

            this.SetArgs.Add(_message);
            this.Emit(ELoggerSignal.MessageChanged);
        }
    }
}