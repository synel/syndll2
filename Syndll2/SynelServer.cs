using System;
using System.Threading;

namespace Syndll2
{
    public class SynelServer : IDisposable
    {
        private readonly int _port;
        private readonly TimeSpan _idleTimeout;
        private IConnection _connection;
        private bool _disposed;

        public SynelServer(int port = 3734, int idleTimeoutSeconds = 3)
        {
            _port = port;
            _idleTimeout = TimeSpan.FromSeconds(idleTimeoutSeconds);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _connection != null)
            {
                _connection.Dispose();
            }

            _disposed = true;
        }

        
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, args);
        }
        
        public void Listen()
        {
            _connection = NetworkConnection.Listen(_port, connection =>
            {
                var signal = new ManualResetEvent(false);

                // Prepare the timer for disconnect on idle timeout
                using (var timer = new Timer(state =>
                {
                    Util.Log("Idle Timeout");
                    signal.Set();
                }, null, _idleTimeout, Timeout.InfiniteTimeSpan))
                {
                    var lineNeedsToBeReset = false;

                    var receiver = new Receiver(connection.Stream, () => connection.Connected);
                    receiver.MessageHandler = message =>
                    {
                        // Stop the idle timeout timer, since we have data
                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        }
                        catch (ObjectDisposedException)
                        {
                            // Swallow these
                        }

                        // Ignore any backlog of messages
                        if (!message.LastInBuffer)
                        {
                            lineNeedsToBeReset = true;
                            return;
                        }

                        if (message.Response != null)
                        {
                            using (var client = new SynelClient(connection, message.Response.TerminalId, true))
                            {
                                // Reset the line if we detected a backlog earlier
                                if (lineNeedsToBeReset)
                                {
                                    client.Terminal.ResetLine();
                                    lineNeedsToBeReset = false;
                                }

                                var notification = new PushNotification(client, message.Response);

                                // Only push valid notifications
                                if (Enum.IsDefined(typeof(NotificationType), notification.Type))
                                {
                                    Util.Log(string.Format("Listener Received: {0}", message.RawResponse), connection.RemoteEndPoint.Address);
                                    OnMessageReceived(new MessageReceivedEventArgs {Notification = notification});
                                }
                            }
                        }
                        signal.Set();
                    };

                    receiver.WatchStream();

                    // Wait until a message is received
                    while (connection.Stream.CanRead && connection.Connected)
                        if (signal.WaitOne(100))
                            break;
                }
            });
        }
    }
}
