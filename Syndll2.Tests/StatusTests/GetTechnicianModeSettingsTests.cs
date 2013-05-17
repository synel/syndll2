using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class GetTechnicianModeSettingsTests
    {
        [TestMethod]
        public void Can_Get_Technician_Mode_Settings()
        {
            using (var client = TestSettings.Connect())
            {
                var settings = client.Terminal.GetTechnicianModeSettings();
                AssertValidSettings(settings);
            }
        }

        [TestMethod]
        public async Task Can_Get_Technician_Mode_Settings_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var settings = await client.Terminal.GetTechnicianModeSettingsAsync();
                AssertValidSettings(settings);
            }
        }

        private static void AssertValidSettings(TechnicianModeSettings settings)
        {
            // Test that we got some status back.
            Assert.IsNotNull(settings);

            // Test the terminal id
            Assert.AreEqual(TestSettings.TerminalId, settings.TerminalId);

            // Test the serial port settings
            Assert.AreEqual(SerialPortModes.Host, settings.SerialPort0Mode);
            Assert.AreEqual(SerialPortModes.FingerprintUnit, settings.SerialPort1Mode);
            Assert.AreEqual(SerialPortModes.Printer, settings.SerialPort2Mode);
            Assert.AreEqual(19200, settings.SerialPort0BaudRate);
            Assert.AreEqual(57600, settings.SerialPort1BaudRate);
            Assert.AreEqual(19200, settings.SerialPort2BaudRate);

            // Test the modem settings
            Assert.IsFalse(settings.HasModem);
            Assert.AreEqual(0, settings.ModemRingsToAnswer);

            // Test the network card settings
            Assert.AreEqual(NetworkCardTypes.F_Ethernet100Mbps, settings.NetworkCardType);
        }
    }
}
