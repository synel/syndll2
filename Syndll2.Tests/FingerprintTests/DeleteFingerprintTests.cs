using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class DeleteFingerprintTests
    {
        [TestMethod]
        public void Can_Delete_Single_Fingerprint_Template_Multiple()
        {
            using (var client = TestSettings.Connect())
            {
                using (var p = client.Terminal.Programming())
                {
                    // Arrange
                    p.Fingerprint.DeleteAllTemplates();
                    p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                    p.Fingerprint.PutTemplate(1, TestTemplate.Data);

                    // Act
                    p.Fingerprint.DeleteTemplate(1, 0);
                    p.Fingerprint.DeleteTemplate(1, 1);
                    var templates = p.Fingerprint.ListTemplates();
                    foreach (var template in templates)
                        Debug.WriteLine("{0} : {1}", template.Key, template.Value);
                    
                    // Assert
                    var status = p.Fingerprint.GetUnitStatus();
                    Assert.AreEqual(0, status.LoadedTemplates);
                }
            }
        }

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
                p.Fingerprint.DeleteTemplate(1,0);
                
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
                await p.Fingerprint.DeleteTemplateAsync(1, 0);

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
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                p.Fingerprint.PutTemplate(2, TestTemplate.Data);
                p.Fingerprint.PutTemplate(2, TestTemplate.Data);
                p.Fingerprint.PutTemplate(3, TestTemplate.Data);

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
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(2, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(2, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(3, TestTemplate.Data);

                // Act
                await p.Fingerprint.DeleteAllTemplatesAsync();

                // Assert
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.AreEqual(0, status.LoadedTemplates);
            }
        }
    }
}
