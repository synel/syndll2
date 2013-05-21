using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class SetTerminalStatusTests
    {
        [TestMethod]
        public void Can_Set_Terminal_Status()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.SetTerminalStatus(DateTime.Now, 'I');
            }
        }

        [TestMethod]
        public async Task Can_Set_Terminal_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.SetTerminalStatusAsync(DateTime.Now, 'I');
            }
        }

        [TestMethod]
        public void Can_Set_Terminal_Clock()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.SetTerminalClock(DateTime.Now);
            }
        }

        [TestMethod]
        public async Task Can_Set_Terminal_Clock_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.SetTerminalClockAsync(DateTime.Now);
            }
        }

        [TestMethod]
        public void Can_Set_Terminal_Clock_With_Verification()
        {
            using (var client = TestSettings.Connect())
            {
                // set the clock to a fake time
                var dt = new DateTime(2000, 1, 1, 23, 00, 00);
                client.Terminal.SetTerminalClock(dt);
                
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
        public void Can_Set_Terminal_Time_Zone()
        {
            using (var client = TestSettings.Connect())
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                client.Terminal.SetTerminalTimeZone(tz);
            }
        }

        [TestMethod]
        public async Task Can_Set_Terminal_Time_Zone_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                await client.Terminal.SetTerminalTimeZoneAsync(tz);
            }
        }
    }
}
