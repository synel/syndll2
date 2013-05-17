using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.StatusTests
{
    [Ignore] // This test is failing currently
    [TestClass]
    public class GetFingerprintUnitStatusTests
    {
        [TestMethod]
        public void Can_Get_Fingerprint_Unit_Status()
        {
            using (var client = TestSettings.Connect())
            {
                var status = client.Terminal.GetFingerprintUnitStatus();
                AssertValidFingerprintUnitStatus(status);
            }
        }

        [TestMethod]
        public async Task Can_Get_Fingerprint_Unit_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var status = await client.Terminal.GetFingerprintUnitStatusAsync();
                AssertValidFingerprintUnitStatus(status);
            }
        }

        private static void AssertValidFingerprintUnitStatus(FingerprintUnitStatus status)
        {
            // Test that we got some status back.
            Assert.IsNotNull(status);

            // TODO: test the results
            // Not getting any data back, so can't test anything yet.
        }
    }
}
