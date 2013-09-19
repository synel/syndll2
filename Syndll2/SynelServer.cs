using System;
using System.Diagnostics;
using System.Threading;

namespace Syndll2
{
    public class SynelServer : IDisposable
    {
        private readonly IConnection _connection;
        private bool _disposed;

        private SynelServer(IConnection connection)
        {
            _connection = connection;
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
                _connection.Dispose();

            _disposed = true;
        }

        public static SynelServer Listen(Action<PushNotification> action)
        {
            return Listen(3734, action);
        }

        public static SynelServer Listen(TimeSpan idleTimeout, Action<PushNotification> action)
        {
            return Listen(3734, idleTimeout, action);
        }

        public static SynelServer Listen(int port, Action<PushNotification> action)
        {
            var idleTimeout = TimeSpan.FromSeconds(3);
            return Listen(3734, idleTimeout, action);
        }

        public static SynelServer Listen(int port, TimeSpan idleTimeout, Action<PushNotification> action)
        {
            var listener = NetworkConnection.Listen(port, connection =>
            {
                var signal = new ManualResetEvent(false);

                // Prepare the timer for disconnect on idle timeout
                using (var timer = new Timer(state =>
                {
                    Util.Log("Idle Timeout");
                    signal.Set();
                }, null, idleTimeout, Timeout.InfiniteTimeSpan))
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
                                    Util.Log(string.Format("Listener Received: {0}", message.RawResponse));
                                    action(notification);
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
            return new SynelServer(listener);
        }
    }
}
