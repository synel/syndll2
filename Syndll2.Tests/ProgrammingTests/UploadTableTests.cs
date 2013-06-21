using System.Diagnostics;
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
                p.ProgressChanged += Programming_ProgressChanged;
                p.UploadTableFromFile(@"TestData\test777.rdy");
            }
        }

        [TestMethod]
        public async Task Can_Upload_Table_From_File_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                p.ProgressChanged += Programming_ProgressChanged;
                await p.UploadTableFromFileAsync(@"TestData\test777.rdy");
            }
        }

        [TestMethod]
        public void Can_Upload_Tables_From_Directory_File()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.ProgressChanged += Programming_ProgressChanged;
                p.UploadTableFromFile(@"TestData\dir001.rdy");
            }
        }

        [TestMethod]
        public async Task Can_Upload_Tables_From_Directory_File_Async()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.ProgressChanged += Programming_ProgressChanged;
                await p.UploadTableFromFileAsync(@"TestData\dir001.rdy");
            }
        }

        private void Programming_ProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
            Debug.WriteLine("");
            Debug.WriteLine("[{0:D3}/{1:D3}] {2} ({3:N0}%)",
                            args.CurrentBlock,
                            args.TotalBlocks,
                            args.Filename,
                            args.ProgressPercentage*100);
            Debug.WriteLine("");
        }

    }
}
