using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class SetTechnicianModeSettingsTests
    {
        [TestMethod]
        public void Can_Set_Technician_Mode_Settings_Raw_RoundTrip()
        {
            using (var client = TestSettings.Connect())
            {
                var settings = client.Terminal.GetTechnicianModeSettings();
                client.Terminal.SetTechnicianModeSettings(settings);
            }
        }
        
        [TestMethod]
        public void Can_Set_Technician_Mode_Settings()
        {
            // test with default settings
            var settings = new TechnicianModeSettings();

            using (var client = TestSettings.Connect())
            {
                client.Terminal.SetTechnicianModeSettings(settings);
            }
        }

        [TestMethod]
        public async Task Can_Set_Technician_Mode_Settings_Async()
        {
            // test with default settings
            var settings = new TechnicianModeSettings();

            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.SetTechnicianModeSettingsAsync(settings);
            }
        }
    }
}
