﻿using System.IO;
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
            Assert.AreEqual(115, rdy.Header.TotalCharacters);
            Assert.AreEqual(4, rdy.Records);
        }
    }
}
