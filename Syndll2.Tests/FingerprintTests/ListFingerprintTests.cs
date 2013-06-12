using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.FingerprintTests
{
    [TestClass]
    public class ListFingerprintTests
    {
        [TestMethod]
        public void Can_List_Fingerprint_Templates()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                p.Fingerprint.DeleteAllTemplates();
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                p.Fingerprint.PutTemplate(1, TestTemplate.Data);
                p.Fingerprint.PutTemplate(2, TestTemplate.Data);

                // Act
                var list = p.Fingerprint.ListTemplates();

                // Assert
                Assert.AreEqual(2, list.Count);
                Assert.IsTrue(list.ContainsKey(1));
                Assert.IsTrue(list.ContainsKey(2));
                Assert.AreEqual(2, list[1]);
                Assert.AreEqual(1, list[2]);
            }
        }

        [TestMethod]
        public async Task Can_List_Fingerprint_Templates_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                // Arrange
                await p.Fingerprint.DeleteAllTemplatesAsync();
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(1, TestTemplate.Data);
                await p.Fingerprint.PutTemplateAsync(2, TestTemplate.Data);

                // Act
                var list = await p.Fingerprint.ListTemplatesAsync();

                // Assert
                Assert.AreEqual(2, list.Count);
                Assert.IsTrue(list.ContainsKey(1));
                Assert.IsTrue(list.ContainsKey(2));
                Assert.AreEqual(2, list[1]);
                Assert.AreEqual(1, list[2]);
            }
        }
    }
}
