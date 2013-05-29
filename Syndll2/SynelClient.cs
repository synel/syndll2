using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
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
            _reader = new StreamReader(_connection.Stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: MaxPacketSize);
            _writer = new StreamWriter(_connection.Stream, Encoding.ASCII, bufferSize: MaxPacketSize);
            _terminal = new TerminalOperations(this);
        }

        /// <summary>
        /// Connects to a terminal over a TCP network connection, using the provided parameters.
        /// </summary>
        /// <param name="host">The terminal's IP address or DNS name.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static SynelClient Connect(string host, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = NetworkConnection.Connect(host, port, timeout);
            return new SynelClient(connection, terminalId);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that asynchronously connects to a terminal over TCP using the provided parameters.
        /// </summary>
        /// <param name="host">The terminal's IP address or DNS name.</param>
        /// <param name="port">The TCP port to use to connect to the terminal.  Defaults to 3734 if not provided.</param>
        /// <param name="terminalId">The terminal's Terminal ID.  Defaults to 0 if not provided.</param>
        /// <param name="timeout">The amount of time before the connection will timeout. Defaults to 2 seconds.</param>
        public static async Task<SynelClient> ConnectAsync(string host, int port = 3734, int terminalId = 0, TimeSpan timeout = default(TimeSpan))
        {
            var connection = await NetworkConnection.ConnectAsync(host, port, timeout);
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
            {
                _connection.Dispose();
                _reader.Dispose();
                _writer.Dispose();
            }

            _disposed = true;
        }

        private const int PacketOverheadSize = 7;
        private const int MaxDataSize = 128;
        private const int MaxPacketSize = MaxDataSize + PacketOverheadSize;
        private const int MaxCrcRetries = 10;

        /// <summary>
        /// Creates the full command string that is to be sent to the terminal.
        /// </summary>
        /// <param name="command">The request command.</param>
        /// <param name="data">Any data that should be sent along with the command.</param>
        /// <returns>The full command string, including CRC and EOT.</returns>
        private string CreateCommand(RequestCommand command, string data = null)
        {
            // sanitize input for null data
            if (data == null)
                data = string.Empty;

            // make sure the input isn't too long
            if (data.Length > MaxDataSize)
                throw new ArgumentException(string.Format("Data must be {0} ASCII characters or less.", MaxDataSize));

            // Begin building the command string
            var sb = new StringBuilder(MaxPacketSize);

            // The first character is the request command code.
            sb.Append((char)command);

            // The second character is the terminal ID.
            sb.Append(Util.TerminalIdToChar(_terminalId));

            // The data block comes next
            sb.Append(data);

            // Then 4 characters of CRC of the string thus far
            sb.Append(SynelCRC.Calculate(sb.ToString()));

            // Finally, a termination character.
            sb.Append(ControlChars.EOT);

            return sb.ToString();
        }

        /// <summary>
        /// Parses the response string returned from the terminal, ensuring that the response is valid.
        /// </summary>
        /// <param name="s">The entire response string, including ACK, NACK, CRC, and EOT where appropriate.</param>
        /// <returns>A validated <see cref="Response"/> object.</returns>
        private Response ParseResponse(string s)
        {
            // check valid input
            if (s == null)
                throw new ArgumentNullException("s");

            // check for small data size, which is also a bad crc
            if (s.Length < PacketOverheadSize)
                throw new InvalidCrcException("Invalid CRC received from the terminal.");

            // check crc
            var packet = s.Substring(0, s.Length - 5);
            var crc = s.Substring(s.Length - 5, 4);
            if (!SynelCRC.Verify(packet, crc))
                throw new InvalidCrcException("Invalid CRC received from the terminal.");

            // Get command
            var cmd = packet[0];
            if (!Enum.IsDefined(typeof(PrimaryResponseCommand), (int)cmd))
                throw new InvalidDataException("Unknown command received: " + cmd);
            var command = (PrimaryResponseCommand)cmd;

            // Get terminal
            byte terminalId = Util.CharToTerminalId(packet[1]);
            if (terminalId != _terminalId)
                throw new InvalidOperationException(
                    string.Format("Got data for terminal {0} while talking to terminal {1}", terminalId, _terminalId));

            // Return when there is no data
            if (packet.Length == 2)
                return new Response(command);

            // Return when there IS data
            var data = packet.Substring(2);
            return new Response(command, data);
        }

        /// <summary>
        /// Communicates with the terminal by sending a request and receiving a response.
        /// </summary>
        /// <param name="requestCommand">The request command to send.</param>
        /// <param name="dataToSend">Any data that should be sent along with the command.</param>
        /// <returns>A validated <see cref="Response"/> object.</returns>
        internal Response SendAndReceive(RequestCommand requestCommand, string dataToSend = null)
        {
            if (!Connected)
                throw new InvalidOperationException("Not connected!");
            
            // CRC retry loop
            for (int i = 1; i <= MaxCrcRetries; i++)
            {
                try
                {
                    // Send and receive
                    var rawRequest = CreateCommand(requestCommand, dataToSend);
                    Send(rawRequest);
                    return ReceiveResponse();
                }
                catch (InvalidCrcException)
                {
                    // swallow these until the retry limit is reached
                    if (i < MaxCrcRetries)
                        Trace.WriteLine("Bad CRC.  Retrying...");
                }
            }

            // We've hit the retry limit, throw a CRC exception.
            throw new InvalidCrcException(string.Format("Retried the operation {0} times, but still got CRC errors.", MaxCrcRetries));
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that communicates with the terminal by sending a request and receiving a response.
        /// </summary>
        /// <param name="requestCommand">The request command to send.</param>
        /// <param name="dataToSend">Any data that should be sent along with the command.</param>
        /// <returns>A task that yields a validated <see cref="Response"/> object.</returns>
        internal async Task<Response> SendAndReceiveAsync(RequestCommand requestCommand, string dataToSend = null)
        {
            if (!Connected)
                throw new InvalidOperationException("Not connected!");

            // CRC retry loop
            for (int i = 1; i <= MaxCrcRetries; i++)
            {
                try
                {
                    // Send and Receive
                    var rawRequest = CreateCommand(requestCommand, dataToSend);
                    await SendAsync(rawRequest);
                    return await ReceiveResponseAsync();
                }
                catch (InvalidCrcException)
                {
                    // swallow these until the retry limit is reached
                    if (i < MaxCrcRetries)
                        Trace.WriteLine("Bad CRC.  Retrying...");
                }
            }

            // We've hit the retry limit, throw a CRC exception.
            throw new InvalidCrcException(string.Format("Retried the operation {0} times, but still got CRC errors.", MaxCrcRetries));
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

            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Sending: " + GetTraceString(command));

            _writer.Write(command);
            _writer.Flush();
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

            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Sending: " + GetTraceString(command));

            await _writer.WriteAsync(command);
            await _writer.FlushAsync();
        }
#endif

        private string Receive()
        {
            // make sure we are connected
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // read a packet from the stream
            var bytesReceived = new List<byte>(MaxPacketSize);
            while (!_reader.EndOfStream)
            {
                // read a byte
                var b = (byte)_reader.Read();

                // add it to the list
                bytesReceived.Add(b);

                // stop when we receive a termination character
                if (b == ControlChars.EOT)
                    break;

                // make sure we can't go on forever
                if (bytesReceived.Count > MaxPacketSize)
                    throw new ProtocolViolationException("Received too much data without a termination character!");
            }

            var s = Encoding.ASCII.GetString(bytesReceived.ToArray());
            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Received: " + GetTraceString(s));
            return s;
        }

#if NET_45
        private async Task<string> ReceiveAsync()
        {
            // make sure we are connected
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // read a packet from the stream
            var bytesReceived = new List<byte>(MaxPacketSize);
            while (!_reader.EndOfStream)
            {
                // read a byte
                var buffer = new char[1];
                var i = await _reader.ReadAsync(buffer, 0, 1);
                if (i == 0)
                    break;

                // add it to the list
                var b = (byte)buffer[0];
                bytesReceived.Add(b);

                // stop when we receive a termination character
                if (b == ControlChars.EOT)
                    break;

                // make sure we can't go on forever
                if (bytesReceived.Count > MaxPacketSize)
                    throw new ProtocolViolationException("Received too much data without a termination character!");
            }

            var s = Encoding.ASCII.GetString(bytesReceived.ToArray());
            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Received: " + GetTraceString(s));
            return s;
        }
#endif

        private Response ReceiveResponse()
        {
            // Receive the data
            var rawResponse = Receive();

            // Parse the response
            var response = ParseResponse(rawResponse);

            // If we got a QueryForHost response, do it all over again.
            if (response.Command == PrimaryResponseCommand.QueryForHost)
            {
                Trace.WriteLine("Query for host received");
                return ReceiveResponse();
            }

            // Return the response.
            return response;
        }

#if NET_45
        private async Task<Response> ReceiveResponseAsync()
        {
            // Receive the data
            var rawResponse = await ReceiveAsync();

            // Parse the response
            var response = ParseResponse(rawResponse);

            // If we got a QueryForHost response, do it all over again.
            if (response.Command == PrimaryResponseCommand.QueryForHost)
            {
                Trace.WriteLine("Query for host received");
                return await ReceiveResponseAsync();
            }

            // Return the response.
            return response;
        }
#endif

        private string GetTraceString(string s)
        {
            return s.Length == 0
                       ? "(NO DATA)"
                       : s.Replace(ControlChars.EOT.ToString(CultureInfo.InvariantCulture), "(EOT)")
                          .Replace(ControlChars.SOH.ToString(CultureInfo.InvariantCulture), "(SOH)")
                          .Replace(ControlChars.ACK.ToString(CultureInfo.InvariantCulture), "(ACK)")
                          .Replace(ControlChars.NACK.ToString(CultureInfo.InvariantCulture), "(NACK)");
        }

        #region Binary UCP connection (TODO)

        /*
         *   Section 4.2 of the Synel Protocol specification describes a binary "Universal Communication Protocol".
         *   This is only used when communicating with a special firmware (8.23F) that supports two fingerprint units.
         *   It is not fully tested, but the below code can be uncommented and used when it becomes necessary to support this.
         */

#if NOTIMPLEMENTED

        internal byte[] SendAndReceive(byte[] command)
        {
            SendBytes(command);
            return ReceiveBytes();
        }

        private void SendBytes(byte[] data)
        {
            // make sure we are connected
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // make sure the stream is ready for writing
            if (!_connection.Stream.CanWrite)
                throw new InvalidOperationException("Cannot write to the stream.");

            // decide on a packet number
            var packetNumber = 0x31;

            // calculate the crc
            var crc = SynelCRC.Calculate(data);

            // create the packet
            var packet = new byte[data.Length + 10];

            // header
            packet[0] = (byte)ControlChars.SOH;
            packet[1] = (byte)(packet.Length >> 8);
            packet[2] = (byte)(packet.Length & 0xFF);
            packet[3] = (byte)packetNumber;
            //packet[4] = host header - reserved for future use

            // data
            data.CopyTo(packet, 5);

            // terminator
            crc.CopyTo(packet, packet.Length - 5);
            packet[packet.Length - 1] = (byte)ControlChars.EOT;

            // Write the packet
            Trace.WriteLine("{0}: Sending Bytes: {1}",
                            Thread.CurrentThread.ManagedThreadId,
                            Util.ByteArrayToString(packet));
            _connection.Stream.Write(packet, 0, packet.Length);
            _connection.Stream.Flush();
        }

        private byte[] ReceiveBytes()
        {
            // Make sure we are connected.
            if (!Connected)
                throw new InvalidOperationException("The client is not connected.");

            // Make sure the stream is ready for reading.
            if (!_connection.Stream.CanRead)
                throw new InvalidOperationException("Cannot read from the stream.");

            // Read the UCP header.
            var header = new byte[5];
            _connection.Stream.Read(header, 0, 5);

            // Check for SOH.
            if (header[0] != ControlChars.SOH)
                throw new InvalidDataException(string.Format("Expected SOH.  Got 0x{0:X2}", header[0]));

            // Get the packet length and number.
            var packetLength = (header[1] << 8) + header[2];
            var packetNumber = header[3];

            // Host address is reserved for future use.
            //var hostAddress = header[4];

            // Read the data
            var data = new byte[packetLength - 10];
            if (data.Length > 0)
                _connection.Stream.Read(data, 0, data.Length);

            // Read the UCP terminator.
            var terminator = new byte[5];
            _connection.Stream.Read(terminator, 0, 5);

            // Check for EOT
            if (terminator[4] != ControlChars.EOT)
                throw new InvalidDataException(string.Format("Expected EOT.  Got 0x{0:X2}", terminator[4]));

            // Validate CRC
            var crc = terminator.Take(4).ToArray();
            if (!SynelCRC.Verify(data, crc))
                throw new InvalidDataException("Invalid CRC received from the terminal.");

            // Return just the data
            Trace.WriteLine("{0}: Received Bytes: {1}{2}{3}",
                            Thread.CurrentThread.ManagedThreadId,
                            Util.ByteArrayToString(header),
                            Util.ByteArrayToString(data),
                            Util.ByteArrayToString(terminator));
            return data;
        }
#endif
        #endregion
    }
}
