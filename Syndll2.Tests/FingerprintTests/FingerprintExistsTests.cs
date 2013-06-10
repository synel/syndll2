using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class FingerprintExistsTests
    {
        [TestMethod]
        public void Can_Check_Fingerprint_Exists()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                var exists = p.Fingerprint.TemplateExists(1, 0);
                Assert.IsTrue(exists);
            }
        }

        [TestMethod]
        public async Task Can_Check_Fingerprint_Exists_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                var exists = await p.Fingerprint.TemplateExistsAsync(1, 0);
                Assert.IsTrue(exists);
            }
        }
    }
}
