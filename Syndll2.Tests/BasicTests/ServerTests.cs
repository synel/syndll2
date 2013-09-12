using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        [Ignore] // run this manually, after enabling polling mode and recording some data
        public void Can_Receive_Inbound_Messages_From_Terminal()
        {
            using (SynelServer.Listen(notification =>
                {
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

                }))
            {

                // run the server for awhile
                Thread.Sleep(100000);
            }
        }
    }
}