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
                p.Fingerprint.SetUnitMode(FingerprintUnitModes.Master);
                var status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintUnitModes.Master, status.FingerprintUnitMode);

                p.Fingerprint.SetUnitMode(FingerprintUnitModes.Slave);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintUnitModes.Slave, status.FingerprintUnitMode);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Unit_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetUnitModeAsync(FingerprintUnitModes.Master);
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintUnitModes.Master, status.FingerprintUnitMode);

                await p.Fingerprint.SetUnitModeAsync(FingerprintUnitModes.Slave);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintUnitModes.Slave, status.FingerprintUnitMode);
            }
        }

        [TestMethod]
        public void Can_Set_Fingerprint_Threshold()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.SetThreshold(FingerprintThreshold.VeryHigh);
                var status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintThreshold.VeryHigh, status.GlobalThreshold);

                p.Fingerprint.SetThreshold(FingerprintThreshold.VeryLow);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintThreshold.VeryLow, status.GlobalThreshold);

                p.Fingerprint.SetThreshold(FingerprintThreshold.High);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintThreshold.High, status.GlobalThreshold);

                p.Fingerprint.SetThreshold(FingerprintThreshold.Low);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintThreshold.Low, status.GlobalThreshold);

                p.Fingerprint.SetThreshold(FingerprintThreshold.Medium);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintThreshold.Medium, status.GlobalThreshold);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Threshold_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.VeryHigh);
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintThreshold.VeryHigh, status.GlobalThreshold);

                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.VeryLow);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintThreshold.VeryLow, status.GlobalThreshold);

                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.High);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintThreshold.High, status.GlobalThreshold);

                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.Low);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintThreshold.Low, status.GlobalThreshold);

                await p.Fingerprint.SetThresholdAsync(FingerprintThreshold.Medium);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintThreshold.Medium, status.GlobalThreshold);
            }
        }

        [TestMethod]
        public void Can_Set_Fingerprint_Enroll_Mode()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.SetEnrollMode(FingerprintEnrollModes.Dual);
                var status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintEnrollModes.Dual, status.EnrollMode);

                p.Fingerprint.SetEnrollMode(FingerprintEnrollModes.Twice);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintEnrollModes.Twice, status.EnrollMode);

                p.Fingerprint.SetEnrollMode(FingerprintEnrollModes.Once);
                status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(FingerprintEnrollModes.Once, status.EnrollMode);
            }
        }

        [TestMethod]
        public async Task Can_Set_Fingerprint_Enroll_Mode_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.SetEnrollModeAsync(FingerprintEnrollModes.Dual);
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintEnrollModes.Dual, status.EnrollMode);

                await p.Fingerprint.SetEnrollModeAsync(FingerprintEnrollModes.Twice);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintEnrollModes.Twice, status.EnrollMode);

                await p.Fingerprint.SetEnrollModeAsync(FingerprintEnrollModes.Once);
                status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(FingerprintEnrollModes.Once, status.EnrollMode);
            }
        }
    }
}
