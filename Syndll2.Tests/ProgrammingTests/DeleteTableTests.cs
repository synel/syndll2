using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class DeleteTableTests
    {
        [TestMethod]
        public void Can_Delete_One_Table()
        {
            using (var client = TestSettings.Connect())
            {
                using (var p = client.Terminal.Programming())
                {
                    try
                    {
                        p.UploadTableFromFile(@"TestData\test777.rdy");
                        var data = client.Terminal.GetSingleRecord('y', 777, "001");
                        Assert.AreEqual(SearchResult.Success, data.ResultCode);

                        // have to reset programming for the next section to work
                        client.Terminal.Run();
                        client.Terminal.Halt();

                        p.DeleteTable('y', 777);
                        data = client.Terminal.GetSingleRecord('y', 777, "001");
                        Assert.AreEqual(SearchResult.TableNotFound, data.ResultCode);
                    }
                    catch (TimeoutException)
                    {
                        Assert.Inconclusive("This terminal does not support single record commands.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task Can_Delete_One_Table_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                try
                {
                    await p.UploadTableFromFileAsync(@"TestData\test777.rdy");
                    var data = await client.Terminal.GetSingleRecordAsync('y', 777, "001");
                    Assert.AreEqual(SearchResult.Success, data.ResultCode);

                    // have to reset programming for the next section to work
                    await client.Terminal.RunAsync();
                    await client.Terminal.HaltAsync();

                    await p.DeleteTableAsync('y', 777);
                    data = await client.Terminal.GetSingleRecordAsync('y', 777, "001");
                    Assert.AreEqual(SearchResult.TableNotFound, data.ResultCode);
                }
                catch (TimeoutException)
                {
                    Assert.Inconclusive("This terminal does not support single record commands.");
                }
            }
        }

        [TestMethod]
        public void Can_Delete_One_Table_Blind()
        {
            using (var client = TestSettings.Connect())
            {
                using (var p = client.Terminal.Programming())
                {
                    p.UploadTableFromFile(@"TestData\test777.rdy");

                    // have to reset programming for the next section to work
                    client.Terminal.Run();
                    client.Terminal.Halt();

                    p.DeleteTable('y', 777);
                }
            }
        }

        [TestMethod]
        public async Task Can_Delete_One_Table_Blind_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.UploadTableFromFileAsync(@"TestData\test777.rdy");

                // have to reset programming for the next section to work
                await client.Terminal.RunAsync();
                await client.Terminal.HaltAsync();

                await p.DeleteTableAsync('y', 777);
            }
        }
    }
}
