using System;

namespace Com.AugustCellars.CoAP.Log
{
    internal sealed class ClassLogger : ILogger
    {
        private readonly LogWriterManager m_logWriterManager;
        private readonly Type m_classType;
        public ClassLogger(Type classType, LogLevel defaultLogLevel, LogWriterManager logWriterManager)
        {
            m_classType = classType;
            Level = defaultLogLevel;
            m_logWriterManager = logWriterManager;
        }

        public LogLevel Level
        {
            get;
            set;
        }

        public bool IsDebugEnabled => LogLevel.Debug >= Logging.Level && LogLevel.Debug >= Level;

        public bool IsErrorEnabled => LogLevel.Error >= Logging.Level && LogLevel.Error >= Level;

        public bool IsFatalEnabled => LogLevel.Fatal >= Logging.Level && LogLevel.Fatal >= Level;

        public bool IsInfoEnabled => LogLevel.Info >= Logging.Level && LogLevel.Info >= Level;

        public bool IsWarnEnabled => LogLevel.Warning >= Logging.Level && LogLevel.Warning >= Level;

        private string Format(string message)
        {
            return $"{m_classType}: {message}";
        }

        public void Debug(string message)
        {
            if (!IsDebugEnabled) return;
            m_logWriterManager.Debug(Format(message));
        }

        public void Debug(string message, Exception exception)
        {
            if (!IsDebugEnabled) return;
            m_logWriterManager.Debug(Format(message), exception);
        }

        public void Error(string message)
        {
            if (!IsErrorEnabled) return;
            m_logWriterManager.Error(Format(message));
        }

        public void Error(string message, Exception exception)
        {
            if (!IsErrorEnabled) return;
            m_logWriterManager.Error(Format(message), exception);
        }

        public void Fatal(string message)
        {
            if (!IsFatalEnabled) return;
            m_logWriterManager.Fatal(Format(message));
        }

        public void Fatal(string message, Exception exception)
        {
            if (!IsFatalEnabled) return;
            m_logWriterManager.Fatal(Format(message), exception);
        }

        public void Info(string message)
        {
            if (!IsInfoEnabled) return;
            m_logWriterManager.Info(Format(message));
        }

        public void Info(string message, Exception exception)
        {
            if (!IsInfoEnabled) return;
            m_logWriterManager.Info(Format(message), exception);
        }

        public void Warn(string message)
        {
            if (!IsWarnEnabled) return;
            m_logWriterManager.Warn(Format(message));
        }

        public void Warn(string message, Exception exception)
        {
            if (!IsWarnEnabled) return;
            m_logWriterManager.Warn(Format(message), exception);
        }
    }
}
