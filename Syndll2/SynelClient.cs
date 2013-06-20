﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Syndll2
{
    /// <summary>
    /// Provides methods for communication with a terminal that uses the Synel Communications Protocol.
    /// Should be used from within a <code>using</code> block, or closed explicitly with the <see cref="Dispose"/> method.
    /// </summary>
    public class SynelClient : IDisposable
    {
        private readonly IConnection _connection;
        private readonly Receiver _receiver;
        private readonly TerminalOperations _terminal;
        private readonly int _terminalId;
        private bool _disposed;

        /// <summary>
        /// Gets a value indicating whether the client is connected to a terminal.
        /// </summary>
        public bool Connected
        {
            get { return _connection.Connected; }
        }

        /// <summary>
        /// Gets the terminalId that is in use by this client.
        /// </summary>
        public int TerminalId
        {
            get { return _terminalId; }
        }

        /// <summary>
        /// Gets an accessor that exposes the operations that can be performed on the terminal.
        /// </summary>
        public TerminalOperations Terminal { get { return _terminal; } }


        private SynelClient(IConnection connection, int terminalId)
        {
            if (terminalId > 31)
                throw new ArgumentOutOfRangeException("terminalId", terminalId,
                                                      "The terminal ID must be between 0 and 31.");

            _terminalId = terminalId;
            _connection = connection;

            _terminal = new TerminalOperations(this);
            _receiver = new Receiver(_connection.Stream);
            _receiver.WatchStream();
        }

        /// <summary>
        /// Connects to a terminal over a TCP network connection, using the provided parameters.
        /// </summary>
        /// <param name="host">A string containing the terminal's IP address or DNS name.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static SynelClient Connect(string host, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = NetworkConnection.Connect(host, port, timeout);
            return new SynelClient(connection, terminalId);
        }

        /// <summary>
        /// Connects to a terminal over a TCP network connection, using the provided parameters.
        /// </summary>
        /// <param name="ipAddress">The terminal's IP address.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static SynelClient Connect(IPAddress ipAddress, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = NetworkConnection.Connect(ipAddress, port, timeout);
            return new SynelClient(connection, terminalId);
        }

        /// <summary>
        /// Connects to a terminal over a TCP network connection, using the provided parameters.
        /// </summary>
        /// <param name="endPoint">An IP endpoint consisting of the terminal's IP and port.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static SynelClient Connect(IPEndPoint endPoint, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = NetworkConnection.Connect(endPoint, timeout);
            return new SynelClient(connection, terminalId);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that asynchronously connects to a terminal over TCP using the provided parameters.
        /// </summary>
        /// <param name="host">A string containing the terminal's IP address or DNS name.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static async Task<SynelClient> ConnectAsync(string host, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = await NetworkConnection.ConnectAsync(host, port, timeout);
            return new SynelClient(connection, terminalId);
        }

        /// <summary>
        /// Returns an awaitable task that asynchronously connects to a terminal over TCP using the provided parameters.
        /// </summary>
        /// <param name="ipAddress">The terminal's IP address.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static async Task<SynelClient> ConnectAsync(IPAddress ipAddress, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = await NetworkConnection.ConnectAsync(ipAddress, port, timeout);
            return new SynelClient(connection, terminalId);
        }

        /// <summary>
        /// Returns an awaitable task that asynchronously connects to a terminal over TCP using the provided parameters.
        /// </summary>
        /// <param name="endPoint">An IP endpoint consisting of the terminal's IP and port.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static async Task<SynelClient> ConnectAsync(IPEndPoint endPoint, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = await NetworkConnection.ConnectAsync(endPoint, timeout);
            return new SynelClient(connection, terminalId);
        }
