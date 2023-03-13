/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace Com.AugustCellars.CoAP.Log
{
    /// <summary>
    /// Logger that writes logs to a <see cref="System.IO.TextWriter"/>.
    /// </summary>
    public class TextWriterLogger : ILogWriter
    {
        private readonly System.IO.TextWriter _Writer;

        private readonly String _logName;

        /// <summary>
        /// Instantiates.
        /// </summary>
        public TextWriterLogger(String logName, System.IO.TextWriter writer)
        {
            _Writer = writer;
            _logName = logName;
        }

        /// <inheritdoc/>
        public void Error(Object sender, String msg, params Object[] args)
        {
            string text = String.Format(msg, args);

            Log("ERROR", text, null);
        }

        /// <inheritdoc/>
        public void Warning(Object sender, String msg, params Object[] args)
        {
            string text = String.Format(msg, args);

            Log("WARNING", text, null);
        }

        /// <inheritdoc/>
        public void Info(Object sender, String msg, params Object[] args)
        {
            string text = String.Format(msg, args);

            Log("INFO", text, null);
        }

        /// <inheritdoc/>
        public void Debug(Object sender, String msg, params Object[] args)
        {
            string text = String.Format(msg, args);

            Log("DEBUG", text, null);
        }

        /// <inheritdoc/>
        public void Debug(string message)
        {
            Log("DEBUG", message, null);
        }

        /// <inheritdoc/>
        public void Debug(string message, Exception exception)
        {
            Log("DEBUG", message, exception);
        }

        /// <inheritdoc/>
        public void Error(string message)
        {
            Log("Error", message, null);
        }

        /// <inheritdoc/>
        public void Error(string message, Exception exception)
        {
            Log("Error", message, exception);
        }

        /// <inheritdoc/>
        public void Fatal(string message)
        {
            Log("Fatal", message, null);
        }

        /// <inheritdoc/>
        public void Fatal(string message, Exception exception)
        {
            Log("Fatal", message, exception);
        }

        /// <inheritdoc/>
        public void Info(string message)
        {
            Log("Info", message, null);
        }

        /// <inheritdoc/>
        public void Info(string message, Exception exception)
        {
            Log("Info", message, exception);
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
            Log("Warn", message, null);
        }

        /// <inheritdoc/>
        public void Warn(string message, Exception exception)
        {
            Log("Warn", message, exception);
        }

        private void Log(String level, string message, Exception exception)
        {
            try {
                String log = "";
                if (_logName != null) {
                    log = "[" + _logName + "]";
                }

                String text = $"{DateTime.Now:T} {log} {level} - {message}";
                if (exception != null) {
                    text += exception.ToString();
                }

                _Writer.WriteLine(text);
                _Writer.Flush();
            }
            catch (Exception e) {
                //  should never get here
                ;
                Console.WriteLine("PANIC!!!!" + e.Message);
            }
        }
    }
}
