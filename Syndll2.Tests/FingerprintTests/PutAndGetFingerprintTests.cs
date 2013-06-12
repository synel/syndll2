using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class PutAndGetFingerprintTests
    {
        [TestMethod]
        public void Can_Put_And_Get_Fingerprint_Template()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                p.Fingerprint.DeleteAllTemplates();
                
                // Act
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                var data = p.Fingerprint.GetTemplate(1, 0);

                // Assert
                Assert.AreEqual(TestTemplate.Data.Length, data.Length);
                CollectionAssert.AreEqual(TestTemplate.Data, data);
            }
        }

        [TestMethod]
        public async Task Can_Put_And_Get_Fingerprint_Template_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                await p.Fingerprint.DeleteAllTemplatesAsync();
                
                // Act
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
                var data = await p.Fingerprint.GetTemplateAsync(1, 0);

                // Assert
                Assert.AreEqual(TestTemplate.Data.Length, data.Length);
                CollectionAssert.AreEqual(TestTemplate.Data, data);
            }
        }
    }
}
