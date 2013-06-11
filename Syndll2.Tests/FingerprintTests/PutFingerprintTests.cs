using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class PutFingerprintTests
    {
        [TestMethod]
        public void Can_Put_Fingerprint_Template()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
            }
        }

        [TestMethod]
        public async Task Can_Put_Fingerprint_Template_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
            }
        }
    }
}
