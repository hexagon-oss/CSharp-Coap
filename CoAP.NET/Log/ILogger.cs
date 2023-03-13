/*
 * Copyright (c) 2011-2012, Longxiang He <helongxiang@smeshlink.com>,
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
    /// Provides methods to log messages.
    /// </summary>
    internal interface ILogger : ILogWriter
    {
        /// <summary>
        /// The log level of the logger.
        /// </summary>
        LogLevel Level { get; set; }
        /// <summary>
        /// Is debug enabled?
        /// </summary>
        bool IsDebugEnabled { get; }
        /// <summary>
        /// Is error enabled?
        /// </summary>
        bool IsErrorEnabled { get; }
        /// <summary>
        /// Is fatal enabled?
        /// </summary>
        bool IsFatalEnabled { get; }
        /// <summary>
        /// Is info enabled?
        /// </summary>
        bool IsInfoEnabled { get; }
        /// <summary>
        /// Is warning enabled?
        /// </summary>
        bool IsWarnEnabled { get; }
    }
}
