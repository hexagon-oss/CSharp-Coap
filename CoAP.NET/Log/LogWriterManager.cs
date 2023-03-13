using System;
using System.Collections.Generic;
using System.Text;

namespace Com.AugustCellars.CoAP.Log
{
    internal sealed class LogWriterManager : ILogWriter, ILogManager
    {
        public HashSet<ILogWriter> m_logWriters = new HashSet<ILogWriter>();

        public void AddLogWriter(ILogWriter logWriter)
        {
            m_logWriters.Add(logWriter);
        }

        public void RemoveLogWriter(ILogWriter logWriter)
        {
            m_logWriters.Remove(logWriter);
        }

        public void Debug(string message)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Debug(message);
            }
        }

        public void Debug(string message, Exception exception)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Debug(message, exception);
            }
        }

        public void Error(string message)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Error(message);
            }
        }

        public void Error(string message, Exception exception)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Error(message, exception);
            }
        }

        public void Fatal(string message)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Fatal(message);
            }
        }

        public void Fatal(string message, Exception exception)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Fatal(message, exception);
            }
        }

        public void Info(string message)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Info(message);
            }
        }

        public void Info(string message, Exception exception)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Info(message, exception);
            }
        }

        public void Warn(string message)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Warn(message);
            }
        }

        public void Warn(string message, Exception exception)
        {
            foreach (var logWriter in m_logWriters)
            {
                logWriter.Warn(message, exception);
            }
        }
    }
}
