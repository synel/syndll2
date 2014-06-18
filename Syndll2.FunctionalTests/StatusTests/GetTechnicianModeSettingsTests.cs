using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.FunctionalTests.StatusTests
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
                Assert.IsNotNull(settings);
                DisplaySettings(settings);
            }
        }

        [TestMethod]
        public async Task Can_Get_Technician_Mode_Settings_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var settings = await client.Terminal.GetTechnicianModeSettingsAsync();
                Assert.IsNotNull(settings);
                DisplaySettings(settings);
            }
        }

        private static void DisplaySettings(TechnicianModeSettings settings)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Terminal ID:     {0}", settings.TerminalId);
            Debug.WriteLine("Serial Port 0:   {0} {1}", settings.SerialPort0BaudRate, settings.SerialPort0Mode);
            Debug.WriteLine("Serial Port 1:   {0} {1}", settings.SerialPort1BaudRate, settings.SerialPort1Mode);
            Debug.WriteLine("Serial Port 2:   {0} {1}", settings.SerialPort2BaudRate, settings.SerialPort2Mode);
            Debug.WriteLine("Modem Installed: {0}{1}", settings.HasModem, settings.HasModem ? " (" + settings.ModemRingsToAnswer + " Rings to Answer)" : "");
            Debug.WriteLine("Network Card:    {0}", settings.NetworkCardType);
            Debug.WriteLine("");
        }
    }
}
