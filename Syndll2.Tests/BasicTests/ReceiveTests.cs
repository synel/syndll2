using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ReceiveTests
    {
        [TestMethod]
        public void Can_Receive_Inbound_Messages()
        {
            using (var client = TestSettings.Connect())
            {
                client.MessageReceived += ClientOnMessageReceived;

                try
                {
                    var status = client.Terminal.GetTerminalStatus();
                }
                catch
                {
                }

                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            }
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine(args.RawResponse);
            if (args.Exception != null)
                Console.WriteLine(args.Exception.Message);
        }
    }
}