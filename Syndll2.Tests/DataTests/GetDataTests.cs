using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class GetDataTests
    {
        [TestMethod]
        public void Can_Get_Data()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetDataAndAcknowledge();
                if (data == null)
                    Assert.Inconclusive("This test is meant to be run individually, and there needs to be a transaction data on the terminal.");
            }
        }

        [TestMethod]
        public async Task Can_Get_Data_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetDataAndAcknowledgeAsync();
                if (data == null)
                    Assert.Inconclusive("This test is meant to be run individually, and there needs to be a transaction data on the terminal.");
            }
        }

        [TestMethod]
        public void Can_Get_Full_Data_Block()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetFullDataBlockAndAcknowledge();
                if (data == null)
                    Assert.Inconclusive("This test is meant to be run individually, and there needs to be two or more transactions on the terminal.");
            }
        }

        [TestMethod]
        public async Task Can_Get_Full_Data_Block_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                var data = await client.Terminal.GetFullDataBlockAndAcknowledgeAsync();
                if (data == null)
                    Assert.Inconclusive("This test is meant to be run individually, and there needs to be two or more transactions on the terminal.");
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

            if (data.Count == 0)
                Assert.Inconclusive("This test is meant to be run individually, and there needs to be some transaction data on the terminal.");
            
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

            if (data.Count == 0)
                Assert.Inconclusive("This test is meant to be run individually, and there needs to be some transaction data on the terminal.");

            Debug.WriteLine("");
            Debug.WriteLine("DATA RETRIEVED");
            Debug.WriteLine("--------------");
            foreach (var item in data)
                Debug.WriteLine(item);
        }
    }
}
