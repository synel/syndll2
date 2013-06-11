using System.Diagnostics;
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
                var list = p.Fingerprint.ListTemplates();

                Debug.WriteLine("");
                foreach (var item in list)
                {
                    Debug.WriteLine("{0:D10} : {1}", item.Key, item.Value);
                }
                Debug.WriteLine("");

            }
        }

        [TestMethod]
        public async Task Can_List_Fingerprint_Templates_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                var list = await p.Fingerprint.ListTemplatesAsync();

                Debug.WriteLine("");
                foreach (var item in list)
                {
                    Debug.WriteLine("{0:D10} : {1}", item.Key, item.Value);
                }
                Debug.WriteLine("");
            }
        }
    }
}
