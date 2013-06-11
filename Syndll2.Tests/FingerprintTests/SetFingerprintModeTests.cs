using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class SetFingerprintModeTests
    {
        [TestMethod]
        public void Can_Set_Fingerprint_Unit_Mode()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.SetUnitMode(FingerprintUnitModes.Slave);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Unit_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetUnitModeAsync(FingerprintUnitModes.Slave);
            }
        }

        [TestMethod]
        public void Can_Set_Fingerprint_Threshold()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.SetThreshold(FingerprintThreshold.Medium);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Threshold_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.Medium);
            }
        }

        [TestMethod]
        public void Can_Set_Fingerprint_Enroll_Mode()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.SetEnrollMode(FingerprintEnrollModes.Once);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Enroll_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetEnrollModeAsync(FingerprintEnrollModes.Once);
            }
        }
    }
}
