using System.Diagnostics;
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

            Debug.WriteLine("");
            Debug.WriteLine("Terminal ID:         {0}", config.TerminalId);
            Debug.WriteLine("Terminal Type:       {0}", config.TerminalType);
            Debug.WriteLine("Firmware Version:    {0} ({1:d})", config.FirmwareVersion, config.FirmwareDate);
            Debug.WriteLine("Keyboard Type:       {0}", config.KeyboardType);
            Debug.WriteLine("Display Type:        {0}", config.DisplayType);
            Debug.WriteLine("FPU Type:            {0}", config.FingerprintUnitType);
            Debug.WriteLine("FPU Mode:            {0}", config.FingerprintUnitMode);
            Debug.WriteLine("Serial Port Info:    {0} {1}", config.HostSerialBaudRate, config.HostSerialParameters.ToUpperInvariant());
            Debug.WriteLine("User Defined Field:  {0}", config.UserDefinedField);
            Debug.WriteLine("");
        }
    }
}
