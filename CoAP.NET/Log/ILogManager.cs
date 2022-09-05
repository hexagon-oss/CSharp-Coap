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
    /// Provides methods to acquire <see cref="CoAP.Log.ILogger"/>.
    /// </summary>
    public interface ILogManager
    {
        /// <summary>
        /// Adds a listener logger that the logging instances will write to.
        /// </summary>
        /// <param name="logger">The logger to be added.</param>
        void AddLogWriter(ILogWriter logger);

        /// <summary>
        /// Removes a listener logger.
        /// </summary>
        /// <param name="logger"></param>
        void RemoveLogWriter(ILogWriter logger);
    }
}
