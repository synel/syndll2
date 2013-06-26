using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void Can_Connect_To_Terminal_Over_Tcp()
        {
            using (var client = TestSettings.Connect())
            {
                Assert.IsTrue(client.Connected);
            }
        }

        [TestMethod]
        public async Task Can_Connect_To_Terminal_Over_Tcp_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                Assert.IsTrue(client.Connected);
            }
        }

#if TESTING_MULTIPLE_TERMINALS
        [TestMethod]
        public void Can_Connect_To_Multiple_Terminals_Over_Tcp()
        {
            using (var client1 = SynelClient.Connect("010.010.011.128"))
            using (var client2 = SynelClient.Connect("010.010.011.140"))
            {
                Assert.IsTrue(client1.Connected);
                Assert.IsTrue(client2.Connected);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void Can_Block_Connections_To_The_Same_Terminal()
        {
            using (var client1 = SynelClient.Connect("10.10.11.128"))
            using (var client2 = SynelClient.Connect("10.10.11.128"))
            {
                Assert.IsTrue(client1.Connected);
                Assert.IsTrue(client2.Connected);
            }
        }
#endif

        [TestMethod]
        public void Can_Parse_IP_Addresses_With_Leading_Zeros()
        {
            var endpoint = NetworkConnection.GetEndPoint("010.088.097.060", 3734);
            var expected = new IPEndPoint(new IPAddress(new byte[] {10, 88, 97, 60}), 3734);
            Assert.AreEqual(expected, endpoint);
        }
    }
}
