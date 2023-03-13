using System;

namespace Com.AugustCellars.CoAP
{
    public interface IResponse : IMessage
    {
        /// <summary>
        /// Gets the response status code.
        /// </summary>
        StatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Round-Trip Time of this response.
        /// </summary>
// ReSharper disable once InconsistentNaming
        Double RTT { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this response is the last response of an exchange.
        /// </summary>
        Boolean Last { get; set; }

        /// <summary>
        /// Get the payload as a string
        /// </summary>
        String ResponseText { get; }

        /// <summary>
        /// Return underlying session
        /// </summary>
        ISession Session { get; set; }
    }
}