using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        [Ignore] // run this manually, after enabling polling mode and recording some data
        public async Task Can_Receive_Inbound_Messages_From_Terminal()
        {
            var server = new SynelServer();
            server.MessageReceived += (sender, args) =>
            {
                var notification = args.Notification;
                if (notification.Type == NotificationType.Data)
                {
                    Console.WriteLine(notification.Data);
                    notification.Acknowledege();
                }

                if (notification.Type == NotificationType.Query)
                {
                    Console.WriteLine(notification.Data);
                    notification.Reply(true, 0, "Success");
                }
            };

            var cts = new CancellationTokenSource();
            server.ListenAsync(cts.Token).Wait(100000, cts.Token);

        }
    }
}