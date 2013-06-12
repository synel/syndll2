using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class DeleteFingerprintTests
    {
        [TestMethod]
        public void Can_Delete_Single_Fingerprint_Template()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                p.Fingerprint.DeleteAllTemplates();
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);

                // Act
                p.Fingerprint.DeleteTemplate(1);

                // Assert
                var status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(0, status.LoadedTemplates);
            }
        }

        [TestMethod]
        public async Task Can_Delete_Single_Fingerprint_Template_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                await p.Fingerprint.DeleteAllTemplatesAsync();
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);

                // Act
                await p.Fingerprint.DeleteTemplateAsync(1);

                // Assert
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(0, status.LoadedTemplates);
            }
        }

        [TestMethod]
        public void Can_Delete_All_Fingerprint_Templates()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);

                // Act
                p.Fingerprint.DeleteAllTemplates();

                // Assert
                var status = p.Fingerprint.GetUnitStatus();
                Assert.AreEqual(0, status.LoadedTemplates);
            }
        }

        [TestMethod]
        public async Task Can_Delete_All_Fingerprint_Templates_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);

                // Act
                await p.Fingerprint.DeleteAllTemplatesAsync();

                // Assert
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(0, status.LoadedTemplates);
            }
        }
    }
}
