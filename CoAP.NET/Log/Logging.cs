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
using System.Linq;

namespace Com.AugustCellars.CoAP.Log
{
    /// <summary>
    /// Log manager.
    /// </summary>
    public sealed class Logging
    {
        static Logging()
        {
            _Manager = new LogWriterManager();
        }

        public static LogLevel Level { get; set; } = LogLevel.Debug;

        private static readonly LogWriterManager _Manager;
        public static ILogManager Manager => _Manager;

        internal static ILogger GetLogger(Type type)
        {
            return new ClassLogger(type, Level, _Manager);
        }
    }

    /// <summary>
    /// Log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// All logs.
        /// </summary>
        All,
        /// <summary>
        /// Debugs and above.
        /// </summary>
        Debug,
        /// <summary>
        /// Infos and above.
        /// </summary>
        Info,
        /// <summary>
        /// Warnings and above.
        /// </summary>
        Warning,
        /// <summary>
        /// Errors and above.
        /// </summary>
        Error,
        /// <summary>
        /// Fatal only.
        /// </summary>
        Fatal,
        /// <summary>
        /// No logs.
        /// </summary>
        None
    }
}
