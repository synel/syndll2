using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class ProgrammingModeTests
    {
        [TestMethod]
        public void Can_Enter_And_Exit_Programming_Mode()
        {
            using (var client = TestSettings.Connect())
            {
                try
                {
                    var status1 = client.Terminal.Halt();
                    var status2 = client.Terminal.Run();

                    Assert.AreEqual(ProgrammingOperationStatus.InProgrammingMode, status1.OperationStatus);
                    Assert.AreEqual(ProgrammingOperationStatus.InRunMode, status2.OperationStatus);
                }
                catch
                {
                    // always go back to run mode if possible
                    client.Terminal.Run();
                }
            }
        }

        [TestMethod]
        public async Task Can_Enter_And_Exit_Programming_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                try
                {
                    var status1 = await client.Terminal.HaltAsync();
                    var status2 = await client.Terminal.RunAsync();

                    Assert.AreEqual(ProgrammingOperationStatus.InProgrammingMode, status1.OperationStatus);
                    Assert.AreEqual(ProgrammingOperationStatus.InRunMode, status2.OperationStatus);
                }
                catch
                {
                    // always go back to run mode if possible
                    client.Terminal.Run();
                }
            }
        }
    }
}
