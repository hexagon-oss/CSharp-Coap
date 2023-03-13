using System;
using System.Collections.Generic;

namespace Com.AugustCellars.CoAP
{
    public interface IMessage
    {
        /// <summary>
        /// Occurs when this message is retransmitting.
        /// </summary>
        event EventHandler Retransmitting;

        /// <summary>
        /// Occurs when this message has been acknowledged by the remote endpoint.
        /// </summary>
        event EventHandler Acknowledged;

        /// <summary>
        /// Occurs when this message has been rejected by the remote endpoint.
        /// </summary>
        event EventHandler Rejected;

        /// <summary>
        /// Occurs when the client stops retransmitting the message and still has
        /// not received anything from the remote endpoint.
        /// </summary>
        event EventHandler TimedOut;

        /// <summary>
        /// Occurs when this message has been canceled.
        /// </summary>
        event EventHandler Cancelled;

        /// <summary>
        /// Gets or sets the type of this CoAP message.
        /// </summary>
        MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the ID of this CoAP message.
        /// </summary>
// ReSharper disable once InconsistentNaming
        int ID { get; set; }

        /// <summary>
        /// Gets the code of this CoAP message.
        /// </summary>
        int Code { get; set; }

        /// <summary>
        /// Gets the code's string representation of this CoAP message.
        /// </summary>
        string CodeString { get; }

        /// <summary>
        /// Gets a value that indicates whether this CoAP message is a request message.
        /// </summary>
        bool IsRequest { get; }

        /// <summary>
        /// Gets a value that indicates whether this CoAP message is a response message.
        /// </summary>
        bool IsResponse { get; }

        bool IsSignal { get; }

        /// <summary>
        /// Gets or sets the 0-8 byte token.
        /// </summary>
        byte[] Token { get; set; }

        /// <summary>
        /// Gets the token represented as a string.
        /// </summary>
        string TokenString { get; }

        /// <summary>
        /// Gets or sets the destination endpoint.
        /// </summary>
        System.Net.EndPoint Destination { get; set; }

