using System;

namespace Com.AugustCellars.CoAP.Log
{
    public interface ILogWriter
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        void Debug(string message, Exception exception);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        void Error(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        void Error(string message, Exception exception);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        void Fatal(string message);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        void Fatal(string message, Exception exception);

        /// <summary>
        /// Logs an info message.
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Logs an info message.
        /// </summary>
        void Info(string message, Exception exception);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void Warn(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void Warn(string message, Exception exception);
    }
}
