using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.FunctionalTests.ProgrammingTests
{
    [TestClass]
    public class SystemParametersTests
    {
        [TestMethod]
        public void Can_Program_Clock_Transition()
        {
            var rdy = RdyFile.Read(@"TestData\sys001.rdy");
            var sys = new SystemParameters(rdy);

            sys.ClockTransitions = new ClockTransition[]
                                   {
                                       new FixedClockTransition(1, new DateTime(2013, 3, 10, 2, 0, 0)),
                                       new FixedClockTransition(-1, new DateTime(2013, 11, 3, 2, 0, 0))
                                   };

            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                var rdyFile = sys.GetRdyFile();
                p.UploadTableFromRdy(rdyFile);
            }
        }
    }
}
