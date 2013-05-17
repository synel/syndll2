using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class GetDataTests
    {
        //[TestInitialize]
        //public void Initialize()
        //{
        //    // start these tests with clean data
        //    using (var client = TestSettings.Connect())
        //    {
        //        client.Operations.GetAllData();
        //        client.Operations.ClearBuffer();
        //    }
        //}

        [TestMethod]
        public void Can_Get_Data()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetDataAndAcknowledge();
                Assert.IsNotNull(data);
            }
        }

        [TestMethod]
        public async Task Can_Get_Data_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetDataAndAcknowledgeAsync();
                Assert.IsNotNull(data);
            }
        }

        [TestMethod]
        public void Can_Get_Full_Data_Block()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetFullDataBlockAndAcknowledge();
                Assert.IsNotNull(data);
            }
        }

        [TestMethod]
        public async Task Can_Get_Full_Data_Block_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetFullDataBlockAndAcknowledgeAsync();
                Assert.IsNotNull(data);
            }
        }

        [TestMethod]
        public void Can_Get_All_Data()
        {
            var data = new List<string>();

            using (var client = TestSettings.Connect())
            {
                string item;
                while ((item = client.Terminal.GetDataAndAcknowledge()) != null)
                    data.Add(item);
            }

            Debug.WriteLine("");
            Debug.WriteLine("DATA RETRIEVED");
            Debug.WriteLine("--------------");
            foreach (var item in data)
                Debug.WriteLine(item);
        }

        [TestMethod]
        public async Task Can_Get_All_Data_Async()
        {
            var data = new List<string>();

            using (var client = TestSettings.Connect())
            {
                string item;
                while ((item = await client.Terminal.GetDataAndAcknowledgeAsync()) != null)
                    data.Add(item);
            }

            Debug.WriteLine("");
            Debug.WriteLine("DATA RETRIEVED");
            Debug.WriteLine("--------------");
            foreach (var item in data)
                Debug.WriteLine(item);
        }
    }
}
