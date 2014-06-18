using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.FunctionalTests.BasicTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        //[Ignore] // run this manually, when appropriate
        public async Task Can_Receive_Inbound_Messages_From_Terminal()
        {
            var cts = new CancellationTokenSource();

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

                cts.Cancel();
            };

            await server.ListenAsync(cts.Token);

        }
    }
}