using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class AcknowledgeLastRecordTests
    {
        [TestMethod]
        public void Can_Acknowledge_Last_Record()
        {
            using (var client = TestSettings.Connect())
            {
                client.Terminal.AcknowledgeLastRecord();
            }
        }

        [TestMethod]
        public async Task Can_Acknowledge_Last_Record_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            {
                await client.Terminal.AcknowledgeLastRecordAsync();
            }
        }
    }
}
