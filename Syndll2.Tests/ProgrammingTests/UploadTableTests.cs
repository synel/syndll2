using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class UploadTableTests
    {
        [TestMethod]
        public void Can_Upload_Table_From_File()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.UploadTableFromFile('V', 800, @"TestData\msg800.rdy");
            }
        }

        [TestMethod]
        public async Task Can_Upload_Table_From_File_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.UploadTableFromFileAsync('V', 800, @"TestData\msg800.rdy");
            }
        }

    }
}
