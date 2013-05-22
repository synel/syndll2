using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class DeleteTableTests
    {
        [TestMethod]
        public void Can_Delete_One_Table()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                p.DeleteTable('V', 800);
            }
        }

        [TestMethod]
        public async Task Can_Delete_One_Table_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                await p.DeleteTableAsync('V', 800);
            }
        }

    }
}