#endif

        /// <summary>
        /// Disconnects from the terminal, and releases any resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _connection.Dispose();

            _disposed = true;
        }

        internal const int PacketOverheadSize = 7;
        internal const int MaxDataSize = 128;
        internal const int MaxPacketSize = MaxDataSize + PacketOverheadSize;
        internal const int MaxRetries = 10;

        /// <summary>
        /// Creates the full command string that is to be sent to the terminal.
        /// </summary>
        /// <param name="command">The request command.</param>
        /// <param name="data">Any data that should be sent along with the command.</param>
        /// <returns>The full command string, including CRC and EOT.</returns>
        private string CreateCommand(RequestCommand command, string data = null)
        {
            return CreateCommand(command, _terminalId, data);
        }

        /// <summary>
        /// Creates the full command string that is to be sent to the terminal.
        /// </summary>
        /// <param name="command">The request command.</param>
        /// <param name="terminalId">The terminal id.</param>
        /// <param name="data">Any data that should be sent along with the command.</param>
        /// <returns>The full command string, including CRC and EOT.</returns>
        internal static string CreateCommand(RequestCommand command, int terminalId, string data = null)
        {
            // sanitize input for null data
            if (data == null)
                data = string.Empty;

            // make sure the input isn't too long, except for when uploading fingerprints.
            if (data.Length > MaxDataSize && command != RequestCommand.Fingerprint)
                throw new ArgumentException(string.Format("Data must be {0} ASCII characters or less.", MaxDataSize));

            // Begin building the command string
            var sb = new StringBuilder(MaxPacketSize);

            // The first character is the request command code.
            sb.Append((char)command);

            // The second character is the terminal ID.
            sb.Append(Util.TerminalIdToChar(terminalId));

            // The data block comes next
            sb.Append(data);

            // Then 4 characters of CRC of the string thus far
            sb.Append(SynelCRC.Calculate(sb.ToString()));

            // Finally, a termination character.
            sb.Append(ControlChars.EOT);

            return sb.ToString();
        }

        /// <summary>
        /// Communicates with the terminal by sending a request and receiving a response.
        /// </summary>
        /// <param name="requestCommand">The request command to send.</param>
        /// <param name="dataToSend">Any data that should be sent along with the command.</param>
        /// <param name="validResponses">If specified, the response must start with one of the valid responses (omit the terminal id).</param>
        /// <returns>A validated <see cref="Response"/> object.</returns>
        internal Response SendAndReceive(RequestCommand requestCommand, string dataToSend = null, params string[] validResponses)
        {
            if (!Connected)
                throw new InvalidOperationException("Not connected!");

            // augment valid responses with the terminal id
            if (validResponses.Length > 0)
            {
                var tid = Util.TerminalIdToChar(_terminalId).ToString(CultureInfo.InvariantCulture);
                validResponses = validResponses.Select(x => x.Insert(1, tid)).ToArray();
            }

            // Setup the receive event handler
            var signal = new SemaphoreSlim(1);
            string rawResponse = null;
            Response response = null;
            Exception exception = null;
            EventHandler<MessageReceivedEventArgs> handler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.RawResponse))
                {
                    // Don't ever handle host query responses here.
                    if (args.RawResponse[0] == 'q')
                        return;

                    // If the valid list is populated, don't handle responses that aren't in it.
                    if (validResponses.Length > 0 && !validResponses.Any(x => args.RawResponse.StartsWith(x)))
                        return;

                    // Ignore responses intended for other terminals
                    if (args.Response != null && args.Response.TerminalId != _terminalId)
                        return;
                }

                rawResponse = args.RawResponse;
                response = args.Response;
                exception = args.Exception;
                signal.Release();
            };
            _receiver.MessageReceived += handler;

            try
            {
                // retry loop
                for (int i = 1; i <= MaxRetries; i++)
                {
                    try
                    {
                        // Reset the signal
                        signal.Wait();

                        // Send the request
                        var rawRequest = CreateCommand(requestCommand, dataToSend);
                        Send(rawRequest);

                        // Wait for the response or timeout
                        if (signal.Wait(5000))
                            signal.Release();
                        _receiver.MessageReceived -= handler;

                        if (rawResponse != null)
                            Util.Log("Received: " + rawResponse);

                        if (exception != null)
                            throw exception;

                        if (response == null)
                            throw new InvalidDataException("No response received from the terminal.");

                        return response;
                    }
                    catch (InvalidCrcException)
                    {
                        // swallow these until the retry limit is reached
                        if (i < MaxRetries)
                            Util.Log("Bad CRC.  Retrying...");
                    }
                }

                // We've hit the retry limit, throw a CRC exception.
                throw new InvalidCrcException(string.Format("Retried the operation {0} times, but still got CRC errors.", MaxRetries));
            }
            finally
            {
                _receiver.MessageReceived -= handler;
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that communicates with the terminal by sending a request and receiving a response.
        /// </summary>
        /// <param name="requestCommand">The request command to send.</param>
        /// <param name="dataToSend">Any data that should be sent along with the command.</param>
        /// <param name="validResponses">If specified, the response must start with one of the valid responses (omit the terminal id).</param>
        /// <returns>A task that yields a validated <see cref="Response"/> object.</returns>
        internal async Task<Response> SendAndReceiveAsync(RequestCommand requestCommand, string dataToSend = null, params string[] validResponses)
        {
            if (!Connected)
                throw new InvalidOperationException("Not connected!");

            // augment valid responses with the terminal id
            if (validResponses.Length > 0)
            {
                var tid = Util.TerminalIdToChar(_terminalId).ToString(CultureInfo.InvariantCulture);
                validResponses = validResponses.Select(x => x.Insert(1, tid)).ToArray();
            }

            // Setup the receive event handler
            var signal = new SemaphoreSlim(1);
            string rawResponse = null;
            Response response = null;
            Exception exception = null;
            EventHandler<MessageReceivedEventArgs> handler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.RawResponse))
                {
                    // Don't ever handle host query responses here.
                    if (args.RawResponse[0] == 'q')
                        return;

                    // If the valid list is populated, don't handle responses that aren't in it.
                    if (validResponses.Length > 0 && !validResponses.Any(x => args.RawResponse.StartsWith(x)))
                        return;

                    // Ignore responses intended for other terminals
                    if (args.Response != null && args.Response.TerminalId != _terminalId)
                        return;
                }

                rawResponse = args.RawResponse;
                response = args.Response;
                exception = args.Exception;
                signal.Release();
            };
            _receiver.MessageReceived += handler;

            try
            {
                // retry loop
                for (int i = 1; i <= MaxRetries; i++)
                {
                    try
                    {
                        // Reset the signal
                        await signal.WaitAsync();

                        // Send the request
                        var rawRequest = CreateCommand(requestCommand, dataToSend);
                        await SendAsync(rawRequest);

                        // Wait for the response or timeout
                        if (await signal.WaitAsync(5000))
                            signal.Release();
                        _receiver.MessageReceived -= handler;

                        if (rawResponse != null)
                            Util.Log("Received: " + rawResponse);

                        if (exception != null)
                            throw exception;

                        if (response == null)
                            throw new InvalidDataException("No response received from the terminal.");

                        return response;
                    }
                    catch (InvalidCrcException)
                    {
                        // swallow these until the retry limit is reached
                        if (i < MaxRetries)
                            Util.Log("Bad CRC.  Retrying...");
                    }
                }

                // We've hit the retry limit, throw a CRC exception.
                throw new InvalidCrcException(string.Format("Retried the operation {0} times, but still got CRC errors.", MaxRetries));
            }
            finally
            {
                _receiver.MessageReceived -= handler;
            }
        }
#endif

#if NET_45
#endif

        private void Send(string command)
        {
            // make sure we are connected
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // make sure the stream is ready for writing
            if (!_connection.Stream.CanWrite)
                throw new InvalidOperationException("The stream cannot be written to.");

            Util.Log("Sending: " + command);

            var bytes = Encoding.ASCII.GetBytes(command);
            _connection.Stream.Write(bytes, 0, bytes.Length);
            _connection.Stream.Flush();
        }

#if NET_45
        private async Task SendAsync(string command)
        {
            // make sure we are connected
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // make sure the stream is ready for writing
            if (!_connection.Stream.CanWrite)
                throw new InvalidOperationException("The stream cannot be written to.");

            Util.Log("Sending: " + command);

            var bytes = Encoding.ASCII.GetBytes(command);
            await _connection.Stream.WriteAsync(bytes, 0, bytes.Length);
            await _connection.Stream.FlushAsync();
        }
#endif
    }
}
