using System;
using System.Globalization;
using System.IO;

namespace Syndll2.Data
{
    public class SingleRecord
    {
        private readonly char _tableType;
        private readonly int _tableId;
        private readonly SearchResult _resultCode;
        private readonly int _recordNumber;
        private readonly string _value;

        public char TableType
        {
            get { return _tableType; }
        }

        public int TableId
        {
            get { return _tableId; }
        }

        public SearchResult ResultCode
        {
            get { return _resultCode; }
        }

        public int RecordNumber
        {
            get { return _recordNumber; }
        }

        public string Value
        {
            get { return _value; }
        }
        
        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // S0MFSV8000000004005 ENTER FUNCTION 60>6
        // CT012345678901234567890123456789012CCCC
        //   0         1         2         3

        // Data Field Breakdown
        // MFS V800 0 000004 005 ENTER FUNCTION 

        internal static SingleRecord Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int minLength = 14;
            if (data.Length < minLength)
                throw new ArgumentException(
                    string.Format(
                        "Single record data should be at least {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", minLength, data.Length),
                    "data");

            return new SingleRecord(data);
        }

        private SingleRecord(string data)
        {
            var subcode = data.Substring(0, 3);
            if (subcode != "MFS")
                throw new InvalidDataException(
                    string.Format("Expected sub-code of \"{0}\" but got \"{1}\".", "MFS", subcode));

            _tableType = data[3];

            if (!int.TryParse(data.Substring(4, 3), NumberStyles.None, CultureInfo.InvariantCulture, out _tableId))
                throw new InvalidDataException("Couldn't parse the table ID from the single record data.");

            int i;
            if (!int.TryParse(data.Substring(7, 1), NumberStyles.None, CultureInfo.InvariantCulture, out i))
                throw new InvalidDataException("Couldn't parse the result code from the single record data.");
            _resultCode = (SearchResult) i;

            if (!int.TryParse(data.Substring(8, 6), NumberStyles.None, CultureInfo.InvariantCulture, out _recordNumber))
                throw new InvalidDataException("Couldn't parse the record number from the single record data.");

            if (data.Length > 14)
                _value = data.Substring(14);
        }
    }
}
