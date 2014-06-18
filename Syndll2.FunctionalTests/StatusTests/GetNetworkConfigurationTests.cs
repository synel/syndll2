using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.FunctionalTests.StatusTests
{
    [TestClass]
    public class GetNetworkConfigurationTests
    {
        [TestMethod]
        public void Can_Get_Network_Configuration()
        {
            using (var client = TestSettings.Connect())
            {
                var configuration = client.Terminal.GetNetworkConfiguration();
                Assert.IsNotNull(configuration);
                DisplayConfiguration(configuration);
            }
        }

        [TestMethod]
        public async Task Can_Get_Network_Configuration_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var configuration = await client.Terminal.GetNetworkConfigurationAsync();
                Assert.IsNotNull(configuration);
                DisplayConfiguration(configuration);
            }
        }

        private static void DisplayConfiguration(NetworkConfiguration config)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Network Card:        {0} (ver {1})", config.NetworkCardType, config.NetworkCardFirmwareVersion);
            Debug.WriteLine("Transport Type:      {0}", new object[] { config.TransportType.ToString().ToUpperInvariant() });
            Debug.WriteLine("MAC Address:         {0}", config.TerminalMACAddress);
            Debug.WriteLine("IP Address/Port:     {0}:{1}", config.TerminalIPAddress, config.TerminalPort);
            Debug.WriteLine("Remote Address/Port: {0}:{1}", config.RemoteIPAddress, config.RemotePort);
            Debug.WriteLine("Subnet Mask:         {0}", config.SubnetMask);
            Debug.WriteLine("Gateway Address:     {0}", config.GatewayIPAddress);
            Debug.WriteLine("Disconnect Time:     {0} seconds", config.DisconnectTime.TotalSeconds);
            Debug.WriteLine("Polling Interval:    {0} seconds", config.PollingInterval.TotalSeconds);
            Debug.WriteLine("Polling Enabled:     {0}", config.EnablePolling);
            Debug.WriteLine("DHCP Enabled:        {0}", config.EnableDHCP);
            Debug.WriteLine("MAC Sending Enabled: {0}", config.EnableSendMAC);
            Debug.WriteLine("");
        }
    }
}
