using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.TimeAmerica;

namespace Syndll2.Tests.DataTests
{
    [TestClass]
    public class PunchDataTests
    {
        [TestMethod]
        public void Can_Get_Time_America_Punch_Data()
        {
            using (var client = TestSettings.Connect())
            {
                var data = client.Terminal.GetDataAndAcknowledge();
                Assert.IsNotNull(data);

                // TODO - programatically put a punch on the clock and then fully test it
            }
        }

        [TestMethod]
        public void Can_Parse_Time_America_Punch_Data()
        {
            const string data = "38243420130514|172641,00001,1,*,@,@,@,@,@,,,,,,,,";

            var punch = PunchData.Parse(data);
            Assert.IsNotNull(data);

            Assert.AreEqual(82434, punch.FirmwareVersion);
            Assert.AreEqual(new DateTime(2013, 05, 14, 17, 26, 41), punch.TransactionTime);
            Assert.AreEqual(1, punch.BadgeNumber);
            Assert.AreEqual(PunchTypes.In, punch.PunchType);
        }
    }
}
