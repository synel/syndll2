using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.FunctionalTests.BasicTests
{
    [TestClass]
    public class DisplayMessageTests
    {
        [TestMethod]
        public void Can_Display_Message()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.DisplayMessage("Hello!", 3);
            }
        }

        [TestMethod]
        public async Task Can_Display_Message_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.DisplayMessageAsync("Hello!", 3);
            }
        }
    }
}
