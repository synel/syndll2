using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Syndll2
{
    /// <summary>
    /// Implements the details of communicating with a terminal over a network connection.
    /// </summary>
    internal class NetworkConnection : IConnection
    {
        private readonly IPEndPoint _endPoint;
        private readonly Socket _socket;
        private NetworkStream _stream;
        private bool _disposed;

        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        public Stream Stream
        {
            get { return _stream; }
        }
        
        private NetworkConnection(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    // todo: see if these timeout values need adjusting
                    ReceiveTimeout = 5000,
                    SendTimeout = 5000
                };
        }

        public static NetworkConnection Connect(string host, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = GetEndPoint(host, port);
            return Connect(endPoint, timeout);
        }

        public static NetworkConnection Connect(IPAddress ipAddress, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = new IPEndPoint(ipAddress, port);
            return Connect(endPoint, timeout);
        }

        public static NetworkConnection Connect(IPEndPoint endPoint, TimeSpan timeout = default(TimeSpan))
        {
            var connection = new NetworkConnection(endPoint);

            Util.Log("Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            GateKeeper.Enter(endPoint, timeout);

            var socket = connection._socket;
            try
            {
                // Now we can try to connect, using the specified timeout.
                var result = socket.BeginConnect(endPoint, socket.EndConnect, null);
                result.AsyncWaitHandle.WaitOne(timeout, true);
                if (!socket.Connected)
                {
                    socket.Close();
                    throw new TimeoutException("Timeout occurred while trying to connect to the terminal.");
                }
            }
            catch
            {
                GateKeeper.Exit(endPoint);
                throw;
            }

            // Get the stream for the connection
            connection._stream = new NetworkStream(socket, true);

            Util.Log("Connected!");

            return connection;
        }

#if NET_45
        public static async Task<NetworkConnection> ConnectAsync(string host, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = GetEndPoint(host, port);
            return await ConnectAsync(endPoint);
        }

        public static async Task<NetworkConnection> ConnectAsync(IPAddress ipAddress, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = new IPEndPoint(ipAddress, port);
            return await ConnectAsync(endPoint);
        }

        public static async Task<NetworkConnection> ConnectAsync(IPEndPoint endPoint, TimeSpan timeout = default(TimeSpan))
        {
            var connection = new NetworkConnection(endPoint);

            Util.Log("Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            await GateKeeper.EnterAsync(endPoint, timeout);
            
            // Now we can try to connect, using the specified timeout.
            var socket = connection._socket;

            var connectTask = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endPoint, null);
            
            await Task.WhenAny(connectTask, Task.Delay(timeout));
            if (!socket.Connected)
            {
                socket.Close();
                throw new TimeoutException("Timeout occurred while trying to connect to the terminal.");
            }

            // Get the stream for the connection
            connection._stream = new NetworkStream(socket, true);

            Util.Log("Connected!");

            return connection;
        }
#endif

        private static IPEndPoint GetEndPoint(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException("host", "The terminal's host IP address or DNS name must be provided.");

            if (port <= 0)
                throw new ArgumentOutOfRangeException("port",
                                                      port,
                                                      "A valid TCP port must be specified.  If uncertain, leave blank and it will use the default of 3734.");

            IPAddress ipAddress;
            if (IPAddress.TryParse(host, out ipAddress))
            {
                // We have an IP address, but make sure that we are not affected by leading zeros being treated as octal numbers.  See:
                // http://connect.microsoft.com/VisualStudio/feedback/details/634288/system-net-ipaddress-parse-mistake

                ipAddress = new IPAddress(host.Split('.').Select(byte.Parse).ToArray());
            }
            else
            {
                // Look up the DNS host name to make sure we have a single endpoint.
                // This ensures the gatekeeper cannot be cheated by different dns names for the same endpoint.
                ipAddress = Dns.GetHostAddresses(host).FirstOrDefault();
                if (ipAddress == null)
                    throw new ArgumentException("Invalid host address.", "host");
            }

            return new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Disconnects any open connection to a terminal.
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
            {
                Util.Log("Already disconnected.");
                GateKeeper.Exit(_endPoint);
                return;
            }

            _socket.Disconnect(false);
            GateKeeper.Exit(_endPoint);

            Util.Log("Disconnected.");
        }

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
                Disconnect();
                if (_stream != null)
                    _stream.Dispose();
            }

            _disposed = true;
        }
    }
}
