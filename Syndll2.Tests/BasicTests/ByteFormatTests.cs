using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class ByteFormatTests
    {
        [TestMethod]
        public void Convert_To_SynelByte_Null()
        {
            var s = SynelByteFormat.Convert((byte[])null);
            Assert.IsNull(s);
        }

        [TestMethod]
        public void Convert_From_SynelByte_Null()
        {
            var ba = SynelByteFormat.Convert((string)null);
            Assert.IsNull(ba);
        }

        [TestMethod]
        public void Convert_To_SynelByte_Empty()
        {
            var s = SynelByteFormat.Convert(new byte[0]);
            Assert.AreEqual(0, s.Length);
        }

        [TestMethod]
        public void Convert_From_SynelByte_Empty()
        {
            var ba = SynelByteFormat.Convert(string.Empty);
            Assert.AreEqual(0, ba.Length);
        }

        [TestMethod]
        public void Convert_To_SynelByte_Data()
        {
            var s = SynelByteFormat.Convert(new byte[] {0, 1, 2, 3, 100, 101, 102, 103, 200, 201, 202, 203, 255});
            Assert.AreEqual("`0`1`2`3f4f5f6f7l8l9l:l;o?", s);
        }

        [TestMethod]
        public void Convert_From_SynelByte_Data()
        {
            var ba = SynelByteFormat.Convert("`0`1`2`3f4f5f6f7l8l9l:l;o?");
            CollectionAssert.AreEqual(new byte[] {0, 1, 2, 3, 100, 101, 102, 103, 200, 201, 202, 203, 255}, ba);
        }
    }
}
