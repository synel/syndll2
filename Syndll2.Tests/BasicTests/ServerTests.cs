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
                    if (notification.Data != null)
                    {
                        Console.WriteLine(notification.Data);
                        notification.Acknowledege();
                    }

                }))
            {

                // run the server for awhile
                Thread.Sleep(10000);
            }
        }
    }
}