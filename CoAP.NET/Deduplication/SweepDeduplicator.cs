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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Com.AugustCellars.CoAP.Log;
#if NETSTANDARD1_3
using System.Threading;
#else
using System.Timers;
#endif
#if LOG_SWEEP_DEDUPLICATOR
using Com.AugustCellars.CoAP.Log;
#endif
using Com.AugustCellars.CoAP.Net;

namespace Com.AugustCellars.CoAP.Deduplication
{
    class SweepDeduplicator : IDeduplicator
    {
#if LOG_SWEEP_DEDUPLICATOR
        static readonly ILogger _Log = LogManager.GetLogger(typeof(SweepDeduplicator));
#endif

        private readonly ConcurrentDictionary<Exchange.KeyTokenID, Exchange> _incommingMessages
            = new ConcurrentDictionary<Exchange.KeyTokenID, Exchange>();
        private Timer _timer;
        private readonly ICoapConfig _config;
		// private int _period;

		private static readonly ILogger _Log = Logging.GetLogger(typeof(SweepDeduplicator));

		public SweepDeduplicator(ICoapConfig config)
        {
	        _Log.Debug("Sweep created.");
			_config = config;
#if NETSTANDARD1_3
            _period = (int) config.MarkAndSweepInterval;
#else
            _timer = new Timer(config.MarkAndSweepInterval);
            _timer.Elapsed += Sweep;
#endif
        }

#if NETSTANDARD1_3
        private static void Sweep(Object obj)
#else
        private void Sweep(Object obj, ElapsedEventArgs e)
#endif
        {
	        _Log.Debug($"Sweeping before: {_incommingMessages.Count} items");
#if NETSTANDARD1_3
            SweepDeduplicator sender = obj as SweepDeduplicator;
#else
			SweepDeduplicator sender = this;
#endif
#if LOG_SWEEP_DEDUPLICATOR
            log.Debug(m => m("Start Mark-And-Sweep with {0} entries", _incommingMessages.Count));
#endif

            DateTime oldestAllowed = DateTime.Now.AddMilliseconds(-sender._config.ExchangeLifetime);
            List<Exchange.KeyTokenID> keysToRemove = new List<Exchange.KeyTokenID>();
            foreach (KeyValuePair<Exchange.KeyTokenID, Exchange> pair in sender._incommingMessages) {
                if (pair.Value.Timestamp < oldestAllowed) {
#if LOG_SWEEP_DEDUPLICATOR
                    log.Debug(m => m("Mark-And-Sweep removes {0}", pair.Key));
#endif
                    keysToRemove.Add(pair.Key);
                }
            }
            if (keysToRemove.Count > 0) {
                Exchange ex;
                foreach (Exchange.KeyTokenID key in keysToRemove) {
                    sender._incommingMessages.TryRemove(key, out ex);
                }
            }
            _Log.Debug($"Sweeping afterwards: {_incommingMessages.Count} items");
		}

        /// <inheritdoc/>
        public void Start()
        {
	        _Log.Debug("Sweep started.");

#if NETSTANDARD1_3
            _timer = new Timer(Sweep, this, _period, _period);
#else
			_timer.Start();
#endif
        }

        /// <inheritdoc/>
        public void Stop()
        {
	        _Log.Debug("Sweep stopped.");

#if NETSTANDARD1_3
            _timer.Dispose();
            _timer = null;
#else
			_timer.Stop();
#endif
            Clear();
        }

        /// <inheritdoc/>
        public void Clear()
        {
	        _Log.Debug($"Sweep cleared {_incommingMessages.Count}.");
			_incommingMessages.Clear();
        }

        /// <inheritdoc/>
        public Exchange FindPrevious(Exchange.KeyTokenID keyToken, Exchange exchange)
        {
            Exchange prev = null;
            _incommingMessages.AddOrUpdate(keyToken, exchange, (k, v) =>
            {
                prev = v;
                return exchange;
            });
            return prev;
        }

        /// <inheritdoc/>
        public Exchange Find(Exchange.KeyTokenID keyToken)
        {
            Exchange prev;
            _incommingMessages.TryGetValue(keyToken, out prev);
            return prev;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
        }
    }
}
