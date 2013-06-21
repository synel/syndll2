using System.Diagnostics;
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
                Assert.IsNotNull(status);
                DisplayTerminalStatus(status);
            }
        }

        [TestMethod]
        public async Task Can_Get_Terminal_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var status = await client.Terminal.GetTerminalStatusAsync();
                Assert.IsNotNull(status);
                DisplayTerminalStatus(status);
            }
        }

        private static void DisplayTerminalStatus(TerminalStatus status)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Hardware Model:      {0}", status.HardwareModel);
            Debug.WriteLine("Hardware Revision:   {0}", status.HardwareRevision);
            Debug.WriteLine("Firmware Version:    {0}", status.FirmwareVersion);
            Debug.WriteLine("Terminal Type:       {0}", status.TerminalType);
            Debug.WriteLine("Current Time:        {0:g}", status.Timestamp);
            Debug.WriteLine("Active Function:     {0}", status.ActiveFunction);
            Debug.WriteLine("Powered On:          {0}", status.PoweredOn);
            Debug.WriteLine("Buffers Full:        {0}", status.BuffersFull);
            Debug.WriteLine("Buffers Faulty:      {0}", status.BuffersFaulty);
            Debug.WriteLine("Buffers Transmitted: {0}", status.BuffersTransmitted);
            Debug.WriteLine("Buffers Empty:       {0}", status.BuffersEmpty);
            Debug.WriteLine("Memory Used:         {0} bytes", status.MemoryUsed);
            Debug.WriteLine("Polling Interval:    {0} seconds", status.PollingInterval.TotalSeconds);
            Debug.WriteLine("Transport Type:      {0}", new object[] { status.TransportType.ToString().ToUpperInvariant() });
            Debug.WriteLine("FPU Mode:            {0}", status.FingerprintUnitMode);
            Debug.WriteLine("User Defined Field:  {0}", new object[] {status.UserDefinedField});
            Debug.WriteLine("");
        }
    }
}
