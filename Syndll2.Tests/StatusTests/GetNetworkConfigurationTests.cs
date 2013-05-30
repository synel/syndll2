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
            Assert.AreEqual(NetworkCardTypes.B_Ethernet10Or100Mbps, config.NetworkCardType);
            Assert.AreEqual(86, config.NetworkCardFirmwareVersion);

            // Test MAC sending and polling modes
            Assert.IsTrue(config.EnableSendMAC);
            Assert.IsTrue(config.EnablePolling);
            Assert.AreEqual(TimeSpan.FromSeconds(15), config.PollingInterval);

            // Test the transport and IP settings // TODO: Can we determine the correct values from the IP address somehow?
            Assert.AreEqual(TransportType.Tcp, config.TransportType);
            Assert.AreEqual(new PhysicalAddress(new byte[] {0x00, 0x08, 0xDC, 0x13, 0x29, 0x92}), config.TerminalMACAddress);
            Assert.AreEqual(IPAddress.Parse(TestSettings.HostAddress), config.TerminalIPAddress);
            Assert.AreEqual(IPAddress.Parse("10.10.10.1"), config.GatewayIPAddress);
            Assert.AreEqual(IPAddress.Parse("10.10.11.35"), config.RemoteIPAddress);
            Assert.AreEqual(IPAddress.Parse("255.255.254.0"), config.SubnetMask);
            Assert.AreEqual(TestSettings.TcpPort, config.TerminalPort);
            Assert.AreEqual(3734, config.RemotePort);
            Assert.IsFalse(config.EnableDHCP);
        }
    }
}
