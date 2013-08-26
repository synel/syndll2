using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.Tests.ProgrammingTests
{
    [TestClass]
    public class SystemParametersTests
    {
        [TestMethod]
        public void Can_Parse_System_Parameters_Rdy_File()
        {
            var rdy = RdyFile.Read(@"TestData\sys001.rdy");
            
            Assert.IsNotNull(rdy);
            Assert.IsFalse(rdy.IsDirectoryFile);
            Assert.AreEqual('p', rdy.Header.TableType);
            Assert.AreEqual(001, rdy.Header.TableId);

            var sys = new SystemParameters(rdy);
            Assert.IsNotNull(sys);
            Assert.AreEqual(10, sys.Parameters.Count);
            Assert.AreEqual("05",sys.Parameters[0]);
            Assert.AreEqual("B", sys.Parameters[1]);
            Assert.AreEqual("00000", sys.Parameters[2]);
            Assert.AreEqual("Y", sys.Parameters[3]);
            Assert.AreEqual("15", sys.Parameters[4]);
            Assert.AreEqual("", sys.Parameters[5]);
            Assert.AreEqual("50", sys.Parameters[6]);
            Assert.AreEqual("N", sys.Parameters[9]);
            Assert.AreEqual("3", sys.Parameters[11]);
            Assert.AreEqual("--", sys.Parameters[13]);
        }

        [TestMethod]
        public void Can_Create_System_Parameters_Rdy_File()
        {
            var sys = new SystemParameters(new Dictionary<int, string>
                                           {
                                               { 0, "05" },
                                               { 1, "B" },
                                               { 2, "00000" },
                                               { 3, "Y" },
                                               { 4, "15" },
                                               { 5, "" },
                                               { 6, "50" },
                                               { 9, "N" },
                                               { 11, "3" },
                                               { 13, "--" }
                                           });

            const string expected = "^00005^001B^00200000^003Y^00415^005^00650^009N^0113^013--^^";

            var rdy = sys.GetRdyFile();
            Assert.AreEqual('p', rdy.Header.TableType);
            Assert.AreEqual(1, rdy.Header.TableId);
            Assert.AreEqual(expected.Length, rdy.Header.RecordSize);
            Assert.AreEqual(1, rdy.Header.RecordCount);
            Assert.AreEqual(1, rdy.Records.Count);
            Assert.AreEqual(expected, rdy.Records[0].Data);
        }

        [TestMethod]
        public void Can_Set_Fixed_Clock_Transitions()
        {
            var rdy = RdyFile.Read(@"TestData\sys001.rdy");
            var sys = new SystemParameters(rdy);
            Assert.AreEqual(0, sys.ClockTransitions.Count);

            sys.ClockTransitions = new ClockTransition[]
                                   {
                                       new FixedClockTransition(1, new DateTime(2013, 3, 10, 2, 0, 0)),
                                       new FixedClockTransition(-1, new DateTime(2013, 11, 3, 2, 0, 0))
                                   };

            var value = sys.Parameters[5];
            Assert.AreEqual("11003130200+110311130200-1", value);
        }
    }
}
