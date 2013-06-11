using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class DeleteFingerprintTests
    {
        [TestMethod]
        public void Can_Delete_Fingerprint_Template()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.DeleteTemplate(1);
            }
        }

        [TestMethod]
        public async Task Can_Delete_Fingerprint_Template_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.DeleteTemplateAsync(1);
            }
        }

        [TestMethod]
        public void Can_Delete_All_Fingerprint_Templates()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.Fingerprint.DeleteAllTemplates();
            }
        }

        [TestMethod]
        public async Task Can_Delete_All_Fingerprint_Templates_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.Fingerprint.DeleteAllTemplatesAsync();
            }
        }
    }
}
