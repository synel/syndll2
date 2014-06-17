using System;
using System.Threading;
using System.Threading.Tasks;

namespace Syndll2
{
    public class SynelServer
    {
        private readonly int _port;
        private readonly TimeSpan _idleTimeout;
        
        public SynelServer(int port = 3734, int idleTimeoutSeconds = 3)
        {
            _port = port;
            _idleTimeout = TimeSpan.FromSeconds(idleTimeoutSeconds);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, args);
        }
        
        public Task ListenAsync(CancellationToken ct)
        {
            return NetworkConnection.ListenAsync(_port, async connection =>
            {
                var lineNeedsToBeReset = false;
                
                var cts = new CancellationTokenSource();
                using (var timer = new Timer(state =>
                {
                    Util.Log("Idle Timeout");
                    cts.Cancel();
                }, null, _idleTimeout, Timeout.InfiniteTimeSpan))
                {
                    while (connection.Connected && !cts.Token.IsCancellationRequested)
                    {
                        var receiver = new Receiver(connection.Stream);
                        var message = await receiver.ReceiveMessageAsync(cts.Token);
                        if (message == null)
                            return;

                        // Stop the idle timeout timer, since we have data
                        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                        // Ignore any backlog of messages
                        if (!message.LastInBuffer)
                        {
                            lineNeedsToBeReset = true;
                            continue;
                        }

                        // Make sure a good message came through
                        if (message.Response == null)
                            continue;

                        using (var client = new SynelClient(connection, message.Response.TerminalId))
                        {
                            // Reset the line if we detected a backlog earlier
                            if (lineNeedsToBeReset)
                            {
                                client.Terminal.ResetLine();
                                lineNeedsToBeReset = false;
                            }

                            var notification = new PushNotification(client, message.Response);

                            // Only push valid notifications
                            if (Enum.IsDefined(typeof (NotificationType), notification.Type))
                            {
                                Util.Log(string.Format("Listener Received: {0}", message.RawResponse), connection.RemoteEndPoint.Address);

                                OnMessageReceived(new MessageReceivedEventArgs {Notification = notification});
                            }
                        }
                    }
                }
            }, ct);
        }
    }
}
