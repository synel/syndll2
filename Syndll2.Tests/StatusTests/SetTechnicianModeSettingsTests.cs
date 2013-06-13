using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class SetTechnicianModeSettingsTests
    {
        [TestMethod]
        public void Can_Set_Technician_Mode_Settings()
        {
            using (var client = TestSettings.Connect())
            {
                var settings = new TechnicianModeSettings(serialPort2BaudRate: 57600);
                client.Terminal.SetTechnicianModeSettings(settings);
                settings = client.Terminal.GetTechnicianModeSettings();
                Assert.AreEqual(57600, settings.SerialPort2BaudRate);

                settings = new TechnicianModeSettings();
                client.Terminal.SetTechnicianModeSettings(settings);
                settings = client.Terminal.GetTechnicianModeSettings();
                Assert.AreEqual(19200, settings.SerialPort2BaudRate);
            }
        }

        [TestMethod]
        public async Task Can_Set_Technician_Mode_Settings_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var settings = new TechnicianModeSettings(serialPort2BaudRate: 57600);
                await client.Terminal.SetTechnicianModeSettingsAsync(settings);
                settings = await client.Terminal.GetTechnicianModeSettingsAsync();
                Assert.AreEqual(57600, settings.SerialPort2BaudRate);

                settings = new TechnicianModeSettings();
                await client.Terminal.SetTechnicianModeSettingsAsync(settings);
                settings = await client.Terminal.GetTechnicianModeSettingsAsync();
                Assert.AreEqual(19200, settings.SerialPort2BaudRate);
            }
        }
    }
}
