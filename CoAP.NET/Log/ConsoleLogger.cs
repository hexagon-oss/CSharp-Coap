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
    /// Create the internal console writer - which is just about ready to become a lie
    /// </summary>
    public sealed class ConsoleLogger : TextWriterLogger
    {
        public ConsoleLogger(Type type)
        : base(type == null ? string.Empty : type.Name, Console.Out)
        {

        }
    }
}
