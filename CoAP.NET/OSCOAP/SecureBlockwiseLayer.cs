﻿/*
 * Copyright (c) 2019-2020, Jim Schaad <ietf@augustcellars.com>
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;

using Com.AugustCellars.CoAP.Stack;
using Com.AugustCellars.CoAP.Log;
using Com.AugustCellars.CoAP.Net;

namespace Com.AugustCellars.CoAP.OSCOAP
{
    public class SecureBlockwiseLayer : AbstractLayer
    {
        static readonly ILogger log = Logging.GetLogger(typeof(SecureBlockwiseLayer));

        private int _maxMessageSize;
        private int _defaultBlockSize;
        private int _blockTimeout;

        public SecureBlockwiseLayer(ICoapConfig config)
        {
            _maxMessageSize = config.OSCOAP_MaxMessageSize;
            _defaultBlockSize = config.OSCOAP_DefaultBlockSize;
            _blockTimeout = config.OSCOAP_BlockwiseStatusLifetime;
            if (log.IsDebugEnabled) {
                log.Debug("SecureBlockwiseLayer uses MaxMessageSize: " + _maxMessageSize + " and DefaultBlockSize:" + _defaultBlockSize);
            }

            config.PropertyChanged += ConfigChanged;

        }

        void ConfigChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ICoapConfig config = (ICoapConfig) sender;
            if (string.Equals(e.PropertyName, "OSCOAP_MaxMessageSize")) {
                _maxMessageSize = config.OSCOAP_MaxMessageSize;
            }
            else if (string.Equals(e.PropertyName, "OSCOAP_DefaultBlockSize")) {
                _defaultBlockSize = config.OSCOAP_DefaultBlockSize;
            }
            else if (string.Equals(e.PropertyName, "OSCOAP_BlockwiseStatusLifetime")) {
                _blockTimeout = config.OSCOAP_BlockwiseStatusLifetime;
            }
        }

        /// <inheritdoc/>
        public override void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            //  This assumes we don't change individual options - if this is not true then we need to do a deep copy.
            exchange.PreSecurityOptions = request.GetOptions().ToList();

            if ((request.Oscoap == null) && (exchange.OscoreContext == null)) {
                base.SendRequest(nextLayer, exchange, request);
            }
            else if (request.HasOption(OptionType.Block2) && request.Block2.NUM > 0) {
                // This is the case if the user has explicitly added a block option
                // for random access.
                // Note: We do not regard it as random access when the block num is
                // 0. This is because the user might just want to do early block
                // size negotiation but actually wants to receive all blocks.
                log.Debug("Request carries explicit defined block2 option: create random access blockwise status");

                BlockwiseStatus status = new BlockwiseStatus(request.ContentFormat);
                BlockOption block2 = request.Block2;
                status.CurrentSZX = block2.SZX;
                status.CurrentNUM = block2.NUM;
                status.IsRandomAccess = true;
                exchange.OSCOAP_ResponseBlockStatus = status;
                base.SendRequest(nextLayer, exchange, request);
            }
            else if (RequiresBlockwise(request)) {
                // This must be a large POST or PUT request
                log.Debug(string.Format(CultureInfo.InvariantCulture, $"Request payload {request.PayloadSize}/{_maxMessageSize} requires Blockwise."));

                BlockwiseStatus status = FindRequestBlockStatus(exchange, request);
                Request block = GetNextRequestBlock(request, exchange.PreSecurityOptions, status);
                exchange.OscoreRequestBlockStatus = status;
                exchange.CurrentRequest = block;

                log.Debug($"Block message to send: {block}");
                base.SendRequest(nextLayer, exchange, block);
            }
            else {
                exchange.CurrentRequest = request;
                base.SendRequest(nextLayer, exchange, request);
            }
        }

        /// <inheritdoc/>
        public override void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            if ((request.Oscoap == null) && (exchange.OscoreContext == null)) {
                base.ReceiveRequest(nextLayer, exchange, request);
            }
            else if (request.HasOption(OptionType.Block1)) {
                // This must be a large POST or PUT request
                BlockOption block1 = request.Block1;
                if (log.IsDebugEnabled) {
                    log.Debug("Request contains block1 option " + block1);
                }

                BlockwiseStatus status = FindRequestBlockStatus(exchange, request);
                if (block1.NUM == 0 && status.CurrentNUM > 0) {
                    // reset the blockwise transfer
                        log.Debug("Block1 num is 0, the client has restarted the blockwise transfer. Reset status.");
                    status = new BlockwiseStatus(request.ContentType);
                    exchange.OscoreRequestBlockStatus = status;
                }

                if (block1.NUM == status.CurrentNUM) {
                    if (request.ContentType == status.ContentFormat) {
                        status.AddBlock(request.Payload);
                    }
                    else {
                        Response error = Response.CreateResponse(request, StatusCode.RequestEntityIncomplete);
                        error.AddOption(new BlockOption(OptionType.Block1, block1.NUM, block1.SZX, block1.M));
                        error.SetPayload("Changed Content-Format");

                        exchange.CurrentResponse = error;
                        base.SendResponse(nextLayer, exchange, error);
                        return;
                    }

                    status.CurrentNUM = status.CurrentNUM + 1;
                    if (block1.M) {
                            log.Debug("There are more blocks to come. Acknowledge this block.");

                        Response piggybacked = Response.CreateResponse(request, StatusCode.Continue);
                        piggybacked.AddOption(new BlockOption(OptionType.Block1, block1.NUM, block1.SZX, true));
                        piggybacked.Last = false;

                        exchange.CurrentResponse = piggybacked;
                        base.SendResponse(nextLayer, exchange, piggybacked);

                        // do not assemble and deliver the request yet
                    }
                    else {
                            log.Debug("This was the last block. Deliver request");

                        // Remember block to acknowledge. TODO: We might make this a boolean flag in status.
                        exchange.Block1ToAck = block1;

                        // Block2 early negotiation
                        EarlyBlock2Negotiation(exchange, request);

                        // Assemble and deliver
                        Request assembled = new Request(request.Method);
                        AssembleMessage(status, assembled, request);

                        exchange.Request = assembled;
                        base.ReceiveRequest(nextLayer, exchange, assembled);
                    }
                }
                else {
                    // ERROR, wrong number, Incomplete
                    if (log.IsWarnEnabled) {
                        log.Warn("Wrong block number. Expected " + status.CurrentNUM + " but received " + block1.NUM + ". Respond with 4.08 (Request Entity Incomplete).");
                    }

                    Response error = Response.CreateResponse(request, StatusCode.RequestEntityIncomplete);
                    error.AddOption(new BlockOption(OptionType.Block1, block1.NUM, block1.SZX, block1.M));
                    error.SetPayload("Wrong block number");
                    exchange.CurrentResponse = error;
                    base.SendResponse(nextLayer, exchange, error);
                }
            }
            else if (exchange.Response != null && request.HasOption(OptionType.Block2)) {
                // The response has already been generated and the client just wants
                // the next block of it
                BlockOption block2 = request.Block2;
                Response response = exchange.Response;
                BlockwiseStatus status = FindResponseBlockStatus(exchange, response);
                status.CurrentNUM = block2.NUM;
                status.CurrentSZX = block2.SZX;

                Response block = GetNextResponseBlock(response, status);
                block.Token = request.Token;
                block.RemoveOptions(OptionType.Observe);

                if (status.Complete) {
                    // clean up blockwise status
                    if (log.IsDebugEnabled) {
                        log.Debug("Ongoing is complete " + status);
                    }

                    exchange.OSCOAP_ResponseBlockStatus = null;
                    ClearBlockCleanup(exchange);
                }
                else {
                    if (log.IsDebugEnabled) {
                        log.Debug("Ongoing is continuing " + status);
                    }
                }

                exchange.CurrentResponse = block;
                base.SendResponse(nextLayer, exchange, block);

            }
            else {
                EarlyBlock2Negotiation(exchange, request);

                exchange.Request = request;
                base.ReceiveRequest(nextLayer, exchange, request);
            }
        }

        /// <inheritdoc/>
        public override void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            if ((exchange.OscoreContext == null) && (response.Oscoap == null)) {
                base.SendResponse(nextLayer, exchange, response);
                return;
            }

            BlockOption block1 = exchange.Block1ToAck;
            if (block1 != null) {
                exchange.Block1ToAck = null;
            }

            if (RequiresBlockwise(exchange, response)) {
                if (log.IsDebugEnabled) {
                    log.Debug("Response payload " + response.PayloadSize + "/" + _maxMessageSize + " requires Blockwise");
                }

                BlockwiseStatus status = FindResponseBlockStatus(exchange, response);

                Response block = GetNextResponseBlock(response, status);

                if (block1 != null) // in case we still have to ack the last block1
                {
                    block.SetOption(block1);
                }

                if (block.Token == null) {
                    block.Token = exchange.Request.Token;
                }

                if (status.Complete) {
                    // clean up blockwise status
                    if (log.IsDebugEnabled) {
                        log.Debug("Ongoing finished on first block " + status);
                    }

                    exchange.OSCOAP_ResponseBlockStatus = null;
                    ClearBlockCleanup(exchange);
                }
                else {
                    if (log.IsDebugEnabled) {
                        log.Debug("Ongoing started " + status);
                    }
                }

                exchange.CurrentResponse = block;
                base.SendResponse(nextLayer, exchange, block);
            }
            else {
                if (block1 != null) {
                    response.SetOption(block1);
                }

                exchange.CurrentResponse = response;
                // Block1 transfer completed
                ClearBlockCleanup(exchange);
                base.SendResponse(nextLayer, exchange, response);
            }
        }

        /// <inheritdoc/>
        public override void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            if ((exchange.OscoreContext == null) && (response.Oscoap == null)) {
                base.ReceiveResponse(nextLayer, exchange, response);
                return;
            }

            // do not continue fetching blocks if canceled
            if (exchange.Request.IsCancelled) {
                // reject (in particular for Block+Observe)
                if (response.Type != MessageType.ACK) {
                    if (log.IsDebugEnabled) {
                        log.Debug("Rejecting blockwise transfer for canceled Exchange");
                    }

                    EmptyMessage rst = EmptyMessage.NewRST(response);
                    SendEmptyMessage(nextLayer, exchange, rst);
                    // Matcher sets exchange as complete when RST is sent
                }

                return;
            }

            if (!response.HasOption(OptionType.Block1) && !response.HasOption(OptionType.Block2)) {
                // There is no block1 or block2 option, therefore it is a normal response
                exchange.Response = response;
                base.ReceiveResponse(nextLayer, exchange, response);
                return;
            }

            BlockOption block1 = response.Block1;
            if (block1 != null) {
                // TODO: What if request has not been sent blockwise (server error)
                log.Debug(string.Format(CultureInfo.InvariantCulture, "Response acknowledges block {block1}"));

                BlockwiseStatus status = exchange.OscoreRequestBlockStatus;
                if (!status.Complete) {
                    // TODO: the response code should be CONTINUE. Otherwise deliver
                    // Send next block
                    int currentSize = 1 << (4 + status.CurrentSZX);
                    int nextNum = status.CurrentNUM + currentSize / block1.Size;
                    
                    log.Debug(string.Format(CultureInfo.InvariantCulture, $"Send next block num = {nextNum}"));
    
                    status.CurrentNUM = nextNum;
                    status.CurrentSZX = block1.SZX;
                    Request nextBlock = GetNextRequestBlock(exchange.Request, exchange.PreSecurityOptions, status);
                    if (nextBlock.Token == null) {
                        nextBlock.Token = response.Token; // reuse same token
                    }

                    exchange.CurrentRequest = nextBlock;
                    log.Debug(string.Format(CultureInfo.InvariantCulture, $"ReceiveResponse: Block message to send: {nextBlock}"));
                    base.SendRequest(nextLayer, exchange, nextBlock);
                    // do not deliver response
                }
                else if (!response.HasOption(OptionType.Block2)) {
                    // All request block have been acknowledged and we receive a piggy-backed
                    // response that needs no blockwise transfer. Thus, deliver it.
                    base.ReceiveResponse(nextLayer, exchange, response);
                }
                else {
                    log.Debug("Response has Block2 option and is therefore sent blockwise");
                }
            }

            BlockOption block2 = response.Block2;
            if (block2 != null) {
                BlockwiseStatus status = FindResponseBlockStatus(exchange, response);

                if (block2.NUM == status.CurrentNUM) {
                    // We got the block we expected :-)
                    status.AddBlock(response.Payload);
                    int? obs = response.Observe;
                    if (obs.HasValue) {
                        status.Observe = obs.Value;
                    }

                    // notify blocking progress
                    exchange.Request.FireResponding(response);

                    if (status.IsRandomAccess) {
                        // The client has requested this specific block and we deliver it
                        exchange.Response = response;
                        base.ReceiveResponse(nextLayer, exchange, response);
                    }
                    else if (block2.M) {
                        log.Debug("Request the next response block");

                        Request request = exchange.Request;
                        int num = block2.NUM + 1;
                        int szx = block2.SZX;
                        bool m = false;

                        Request block = new Request(request.Method);
                        // NON could make sense over SMS or similar transports
                        block.Type = request.Type;
                        block.Destination = request.Destination;
                        block.SetOptions(exchange.PreSecurityOptions /* request.GetOptions()*/);
                        block.SetOption(new BlockOption(OptionType.Block2, num, szx, m));
                        // we use the same token to ease traceability (GET without Observe no longer cancels relations)
                        block.Token = response.Token;
                        // make sure not to use Observe for block retrieval
                        block.RemoveOptions(OptionType.Observe);

                        status.CurrentNUM = num;

                        exchange.CurrentRequest = block;
                        log.Debug(string.Format(CultureInfo.InvariantCulture, $"ReceiveResponse: Block request is {block}"));
                        base.SendRequest(nextLayer, exchange, block);
                    }
                    else {
                        if (log.IsDebugEnabled) {
                            log.Debug("We have received all " + status.BlockCount + " blocks of the response. Assemble and deliver.");
                        }

                        Response assembled = new Response(response.StatusCode);
                        AssembleMessage(status, assembled, response);
                        assembled.Type = response.Type;

                        // set overall transfer RTT
                        assembled.RTT = (DateTime.Now - exchange.Timestamp).TotalMilliseconds;

                        // Check if this response is a notification
                        int observe = status.Observe;
                        if (observe != BlockwiseStatus.NoObserve) {
                            assembled.AddOption(Option.Create(OptionType.Observe, observe));
                            // This is necessary for notifications that are sent blockwise:
                            // Reset block number AND container with all blocks
                            exchange.OSCOAP_ResponseBlockStatus = null;
                        }

                        if (log.IsDebugEnabled) {
                            log.Debug("Assembled response: " + assembled);
                        }

                        exchange.Response = assembled;
                        base.ReceiveResponse(nextLayer, exchange, assembled);
                    }

                }
                else {
                    // ERROR, wrong block number (server error)
                    // TODO: This scenario is not specified in the draft.
                    // Currently, we reject it and cancel the request.
                    if (log.IsWarnEnabled) {
                        log.Warn("Wrong block number. Expected " + status.CurrentNUM + " but received " + block2.NUM + ". Reject response; exchange has failed.");
                    }

                    if (response.Type == MessageType.CON) {
                        EmptyMessage rst = EmptyMessage.NewRST(response);
                        base.SendEmptyMessage(nextLayer, exchange, rst);
                    }

                    exchange.Request.IsCancelled = true;
                }
            }
        }

        private void EarlyBlock2Negotiation(Exchange exchange, Request request)
        {
            // Call this method when a request has completely arrived (might have
            // been sent in one piece without blockwise).
            if (request.HasOption(OptionType.Block2)) {
                BlockOption block2 = request.Block2;
                BlockwiseStatus status2 = new BlockwiseStatus(request.ContentType, block2.NUM, block2.SZX);
                if (log.IsDebugEnabled) {
                    log.Debug("Request with early block negotiation " + block2 + ". Create and set new Block2 status: " + status2);
                }

                exchange.OSCOAP_ResponseBlockStatus = status2;
            }
        }

        /// <summary>
        /// Notice:
        /// This method is used by SendRequest and ReceiveRequest.
        /// Be careful, making changes to the status in here.
        /// </summary>
        private BlockwiseStatus FindRequestBlockStatus(Exchange exchange, Request request)
        {
            BlockwiseStatus status = exchange.OscoreRequestBlockStatus;
            if (status == null) {
                status = new BlockwiseStatus(request.ContentType);
                status.CurrentSZX = BlockOption.EncodeSZX(_defaultBlockSize);
                exchange.OscoreRequestBlockStatus = status;
                if (log.IsDebugEnabled) {
                    log.Debug("There is no assembler status yet. Create and set new Block1 status: " + status);
                }
            }
            else {
                if (log.IsDebugEnabled) {
                    log.Debug("Current Block1 status: " + status);
                }
            }

            // sets a timeout to complete exchange
            PrepareBlockCleanup(exchange);
            return status;
        }

        /// <summary>
        /// Notice:
        /// This method is used by SendResponse and ReceiveResponse.
        /// Be careful, making changes to the status in here.
        /// </summary>
        private BlockwiseStatus FindResponseBlockStatus(Exchange exchange, Response response)
        {
            BlockwiseStatus status = exchange.OSCOAP_ResponseBlockStatus;
            if (status == null) {
                status = new BlockwiseStatus(response.ContentType);
                status.CurrentSZX = BlockOption.EncodeSZX(_defaultBlockSize);
                exchange.OSCOAP_ResponseBlockStatus = status;
                if (log.IsDebugEnabled) {
                    log.Debug("There is no blockwise status yet. Create and set new Block2 status: " + status);
                }
            }
            else {
                if (log.IsDebugEnabled) {
                    log.Debug("Current Block2 status: " + status);
                }
            }

            // sets a timeout to complete exchange
            PrepareBlockCleanup(exchange);
            return status;
        }

        private Request GetNextRequestBlock(Request request, List<Option> originalOptions, BlockwiseStatus status)
        {
            int num = status.CurrentNUM;
            int szx = status.CurrentSZX;
            Request block = new Request(request.Method);
            block.SetOptions(originalOptions);
            block.Destination = request.Destination;
            block.Token = request.Token;
            block.Type = MessageType.CON;

            int currentSize = 1 << (4 + szx);
            int from = num * currentSize;
            int to = Math.Min((num + 1) * currentSize, request.PayloadSize);
            int length = to - from;
            byte[] blockPayload = new byte[length];
            Array.Copy(request.Payload, from, blockPayload, 0, length);
            block.Payload = blockPayload;

            bool m = to < request.PayloadSize;
            block.AddOption(new BlockOption(OptionType.Block1, num, szx, m));

            status.Complete = !m;
            return block;
        }

        private Response GetNextResponseBlock(Response response, BlockwiseStatus status)
        {
            Response block;
            int szx = status.CurrentSZX;
            int num = status.CurrentNUM;

            if (response.HasOption(OptionType.Observe)) {
                // a blockwise notification transmits the first block only
                block = response;
            }
            else {
                block = new Response(response.StatusCode);
                block.Destination = response.Destination;
                block.Token = response.Token;
                block.SetOptions(response.GetOptions());
                block.TimedOut += (o, e) => response.IsTimedOut = true;
            }

            int payloadSize = response.PayloadSize;
            int currentSize = 1 << (4 + szx);
            int from = num * currentSize;
            if (payloadSize > 0 && payloadSize > from) {
                int to = Math.Min((num + 1) * currentSize, response.PayloadSize);
                int length = to - from;
                byte[] blockPayload = new byte[length];
                bool m = to < response.PayloadSize;
                block.SetBlock2(szx, m, num);

                // crop payload -- do after calculation of m in case block==response
                Array.Copy(response.Payload, from, blockPayload, 0, length);
                block.Payload = blockPayload;

                // do not complete notifications
                block.Last = !m && !response.HasOption(OptionType.Observe);

                status.Complete = !m;
            }
            else {
                block.AddOption(new BlockOption(OptionType.Block2, num, szx, false));
                block.Last = true;
                status.Complete = true;
            }

            return block;
        }

        private void AssembleMessage(BlockwiseStatus status, Message message, Message last)
        {
            // The assembled request will contain the options of the last block
            message.ID = last.ID;
            message.Source = last.Source;
            message.Token = last.Token;
            message.Type = last.Type;
            message.SetOptions(last.GetOptions());

            int length = 0;
            foreach (byte[] block in status.Blocks) {
                length += block.Length;
            }

            byte[] payload = new byte[length];
            int offset = 0;
            foreach (byte[] block in status.Blocks) {
                Array.Copy(block, 0, payload, offset, block.Length);
                offset += block.Length;
            }

            message.Payload = payload;
        }

        private bool RequiresBlockwise(Request request)
        {
            if (request.Method == Method.PUT || request.Method == Method.POST) {
                return request.PayloadSize > _maxMessageSize;
            }
            else {
                return false;
            }
        }

        private bool RequiresBlockwise(Exchange exchange, Response response)
        {
            return response.PayloadSize > _maxMessageSize
                   || exchange.OSCOAP_ResponseBlockStatus != null;
        }

        /// <summary>
        /// Schedules a clean-up task.
        /// Use the <see cref="ICoapConfig.BlockwiseStatusLifetime"/> to set the timeout.
        /// </summary>
        protected void PrepareBlockCleanup(Exchange exchange)
        {
            Timer timer = new Timer();
            timer.AutoReset = false;
            timer.Interval = _blockTimeout;
            timer.Elapsed += (o, e) => BlockwiseTimeout(exchange);

            Timer old = exchange.Set("BlockCleanupTimer", timer) as Timer;
            if (old != null) {
                try {
                    old.Stop();
                    old.Dispose();
                }
                catch (ObjectDisposedException) {
                    // ignore
                }
            }

            timer.Start();
        }

        /// <summary>
        /// Clears the clean-up task.
        /// </summary>
        protected void ClearBlockCleanup(Exchange exchange)
        {
            Timer timer = exchange.Remove("BlockCleanupTimer") as Timer;
            if (timer != null) {
                try {
                    timer.Dispose();
                }
                catch (ObjectDisposedException) {
                    // ignore
                }
            }
        }

        private void BlockwiseTimeout(Exchange exchange)
        {
            if (exchange.Request == null) {
                if (log.IsInfoEnabled) {
                    log.Info("Block1 transfer timed out: " + exchange.CurrentRequest);
                }
            }
            else {
                if (log.IsInfoEnabled) {
                    log.Info("Block2 transfer timed out: " + exchange.Request);
                }
            }

            exchange.Complete = true;
        }
    }
}
