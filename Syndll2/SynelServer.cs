using System;
using System.Collections.Generic;
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

        public static SynelServer Listen(int port, Action<PushNotification> action)
        {
            var connection = NetworkConnection.Listen(port, stream =>
                {
                    var history = new List<string>();

                    var receiver = new Receiver(stream);
                    var signal = new SemaphoreSlim(1);
                    receiver.MessageReceived += (sender, args) =>
                        {
                            if (!history.Contains(args.RawResponse))
                            {
                                history.Add(args.RawResponse);

                                Util.Log(string.Format("Received: {0}", args.RawResponse));
                                if (args.Response != null)
                                {
                                    var notification = new PushNotification(stream, args.Response);
                                    action(notification);
                                }
                            }
                            signal.Release();
                        };
                    receiver.WatchStream();
                    while (stream.CanRead)
                        signal.Wait();
                });
            return new SynelServer(connection);
        }
    }
}
