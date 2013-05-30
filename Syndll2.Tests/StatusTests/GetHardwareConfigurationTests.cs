using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class GetHardwareConfigurationTests
    {
        [TestMethod]
        public void Can_Get_Hardware_Configuration()
        {
            using (var client = TestSettings.Connect())
            {
                var configuration = client.Terminal.GetHardwareConfiguration();
                AssertValidHardwareConfiguration(configuration);
            }
        }

        [TestMethod]
        public async Task Can_Get_Hardware_Configuration_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var configuration = await client.Terminal.GetHardwareConfigurationAsync();
                AssertValidHardwareConfiguration(configuration);
            }
        }

        private static void AssertValidHardwareConfiguration(HardwareConfiguration config)
        {
            // Test that we got some configuration data back.
            Assert.IsNotNull(config);

            // Test the firmware version and date
            Assert.AreEqual(80234, config.FirmwareVersion);
            Assert.AreEqual(new DateTime(2005, 6, 9), config.FirmwareDate);

            // Test the hardware configuration
            Assert.AreEqual(TerminalTypes.SY78x, config.TerminalType);
            Assert.AreEqual(KeyboardTypes.Big_24Keys, config.KeyboardType);
            Assert.AreEqual(DisplayTypes.DoubleLine, config.DisplayType);
            Assert.AreEqual(FingerprintUnitModes.Slave, config.FingerprintUnitMode);
            Assert.AreEqual(3, config.FingerprintUnitType);

            // Test the host serial parameters
            Assert.AreEqual("8n1", config.HostSerialParameters);
            Assert.AreEqual(19200, config.HostSerialBaudRate);

            // Test the terminal id
            Assert.AreEqual(TestSettings.TerminalId, config.TerminalId);
        }
    }
}
