using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [TestClass]
    public class GetFingerprintUnitStatusTests
    {
        [TestMethod]
        public void Can_Get_Fingerprint_Unit_Status()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                var status = p.Fingerprint.GetUnitStatus();
                AssertValidFingerprintUnitStatus(status);
            }
        }

        [TestMethod]
        public async Task Can_Get_Fingerprint_Unit_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                var status = await p.Fingerprint.GetUnitStatusAsync();
                AssertValidFingerprintUnitStatus(status);
            }
        }

        private static void AssertValidFingerprintUnitStatus(FingerprintUnitStatus status)
        {
            // Test that we got some status back.
            Assert.IsNotNull(status);

            Assert.AreEqual(FingerprintComparisonModes.Unknown, status.ComparisonMode);
            Assert.AreEqual("B16F06080800", status.KernelVersion);
            Assert.AreEqual(23, status.LoadedTemplates);
            Assert.AreEqual(9090, status.MaximumTemplates);
            Assert.AreEqual(FingerprintUnitModes.Slave, status.FingerprintUnitMode);
            Assert.AreEqual(FingerprintThreshold.Medium, status.GlobalThreshold);
        }
    }
}
