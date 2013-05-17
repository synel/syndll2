using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Syndll2.Tests.BasicTests
{
    [TestClass]
    public class NumericFormatTests
    {
        [TestMethod]
        public void Convert_To_SynelNumeric1_0()
        {
            var s = SynelNumericFormat.Convert(0, 1);
            Assert.AreEqual("0", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric1_0()
        {
            var i = SynelNumericFormat.Convert("0");
            Assert.AreEqual(0, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric2_0()
        {
            var s = SynelNumericFormat.Convert(0, 2);
            Assert.AreEqual("00", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric2_0()
        {
            var i = SynelNumericFormat.Convert("00");
            Assert.AreEqual(0, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric3_0()
        {
            var s = SynelNumericFormat.Convert(0, 3);
            Assert.AreEqual("000", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric3_0()
        {
            var i = SynelNumericFormat.Convert("000");
            Assert.AreEqual(0, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric1_1()
        {
            var s = SynelNumericFormat.Convert(1, 1);
            Assert.AreEqual("1", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric1_1()
        {
            var i = SynelNumericFormat.Convert("1");
            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric2_1()
        {
            var s = SynelNumericFormat.Convert(1, 2);
            Assert.AreEqual("01", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric2_1()
        {
            var i = SynelNumericFormat.Convert("01");
            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric3_1()
        {
            var s = SynelNumericFormat.Convert(1, 3);
            Assert.AreEqual("001", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric3_1()
        {
            var i = SynelNumericFormat.Convert("001");
            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric1_10()
        {
            var s = SynelNumericFormat.Convert(10, 1);
            Assert.AreEqual(":", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric1_10()
        {
            var i = SynelNumericFormat.Convert(":");
            Assert.AreEqual(10, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric2_100()
        {
            var s = SynelNumericFormat.Convert(100, 2);
            Assert.AreEqual(":0", s);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric2_200()
        {
            var s = SynelNumericFormat.Convert(200, 2);
            Assert.AreEqual("D0", s);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric2_222()
        {
            var s = SynelNumericFormat.Convert(222, 2);
            Assert.AreEqual("F2", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric2_100()
        {
            var i = SynelNumericFormat.Convert(":0");
            Assert.AreEqual(100, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric3_100()
        {
            var s = SynelNumericFormat.Convert(100, 3);
            Assert.AreEqual("100", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric3_100()
        {
            var i = SynelNumericFormat.Convert("100");
            Assert.AreEqual(100, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric3_1000()
        {
            var s = SynelNumericFormat.Convert(1000, 3);
            Assert.AreEqual(":00", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric3_1000()
        {
            var i = SynelNumericFormat.Convert(":00");
            Assert.AreEqual(1000, i);
        }

        [TestMethod]
        public void Convert_To_SynelNumeric3_7899()
        {
            var s = SynelNumericFormat.Convert(7899, 3);
            Assert.AreEqual("~99", s);
        }

        [TestMethod]
        public void Convert_From_SynelNumeric3_7899()
        {
            var i = SynelNumericFormat.Convert("~99");
            Assert.AreEqual(7899, i);
        }

    }
}
