using System;
using System.Globalization;
using System.IO;
using Syndll2.Data;

namespace Syndll2.TimeAmerica
{
    /// <summary>
    /// Represents punch data retrieved from a 700 series terminal running one of the Time America applications.
    /// </summary>
    public class PunchData : TerminalData<PunchData>
    {
        private int _firmwareVersion;
        private DateTime _transactionTime;
        private int _badgeNumber;
        private PunchTypes _punchType;

        public int FirmwareVersion
        {
            get { return _firmwareVersion; }
        }

        public DateTime TransactionTime
        {
            get { return _transactionTime; }
        }

        public int BadgeNumber
        {
            get { return _badgeNumber; }
        }

        public PunchTypes PunchType
        {
            get { return _punchType; }
        }

        private PunchData() { }

        protected override void ParseData(string data)
        {
            // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
            // Only the Data field should be passed in.
            // d038243420130514|172641,00001,1,*,@,@,@,@,@,,,,,,,,69?1
            // CT0123456789012345678901234567890123456789012345678CCCC
            //   0         1         2         3         4

            // Data Field Breakdown
            // 3 82434 20130514 | 172641 , 00001 , 1 , * , @ , @ , @ , @ , @ , , , , , , , ,
            
            // TODO: what is that first character?

            if (!int.TryParse(data.Substring(1, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _firmwareVersion))
                throw new InvalidDataException("Couldn't parse the firmware version from the punch data.");

            var fields = data.Substring(6).Split(',');
            const int expectedLength = 17;
            if (fields.Length != expectedLength)
                throw new InvalidDataException(string.Format("Expected {0} fields in the punch data, but got {1}.", expectedLength, fields.Length));

            if (!DateTime.TryParseExact(fields[0], "yyyyMMdd|HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _transactionTime))
                throw new InvalidDataException("Couldn't parse the transaction time from the punch data.");

            if (!int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out _badgeNumber))
                throw new InvalidDataException("Couldn't parse the badge number from the punch data.");

            // TODO: field 1

            if (fields[3].Length == 0)
                throw new InvalidDataException("Couldn't parse the punch type from the punch data.");
            _punchType = (PunchTypes)fields[3][0];

            // TODO: remaining fields  (wages? labor levels?)

        }
    }
}
