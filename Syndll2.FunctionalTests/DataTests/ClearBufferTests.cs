using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.FunctionalTests.DataTests
{
    [TestClass]
    [Ignore] // Run these test manually when desired.
    public class ClearBufferTests
    {
        [TestMethod]
        public void Can_Clear_Buffer()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.ClearBuffer();
            }
        }

        [TestMethod]
        public async Task Can_Clear_Buffer_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.ClearBufferAsync();
            }
        }

        // The following are commented because they appear not to function correctly.  (The date is ignored).

        //[TestMethod]
        //public void Can_Clear_Buffer_By_Date()
        //{
        //    using (var client = TestSettings.Connect())
        //    {
        //        client.Terminal.ClearBuffer(DateTime.Today);
        //    }
        //}

        //[TestMethod]
        //public async Task Can_Clear_Buffer_By_Date_Async()
        //{
        //    using (var client = await TestSettings.ConnectAsync())
        //    {
        //        await client.Terminal.ClearBufferAsync(DateTime.Today);
        //    }
        //}
    }
}
