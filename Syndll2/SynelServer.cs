using System;
using System.Collections.Generic;
using System.Net;
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
            var connection = NetworkConnection.Listen(port, (stream, socket) =>
                {
                    var history = new List<string>();

                    var receiver = new Receiver(stream);
                    var signal = new ManualResetEvent(false);
                    
                    receiver.MessageHandler = message => 
                        {
                            if (!history.Contains(message.RawResponse))
                            {
                                history.Add(message.RawResponse);

                                Util.Log(string.Format("Received: {0}", message.RawResponse));
                                if (message.Response != null)
                                {
                                    var notification = new PushNotification(stream, message.Response, (IPEndPoint) socket.RemoteEndPoint);
                                    action(notification);
                                }
                            }
                            signal.Set();
                        };
                    
                    receiver.WatchStream();

                    // Wait until a message is received
                    while (stream.CanRead && socket.Connected)
                        if (signal.WaitOne(100))
                            break;
                });
            return new SynelServer(connection);
        }
    }
}
