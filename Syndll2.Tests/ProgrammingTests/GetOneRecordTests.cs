using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class GetOneRecordTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.UploadTableFromFile(@"TestData\test777.rdy");
            }
        }

        [TestMethod]
        public void Can_Get_One_Record()
        {
            using (var client = TestSettings.Connect())
            {
                try
                {
                    var data = client.Terminal.GetSingleRecord('y', 777, "001");
                    AssertValidData(data);
                }
                catch (TimeoutException)
                {
                    Assert.Inconclusive("This terminal does not support single record commands.");
                }
            }
        }

        [TestMethod]
        public async Task Can_Get_One_Record_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                try
                {
                    var data = await client.Terminal.GetSingleRecordAsync('y', 777, "001");
                    AssertValidData(data);
                }
                catch (TimeoutException)
                {
                    Assert.Inconclusive("This terminal does not support single record commands.");
                }
            }
        }

        private static void AssertValidData(SingleRecord data)
        {
            Assert.AreEqual('y', data.TableType);
            Assert.AreEqual(777, data.TableId);
            Assert.AreEqual(SearchResult.Success, data.ResultCode);
            Assert.AreEqual(0, data.RecordNumber);
            Assert.AreEqual("001Testing SynDll2 ", data.Value);
        }


        [TestMethod]
        public void Can_Error_When_Getting_Nonexistent_Record()
        {
            using (var client = TestSettings.Connect())
            {
                try
                {
                    var data = client.Terminal.GetSingleRecord('y', 777, "002");
                    Assert.AreEqual(SearchResult.KeyNotFound, data.ResultCode);
                }
                catch (TimeoutException)
                {
                    Assert.Inconclusive("This terminal does not support single record commands.");
                }
            }
        }

        [TestMethod]
        public void Can_Error_When_Getting_Record_From_Nonexistent_Table()
        {
            using (var client = TestSettings.Connect())
            {
                try
                {
                    var data = client.Terminal.GetSingleRecord('y', 778, "001");
                    Assert.AreEqual(SearchResult.TableNotFound, data.ResultCode);
                }
                catch (TimeoutException)
                {
                    Assert.Inconclusive("This terminal does not support single record commands.");
                }
            }
        }
    }
}
