using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class OneRecordTests
    {
        [TestMethod]
        public void Can_Get_One_Record()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetSingleRecord('V', 800, "005");

                Assert.AreEqual('V', data.TableType);
                Assert.AreEqual(800, data.TableId);
                Assert.AreEqual(SearchResult.Success, data.ResultCode);
                Assert.AreEqual(4, data.RecordNumber);
                Assert.AreEqual("005 ENTER FUNCTION ", data.Value);
            }
        }

        [TestMethod]
        public async Task Can_Get_One_Record_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetSingleRecordAsync('V', 800, "005");

                Assert.AreEqual('V', data.TableType);
                Assert.AreEqual(800, data.TableId);
                Assert.AreEqual(SearchResult.Success, data.ResultCode);
                Assert.AreEqual(4, data.RecordNumber);
                Assert.AreEqual("005 ENTER FUNCTION ", data.Value);
            }
        }

    }
}
