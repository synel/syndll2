using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class ClearBufferTests
    {
        [TestMethod]
        public void Can_Clear_Buffer()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.ClearBuffer();
            }
        }

        [TestMethod]
        public async Task Can_Clear_Buffer_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.ClearBufferAsync();
            }
        }
    }
}
