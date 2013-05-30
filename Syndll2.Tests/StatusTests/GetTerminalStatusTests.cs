using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class GetTerminalStatusTests
    {
        [TestMethod]
        public void Can_Get_Terminal_Status_Raw_Multi_Threaded()
        {
            // to test the gatekeeper functionality
            Parallel.For(0, 10, x => Can_Get_Terminal_Status());
        }

        [TestMethod]
        public void Can_Get_Terminal_Status()
        {
            using (var client = TestSettings.Connect())
            {
                var status = client.Terminal.GetTerminalStatus();
                AssertValidTerminalStatus(status);
            }
        }

        [TestMethod]
        public async Task Can_Get_Terminal_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var status = await client.Terminal.GetTerminalStatusAsync();
                AssertValidTerminalStatus(status);
            }
        }

        private static void AssertValidTerminalStatus(TerminalStatus status)
        {
            // Test that we got some status back.
            Assert.IsNotNull(status);

            // Test the hardware and firmware versions
            Assert.AreEqual(8, status.HardwareModel);
            Assert.AreEqual(0, status.HardwareRevision);
            Assert.AreEqual(80234, status.FirmwareVersion);

            // Test that we are talking to a 785 and it is powered on.
            Assert.AreEqual(TerminalTypes.SY78x, status.TerminalType);
            Assert.IsTrue(status.PoweredOn);

            // Test that the clock is +/- 5 minutes.  Higher accuracy isn't important for this test.
            Assert.AreEqual(DateTimeKind.Unspecified, status.Timestamp.Kind);
            Assert.IsTrue(Math.Abs((DateTime.Now - status.Timestamp).TotalMinutes) < 5);

            // Test the current function.  TODO: this will vary
            Assert.AreEqual('I', status.ActiveFunction);

            // Test the buffers and memory reported.  TODO: these may vary
            //Assert.AreEqual(0, status.BuffersFull);
            //Assert.AreEqual(0, status.BuffersFaulty);
            //Assert.AreEqual(0, status.BuffersTransmitted);
            //Assert.AreEqual(6128, status.BuffersEmpty);
            //Assert.AreEqual(79872, status.MemoryUsed);

            // Test the user defined field
            Assert.AreEqual(7234, status.UserDefinedField);

            // Test the network info
            Assert.AreEqual(TransportType.Tcp, status.TransportType);
            Assert.AreEqual(TimeSpan.FromSeconds(15), status.PollingInterval);

            // Test the fingerprint mode
            Assert.AreEqual(FingerprintUnitModes.Slave, status.FingerprintUnitMode);
        }
    }
}
