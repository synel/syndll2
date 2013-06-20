using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class GetEmptyDataTests
    {
        [TestInitialize]
        public void Initialize()
        {
            // start these tests with all data acknowledged
            using (var client = TestSettings.Connect())
            {
                while (client.Terminal.GetDataAndAcknowledge() != null) { }
            }
        }

        [TestMethod]
        public void Get_Data_Returns_Null_When_Terminal_Is_Empty()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetData();
                Assert.IsNull(data);
            }
        }

        [TestMethod]
        public async Task Get_Data_Returns_Null_When_Terminal_Is_Empty_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetDataAsync();
                Assert.IsNull(data);
            }
        }

        [TestMethod]
        public void Get_Full_Data_Block_Returns_Null_When_Terminal_Is_Empty()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetFullDataBlock();
                Assert.IsNull(data);
            }
        }

        [TestMethod]
        public async Task Get_Full_Data_Block_Returns_Null_When_Terminal_Is_Empty_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetFullDataBlockAsync();
                Assert.IsNull(data);
            }
        }
    }
}
