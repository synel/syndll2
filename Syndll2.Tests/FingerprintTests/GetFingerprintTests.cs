using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class GetFingerprintTests
    {
        [TestMethod]
        public void Can_Get_Fingerprint_Unit_Template()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                var data = p.Fingerprint.GetTemplate(1, 0);
                Assert.AreEqual(384, data.Length);
            }
        }

        [TestMethod]
        public async Task Can_Get_Fingerprint_Unit_Template_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                var data = await p.Fingerprint.GetTemplateAsync(1, 0);
                Assert.AreEqual(384, data.Length);
            }
        }
    }
}
