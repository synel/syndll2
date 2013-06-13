using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ReceiveTests
    {
        private string _rawData;

        [TestMethod]
        public void Can_Receive_Inbound_Messages_From_Event()
        {
            using (var client = TestSettings.Connect())
            {
                client.MessageReceived += ClientOnMessageReceived;

                client.Terminal.GetTerminalStatus();
            }

            Assert.IsNotNull(_rawData);
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (args.Exception != null)
                throw args.Exception;

            _rawData = args.RawResponse;
        }
    }
}