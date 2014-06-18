using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.FunctionalTests.ProgrammingTests
{
    [TestClass]
    public class ProgrammingModeTests
    {
        [TestMethod]
        public void Can_Enter_And_Exit_Programming_Mode()
        {
            using (var client = TestSettings.Connect())
            {
                ProgrammingStatus status1, status2;
                try
                {
                    status1 = client.Terminal.Halt();
                    status2 = client.Terminal.Run();
                }
                catch
                {
                    // always go back to run mode if possible
                    client.Terminal.Run();
                    throw;
                }

                if (status1.OperationStatus == ProgrammingOperationStatus.Unknown || status2.OperationStatus == ProgrammingOperationStatus.Unknown)
                {
                    Assert.Inconclusive("Could not verify programming status mode.");
                }
                else
                {
                    Assert.AreEqual(ProgrammingOperationStatus.InProgrammingMode, status1.OperationStatus);
                    Assert.AreEqual(ProgrammingOperationStatus.InRunMode, status2.OperationStatus);
                }
            }
        }

        [TestMethod]
        public async Task Can_Enter_And_Exit_Programming_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                ProgrammingStatus status1, status2;
                try
                {
                    status1 = await client.Terminal.HaltAsync();
                    status2 = await client.Terminal.RunAsync();
                }
                catch
                {
                    // always go back to run mode if possible
                    client.Terminal.Run();
                    throw;
                }

                if (status1.OperationStatus == ProgrammingOperationStatus.Unknown || status2.OperationStatus == ProgrammingOperationStatus.Unknown)
                {
                    Assert.Inconclusive("Could not verify programming status mode.");
                }
                else
                {
                    Assert.AreEqual(ProgrammingOperationStatus.InProgrammingMode, status1.OperationStatus);
                    Assert.AreEqual(ProgrammingOperationStatus.InRunMode, status2.OperationStatus);
                }
            }
        }
    }
}