        /// <summary>
        /// Gets or sets the source endpoint.
        /// </summary>
        System.Net.EndPoint Source { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message has been acknowledged.
        /// </summary>
        bool IsAcknowledged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message has been rejected.
        /// </summary>
        bool IsRejected { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether this CoAP message has timed out.
        /// Confirmable messages in particular might timeout.
        /// </summary>
        bool IsTimedOut { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether this CoAP message is canceled.
        /// </summary>
        bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message is a duplicate.
        /// </summary>
        bool Duplicate { get; set; }

        /// <summary>
        /// Gets or sets the serialized message as byte array, or null if not serialized yet.
        /// </summary>
        byte[] Bytes { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this message has been received or sent,
        /// or <see cref="DateTime.MinValue"/> if neither has happened yet.
        /// </summary>
        DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the max times this message should be retransmitted if no ACK received.
        /// A value of 0 means that the <see cref="ICoapConfig.MaxRetransmit"/>
        /// should be taken into account, while a negative means NO retransmission.
        /// The default value is 0.
        /// </summary>
        int MaxRetransmit { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in milliseconds after which this message will time out.
        /// A value of 0 indicates that the time should be decided automatically,
        /// while a negative that never time out. The default value is 0.
        /// </summary>
        int AckTimeout { get; set; }

        /// <summary>
        /// Gets the size of the payload of this CoAP message.
        /// </summary>
        int PayloadSize { get; }

        /// <summary>
        /// Gets or sets the payload of this CoAP message.
        /// </summary>
        byte[] Payload { get; set; }

        /// <summary>
        /// Gets or sets the payload of this CoAP message in string representation.
        /// </summary>
        string PayloadString { get; set; }

        /// <summary>
        /// Gets If-Match options.
        /// </summary>
        IEnumerable<byte[]> IfMatches { get; }

        /// <summary>
        /// Return all ETags on the message
        /// </summary>
        IEnumerable<byte[]> ETags { get; }

        /// <summary>
        /// Get - Does the message have an IfNoneMatch option?
        /// Set - Set or clear IfNoneMatch option to value
        /// </summary>
        bool IfNoneMatch { get; set; }

        /// <summary>
        /// Get/Set URI host option
        /// </summary>
        string UriHost { get; set; }

        /// <summary>
        /// Get/Set the UriPath options
        /// </summary>
        string UriPath { get; set; }

        /// <summary>
        /// Get the UriPath as an enumeration
        /// </summary>
        IEnumerable<string> UriPaths { get; }

        /// <summary>
        /// Get/Set UriQuery properties
        /// </summary>
        string UriQuery { get; set; }

        /// <summary>
        /// Get enumeration of all UriQuery properties
        /// </summary>
        IEnumerable<string> UriQueries { get; }

        /// <summary>
        /// Get/Set the UriPort option
        /// </summary>
        int UriPort { get; set; }

        /// <summary>
        /// Return location path and query options
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets or set the location-path of this CoAP message.
        /// </summary>
        string LocationPath { get; set; }

        /// <summary>
        /// Return enumeration of all Location Path options
        /// </summary>
        IEnumerable<string> LocationPaths { get; }

        /// <summary>
        /// Return all Location-Query options
        /// </summary>
        string LocationQuery { get; set; }

        /// <summary>
        /// Return enumerator of all Location-Query options
        /// </summary>
        IEnumerable<string> LocationQueries { get; }

        /// <summary>
        /// Gets or sets the content-type of this CoAP message.
        /// </summary>
        int ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content-format of this CoAP message,
        /// same as ContentType, only another name.
        /// </summary>
        int ContentFormat { get; set; }

        /// <summary>
        /// Gets or sets the max-age of this CoAP message.
        /// </summary>
        long MaxAge { get; set; }

        /// <summary>
        /// Get first Accept option
        /// Set - add additional options or remove all for MediaType.Undefined
        /// </summary>
        int Accept { get; set; }

        /// <summary>
        /// Get/Set the Proxy-Uri option
        /// </summary>
        Uri ProxyUri { get; set; }

        /// <summary>
        /// Get/Set the ProxySchema option on a message
        /// </summary>
        string ProxyScheme { get; set; }

        /// <summary>
        /// Get/Set the observe option value
        /// </summary>
        int? Observe { get; set; }

        /// <summary>
        /// Gets or sets the Size1 option. Be <code>null</code> if not set.
        /// </summary>
        int? Size1 { get; set; }

        /// <summary>
        /// Gets or sets the Size2 option. Be <code>null</code> if not set.
        /// </summary>
        int? Size2 { get; set; }

        /// <summary>
        /// Get/Set the Block1 option
        /// </summary>
        BlockOption Block1 { get; set; }

        /// <summary>
        /// Get/Set the Block1 option
        /// </summary>
        BlockOption Block2 { get; set; }

        /// <summary>
        /// Get/Set the OSCOAP option value
        /// </summary>
        OSCOAP.OscoapOption Oscoap { get; set; }

        /// <summary>
        /// Sets the payload of this CoAP message.
        /// </summary>
        /// <param name="payload">The string representation of the payload</param>
        Message SetPayload(string payload);

        /// <summary>
        /// Sets the payload of this CoAP message.
        /// </summary>
        /// <param name="payload">The string representation of the payload</param>
        /// <param name="mediaType">The content-type of the payload</param>
        Message SetPayload(string payload, int mediaType);

        /// <summary>
        /// Sets the payload of this CoAP message.
        /// </summary>
        /// <param name="payload">the payload bytes</param>
        /// <param name="mediaType">the content-type of the payload</param>
        Message SetPayload(byte[] payload, int mediaType);

        /// <summary>
        /// Cancels this message.
        /// </summary>
        void Cancel();

        /// <summary>
        /// To string.
        /// </summary>
        string ToString();

        /// <summary>
        /// Equals.
        /// </summary>
        bool Equals(object obj);

        /// <summary>
        /// Get hash code.
        /// </summary>
        int GetHashCode();

        /// <summary>
        /// Checks if a value is matched by the IfMatch options.
        /// If no IfMatch options exist, then return true.
        /// </summary>
        /// <param name="what">ETag value to check</param>
        /// <returns>what is in the IfMatch list</returns>
        bool IsIfMatch(byte[] what);

        /// <summary>
        /// Add an If-Match option with an ETag
        /// </summary>
        /// <param name="opaque">ETag to add</param>
        /// <returns>Current message</returns>
        Message AddIfMatch(byte[] opaque);

        /// <summary>
        /// Remove an If-Match option from the message
        /// </summary>
        /// <param name="opaque">ETag value to remove</param>
        /// <returns>Current message</returns>
        Message RemoveIfMatch(byte[] opaque);

        /// <summary>
        /// Remove all If-Match options from the message
        /// </summary>
        /// <returns>Current message</returns>
        Message ClearIfMatches();

        /// <summary>
        /// Does the message contain a specific ETag option value?
        /// </summary>
        /// <param name="what">EETag value to check for</param>
        /// <returns>true if present</returns>
        bool ContainsETag(byte[] what);

        /// <summary>
        /// Add an ETag option to the message
        /// </summary>
        /// <param name="opaque">ETag to add</param>
        /// <returns>Current Message</returns>
        Message AddETag(byte[] opaque);

        /// <summary>
        /// Remove an ETag option from a message
        /// </summary>
        /// <param name="opaque">ETag to be removed</param>
        /// <returns>Current message</returns>
        Message RemoveETag(byte[] opaque);

        /// <summary>
        /// Clear all ETags from a message
        /// </summary>
        /// <returns>Current message</returns>
        Message ClearETags();

        /// <summary>
        /// Add Uri Path element corresponding to the path given
        /// </summary>
        /// <param name="path">Path element</param>
        /// <returns>Current Message</returns>
        Message AddUriPath(string path);

        /// <summary>
        /// Remove path element from options
        /// </summary>
        /// <param name="path">Current message</param>
        /// <returns></returns>
        Message RemoveUriPath(string path);

        /// <summary>
        /// Remove all Uri Path options
        /// </summary>
        /// <returns>Current message</returns>
        Message ClearUriPath();

        /// <summary>
        /// Add one Uri Query option
        /// </summary>
        /// <param name="query">query to add</param>
        /// <returns>Current Message</returns>
        Message AddUriQuery(string query);

        /// <summary>
        /// Remove first occurance of URI Query
        /// </summary>
        /// <param name="query">Query to remove</param>
        /// <returns>Current message</returns>
        Message RemoveUriQuery(string query);

        /// <summary>
        /// Remove all Uri Query options
        /// </summary>
        /// <returns>Current message</returns>
        Message ClearUriQuery();

        /// <summary>
        /// Add a Location Path option
        /// </summary>
        /// <param name="path">option to add</param>
        /// <returns>Current message</returns>
        Message AddLocationPath(string path);

        /// <summary>
        /// Remove specified location path element
        /// </summary>
        /// <param name="path">Element to remove</param>
        /// <returns>Current message</returns>
        Message RemoveLocationPath(string path);

        /// <summary>
        /// Clear all Location-Path options from the message
        /// </summary>
        /// <returns>Current Message</returns>
        Message ClearLocationPath();

        /// <summary>
        /// Add a Location-Query option
        /// </summary>
        /// <param name="query">query element to add</param>
        /// <returns>Current message</returns>
        Message AddLocationQuery(string query);

        /// <summary>
        /// Remove a given Location-Query from the message
        /// </summary>
        /// <param name="query">query to remove</param>
        /// <returns>Current message</returns>
        Message RemoveLocationQuery(string query);

        /// <summary>
        /// Remove all Location-Query options
        /// </summary>
        /// <returns>Current message</returns>
        Message ClearLocationQuery();

        /// <summary>
        /// Create a Block1 option and add it to the message
        /// </summary>
        /// <param name="szx">Size of blocks to use</param>
        /// <param name="m">more data?</param>
        /// <param name="num">block index</param>
        void SetBlock1(int szx, bool m, int num);

        /// <summary>
        /// Create a Block2 option and add it to the message
        /// </summary>
        /// <param name="szx">Size of blocks to use</param>
        /// <param name="m">more data?</param>
        /// <param name="num">block index</param>
        void SetBlock2(int szx, bool m, int num);

        /// <summary>
        /// Adds an option to the list of options of this CoAP message.
        /// </summary>
        /// <param name="option">the option to add</param>
        Message AddOption(Option option);

        /// <summary>
        /// Adds all option to the list of options of this CoAP message.
        /// </summary>
        /// <param name="options">the options to add</param>
        void AddOptions(IEnumerable<Option> options);

        /// <summary>
        /// Removes all options of the given type from this CoAP message.
        /// </summary>
        /// <param name="optionType">the type of option to remove</param>
        bool RemoveOptions(OptionType optionType);

        /// <summary>
        /// Gets all options of the given type.
        /// </summary>
        /// <param name="optionType">the option type</param>
        /// <returns></returns>
        IEnumerable<Option> GetOptions(OptionType optionType);

        /// <summary>
        /// Gets a sorted list of all options.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Option> GetOptions();

        /// <summary>
        /// Sets an option.
        /// </summary>
        /// <param name="opt">the option to set</param>
        void SetOption(Option opt);

        /// <summary>
        /// Sets all options with the specified option type.
        /// </summary>
        /// <param name="options">the options to set</param>
        void SetOptions(IEnumerable<Option> options);

        /// <summary>
        /// Checks if this CoAP message has options of the specified option type.
        /// </summary>
        /// <param name="type">the option type</param>
        /// <returns>rrue if options of the specified type exist</returns>
        bool HasOption(OptionType type);

        /// <summary>
        /// Gets the first option of the specified option type.
        /// </summary>
        /// <param name="optionType">the option type</param>
        /// <returns>the first option of the specified type, or null</returns>
        Option GetFirstOption(OptionType optionType);
    }
}