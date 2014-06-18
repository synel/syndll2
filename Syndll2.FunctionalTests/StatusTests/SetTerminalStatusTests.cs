using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.FunctionalTests.StatusTests
{
    [TestClass]
    public class SetTerminalStatusTests
    {
        [TestMethod]
        public void Can_Set_Terminal_Clock()
        {
            using (var client = TestSettings.Connect())
            {
                // set the clock to a fake time
                var dt = new DateTime(2000, 1, 1, 00, 00, 00);
                client.Terminal.SetTerminalClock(dt);

                // wait for the terminal to process the update
                Thread.Sleep(100);

                // get the status, which contains the clock timestamp
                var status = client.Terminal.GetTerminalStatus();

                // put back the real time
                client.Terminal.SetTerminalClock(DateTime.Now);

                // result should be either exact, or off by one minute at most
                Assert.IsTrue(status.Timestamp - dt < TimeSpan.FromMinutes(1),
                              "Set: {0}  Got: {1}", dt, status.Timestamp);
            }
        }

        [TestMethod]
        public async Task Can_Set_Terminal_Clock_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                // set the clock to a fake time
                var dt = new DateTime(2000, 1, 1, 00, 00, 00);
                await client.Terminal.SetTerminalClockAsync(dt);

                // wait for the terminal to process the update
                Thread.Sleep(100);

                // get the status, which contains the clock timestamp
                var status = await client.Terminal.GetTerminalStatusAsync();

                // put back the real time
                await client.Terminal.SetTerminalClockAsync(DateTime.Now);

                // result should be either exact, or off by one minute at most
                Assert.IsTrue(status.Timestamp - dt < TimeSpan.FromMinutes(1),
                              "Set: {0}  Got: {1}", dt, status.Timestamp);
            }
        }

        [TestMethod]
        public void Can_Set_Active_Function()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.SetActiveFunction('G');
                Thread.Sleep(100);
                var status = client.Terminal.GetTerminalStatus();
                Assert.AreEqual('G', status.ActiveFunction);
                
                client.Terminal.SetActiveFunction('I');
                Thread.Sleep(100);
                status = client.Terminal.GetTerminalStatus();
                Assert.AreEqual('I', status.ActiveFunction);
            }
        }

        [TestMethod]
        public async Task Can_Set_Active_Function_Async()
        {
            using (var client = TestSettings.Connect())
            {
                await client.Terminal.SetActiveFunctionAsync('G');
                Thread.Sleep(100);
                var status = await client.Terminal.GetTerminalStatusAsync();
                Assert.AreEqual('G', status.ActiveFunction);
                
                await client.Terminal.SetActiveFunctionAsync('I');
                Thread.Sleep(100);
                status = await client.Terminal.GetTerminalStatusAsync();
                Assert.AreEqual('I', status.ActiveFunction);
            }
        }
    }
}
