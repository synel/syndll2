using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class CRCTests
    {
        [TestMethod]
        public void CRC_Calculate_Test_EmptyString()
        {
            var crc = SynelCRC.Calculate("");
            Assert.AreEqual("0000", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_1234()
        {
            var crc = SynelCRC.Calculate("1234");
            Assert.AreEqual("=789", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_9AF0()
        {
            var crc = SynelCRC.Calculate("9AF0");
            Assert.AreEqual("643:", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_A4()
        {
            var crc = SynelCRC.Calculate("A4");
            Assert.AreEqual("482:", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_B4()
        {
            var crc = SynelCRC.Calculate("B4");
            Assert.AreEqual("1=79", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_C4()
        {
            var crc = SynelCRC.Calculate("C4");
            Assert.AreEqual("2>48", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_D4()
        {
            var crc = SynelCRC.Calculate("D4");
            Assert.AreEqual(";7=?", crc);
        }

        [TestMethod]
        public void CRC_Calculate_Test_Binary()
        {
            var crc = SynelCRC.Calculate(new byte[] {0x4c, 0x31});
            CollectionAssert.AreEqual(new byte[] {0x36, 0x3e, 0x3d, 0x33}, crc);
        }

        [TestMethod]
        public void CRC_Calculate_Perf_Test_String()
        {
            var sw = new Stopwatch();
            sw.Start();

            const string data = "L1";
            for (int i = 0; i < 1000000; i++)
                SynelCRC.Calculate(data);

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 300, "Performance test failed at " + sw.ElapsedMilliseconds + " ms.");
        }

        [TestMethod]
        public void CRC_Calculate_Perf_Test_Binary()
        {
            var sw = new Stopwatch();
            sw.Start();

            var data = new byte[] { 0x4c, 0x31 };
            for (int i = 0; i < 1000000; i++)
                SynelCRC.Calculate(data);

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 200, "Performance test failed at " + sw.ElapsedMilliseconds + " ms.");
        }

        [TestMethod]
        public void CRC_Verify_Perf_Test_Binary()
        {
            var sw = new Stopwatch();
            sw.Start();

            var data = new byte[] {0x4c, 0x31};
            var crc = new byte[] {0x36, 0x3e, 0x3d, 0x33};
            for (int i = 0; i < 1000000; i++)
                SynelCRC.Verify(data, crc);

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 200, "Performance test failed at " + sw.ElapsedMilliseconds + " ms.");
        }

        [TestMethod]
        public void CRC_Verify_Test_Equal()
        {
            var data = new byte[] { 0x4c, 0x31 };
            var crc = new byte[] { 0x36, 0x3e, 0x3d, 0x33 };
            var result = SynelCRC.Verify(data, crc);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CRC_Verify_Test_NotEqual()
        {
            var data = new byte[] { 0x4c, 0x31 };
            var crc = new byte[] { 0x01, 0x02, 0x03, 0x04 }; // bad crc
            var result = SynelCRC.Verify(data, crc);
            Assert.IsFalse(result);
        }
    }
}
