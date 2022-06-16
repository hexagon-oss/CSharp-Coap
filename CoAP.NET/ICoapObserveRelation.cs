using System;
using System.Collections.Generic;
using System.Text;
using Com.AugustCellars.CoAP.Observe;

namespace Com.AugustCellars.CoAP
{
    public interface ICoapObserveRelation
    {
        event Action<Response> OnResponseUpdated;

        bool Reconnect { get; set; }

        /// <summary>
        /// Return the original request that caused the observe relationship to be established.
        /// </summary>
        Request Request { get; }

        /// <summary>
        /// Return the most recent response that was received from the observe relationship.
        /// </summary>
        Response Current { get; set; }

        /// <summary>
        /// Return the orderer.  This is the filter function that is used to determine if
        /// a new notification is really new or if it is a repeat or old data.
        /// </summary>
        ObserveNotificationOrderer Orderer { get; }

        /// <summary>
        /// Is the observe relationship canceled?
        /// 
        /// Setting this property does not send a request to the server to remove the observation.
        /// </summary>
        Boolean Canceled { get; set; }

        void ReactiveCancel();

        /// <summary>
        /// Send a message to the resource being observed that we want to cancel
        /// the observation.
        /// </summary>
        void ProactiveCancel();
    }
}
