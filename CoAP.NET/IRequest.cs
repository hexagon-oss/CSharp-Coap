using System;
using System.Threading;
using Com.AugustCellars.CoAP.Net;
using Com.AugustCellars.CoAP.Observe;
using Com.AugustCellars.CoAP.OSCOAP;

namespace Com.AugustCellars.CoAP
{
    public interface IRequest : IMessage
    {
        /// <summary>
        /// Fired when a response arrives.
        /// </summary>
        event EventHandler<ResponseEventArgs> Respond;

        /// <summary>
        /// Occurs when a block of response arrives in a blockwise transfer.
        /// </summary>
        event EventHandler<ResponseEventArgs> Responding;

        /// <summary>
        /// Occurs when a observing request is re-registering.
        /// </summary>
        event EventHandler<ReregisterEventArgs> Reregistering;

        /// <summary>
        /// Gets the request method.
        /// </summary>
        Method Method { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this request is a multicast request or not.
        /// </summary>
        new bool IsMulticast { get; }

        /// <summary>
        /// Gets or sets the URI of this CoAP message.
        /// </summary>
// ReSharper disable once InconsistentNaming
        Uri URI { get; set; }

        /// <summary>
        /// Return the endpoint to use for the request
        /// </summary>
        IEndPoint EndPoint { get; set; }

        /// <summary>
        /// The observe relationship if one exists.
        /// </summary>
        CoapObserveRelation ObserveRelation { get; set; }

        /// <summary>
        /// Gets or sets the response to this request.
        /// </summary>
        Response Response { get; set; }

        /// <summary>
        /// Should we attempt to reconnect to keep an observe relationship fresh
        /// in the event the MAX-AGE expires on the current value?
        /// </summary>
        bool ObserveReconnect { get; set; }

        /// <summary>
        /// Set the context structure used to OSCORE protect the message
        /// </summary>
        SecurityContext OscoapContext { get; set; }

        SecurityContext OscoreContext { get; set; }

        /// <summary>
        /// Return the security context associated with TLS.
        /// </summary>
        ISecureSession TlsContext { get; }

        /// <summary>
        /// Give information about what session the request came from.
        /// </summary>
        ISession Session { get; set; }

        /// <summary>
        /// Alias function to set the URI on the request
        /// </summary>
        /// <param name="uri">URI to send the message to</param>
        /// <returns>Current message</returns>
        Request SetUri(String uri);

        /// <summary>
        /// Sets CoAP's observe option. If the target resource of this request
        /// responds with a success code and also sets the observe option, it will
        /// send more responses in the future whenever the resource's state changes.
        /// </summary>
        /// <returns>Current request</returns>
        Request MarkObserve();

        /// <summary>
        /// Sets CoAP's observe option to the value of 1 to proactively cancel.
        /// </summary>
        /// <returns>Current request</returns>
        Request MarkObserveCancel();

        /// <summary>
        /// Gets the value of a query parameter as a <code>String</code>,
        /// or <code>null</code> if the parameter does not exist.
        /// </summary>
        /// <param name="name">a <code>String</code> specifying the name of the parameter</param>
        /// <returns>a <code>String</code> representing the single value of the parameter</returns>
        string GetParameter(string name);

        /// <summary>
        /// Send the request.
        /// </summary>
        void Execute();

        /// <summary>
        /// Sends this message.
        /// </summary>
        Request Send();

        /// <summary>
        /// Sends the request over the specified endpoint.
        /// </summary>
        Request Send(IEndPoint endpoint);

        /// <summary>
        /// Wait for a response.
        /// </summary>
        /// <exception cref="System.Threading.ThreadInterruptedException"></exception>
        IResponse WaitForResponse();

        /// <summary>
        /// Wait for a response.
        /// </summary>
        /// <param name="millisecondsTimeout">the maximum time to wait in milliseconds</param>
        /// <returns>the response, or null if timeout occured</returns>
        /// <exception cref="System.Threading.ThreadInterruptedException"></exception>
        IResponse WaitForResponse(int millisecondsTimeout);

        IResponse WaitForResponse(TimeSpan timeout, CancellationToken? cancellationToken = null);
    }
}
