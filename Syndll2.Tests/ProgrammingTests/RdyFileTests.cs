using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class RdyFileTests
    {
        [TestMethod]
        public void Can_Read_Normal_Rdy_File()
        {
            RdyFile rdy;
            using (var stream = File.OpenRead(@"TestData\msg800.rdy"))
            {
                rdy = RdyFile.Read(stream);
            }

            Assert.IsNotNull(rdy);
            Assert.IsFalse(rdy.IsDirectoryFile);
            Assert.AreEqual('V', rdy.Header.TableType);
            Assert.AreEqual(800, rdy.Header.TableId);
            Assert.AreEqual(64, rdy.Header.RecordCount);
        }

        [TestMethod]
        public async Task Can_Read_Normal_Rdy_File_Async()
        {
            RdyFile rdy;
            using (var stream = File.OpenRead(@"TestData\msg800.rdy"))
            {
                rdy = await RdyFile.ReadAsync(stream);
            }

            Assert.IsNotNull(rdy);
            Assert.IsFalse(rdy.IsDirectoryFile);
            Assert.AreEqual('V', rdy.Header.TableType);
            Assert.AreEqual(800, rdy.Header.TableId);
            Assert.AreEqual(64, rdy.Header.RecordCount);
        }

        [TestMethod]
        public void Can_Read_Directory_Rdy_File()
        {
            RdyFile rdy;
            using (var stream = File.OpenRead(@"TestData\dir001.rdy"))
            {
                rdy = RdyFile.Read(stream);
            }

            Assert.IsNotNull(rdy);
            Assert.IsTrue(rdy.IsDirectoryFile);
            Assert.AreEqual('z', rdy.Header.TableType);
            Assert.AreEqual(1, rdy.Header.TableId);
            Assert.AreEqual(87, rdy.Header.TotalCharacters);
            Assert.AreEqual(3, rdy.Records.Count);
        }

        [TestMethod]
        public async Task Can_Read_Directory_Rdy_File_Async()
        {
            RdyFile rdy;
            using (var stream = File.OpenRead(@"TestData\dir001.rdy"))
            {
                rdy = await RdyFile.ReadAsync(stream);
            }

            Assert.IsNotNull(rdy);
            Assert.IsTrue(rdy.IsDirectoryFile);
            Assert.AreEqual('z', rdy.Header.TableType);
            Assert.AreEqual(1, rdy.Header.TableId);
            Assert.AreEqual(87, rdy.Header.TotalCharacters);
            Assert.AreEqual(3, rdy.Records.Count);
        }

        [TestMethod]
        public void Can_Create_New_Rdy_File()
        {
            var rdy = RdyFile.Create('V', 120, 23, 7);

            rdy.AddRecord("00001N0", "Matt Johnson");
            rdy.AddRecord("00002N0", "John Doe");

            Assert.AreEqual('V', rdy.Header.TableType);
            Assert.AreEqual(120, rdy.Header.TableId);
            Assert.AreEqual(23, rdy.Header.RecordSize);
            Assert.AreEqual(7, rdy.Header.KeyLength);
            Assert.AreEqual(0, rdy.Header.KeyOffset);
            Assert.AreEqual(false, rdy.Header.Sorted);
            Assert.AreEqual(false, rdy.Header.Packed);

            Assert.AreEqual(2, rdy.Header.RecordCount);
            var expectedTotalChars = (rdy.Header.RecordCount * rdy.Header.RecordSize) + RdyFile.RdyHeader.HeaderSize;
            Assert.AreEqual(expectedTotalChars, rdy.Header.TotalCharacters);

            Debug.WriteLine(rdy.ToString());
            const string expected = "V12000069A2323002070000\r\n00001N0Matt Johnson    \r\n00002N0John Doe        \r\n";
            Assert.AreEqual(expected, rdy.ToString());
        }
    }
}
