using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Syndll2
{
    /// <summary>
    /// Implements the details of communicating with a terminal over a network connection.
    /// </summary>
    internal class NetworkConnection : IConnection
    {
        private readonly Socket _socket;
        private readonly ManualResetEvent _acceptSignaler = new ManualResetEvent(false);
        private IPEndPoint _remoteEndPoint;
        private bool _disposed;

        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        public Stream Stream { get; internal set; }

        private NetworkConnection(Socket socket = null)
        {
            _socket = socket ??
                      new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
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
            Util.Log("Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            GateKeeper.Enter(endPoint, timeout);

            var connection = new NetworkConnection();
            var socket = connection._socket;
            try
            {
                // Now we can try to connect, using the specified timeout.
                var result = socket.BeginConnect(endPoint, ar =>
                    {
                        try
                        {
                            socket.EndConnect(ar);
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow these
                        }
                    }, null);
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

            // Track the endpoint separately in the connection so we can clean up properly
            connection._remoteEndPoint = endPoint;

            // Get the stream for the connection
            connection.Stream = new NetworkStream(socket, true);

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
            Util.Log("Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            await GateKeeper.EnterAsync(endPoint, timeout);

            // Now we can try to connect, using the specified timeout.
            var connection = new NetworkConnection();
            var socket = connection._socket;

            var connectTask = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endPoint, null);

            await Task.WhenAny(connectTask, Task.Delay(timeout));
            if (!socket.Connected)
            {
                socket.Close();
                throw new TimeoutException("Timeout occurred while trying to connect to the terminal.");
            }

            // Track the endpoint separately in the connection so we can clean up properly
            connection._remoteEndPoint = endPoint;

            // Get the stream for the connection
            connection.Stream = new NetworkStream(socket, true);

            Util.Log("Connected!");

            return connection;
        }
#endif

        public static NetworkConnection Listen(Action<Stream, Socket> action)
        {
            return Listen(3734, action);
        }

        public static NetworkConnection Listen(int port, Action<Stream, Socket> action)
        {
            var listener = new NetworkConnection();

            var localEndPoint = new IPEndPoint(IPAddress.Any, port);
            listener._socket.Bind(localEndPoint);
            listener._socket.Listen(100);

            Util.Log("Listening for inbound connections.");

            Task.Factory.StartNew(() =>
            {
                while (!listener._disposed)
                {
                    listener._acceptSignaler.Reset();
                    listener._socket.BeginAccept(ar =>
                    {
                        listener._acceptSignaler.Set();
                        var socket = listener._socket.EndAccept(ar);

                        var ep = (IPEndPoint)socket.RemoteEndPoint;
                        Util.Log(string.Format("Inbound connection from {0}", ep.Address));

                        GateKeeper.Enter(ep, TimeSpan.FromSeconds(2));

                        try
                        {
                            using (var stream = new NetworkStream(socket))
                            {
                                action(stream, socket);
                            }
                        }
                        finally
                        {
                            socket.Disconnect(false);
                            GateKeeper.Exit(ep);
                            Util.Log("Disconnected.");
                        }

                    }, null);

                    listener._acceptSignaler.WaitOne();
                }
            });

            return listener;
        }

        internal static IPEndPoint GetEndPoint(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException("host", "The terminal's host IP address or DNS name must be provided.");

            if (port <= 0)
                throw new ArgumentOutOfRangeException("port",
                                                      port,
                                                      "A valid TCP port must be specified.  If uncertain, leave blank and it will use the default of 3734.");

            IPAddress ipAddress;
            try
            {
                // first try to parse it ourself, so we don't run into issues with leading zeros being treated as octal numbers.
                // http://connect.microsoft.com/VisualStudio/feedback/details/634288/system-net-ipaddress-parse-mistake

                ipAddress = new IPAddress(host.Split('.').Select(byte.Parse).ToArray());
            }
            catch
            {
                // Look up the DNS host name to make sure we have a single endpoint.
                // This ensures the gatekeeper cannot be cheated by different dns names for the same endpoint.
                try
                {
                    ipAddress = Dns.GetHostAddresses(host).First();
                }
                catch
                {
                    throw new ArgumentException("Invalid host address.", "host");
                }
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
                GateKeeper.Exit(_remoteEndPoint);
                return;
            }

            _socket.Disconnect(false);
            GateKeeper.Exit(_remoteEndPoint);

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
                if (Stream != null)
                    Stream.Dispose();
                _socket.Dispose();
                _acceptSignaler.Dispose();
            }

            _disposed = true;
        }
    }
}
