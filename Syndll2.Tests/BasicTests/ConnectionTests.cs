using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void Can_Parse_IP_Addresses_With_Leading_Zeros()
        {
            var endpoint = NetworkConnection.GetEndPoint("010.088.097.060", 3734);
            var expected = new IPEndPoint(new IPAddress(new byte[] {10, 88, 97, 60}), 3734);
            Assert.AreEqual(expected, endpoint);
        }
    }
}
