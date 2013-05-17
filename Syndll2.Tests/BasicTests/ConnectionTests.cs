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
    }
}
