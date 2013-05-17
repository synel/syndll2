using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
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
                AssertValidNetworkConfiguration(configuration);
            }
        }

        [TestMethod]
        public async Task Can_Get_Network_Configuration_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var configuration = await client.Terminal.GetNetworkConfigurationAsync();
                AssertValidNetworkConfiguration(configuration);
            }
        }

        private static void AssertValidNetworkConfiguration(NetworkConfiguration config)
        {
            // Test that we got some configuration data back.
            Assert.IsNotNull(config);

            // Test the network card type and firmware version
            Assert.AreEqual(NetworkCardTypes.F_Ethernet100Mbps, config.NetworkCardType);
            Assert.AreEqual(50, config.NetworkCardFirmwareVersion);

            // Test MAC sending and polling modes
            Assert.IsFalse(config.EnableSendMAC);
            Assert.IsFalse(config.EnablePolling);
            Assert.AreEqual(TimeSpan.Zero, config.PollingInterval);

            // Test the transport and IP settings // TODO: Can we determine the correct values from the IP address somehow?
            Assert.AreEqual(TransportType.Tcp, config.TransportType);
            Assert.AreEqual(new PhysicalAddress(new byte[] {0x00, 0x0E, 0xE3, 0x02, 0x84, 0x86}), config.TerminalMACAddress);
            Assert.AreEqual(IPAddress.Parse(TestSettings.HostAddress), config.TerminalIPAddress);
            Assert.AreEqual(IPAddress.Parse("10.10.10.1"), config.GatewayIPAddress);
            Assert.AreEqual(IPAddress.Parse("0.0.0.0"), config.RemoteIPAddress);
            Assert.AreEqual(IPAddress.Parse("255.255.254.0"), config.SubnetMask);
            Assert.AreEqual(TestSettings.TcpPort, config.TerminalPort);
            Assert.AreEqual(3734, config.RemotePort);
            Assert.IsFalse(config.EnableDHCP);
        }
    }
}
