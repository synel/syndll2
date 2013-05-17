using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void TestStringToBytes()
        {
            var s = "000102030405060708090A0B0C0D0E0F1011121314151617181920A0BCFF";
            var b = Util.StringToByteArray(s);
            var x = new byte[]
                {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                    0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
                    0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0xA0, 0xBC, 0xFF
                };
            CollectionAssert.AreEqual(x, b);
        }
    }
}
