using System;
using System.Diagnostics;
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
        private readonly IPEndPoint _endPoint;
        private readonly TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _disposed;

        public bool Connected
        {
            get { return _tcpClient != null && _tcpClient.Connected; }
        }

        public Stream Stream
        {
            get { return _stream; }
        }
        
        private NetworkConnection(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            _tcpClient = new TcpClient
                {
                    // todo: see if these timeout values need adjusting
                    ReceiveTimeout = 5000,
                    SendTimeout = 5000
                };
        }

        public static NetworkConnection Connect(string host, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = GetEndPoint(host, port);
            var connection = new NetworkConnection(endPoint);

            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            GateKeeper.Enter(endPoint, timeout);

            // Now we can try to connect, using the specified timeout.
            var client = connection._tcpClient;
            var result = client.BeginConnect(endPoint.Address, endPoint.Port, null, null);
            result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!client.Connected)
            {
                client.Close();
                throw new TimeoutException("Timeout occurred while trying to connect to the terminal.");
            }

            // Get the stream for the connection
            connection._stream = client.GetStream();

            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Connected!");

            return connection;
        }

#if NET_45
        public static async Task<NetworkConnection> ConnectAsync(string host, int port = 3734, TimeSpan timeout = default(TimeSpan))
        {
            var endPoint = GetEndPoint(host, port);
            var connection = new NetworkConnection(endPoint);

            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Connecting to terminal...");

            // default timeout is two seconds
            if (timeout <= TimeSpan.Zero)
                timeout = TimeSpan.FromSeconds(2);

            // Enter the gate.  This will block until it is safe to connect.
            await GateKeeper.EnterAsync(endPoint, timeout);
            
            // Now we can try to connect, using the specified timeout.
            var client = connection._tcpClient;
            await Task.WhenAny(client.ConnectAsync(endPoint.Address, endPoint.Port), Task.Delay(timeout));
            if (!client.Connected)
            {
                client.Close();
                throw new TimeoutException("Timeout occurred while trying to connect to the terminal.");
            }

            // Get the stream for the connection
            connection._stream = client.GetStream();

            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Connected!");

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

            // Make sure we have a single endpoint.
            // This ensures the gatekeeper cannot be cheated by different dns names for the same endpoint.
            var ipAddress = Dns.GetHostAddresses(host).FirstOrDefault();
            if (ipAddress == null)
                throw new ArgumentException("Invalid host address.", "host");

            return new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Disconnects any open connection to a terminal.
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
            {
                Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Already disconnected.");
                GateKeeper.Exit(_endPoint);
                return;
            }

            _stream.Close();
            _tcpClient.Close();
            GateKeeper.Exit(_endPoint);

            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + ": Disconnected.");
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
                _stream.Dispose();
            }

            _disposed = true;
        }
    }
}
