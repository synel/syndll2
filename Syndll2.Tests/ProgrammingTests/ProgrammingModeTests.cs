using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class ProgrammingModeTests
    {
        [TestMethod]
        public void Can_Enter_Programming_Mode()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.Halt();
            }
        }

        [TestMethod]
        public async Task Can_Enter_Programming_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.HaltAsync();
            }
        }

        [TestMethod]
        public void Can_Exit_Programming_Mode()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.Run();
            }
        }

        [TestMethod]
        public async Task Can_Exit_Programming_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.RunAsync();
            }
        }
    }
}
